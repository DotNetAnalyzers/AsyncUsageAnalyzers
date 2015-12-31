// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Threading;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer reports a diagnostic if an asynchronous method does not include a <see cref="CancellationToken"/>
    /// parameter.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class IncludeCancellationParameterAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="IncludeCancellationParameterAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "IncludeCancellationParameter";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(UsageResources.IncludeCancellationParameterTitle), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(UsageResources.IncludeCancellationParameterMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string Category = "AsyncUsage.CSharp.Usage";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(UsageResources.IncludeCancellationParameterDescription), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/IncludeCancellationParameter.md";

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
            Analyzer analyzer = new Analyzer(context.Compilation.GetOrCreateGeneratedDocumentCache(), context.Compilation);
            context.RegisterSymbolAction(analyzer.HandleMethodDeclaration, SymbolKind.Method);
        }

        private sealed class Analyzer
        {
            private readonly ConcurrentDictionary<SyntaxTree, bool> generatedHeaderCache;
            private INamedTypeSymbol cancellationTokenType;

            public Analyzer(ConcurrentDictionary<SyntaxTree, bool> generatedHeaderCache, Compilation compilation)
            {
                this.generatedHeaderCache = generatedHeaderCache;
                this.cancellationTokenType = compilation.GetTypeByMetadataName(typeof(CancellationToken).FullName);
            }

            public void HandleMethodDeclaration(SymbolAnalysisContext context)
            {
                IMethodSymbol symbol = (IMethodSymbol)context.Symbol;
                if (this.cancellationTokenType == null)
                {
                    return;
                }

                if (symbol.DeclaredAccessibility == Accessibility.Private || symbol.DeclaredAccessibility == Accessibility.NotApplicable)
                {
                    return;
                }

                if (symbol.IsImplicitlyDeclared)
                {
                    return;
                }

                switch (symbol.MethodKind)
                {
                case MethodKind.LambdaMethod:
                case MethodKind.Constructor:
                case MethodKind.Conversion:
                case MethodKind.UserDefinedOperator:
                case MethodKind.PropertyGet:
                case MethodKind.PropertySet:
                case MethodKind.StaticConstructor:
                case MethodKind.BuiltinOperator:
                case MethodKind.Destructor:
                case MethodKind.EventAdd:
                case MethodKind.EventRaise:
                case MethodKind.EventRemove:
                    // These can't be async
                    return;

                case MethodKind.DelegateInvoke:
                    // Not sure if this is reachable
                    return;

                case MethodKind.ExplicitInterfaceImplementation:
                    // These are ignored
                    return;

                case MethodKind.ReducedExtension:
                case MethodKind.Ordinary:
                case MethodKind.DeclareMethod:
                default:
                    break;
                }

                if (!symbol.IsInAnalyzedSource(this.generatedHeaderCache, context.CancellationToken))
                {
                    return;
                }

                if (!symbol.HasAsyncSignature())
                {
                    return;
                }

                if (symbol.IsOverrideOrImplementation())
                {
                    return;
                }

                if (symbol.IsTestMethod())
                {
                    return;
                }

                foreach (var parameterSymbol in symbol.Parameters)
                {
                    if (parameterSymbol.RefKind == RefKind.Out)
                    {
                        continue;
                    }

                    INamedTypeSymbol parameterType = parameterSymbol.Type as INamedTypeSymbol;
                    if (this.cancellationTokenType.Equals(parameterType))
                    {
                        return;
                    }
                }

                foreach (var parameterSymbol in symbol.Parameters)
                {
                    if (parameterSymbol.RefKind == RefKind.Out)
                    {
                        continue;
                    }

                    foreach (var member in parameterSymbol.Type.GetMembers(nameof(CancellationToken)))
                    {
                        if (member.Kind != SymbolKind.Property)
                        {
                            continue;
                        }

                        if (member.DeclaredAccessibility != Accessibility.Public)
                        {
                            continue;
                        }

                        var propertySymbol = (IPropertySymbol)member;
                        if (this.cancellationTokenType.Equals(propertySymbol.Type))
                        {
                            return;
                        }
                    }
                }

                context.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));
            }
        }
    }
}
