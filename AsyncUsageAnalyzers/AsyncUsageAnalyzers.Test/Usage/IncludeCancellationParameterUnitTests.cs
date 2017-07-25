// Copyright (c) Tunnel Vision Laboratories, LLC. All Rights Reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace AsyncUsageAnalyzers.Test.Usage
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AsyncUsageAnalyzers.Usage;
    using Microsoft.CodeAnalysis.Diagnostics;
    using TestHelper;
    using Xunit;

    public class IncludeCancellationParameterUnitTests : DiagnosticVerifier
    {
        public static IEnumerable<object[]> AsynchronousUnitTestWithReturnValue
        {
            get
            {
                yield return new[] { "    public          Task      MethodAsync(string param1, string param2) { return null; }", "TestMethod" };
                yield return new[] { "    public          Task      MethodAsync(string param1, string param2) { return null; }", "Fact" };
                yield return new[] { "    public          Task      MethodAsync(string param1, string param2) { return null; }", "Theory" };
                yield return new[] { "    public          Task      MethodAsync(string param1, string param2) { return null; }", "Test" };
                yield return new[] { "    public          Task<int> MethodAsync(string param1, string param2) { return null; }", "TestMethod" };
                yield return new[] { "    public          Task<int> MethodAsync(string param1, string param2) { return null; }", "Fact" };
                yield return new[] { "    public          Task<int> MethodAsync(string param1, string param2) { return null; }", "Theory" };
                yield return new[] { "    public          Task<int> MethodAsync(string param1, string param2) { return null; }", "Test" };
                yield return new[] { "    public          TASK      MethodAsync(string param1, string param2) { return null; }", "TestMethod" };
                yield return new[] { "    public          TASK      MethodAsync(string param1, string param2) { return null; }", "Fact" };
                yield return new[] { "    public          TASK      MethodAsync(string param1, string param2) { return null; }", "Theory" };
                yield return new[] { "    public          TASK      MethodAsync(string param1, string param2) { return null; }", "Test" };
            }
        }

        [Fact]
        public async Task TestPropertyAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    public Task PropertyName { get; set; }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestPrivateMemberAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    async Task Method1Async() { }
    private async Task Method2Async() { }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestCancellationTokenIsOutParameterAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public Task MethodAsync(out CancellationToken cancellationToken)
    {
        cancellationToken = CancellationToken.None;
        return Task.FromResult(0);
    }
}
";

            var expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(6, 17);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestCancellationTokenInStructureAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public async Task MethodAsync(Context context) { }
}

struct Context
{
    public CancellationToken CancellationToken { get; }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestContextStructureUsesMethodAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public async Task MethodAsync(Context context) { }
}

struct Context
{
    public CancellationToken CancellationToken() => default(CancellationToken);
}
";

            var expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(6, 23);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestContextStructurePropertyIsNotPublicAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public async Task MethodAsync(Context context) { }
}

struct Context
{
    internal CancellationToken CancellationToken { get; }
}
";

            var expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(6, 23);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestContextStructurePropertyHasWrongNameAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public async Task MethodAsync(Context context) { }
}

struct Context
{
    public CancellationToken Token { get; }
}
";

            var expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(6, 23);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestContextStructurePropertyReturnsAnotherTypeAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public async Task MethodAsync(Context context) { }
}

struct Context
{
    public int CancellationToken { get; }
}
";

            var expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(6, 23);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestContextStructureIsOutParameterAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public Task MethodAsync(out Context context)
    {
        context = default(Context);
        return Task.FromResult(0);
    }
}

struct Context
{
    public CancellationToken CancellationToken { get; }
}
";

            var expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(6, 17);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestCancellationTokenInInterfaceAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
class ClassName
{
    public async Task MethodAsync(IContext context) { }
}

