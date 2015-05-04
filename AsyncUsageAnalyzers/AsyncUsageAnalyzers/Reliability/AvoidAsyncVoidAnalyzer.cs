namespace AsyncUsageAnalyzers.Reliability
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer identifies code using <c>void</c>-returning <see langword="async"/> methods, and reports a
    /// warning.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [NoCodeFix("No clear transformation to correct violations of this rule.")]
    public class AvoidAsyncVoidAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="AvoidAsyncVoidAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "AvoidAsyncVoid";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(ReliabilityResources.AvoidAsyncVoidTitle), ReliabilityResources.ResourceManager, typeof(ReliabilityResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(ReliabilityResources.AvoidAsyncVoidMessageFormat), ReliabilityResources.ResourceManager, typeof(ReliabilityResources));
        private static readonly string Category = "AsyncUsage.CSharp.Reliability";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(ReliabilityResources.AvoidAsyncVoidDescription), ReliabilityResources.ResourceManager, typeof(ReliabilityResources));
        private static readonly string HelpLink = null;

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
            if (!symbol.IsAsync || !symbol.ReturnsVoid)
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, symbol.Locations[0], symbol.Name));
        }
    }
}
