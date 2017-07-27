// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    /// <summary>
    /// Implements a code fix for <see cref="UseConfigureAwaitCodeFixProvider"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(UseConfigureAwaitCodeFixProvider))]
    [Shared]
    internal class UseConfigureAwaitCodeFixProvider : CodeFixProvider
    {
        private const string UseConfigureAwaitFalseEquivalenceId = nameof(UseConfigureAwaitCodeFixProvider) + "_False";
        private const string UseConfigureAwaitTrueEquivalenceId = nameof(UseConfigureAwaitCodeFixProvider) + "_True";

        private static readonly ImmutableArray<string> FixableDiagnostics =
            ImmutableArray.Create(UseConfigureAwaitAnalyzer.DiagnosticId);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return CustomFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            foreach (var diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Id.Equals(UseConfigureAwaitAnalyzer.DiagnosticId))
                {
                    continue;
                }

                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Use ConfigureAwait(false)",
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, SyntaxKind.FalseLiteralExpression, cancellationToken),
                        UseConfigureAwaitFalseEquivalenceId),
                    diagnostic);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        "Use ConfigureAwait(true)",
                        cancellationToken => GetTransformedDocumentAsync(context.Document, diagnostic, SyntaxKind.TrueLiteralExpression, cancellationToken),
                        UseConfigureAwaitTrueEquivalenceId),
                    diagnostic);
            }

            return Task.FromResult(true);
        }

        private static async Task<Document> GetTransformedDocumentAsync(Document document, Diagnostic diagnostic, SyntaxKind literalKind, CancellationToken cancellationToken)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            ExpressionSyntax expression = (ExpressionSyntax)root.FindNode(diagnostic.Location.SourceSpan);
            var newExpression = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expression,
                    SyntaxFactory.IdentifierName(nameof(Task.ConfigureAwait))),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(
                            SyntaxFactory.LiteralExpression(literalKind)))))
                .WithAdditionalAnnotations(Formatter.Annotation);

            SyntaxNode newRoot = root.ReplaceNode(expression, newExpression);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
