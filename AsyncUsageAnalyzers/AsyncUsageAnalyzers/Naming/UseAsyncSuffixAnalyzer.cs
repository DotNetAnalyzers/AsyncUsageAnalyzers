// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Naming
{
    using System;
    using System.Collections.Immutable;
    using AsyncUsageAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer identifies asynchronous methods using the Task-based Asynchronous Pattern (TAP) according to their
    /// signature, and reports a warning if the method name does not include the suffix <c>Async</c>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class UseAsyncSuffixAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="UseAsyncSuffixAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "UseAsyncSuffix";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(NamingResources.UseAsyncSuffixTitle), NamingResources.ResourceManager, typeof(NamingResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(NamingResources.UseAsyncSuffixMessageFormat), NamingResources.ResourceManager, typeof(NamingResources));
        private static readonly string Category = "AsyncUsage.CSharp.Naming";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(NamingResources.UseAsyncSuffixDescription), NamingResources.ResourceManager, typeof(NamingResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/UseAsyncSuffix.md";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        private static readonly Action<SymbolAnalysisContext> MethodDeclarationAction = HandleMethodDeclaration;

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.EnableConcurrentExecution();
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);

            context.RegisterSymbolAction(MethodDeclarationAction, SymbolKind.Method);
        }

        private static void HandleMethodDeclaration(SymbolAnalysisContext context)
        {
            IMethodSymbol symbol = (IMethodSymbol)context.Symbol;
            if (symbol.Name.EndsWith("Async", StringComparison.Ordinal))
            {
                return;
            }

            if (!symbol.HasAsyncSignature())
            {
                return;
            }

            if (symbol.MethodKind == MethodKind.PropertyGet || symbol.MethodKind == MethodKind.PropertySet)
            {
                return;
            }

            if (symbol.IsOverrideOrImplementation())
            {
                return;
            }

            if (symbol.IsAsyncMain())
            {
                return;
            }

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));
        }
    }
}
