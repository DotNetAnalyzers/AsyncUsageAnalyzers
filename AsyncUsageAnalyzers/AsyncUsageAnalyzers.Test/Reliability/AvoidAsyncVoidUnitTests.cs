// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Reliability
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Reliability;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class AvoidAsyncVoidUnitTests : DiagnosticVerifier
    {
        [Fact]
        public async Task TestAsyncReturnVoidAsync()
        {
            string testCode = @"
class ClassName
{
    async void MethodNameAsync() { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodNameAsync").WithLocation(4, 16);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncEventHandlerReturnVoidAsync()
        {
            string testCode = @"
using System;
class ClassName
{
    ClassName()
    {
        AppDomain.CurrentDomain.DomainUnload += MethodNameAsync;
    }

    async void MethodNameAsync(object sender, EventArgs e) { }
}
";

            // This analyzer does not currently handle this case differently from any other method
            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodNameAsync").WithLocation(10, 16);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncLambdaEventHandlerReturnVoidAsync()
        {
            string testCode = @"
using System;
class ClassName
{
    static event Action<object> SingleArgumentEvent;

    ClassName()
    {
        AppDomain.CurrentDomain.DomainUnload += async (sender, e) => { };
        AppDomain.CurrentDomain.DomainUnload += async delegate (object sender, EventArgs e) { };
        SingleArgumentEvent += async arg => { };
    }
}
";

            // This analyzer does not currently handle this case differently from any other method
            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments("<anonymous>").WithLocation(9, 49),
                this.CSharpDiagnostic().WithArguments("<anonymous>").WithLocation(10, 49),
                this.CSharpDiagnostic().WithArguments("<anonymous>").WithLocation(11, 32),
            };
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncLambdaReturnTaskAsync()
        {
            string testCode = @"
using System;
using System.Threading.Tasks;
class ClassName
{
    static Func<Task> ZeroArgumentFunction;
    static Func<object, Task> SingleArgumentFunction;

    ClassName()
    {
        ZeroArgumentFunction = async () => await Task.Delay(42);
        SingleArgumentFunction = async arg => await Task.Delay(42);
        SingleArgumentFunction = async (object arg) => await Task.Delay(42);
        SingleArgumentFunction = async delegate (object arg) { await Task.Delay(42); };
    }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncReturnTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task MethodNameAsync() { }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncReturnGenericTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task<int> MethodNameAsync() { return 3; }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestReturnTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    Task MethodNameAsync() { return Task.FromResult(3); }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestReturnGenericTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    Task<int> MethodNameAsync() { return Task.FromResult(3); }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AvoidAsyncVoidAnalyzer();
        }
    }
}
