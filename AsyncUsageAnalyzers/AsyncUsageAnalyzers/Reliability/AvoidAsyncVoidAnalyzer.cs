// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Reliability
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer identifies code using <c>void</c>-returning <see langword="async"/> methods, and reports a
    /// warning.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoCodeFix("No clear transformation to correct violations of this rule.")]
    internal class AvoidAsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="AvoidAsyncVoidAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "AvoidAsyncVoid";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReliabilityResources.AvoidAsyncVoidTitle), ReliabilityResources.ResourceManager, typeof(ReliabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReliabilityResources.AvoidAsyncVoidMessageFormat), ReliabilityResources.ResourceManager, typeof(ReliabilityResources));
        private static readonly string Category = "AsyncUsage.CSharp.Reliability";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReliabilityResources.AvoidAsyncVoidDescription), ReliabilityResources.ResourceManager, typeof(ReliabilityResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/AvoidAsyncVoid.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly ImmutableArray<SyntaxKind> AnonymousFunctionExpressionKinds =
            ImmutableArray.Create(
                SyntaxKind.AnonymousMethodExpression,
                SyntaxKind.ParenthesizedLambdaExpression,
                SyntaxKind.SimpleLambdaExpression);

        private static readonly Action<CompilationStartAnalysisContext> CompilationStartAction = HandleCompilationStart;
        private static readonly Action<SyntaxNodeAnalysisContext> AnonymousFunctionExpressionAction = HandleAnonymousFunctionExpression;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(CompilationStartAction);
        }

        private static void HandleCompilationStart(CompilationStartAnalysisContext context)
        {
            Analyzer analyzer = new Analyzer(context.Compilation.GetOrCreateGeneratedDocumentCache());
            context.RegisterSymbolAction(analyzer.HandleMethodDeclaration, SymbolKind.Method);
            context.RegisterSyntaxNodeActionHonorExclusions(AnonymousFunctionExpressionAction, AnonymousFunctionExpressionKinds);
        }

        private static void HandleAnonymousFunctionExpression(SyntaxNodeAnalysisContext context)
        {
            AnonymousFunctionExpressionSyntax node = (AnonymousFunctionExpressionSyntax)context.Node;
            if (node.AsyncKeyword.IsKind(SyntaxKind.None) || node.AsyncKeyword.IsMissing)
                return;

            TypeInfo typeInfo = context.SemanticModel.GetTypeInfo(node);
            INamedTypeSymbol convertedType = typeInfo.ConvertedType as INamedTypeSymbol;
            if (convertedType == null)
                return;

            if (convertedType.TypeKind != TypeKind.Delegate || convertedType.DelegateInvokeMethod == null)
                return;

            if (!convertedType.DelegateInvokeMethod.ReturnsVoid)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, node.AsyncKeyword.GetLocation(), "<anonymous>"));
        }

        private sealed class Analyzer
        {
            private readonly ConcurrentDictionary<SyntaxTree, bool> generatedHeaderCache;

            public Analyzer(ConcurrentDictionary<SyntaxTree, bool> generatedHeaderCache)
            {
                this.generatedHeaderCache = generatedHeaderCache;
            }

            public void HandleMethodDeclaration(SymbolAnalysisContext context)
            {
                IMethodSymbol symbol = (IMethodSymbol)context.Symbol;
                if (!symbol.IsAsync || !symbol.ReturnsVoid)
                    return;

                Location location = symbol.Locations[0];
                if (!location.IsInSource || location.SourceTree.IsGeneratedDocument(this.generatedHeaderCache, context.CancellationToken))
                    return;

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, location, symbol.Name));
            }
        }
    }
}
