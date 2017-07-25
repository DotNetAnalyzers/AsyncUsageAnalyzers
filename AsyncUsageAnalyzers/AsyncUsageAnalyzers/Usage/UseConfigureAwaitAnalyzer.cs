// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The continuation behavior for a <see cref="Task"/> should be configured by calling
    /// <see cref="Task.ConfigureAwait"/> prior to awaiting the task. This analyzer reports a diagnostic if an
    /// <see langword="await"/> expression is used on a <see cref="Task"/> that has not been configured.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UseConfigureAwaitAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="UseConfigureAwaitAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "UseConfigureAwait";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(UsageResources.UseConfigureAwaitTitle), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(UsageResources.UseConfigureAwaitMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string Category = "AsyncUsage.CSharp.Usage";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(UsageResources.UseConfigureAwaitDescription), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/UseConfigureAwait.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<SyntaxNodeAnalysisContext> AwaitExpressionAction = HandleAwaitExpression;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSyntaxNodeAction(AwaitExpressionAction, SyntaxKind.AwaitExpression);
        }

        private static void HandleAwaitExpression(SyntaxNodeAnalysisContext context)
        {
            AwaitExpressionSyntax syntax = (AwaitExpressionSyntax)context.Node;
            ExpressionSyntax expression = syntax.Expression;
            if (!IsTask(expression, context.SemanticModel))
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, expression.GetLocation()));
        }

        private static bool IsTask(ExpressionSyntax expression, SemanticModel semanticModel)
        {
            var type = semanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;
            if (type == null)
            {
                return false;
            }

            INamedTypeSymbol taskType;
            if (type.IsGenericType)
            {
                type = type.ConstructedFrom;
                taskType = semanticModel.Compilation.GetTypeByMetadataName(typeof(Task<>).FullName);
            }
            else
            {
                taskType = semanticModel.Compilation.GetTypeByMetadataName(typeof(Task).FullName);
            }

            return type.Equals(taskType);
        }
    }
}
