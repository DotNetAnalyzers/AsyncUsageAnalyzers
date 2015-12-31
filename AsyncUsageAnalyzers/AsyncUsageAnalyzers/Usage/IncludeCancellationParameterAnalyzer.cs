// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Usage
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Immutable;
    using System.Threading;
    using System.Threading.Tasks;
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

                if (!symbol.IsAsync)
                {
                    // This check conveniently covers Task and Task<T> by ignoring the `1 in Task<T>.
                    if (!string.Equals(nameof(Task), symbol.ReturnType?.Name, StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (!string.Equals(typeof(Task).Namespace, symbol.ReturnType?.ContainingNamespace?.ToString(), StringComparison.Ordinal))
                    {
                        return;
                    }
                }

                if (symbol.MethodKind == MethodKind.PropertyGet || symbol.MethodKind == MethodKind.PropertySet)
                {
                    return;
                }

                if (IsTestMethod(symbol))
                {
                    return;
                }

                foreach (var parameterSymbol in symbol.Parameters)
                {
                    INamedTypeSymbol parameterType = parameterSymbol.Type as INamedTypeSymbol;
                    if (this.cancellationTokenType.Equals(parameterType))
                    {
                        return;
                    }
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

            private static bool IsTestMethod(IMethodSymbol methodSymbol)
            {
                foreach (AttributeData attributeData in methodSymbol.GetAttributes())
                {
                    var attributeClass = attributeData.AttributeClass;
                    if (attributeClass == null)
                    {
                        continue;
                    }

                    if (string.Equals(attributeClass.Name, "TestMethodAttribute", StringComparison.Ordinal)
                        || string.Equals(attributeClass.Name, "FactAttribute", StringComparison.Ordinal)
                        || string.Equals(attributeClass.Name, "TheoryAttribute", StringComparison.Ordinal)
                        || string.Equals(attributeClass.Name, "TestAttribute", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
