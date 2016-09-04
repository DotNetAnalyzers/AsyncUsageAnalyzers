// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

/* Contributor: Tomasz Maczyński */

namespace AsyncUsageAnalyzers.Test.Usage
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Usage;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class DontUseThreadSleepTests : DontUseThreadSleepTestsBase
    {
        protected override DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments) =>
            diagnostic;

        [Fact]
        public async Task TestThreadSleepInMethodAsync()
        {
            var testCode = @"
using System.Threading.Tasks;
using System.Threading;

class ClassA
{
    public void NonAsyncMethod()
    {
        Thread.Sleep(1000);
        System.Threading.Thread.Sleep(1000);
        global::System.Threading.Thread.Sleep(1000);
    }
}";
            var expected = new[]
            {
                this.CSharpDiagnostic().WithLocation(9, 9),
                this.CSharpDiagnostic().WithLocation(10, 9),
                this.CSharpDiagnostic().WithLocation(11, 9)
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    testCode /* source code should not be changed as there's no automatic code fix */,
                    cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestThreadSleepInAnonymousFunctionAsync()
        {
            var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    Func<int,int> testFunc = (x) =>
    {
        Thread.Sleep(0);
        return x;
    };
}";
            var expected = this.CSharpDiagnostic().WithLocation(10, 9);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    testCode /* source code should not be changed as there's no automatic code fix */,
                    cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
        }

        [Fact]
        public async Task TestThreadSleepInAnonymousMethodAsync()
        {
            var testCode = @"
using System;
using System.Threading;
using System.Threading.Tasks;

class ClassA
{
    public delegate void SampleDelegate();
    SampleDelegate AnonymousMethod = delegate ()
    {
        Thread.Sleep(0);
    };
}";
            var expected = this.CSharpDiagnostic().WithLocation(11, 9);

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAllFixAsync(
                    testCode,
                    testCode /* source code should not be changed as there's no automatic code fix */,
                    cancellationToken: CancellationToken.None)
                .ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepAnalyzer();
        }
    }
}