interface IContext
{
    CancellationToken CancellationToken { get; }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestAsyncVoidMethodAsync()
        {
            string testCode = @"
class ClassName
{
    public async void Method1Async() { }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestMethodsWithCancellationTokenAsync()
        {
            string testCode = @"
using System.Threading;
using System.Threading.Tasks;
using CT = System.Threading.CancellationToken;
class ClassName
{
    public async Task Method1Async(CancellationToken cancellationToken) { }
    public async Task Method2Async(CT cancellationToken) { }
    public async Task<int> Method3Async(CancellationToken cancellationToken) { return 0; }
    public async Task<int> Method4Async(CT cancellationToken) { return 0; }
    public Task Method5Async(CancellationToken cancellationToken) { return Task.FromResult(0); }
    public Task Method6Async(CT cancellationToken) { return Task.FromResult(0); }
    public Task<int> Method7Async(CancellationToken cancellationToken) { return Task.FromResult(0); }
    public Task<int> Method8Async(CT cancellationToken) { return Task.FromResult(0); }
}
";

            await this.VerifyCSharpDiagnosticAsync(testCode, EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestMethodsWithoutCancellationTokenAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class ClassName
{
    public async Task Method1Async() { }
    public async Task<int> Method2Async() { return 0; }
    public Task Method3Async() { return Task.FromResult(0); }
    public Task<int> Method4Async() { return Task.FromResult(0); }
}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments("Method1Async").WithLocation(5, 23),
                this.CSharpDiagnostic().WithArguments("Method2Async").WithLocation(6, 28),
                this.CSharpDiagnostic().WithArguments("Method3Async").WithLocation(7, 17),
                this.CSharpDiagnostic().WithArguments("Method4Async").WithLocation(8, 22),
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestMethodsWithWrongCancellationTokenAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
using CT = CancellationToken;
class ClassName
{
    public async Task Method1Async(CancellationToken cancellationToken) { }
    public async Task Method2Async(CT cancellationToken) { }
    public async Task<int> Method3Async(CancellationToken cancellationToken) { return 0; }
    public async Task<int> Method4Async(CT cancellationToken) { return 0; }
    public Task Method5Async(CancellationToken cancellationToken) { return Task.FromResult(0); }
    public Task Method6Async(CT cancellationToken) { return Task.FromResult(0); }
    public Task<int> Method7Async(CancellationToken cancellationToken) { return Task.FromResult(0); }
    public Task<int> Method8Async(CT cancellationToken) { return Task.FromResult(0); }
}

struct CancellationToken
{
}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments("Method1Async").WithLocation(6, 23),
                this.CSharpDiagnostic().WithArguments("Method2Async").WithLocation(7, 23),
                this.CSharpDiagnostic().WithArguments("Method3Async").WithLocation(8, 28),
                this.CSharpDiagnostic().WithArguments("Method4Async").WithLocation(9, 28),
                this.CSharpDiagnostic().WithArguments("Method5Async").WithLocation(10, 17),
                this.CSharpDiagnostic().WithArguments("Method6Async").WithLocation(11, 17),
                this.CSharpDiagnostic().WithArguments("Method7Async").WithLocation(12, 22),
                this.CSharpDiagnostic().WithArguments("Method8Async").WithLocation(13, 22),
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestInheritedMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
class BaseName
{
    protected virtual Task MethodAsync() { return Task.FromResult(0); }
}
class ClassName : BaseName
{
    protected override Task MethodAsync() { return Task.FromResult(0); }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(5, 28);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestExplicitInterfaceMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
interface InterfaceName
{
    Task MethodAsync();
}
class ClassName : InterfaceName
{
    Task InterfaceName.MethodAsync() { return Task.FromResult(0); }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(5, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestExplicitInterfaceMethodPlusExtraAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
interface InterfaceName
{
    Task MethodAsync();
}
class ClassName : InterfaceName
{
    public Task MethodAsync() { return Task.FromResult(0); }
    Task InterfaceName.MethodAsync() { return Task.FromResult(0); }
}
";

            DiagnosticResult[] expected =
            {
                this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(5, 10),
                this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(9, 17),
            };

            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestImplicitInterfaceMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
interface InterfaceName
{
    Task MethodAsync();
}
class ClassName : InterfaceName
{
    public Task MethodAsync() { return Task.FromResult(0); }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(5, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestImplicitGenericInterfaceMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
interface InterfaceName<T>
{
    Task MethodAsync(T value);
}
class ClassName : InterfaceName<int>
{
    public Task MethodAsync(int value) { return Task.FromResult(0); }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(5, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Fact]
        public async Task TestImplicitInterfaceGenericMethodAsync()
        {
            string testCode = @"
using System.Threading.Tasks;
interface InterfaceName
{
    Task MethodAsync<T>(T value);
}
class ClassName : InterfaceName
{
    public Task MethodAsync<T>(T value) { return Task.FromResult(0); }
}
";

            DiagnosticResult expected = this.CSharpDiagnostic().WithArguments("MethodAsync").WithLocation(5, 10);
            await this.VerifyCSharpDiagnosticAsync(testCode, expected, CancellationToken.None).ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(AsynchronousUnitTestWithReturnValue))]
        public async Task TestAsynchronousUnitTestMethodAsync(string declaration, string testAttribute)
        {
            var testCode = @"
using System.Threading.Tasks;
using TASK = System.Threading.Tasks.Task<int>;
public class ClassName
{
    [##]
$$
}
internal sealed class ##Attribute : System.Attribute { }
";

            await this.VerifyCSharpDiagnosticAsync(testCode.Replace("$$", declaration).Replace("##", testAttribute), EmptyDiagnosticResults, CancellationToken.None).ConfigureAwait(false);
        }

        protected override IEnumerable<DiagnosticAnalyzer> GetCSharpDiagnosticAnalyzers()
        {
            yield return new IncludeCancellationParameterAnalyzer();
        }
    }
}
