namespace AsyncUsageAnalyzers.Naming
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Threading.Tasks;
    using Helpers;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;

    /// <summary>
    /// Implements a code fix for <see cref="AvoidAsyncSuffixAnalyzer"/>.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, LanguageNames.VisualBasic, Name = nameof(AvoidAsyncSuffixCodeFixProvider))]
    [Shared]
    public class AvoidAsyncSuffixCodeFixProvider : CodeFixProvider
    {
        private static readonly ImmutableArray<string> FixableDiagnostics =
            ImmutableArray.Create(AvoidAsyncSuffixAnalyzer.DiagnosticId);

        /// <inheritdoc/>
        public override ImmutableArray<string> FixableDiagnosticIds => FixableDiagnostics;

        /// <inheritdoc/>
        public override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <inheritdoc/>
        public override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var document = context.Document;
            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                if (!diagnostic.Id.Equals(AvoidAsyncSuffixAnalyzer.DiagnosticId))
                {
                    continue;
                }

                var token = root.FindToken(diagnostic.Location.SourceSpan.Start);
                var newName = token.ValueText.Substring(0, token.ValueText.Length - "Async".Length);
                context.RegisterCodeFix(
                    CodeAction.Create(
                        $"Rename method to '{newName}'",
                        cancellationToken => RenameHelper.RenameSymbolAsync(document, root, token, newName, cancellationToken),
                        nameof(AvoidAsyncSuffixCodeFixProvider)),
                    diagnostic);
            }
        }
    }
}
