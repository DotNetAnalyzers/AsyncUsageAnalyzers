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
        [Fact]
        public async Task TestSimpleExpressionAsync()
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
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task MethodNameAsync()
    {
        await Task.Delay(1000).ConfigureAwait(false);
    }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithLocation(7, 15);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestNestedExpressionsAsync()
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
            string fixedCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<Task> FirstMethodAsync()
    {
        return Task.FromResult(true);
    }

    async Task MethodNameAsync()
    {
        await (await FirstMethodAsync().ConfigureAwait(false)).ConfigureAwait(false);
    }
}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithLocation(12, 15),
                this.CSharpDiagnostic().WithLocation(12, 22)
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
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
