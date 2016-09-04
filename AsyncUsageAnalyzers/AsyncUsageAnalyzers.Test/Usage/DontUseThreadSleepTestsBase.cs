// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

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
            await Task.FromResult(1);
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
            await Task.FromResult(1);
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
        Thread.Sleep(0);
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
        await System.Threading.Tasks.Task.Delay(0);
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
        await Task.Delay(1000);
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
