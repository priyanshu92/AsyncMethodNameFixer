using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using TestHelper;
using AsyncMethodNameFixer;

namespace AsyncMethodNameFixer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_Empty_Code()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void Should_Give_Warning_And_Fix_If_Async_Method_Name_Does_Not_End_With_Async()
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
            public async Task MyMethod(string input)
            {
                await Task.Delay(1000);
            } 
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "AsyncMethodDiagnostic",
                Message = String.Format("Method name '{0}' is missing 'Async' at the end", "MyMethod"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 31)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
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
            public async Task MyMethodAsync(string input)
            {
                await Task.Delay(1000);
            } 
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        [TestMethod]
        public void Should_Give_Warning_And_Fix_If_Non_Async_Method_Name_Ends_With_Async()
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
            public void AsyncMethodAsync()
            {
                Console.WriteLine(""Hello World"");
            } 
        }
    }";
            var expected = new DiagnosticResult
            {
                Id = "NonAsyncMethodDiagnostic",
                Message = String.Format("Method name '{0}' is having 'Async' at the end", "AsyncMethodAsync"),
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[] {
                            new DiagnosticResultLocation("Test0.cs", 13, 25)
                        }
            };

            VerifyCSharpDiagnostic(test, expected);

            var fixtest = @"
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
            public void AsyncMethod()
            {
                Console.WriteLine(""Hello World"");
            } 
        }
    }";
            VerifyCSharpFix(test, fixtest);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider()
        {
            return new AsyncMethodNameFixerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer()
        {
            return new AsyncMethodNameFixerAnalyzer();
        }
    }
}
