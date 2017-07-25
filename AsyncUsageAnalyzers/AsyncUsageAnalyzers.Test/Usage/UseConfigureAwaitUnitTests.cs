// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Usage
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Usage;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class UseConfigureAwaitUnitTests : CodeFixVerifier
    {
        [Theory]
        [InlineData(0, "false")]
        [InlineData(1, "true")]
        public async Task TestSimpleExpressionAsync(int codeFixIndex, string configureAwaitArgument)
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task MethodNameAsync()
    {
        await Task.Delay(1000);
    }
}
";
            string fixedCode = $@"
using System.Threading.Tasks;
class ClassName
{{
    async Task MethodNameAsync()
    {{
        await Task.Delay(1000).ConfigureAwait({configureAwaitArgument});
    }}
}}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(7, 15);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, codeFixIndex: codeFixIndex, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestDynamicExpressionAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task MethodNameAsync()
    {
        await (dynamic)Task.Delay(1000);
    }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [InlineData(0, "false")]
        [InlineData(1, "true")]
        public async Task TestNestedExpressionsAsync(int codeFixIndex, string configureAwaitArgument)
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<Task> FirstMethodAsync()
    {
        return Task.FromResult(true);
    }

    async Task MethodNameAsync()
    {
        await (await FirstMethodAsync());
    }
}
";
            string fixedCode = $@"
using System.Threading.Tasks;
class ClassName
{{
    async Task<Task> FirstMethodAsync()
    {{
        return Task.FromResult(true);
    }}

    async Task MethodNameAsync()
    {{
        await (await FirstMethodAsync().ConfigureAwait({configureAwaitArgument})).ConfigureAwait({configureAwaitArgument});
    }}
}}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithLocation(12, 15),
                this.CSharpDiagnostic().WithLocation(12, 22),
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, codeFixIndex: codeFixIndex, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new UseConfigureAwaitAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new UseConfigureAwaitCodeFixProvider();
        }
    }
}
