// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Usage
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Text;

    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(DontUseThreadSleepCodeUniversalCodeFixProvider))]
    [Shared]
    internal class DontUseThreadSleepCodeUniversalCodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableArray<string> FixableDiagnostics =
                ImmutableArray.Create(DontUseThreadSleepAnalyzer.DiagnosticId, DontUseThreadSleepInAsyncCodeAnalyzer.DiagnosticId);

        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return CustomFixAllProviders.BatchFixer;
        }

        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (diagnostic.Id == DontUseThreadSleepInAsyncCodeAnalyzer.DiagnosticId)
                {
                    RegisterCodeFixForDiagnosic(context, diagnostic);
                }
                else if (diagnostic.Id == DontUseThreadSleepAnalyzer.DiagnosticId)
                {
                    var document = context.Document;

                    var root = await document.GetSyntaxRootAsync().ConfigureAwait(false);
                    var invocationExpression = root.FindNode(TextSpan.FromBounds(diagnostic.Location.SourceSpan.Start, diagnostic.Location.SourceSpan.End), getInnermostNodeForTie: true) as InvocationExpressionSyntax;

                    if (invocationExpression == null)
                    {
                        return;
                    }

                    if (invocationExpression.IsInsideAsyncCode())
                    {
                        RegisterCodeFixForDiagnosic(context, diagnostic);
                    }
                }
            }
        }

        private static void RegisterCodeFixForDiagnosic(CodeFixContext context, Diagnostic diagnostic)
        {
            context.RegisterCodeFix(
                CodeAction.Create(
                    "Use `await Task.Delay(...)` or `await Task.Yield()`",
                    cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, cancellationToken)),
                diagnostic);
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
        {
            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var firstNodeWithCorrectSpan = root
                .FindNode(diagnostic.Location.SourceSpan);
            InvocationExpressionSyntax expression = firstNodeWithCorrectSpan
                .DescendantNodesAndSelf()
                .OfType<InvocationExpressionSyntax>()
                .First();

            var arguments = expression.ArgumentList;

            var newExpression = await IsArgumentListWithZeroValueAsync(document, arguments).ConfigureAwait(true)
                ? GenerateTaskYieldExpression()
                : GenerateTaskDelayExpression(arguments);

            SyntaxNode newRoot = root.ReplaceNode(expression, newExpression.WithTriviaFrom(expression));
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }

        private static async Task<bool> IsArgumentListWithZeroValueAsync(Document document, ArgumentListSyntax argumentListSyntax)
        {
            // all valid overloads of Thread.Sleep() method take exactly one argument
            if (argumentListSyntax.Arguments.Count != 1)
            {
                return false;
            }

            var argumentExpression = argumentListSyntax.Arguments.First().Expression;

            var argumentString = argumentExpression.ToString().Trim();
            if (argumentString == "0" || argumentString == "TimeSpan.Zero")
            {
                return true;
            }

            var semanticModel = await document.GetSemanticModelAsync().ConfigureAwait(false);
            var optionalValue = semanticModel.GetConstantValue(argumentExpression);
            if (optionalValue.HasValue && optionalValue.Value.Equals(0))
            {
                return true;
            }

            var memberAccessExpression = argumentExpression as MemberAccessExpressionSyntax;
            if (memberAccessExpression != null)
            {
                IFieldSymbol propertySymbol = null;
                return memberAccessExpression.TryGetFieldSymbolByTypeNameAndMethodName(semanticModel, "System.TimeSpan", "Zero", out propertySymbol);
            }

            return false;
        }

        private static AwaitExpressionSyntax GenerateTaskDelayExpression(ArgumentListSyntax methodArgumentList) =>
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("System"),
                                    SyntaxFactory.IdentifierName("Threading")),
                                SyntaxFactory.IdentifierName("Tasks")),
                            SyntaxFactory.IdentifierName("Task")),
                        SyntaxFactory.IdentifierName("Delay")))
                    .WithArgumentList(methodArgumentList));

        private static AwaitExpressionSyntax GenerateTaskYieldExpression() =>
            SyntaxFactory.AwaitExpression(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        SyntaxFactory.MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.MemberAccessExpression(
                                    SyntaxKind.SimpleMemberAccessExpression,
                                    SyntaxFactory.IdentifierName("System"),
                                    SyntaxFactory.IdentifierName("Threading")),
                                SyntaxFactory.IdentifierName("Tasks")),
                            SyntaxFactory.IdentifierName("Task")),
                        SyntaxFactory.IdentifierName("Yield"))));
    }
}
