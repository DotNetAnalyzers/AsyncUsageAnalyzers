namespace AsyncUsageAnalyzers.Test.Naming
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Naming;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class UseAsyncSuffixUnitTests : CodeFixVerifier
    {
        [Fact]
        public async Task TestAsyncReturnVoidAsync()
        {
            string testCode = @"
class ClassName
{
    async void FirstMethod() { }
    async void SecondMethodAsync() { }
}
";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncEventHandlerReturnVoidAsync()
        {
            string testCode = @"
using System;
class ClassName
{
    async void FirstMethod(object sender, EventArgs e) { }
    async void SecondMethodAsync(object sender, EventArgs e) { }
}
";

            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncReturnTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task FirstMethod() { }
    async Task SecondMethodAsync() { }
}
";
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task FirstMethodAsync() { }
    async Task SecondMethodAsync() { }
}
";

            DiagnosticResult expected = CSharpDiagnostic().WithArguments("FirstMethod").WithLocation(5, 16);
            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncReturnGenericTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<int> FirstMethod() { return 3; }
    async Task<int> SecondMethodAsync() { return 3; }
}
";
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<int> FirstMethodAsync() { return 3; }
    async Task<int> SecondMethodAsync() { return 3; }
}
";

            DiagnosticResult expected = CSharpDiagnostic().WithArguments("FirstMethod").WithLocation(5, 21);
            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestReturnTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    Task FirstMethod() { return Task.FromResult(3); }
    Task SecondMethodAsync() { return Task.FromResult(3); }
}
";
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    Task FirstMethodAsync() { return Task.FromResult(3); }
    Task SecondMethodAsync() { return Task.FromResult(3); }
}
";

            DiagnosticResult expected = CSharpDiagnostic().WithArguments("FirstMethod").WithLocation(5, 10);
            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestReturnGenericTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    Task<int> FirstMethod() { return Task.FromResult(3); }
    Task<int> SecondMethodAsync() { return Task.FromResult(3); }
}
";
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    Task<int> FirstMethodAsync() { return Task.FromResult(3); }
    Task<int> SecondMethodAsync() { return Task.FromResult(3); }
}
";

            DiagnosticResult expected = CSharpDiagnostic().WithArguments("FirstMethod").WithLocation(5, 15);
            await VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestPropertyGetterAndSetterTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    public Task<string> TaskString { get; set; }
}
";
            await VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new UseAsyncSuffixAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseAsyncSuffixCodeFixProvider();
        }
    }
}
