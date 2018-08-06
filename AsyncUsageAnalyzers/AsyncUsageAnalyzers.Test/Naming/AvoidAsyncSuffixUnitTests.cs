// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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

    public class AvoidAsyncSuffixUnitTests : CodeFixVerifier
    {
        [Fact]
        public async Task TestReturnVoidAsync()
        {
            string testCode = @"
class ClassName
{
    void FirstMethod() { }
    void SecondMethodAsync() { }
}
";
            string fixedCode = @"
class ClassName
{
    void FirstMethod() { }
    void SecondMethod() { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("SecondMethodAsync").WithLocation(5, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestInheritedReturnVoidAsync()
        {
            string testCode = @"
class BaseName
{
    protected virtual void MethodAsync() { }
}
class ClassName : BaseName
{
    protected override void MethodAsync() { }
}
";
            string fixedCode = @"
class BaseName
{
    protected virtual void Method() { }
}
class ClassName : BaseName
{
    protected override void Method() { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(4, 28);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestExplicitInterfaceReturnVoidAsync()
        {
            string testCode = @"
interface InterfaceName
{
    void MethodAsync();
}
class ClassName : InterfaceName
{
    void InterfaceName.MethodAsync() { }
}
";
            string fixedCode = @"
interface InterfaceName
{
    void Method();
}
class ClassName : InterfaceName
{
    void InterfaceName.Method() { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(4, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestExplicitInterfaceReturnVoidPlusExtraAsync()
        {
            string testCode = @"
interface InterfaceName
{
    void MethodAsync();
}
class ClassName : InterfaceName
{
    void MethodAsync() { }
    void InterfaceName.MethodAsync() { }
}
";
            string fixedCode = @"
interface InterfaceName
{
    void Method();
}
class ClassName : InterfaceName
{
    void Method() { }
    void InterfaceName.Method() { }
}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(4, 10),
                this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(8, 10),
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestImplicitInterfaceReturnVoidAsync()
        {
            string testCode = @"
interface InterfaceName
{
    void MethodAsync();
}
class ClassName : InterfaceName
{
    public void MethodAsync() { }
}
";
            string fixedCode = @"
interface InterfaceName
{
    void Method();
}
class ClassName : InterfaceName
{
    public void Method() { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(4, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestImplicitGenericInterfaceReturnVoidAsync()
        {
            string testCode = @"
interface InterfaceName<T>
{
    void MethodAsync(T value);
}
class ClassName : InterfaceName<int>
{
    public void MethodAsync(int value) { }
}
";
            string fixedCode = @"
interface InterfaceName<T>
{
    void Method(T value);
}
class ClassName : InterfaceName<int>
{
    public void Method(int value) { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(4, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestImplicitGenericInterfaceMethodReturnVoidAsync()
        {
            string testCode = @"
interface InterfaceName
{
    void MethodAsync<T>(T value);
}
class ClassName : InterfaceName
{
    public void MethodAsync<T>(T value) { }
}
";
            string fixedCode = @"
interface InterfaceName
{
    void Method<T>(T value);
}
class ClassName : InterfaceName
{
    public void Method<T>(T value) { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(4, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
        }

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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestEventHandlerReturnVoidAsync()
        {
            string testCode = @"
using System;
class ClassName
{
    void FirstMethod(object sender, EventArgs e) { }
    void SecondMethodAsync(object sender, EventArgs e) { }
}
";
            string fixedCode = @"
using System;
class ClassName
{
    void FirstMethod(object sender, EventArgs e) { }
    void SecondMethod(object sender, EventArgs e) { }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("SecondMethodAsync").WithLocation(6, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpDiagnosticAsync(fixedCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
            await this.VerifyCSharpFixAsync(testCode, fixedCode, cancellationToken: CancellationToken.None).ConfigureAwait(false);
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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
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

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestReturnValueTaskAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    ValueTask<int> FirstMethod() { return new ValueTask<int>(3); }
    ValueTask<int> SecondMethodAsync() { return new ValueTask<int>(3); }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new AvoidAsyncSuffixAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AvoidAsyncSuffixCodeFixProvider();
        }
    }
}
