namespace AsyncUsageAnalyzers.Usage
{
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
    public class UseConfigureAwaitAnalyzer : DiagnosticAnalyzer
    {
        /// <summary>
        /// The ID for diagnostics produced by the <see cref="UseConfigureAwaitAnalyzer"/> analyzer.
        /// </summary>
        public const string DiagnosticId = "UseConfigureAwait";
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(UsageResources.UseConfigureAwaitTitle), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(UsageResources.UseConfigureAwaitMessageFormat), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string Category = "AsyncUsage.CSharp.Usage";
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(UsageResources.UseConfigureAwaitDescription), UsageResources.ResourceManager, typeof(UsageResources));
        private static readonly string HelpLink = "https://github.com/DotNetAnalyzers/AsyncUsageAnalyzers";

        private static readonly DiagnosticDescriptor Descriptor =
            new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Hidden, true, Description, HelpLink);

        private static readonly ImmutableArray<DiagnosticDescriptor> SupportedDiagnosticsValue =
            ImmutableArray.Create(Descriptor);

        /// <inheritdoc/>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        {
            get
            {
                return SupportedDiagnosticsValue;
            }
        }

        /// <inheritdoc/>
        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeActionHonorExclusions(HandleAwaitExpression, SyntaxKind.AwaitExpression);
        }

        private void HandleAwaitExpression(SyntaxNodeAnalysisContext context)
        {
            AwaitExpressionSyntax syntax = (AwaitExpressionSyntax)context.Node;
            ExpressionSyntax expression = syntax.Expression;
            if (!IsTask(expression, context))
                return;

            context.ReportDiagnostic(Diagnostic.Create(Descriptor, expression.GetLocation()));
        }

        private static bool IsTask(ExpressionSyntax expression, SyntaxNodeAnalysisContext context)
        {
            var type = context.SemanticModel.GetTypeInfo(expression).Type as INamedTypeSymbol;
            if (type == null)
                return false;

            INamedTypeSymbol taskType;
            if (type.IsGenericType)
            {
                type = type.ConstructedFrom;
                taskType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Task<>).FullName);
            }
            else
            {
                taskType = context.SemanticModel.Compilation.GetTypeByMetadataName(typeof(Task).FullName);
            }

            return type.Equals(taskType);
        }
    }
}
