using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestHelper;

namespace AsyncMethodNameFixer.Test
{
    [TestClass]
    public class UnitTest : CodeFixVerifier
    {
        private void ExpectDiagnostic(string inputText, string message, int column, int row, string diagnostic)
        {
            var expected = new DiagnosticResult
            {
                Id = diagnostic,
                Message = message,
                Severity = DiagnosticSeverity.Warning,
                Locations =
                    new[]
                    {
                        new DiagnosticResultLocation("Test0.cs", column, row)
                    }
            };

            VerifyCSharpDiagnostic(inputText, expected);
        }

        private void ExpectMissingAsync(string inputText, string methodName, int column, int row)
        {
            var message = $"Asynchronous method '{ methodName }' is missing 'Async' at the end";
            ExpectDiagnostic(inputText, message, column, row, AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId);
        }

        private void ExpectUnnecessaryAsync(string inputText, string methodName, int column, int row)
        {
            var message = $"Synchronous method '{ methodName }' is having 'Async' at the end";
            ExpectDiagnostic(inputText, message, column, row, AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_Empty_Code()
        {
            var test = @"";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_Overridden_Methods()
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
            public override async Task OverriddenMethod(string input)
            {
                await Task.Delay(1000);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_Main_Method()
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
            public async Task Main()
            {
                await Task.Delay(1000);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_MS_Test_Methods()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    namespace ConsoleApplication1
    {
        [TestClass]
        public class TypeName
        {
            [TestMethod]
            public async Task TestMethod1()
            {
                await Task.Delay(1000);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_NUnit_Test_Methods()
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
        public class TypeName
        {
            [Test]
            public async Task TestMethod1()
            {
                await Task.Delay(1000);
            }

            [Theory]
            public async Task TestMethod2()
            {
                await Task.Delay(1000);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_XUnit_Test_Methods()
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
        public class TypeName
        {
            [Fact]
            public async Task TestMethod1()
            {
                await Task.Delay(1000);
            }

            [Theory]
            public async Task TestMethod2()
            {
                await Task.Delay(1000);
            }

            [Theory]
            public void TestMethod3Async()
            {
                await Task.Delay(1000);
            }
        }
    }";

            VerifyCSharpDiagnostic(test);
        }

        [TestMethod]
        public void No_Diagnostics_Should_Show_For_IAsyncEnumerable_Method_Name_Ending_With_Async()
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
        public class TypeName
        {
            public IAsyncEnumerable<int> FooAsync() => null;
        }
    }";

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
            ExpectMissingAsync(test, "MyMethod", 13, 31);
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
        public void Should_Give_Warning_And_Fix_If_Awaitable_Method_Name_Does_Not_End_With_Async()
        {
            var test = @"
    using System.Threading.Tasks;
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public Task MyMethod(string input)
            {
                return Task.Delay(1000);
            }
        }
    }";
            ExpectMissingAsync(test, "MyMethod", 7, 25);
        }

        [TestMethod]
        public void Should_Give_Warning_And_Fix_If_AsyncVoid_Method_Name_Does_Not_End_With_Async()
        {
            var test = @"
    using System.Threading.Tasks;
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public async void MyMethod(string input)
            {
                await Task.Delay(1000);
            }
        }
    }";
            ExpectMissingAsync(test, "MyMethod", 7, 31);
        }

        [TestMethod]
        public void Should_Give_Warning_And_Fix_If_GenericTask_Method_Name_Does_Not_End_With_Async()
        {
            var test = @"
    using System.Threading.Tasks;
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public Task<int> MyMethod(string input)
            {
               return Task.FromResult(0);
            }
        }
    }";
            ExpectMissingAsync(test, "MyMethod", 7, 30);
        }

        [TestMethod]
        public void Should_Give_Warning_And_Fix_If_NonTaskAwaitable_Method_Name_Does_Not_End_With_Async()
        {
            var test = @"
    using System.Threading.Tasks;
    namespace ConsoleApplication1
    {
        interface IAwaitable {
        int GetAwaiter();
        }

        class TypeName
        {
            public IAwaitable MyMethod(string input)
            {
                return null;
            }
        }
    }";
            ExpectMissingAsync(test, "MyMethod", 11, 31);
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

            ExpectUnnecessaryAsync(test, "AsyncMethodAsync", 13, 25);
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

        [TestMethod]
        public void Should_Give_Warning_And_Fix_If_Return_Type_Is_IAsyncEnumerable_Method_Name_Does_Not_End_With_Async()
        {
            var test = @"
    using System.Threading.Tasks;
    using System.Collections.Generic;
    namespace ConsoleApplication1
    {
        class TypeName
        {
            public IAsyncEnumerable<int> MyMethod(string input)
            {
               return null;
            }
        }
    }";
            ExpectMissingAsync(test, "MyMethod", 8, 42);
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