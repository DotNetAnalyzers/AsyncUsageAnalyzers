using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using TestHelper;
using Xunit;

namespace AsyncUsageAnalyzers.Test
{
    public class UnitTest : CodeFixVerifier
    {

        //No diagnostics expected to show up
        [Fact]
        public async Task TestMethod1()
        {
            var test = @"";

            await VerifyCSharpDiagnosticAsync(test, EmptyDiagnosticResults, CancellationToken.None);
        }

        //Diagnostic and CodeFix both triggered and checked for
        [Fact]
        public async Task TestMethod2()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
        }
    }";
            DiagnosticResult expected = CSharpDiagnostic().WithArguments("TypeName").WithLocation(11, 15);

            await VerifyCSharpDiagnosticAsync(test, expected, CancellationToken.None);

            var fixtest = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class TYPENAME
        {   
        }
    }";
            await VerifyCSharpFixAsync(test, fixtest, cancellationToken: CancellationToken.None);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AsyncUsageAnalyzersCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AsyncUsageAnalyzersAnalyzer();
        }
    }
}