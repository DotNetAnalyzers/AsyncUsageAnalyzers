// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Naming
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
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

        private static readonly Action<CompilationStartAnalysisContext> CompilationStartAction = HandleCompilationStart;

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
                if (symbol.Name.EndsWith("Async", StringComparison.Ordinal))
                {
                    return;
                }

                if (symbol.Locations.IsDefaultOrEmpty)
                {
                    return;
                }

                if (symbol.IsOverride)
                {
                    return;
                }

                if (!symbol.ExplicitInterfaceImplementations.IsDefaultOrEmpty)
                {
                    return;
                }

                Location location = symbol.Locations[0];
                if (!location.IsInSource || location.SourceTree.IsGeneratedDocument(this.generatedHeaderCache, context.CancellationToken))
                {
                    return;
                }

                // void-returning methods are not asynchronous according to their signature, even if they use `async`
                if (symbol.ReturnsVoid)
                {
                    return;
                }

                // This check conveniently covers Task and Task<T> by ignoring the `1 in Task<T>.
                if (!string.Equals(nameof(Task), symbol.ReturnType?.Name, StringComparison.Ordinal))
                {
                    return;
                }

                if (!string.Equals(typeof(Task).Namespace, symbol.ReturnType?.ContainingNamespace?.ToString(), StringComparison.Ordinal))
                {
                    return;
                }

                if (symbol.MethodKind == MethodKind.PropertyGet || symbol.MethodKind == MethodKind.PropertySet)
                {
                    return;
                }

                if ((symbol.ContainingType.TypeKind == TypeKind.Class || symbol.ContainingType.TypeKind == TypeKind.Struct)
                    && symbol.DeclaredAccessibility == Accessibility.Public)
                {
                    // As a final check, make sure the method isn't implicitly implementing an interface method
                    foreach (INamedTypeSymbol interfaceType in symbol.ContainingType.AllInterfaces)
                    {
                        foreach (var member in interfaceType.GetMembers(symbol.Name))
                        {
                            if (Equals(symbol.ContainingType.FindImplementationForInterfaceMember(member), symbol))
                            {
                                return;
                            }
                        }
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));
            }
        }
    }
}
