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

    public class DontUseThreadSleepInAsyncCodeTests : DontUseThreadSleepTestsBase
    {
        protected override DiagnosticResult OptionallyAddArgumentsToDiagnostic(DiagnosticResult diagnostic, params object[] arguments) =>
            diagnostic.WithArguments(arguments);

        [Fact]
        public async Task TestThreadSleepInNonAsyncCodeAsync()
        {
            string testCode = @"
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

    Func<int,int> testFunc = (x) =>
    {
        Thread.Sleep(0);
        return x;
    };

    public void NonAsyncMethod()
    {
        Thread.Sleep(1000);
        System.Threading.Thread.Sleep(1000);
        global::System.Threading.Thread.Sleep(1000);
    }
}";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new DontUseThreadSleepInAsyncCodeAnalyzer();
        }
    }
}
