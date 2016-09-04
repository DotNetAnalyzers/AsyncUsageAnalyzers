// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Usage
{
    using System.Collections.Immutable;
    using AsyncUsageAnalyzers.Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// This analyzer reports a diagnostic if System.Threading.Thread.Sleep() method is inside async code
    /// (i.e. asynchronous methods, asynchronous anonymous functions or asynchronous anonymous methods).
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    internal class DontUseThreadSleepInAsyncCodeAnalyzer : DontUseThreadSleepAnalyzerBase
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="DontUseThreadSleepInAsyncCodeAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "DontUseThreadSleepInAsyncCode";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncCodeTitle), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncCodeMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string Category = "AsyncUsage.CSharp.Usage";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(UsageResources.DontUseThreadSleepInAsyncCodeDescription), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers/blob/master/documentation/DontUseThreadSleepInAsyncCode.md";
        private static readonly DiagnosticDescriptor Descriptor = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, AnalyzerConstants.EnabledByDefault, Description, HelpLink);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
            ImmutableArray.Create(Descriptor);

        protected override AnalyzerBase GetAnalyzer() => new Analyzer();

        private sealed class Analyzer : DontUseThreadSleepAnalyzerBase.AnalyzerBase
        {
            protected override void ReportDiagnosticOnThreadSleepInvocation(SyntaxNodeAnalysisContext context, InvocationExpressionSyntax invocationExpression)
            {
                SyntaxNode asycNode = null;
                if (invocationExpression.IsInsideAsyncCode(ref asycNode))
                {
                    var asyncMethod = asycNode as MethodDeclarationSyntax;
                    if (asyncMethod != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), GetMethodText(asyncMethod.Identifier.Text)));
                    }

                    var asyncFunction = asycNode as AnonymousFunctionExpressionSyntax;
                    if (asyncFunction != null)
                    {
                        context.ReportDiagnostic(Diagnostic.Create(Descriptor, invocationExpression.GetLocation(), UsageResources.AsyncAnonymousFunctionsAndMethods));
                    }
                }
            }
        }

        private static string GetMethodText(string methodName) =>
            string.Format(UsageResources.MethodFormat, methodName);
    }
}