// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Usage
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer reports a diagnostic if System.Threading.Thread.Sleep() method is called.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DontUseThreadSleepAnalyzer : DontUseThreadSleepAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DontUseThreadSleepAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DontUseThreadSleep";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepTitle), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string Category = "AsyncUsage.CSharp.Usage";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepDescription), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/DontUseThreadSleep.md";
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        protected override AnalyzerBase GetAnalyzer() => new Analyzer();

        private sealed class Analyzer : DontUseThreadSleepAnalyzerBase.AnalyzerBase
        {
            protected override void ReportDiagnosticOnThreadSleepInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
            {
                context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation()));
            }
        }
    }
}
