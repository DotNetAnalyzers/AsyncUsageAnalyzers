namespace AsyncUsageAnalyzers.Naming
{
    using System;
    using System.Collections.Immutable;
    using System.Threading.Tasks;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer identifies asynchronous methods using the Task-based Asynchronous Pattern (TAP) according to their
    /// signature, and reports a warning if the method name does not include the suffix <c>Async</c>.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UseAsyncSuffixAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="UseAsyncSuffixAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "UseAsyncSuffix";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(NamingResources.UseAsyncSuffixTitle), NamingResources.ResourceManager, typeof(NamingResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(NamingResources.UseAsyncSuffixMessageFormat), NamingResources.ResourceManager, typeof(NamingResources));
        private static readonly string Category = "AsyncUsage.CSharp.Naming";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(NamingResources.UseAsyncSuffixDescription), NamingResources.ResourceManager, typeof(NamingResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, true, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(HandleMethodDeclaration, SymbolKind.Method);
        }

        private void HandleMethodDeclaration(SymbolAnalysisContext context)
        {
            IMethodSymbol symbol = (IMethodSymbol)context.Symbol;
            if (symbol.Name.EndsWith("Async", StringComparison.Ordinal))
                return;

            if (symbol.Locations.IsDefaultOrEmpty)
                return;

            Location location = symbol.Locations[0];
            if (!location.IsInSource || location.SourceTree.IsGeneratedDocument(context.CancellationToken))
                return;

            // void-returning methods are not asynchronous according to their signature, even if they use `async`
            if (symbol.ReturnsVoid)
                return;

            if (!string.Equals(nameof(Task), symbol.ReturnType?.Name, StringComparison.Ordinal))
                return;

            if (!string.Equals(typeof(Task).Namespace, symbol.ReturnType?.ContainingNamespace?.ToString(), StringComparison.Ordinal))
                return;

            if (symbol.MethodKind == MethodKind.PropertyGet || symbol.MethodKind == MethodKind.PropertySet)
                return;

            if (symbol.IsOverride)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));
        }
    }
}
