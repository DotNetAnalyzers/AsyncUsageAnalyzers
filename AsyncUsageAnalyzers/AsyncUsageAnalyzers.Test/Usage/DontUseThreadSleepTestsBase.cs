// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Usage
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Usage;
    using Microsoft.CodeAnalysis.CodeFixes;
    using TestHelper;
    using Xunit;

    public abstract class DontUseThreadSleepTestsBase : CodeFixVerifier
    {
        /// <summary>
        /// Returns a new diagnostic with updated arguments or leaves a diagnostic intact.
        /// </summary>
        /// <param name="diagnostic">a diagnostic to be modified</param>
        /// <param name="arguments">arguments which can be used to update diagnostic</param>
        /// <returns>An appropriately modified diagnostic or unchanged diagnostic</returns>
        protected abstract DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments);

        [Fact]
        public async Task TestThreadSleepInAsyncMethodAsync()
        {
            var testCode = @"
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Thread;

class ClassA
{
    public async Task<int> MethodAsync()
    {
        Sleep(1);
        Thread.Sleep(2);
        System.Threading.Thread.Sleep(3);
        global::System.Threading.Thread.Sleep(4);
        
        return await Task.FromResult(0); 
    }
}";
            var fixedCode = @"
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Thread;

class ClassA
{
    public async Task<int> MethodAsync()
    {
        await System.Threading.Tasks.Task.Delay(1);
        await System.Threading.Tasks.Task.Delay(2);
        await System.Threading.Tasks.Task.Delay(3);
        await System.Threading.Tasks.Task.Delay(4);
        
        return await Task.FromResult(0); 
    }
}";
            var expectedResults = new[]
                {
                    this.CSharpDiagnostic().WithLocation(10, 9),
                    this.CSharpDiagnostic().WithLocation(11, 9),
                    this.CSharpDiagnostic().WithLocation(12, 9),
                    this.CSharpDiagnostic().WithLocation(13, 9)
                }
                .Select(diag => this.OptionallyAddArgumentsToDiagnostic(diag, string.Format(UsageResources.MethodFormat, "MethodAsync")))
                .ToArray();

            await this.VerifyCSharpDiagnosticAsync(testCode, expectedResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("0 /* some inline comment */")]
        [InlineData("(int)0L")]
        [InlineData("1-1")]
        [InlineData("TimeSpan.Zero")]
        [InlineData("System.TimeSpan.Zero")]
        public async Task TestThreadSleepZeroInAsyncMethodAsync(string zeroParams)
        {
            var testCode = $@"
using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Thread;

class ClassA
{{
    public async Task<int> MethodAsync()
    {{
        Sleep({zeroParams});
        Thread.Sleep({zeroParams});
        System.Threading.Thread.Sleep({zeroParams});
        global::System.Threading.Thread.Sleep({zeroParams});
        
        return await Task.FromResult(0); 
    }}
}}";
            var fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;
using static System.Threading.Thread;

class ClassA
{
    public async Task<int> MethodAsync()
    {
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        await System.Threading.Tasks.Task.Yield();
        
        return await Task.FromResult(0); 
    }
}";
            var expectedResults = new[]
                {
                    this.CSharpDiagnostic().WithLocation(11, 9),
                    this.CSharpDiagnostic().WithLocation(12, 9),
                    this.CSharpDiagnostic().WithLocation(13, 9),
                    this.CSharpDiagnostic().WithLocation(14, 9)
                }
                .Select(diag => this.OptionallyAddArgumentsToDiagnostic(diag, string.Format(UsageResources.MethodFormat, "MethodAsync")))
                .ToArray();

            await this.VerifyCSharpDiagnosticAsync(testCode, expectedResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestThreadSleepInAsyncAnonymousFunctionAsync()
        {
            var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public void MethodA()
    {
        Func<Task> testFunc = async () =>
        {
            Thread.Sleep(1);
            await Task.FromResult(0);
        };
    }
}";

            var fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public void MethodA()
    {
        Func<Task> testFunc = async () =>
        {
            await System.Threading.Tasks.Task.Delay(1);
            await Task.FromResult(0);
        };
    }
}";
            var expected = this.OptionallyAddArgumentsToDiagnostic(this.CSharpDiagnostic().WithLocation(12, 13), UsageResources.AsyncAnonymousFunctionsAndMethods);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("0 /* some inline comment */")]
        [InlineData("(int)0L")]
        [InlineData("1-1")]
        [InlineData("TimeSpan.Zero")]
        [InlineData("System.TimeSpan.Zero")]
        public async Task TestThreadSleepZeroInAsyncAnonymousFunctionAsync(string zeroParams)
        {
            var testCode = $@"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{{
    public void MethodA()
    {{
        Func<Task> testFunc = async () =>
        {{
            Thread.Sleep({zeroParams});
            await Task.FromResult(0);
        }};
    }}
}}";

            var fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public void MethodA()
    {
        Func<Task> testFunc = async () =>
        {
            await System.Threading.Tasks.Task.Yield();
            await Task.FromResult(0);
        };
    }
}";
            var expected = this.OptionallyAddArgumentsToDiagnostic(this.CSharpDiagnostic().WithLocation(12, 13), UsageResources.AsyncAnonymousFunctionsAndMethods);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestThreadSleepInAsyncAnonymousMethodAsync()
        {
            var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate Task<int> SampleDelegate();
    SampleDelegate AsyncAnonymousMethod = async delegate ()
    {
        Thread.Sleep(1);
        return await Task.FromResult(0);
    };
}";
            var fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate Task<int> SampleDelegate();
    SampleDelegate AsyncAnonymousMethod = async delegate ()
    {
        await System.Threading.Tasks.Task.Delay(1);
        return await Task.FromResult(0);
    };
}";
            var result = this.OptionallyAddArgumentsToDiagnostic(this.CSharpDiagnostic().WithLocation(11, 9), UsageResources.AsyncAnonymousFunctionsAndMethods);

            await this.VerifyCSharpDiagnosticAsync(testCode, result, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("0 /* some inline comment */")]
        [InlineData("(int)0L")]
        [InlineData("1-1")]
        [InlineData("TimeSpan.Zero")]
        [InlineData("System.TimeSpan.Zero")]
        public async Task TestThreadSleepZeroInAsyncAnonymousMethodAsync(string zeroParams)
        {
            var testCode = $@"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{{
    public delegate Task<int> SampleDelegate();
    SampleDelegate AsyncAnonymousMethod = async delegate ()
    {{
        Thread.Sleep({zeroParams});
        return await Task.FromResult(0);
    }};
}}";
            var fixedCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate Task<int> SampleDelegate();
    SampleDelegate AsyncAnonymousMethod = async delegate ()
    {
        await System.Threading.Tasks.Task.Yield();
        return await Task.FromResult(0);
    };
}";
            var result = this.OptionallyAddArgumentsToDiagnostic(this.CSharpDiagnostic().WithLocation(11, 9), UsageResources.AsyncAnonymousFunctionsAndMethods);

            await this.VerifyCSharpDiagnosticAsync(testCode, result, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    fixedCode,
                    cancellationToken: CancellationToken.None,
                    allowNewCompilerDiagnostics: true /* expected new diagnostic is "hidden CS8019: Unnecessary using directive." */)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestUsingTaskDelayIsOKAsync()
        {
            var testCode = @"
using System.Threading.Tasks;
using System.Threading;

class ClassA
{
    public async Task<int> Method1Async()
    {
        await Task.Delay(1);
        return await Task.FromResult(0);
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestUsingTaskYieldIsOKAsync()
        {
            var testCode = @"
using System.Threading.Tasks;
using System.Threading;

class ClassA
{
    public async Task<int> Method1Async()
    {
        await Task.Delay(1);
        return await Task.FromResult(0);
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new DontUseThreadSleepCodeUniversalCodeFixProvider();
        }
    }
}
