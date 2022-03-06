using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace AsyncMethodNameFixer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodNameFixerAnalyzer : DiagnosticAnalyzer
    {
        public const string AsyncDiagnosticId = "AMNF0001";
        public const string NonAsyncDiagnosticId = "AMNF0002";

        private static readonly LocalizableString AsyncTitle = new LocalizableResourceString(nameof(Resources.AsyncAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsyncMessageFormat = new LocalizableResourceString(nameof(Resources.AsyncAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsyncDescription = new LocalizableResourceString(nameof(Resources.AsyncAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString NonAsyncTitle = new LocalizableResourceString(nameof(Resources.NonAsyncAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NonAsyncMessageFormat = new LocalizableResourceString(nameof(Resources.NonAsyncAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NonAsyncDescription = new LocalizableResourceString(nameof(Resources.NonAsyncAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor AsyncRule = new DiagnosticDescriptor(AsyncDiagnosticId, AsyncTitle, AsyncMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: AsyncDescription);

        private static readonly DiagnosticDescriptor NonAsyncRule = new DiagnosticDescriptor(NonAsyncDiagnosticId, NonAsyncTitle, NonAsyncMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NonAsyncDescription);

        private static readonly IList<string> testMethodAttributes = new List<string> { "TestMethod", "Test", "SetUp", "Theory", "Fact", "DataTestMethod" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AsyncRule, NonAsyncRule);

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static bool IsAwaitable(IMethodSymbol method)
        {
            //TODO - this can't detect awaitable return types that are subclasses of awaitable base-classes.
            //It seems unlikely anyone would want to do that in practice though!

            //The defining characteristic of an asynchronous method is that its return type implements GetAwaiter
            //It's probably overkill to also check the interfaces but it's more complete in case anyone decides not
            //to uses Tasks in future.
            var returnType = method.ReturnType;

            var allMembers = returnType.Interfaces.SelectMany(i => i.MemberNames)
                .Concat(returnType.GetMembers().Select(m => m.Name))
                .ToArray();
            var isAwaitable = allMembers.Contains(WellKnownMemberNames.GetAwaiter);
            //also check for async in case this is async void
            return isAwaitable || method.IsAsync || method.ReturnType.Name.Equals("IAsyncEnumerable");
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (ShouldHaveAsyncInTheEnd(methodSymbol))
            {
                var diagnostic = Diagnostic.Create(AsyncRule, methodSymbol.Locations[0], methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }

            if (ShouldNotHaveAsyncInTheEnd(methodSymbol))
            {
                var diagnostic = Diagnostic.Create(NonAsyncRule, methodSymbol.Locations[0], methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool ShouldHaveAsyncInTheEnd(IMethodSymbol methodSymbol)
        {
            var interfaces = methodSymbol.ContainingType.Interfaces;

            foreach (var item in interfaces)
            {
                if (item.MemberNames.Contains(methodSymbol.Name))
                    return false;
            }

            var methodAttributes = methodSymbol.GetAttributes();

            foreach (var testMethodAttribute in testMethodAttributes)
            {
                if (methodAttributes.Any(x => x.AttributeClass.Name.StartsWith(testMethodAttribute)))
                    return false;
            }

            return IsAwaitable(methodSymbol)
                && !methodSymbol.IsOverride
                && !methodSymbol.Name.Equals("Main")
                && !methodSymbol.Name.Equals("<Main>$")
                && !methodSymbol.Name.EndsWith("Async");
        }

        private static bool ShouldNotHaveAsyncInTheEnd(IMethodSymbol methodSymbol)
        {
            var methodAttributes = methodSymbol.GetAttributes();

            foreach (var testMethodAttribute in testMethodAttributes)
            {
                if (methodAttributes.Any(x => x.AttributeClass.Name.StartsWith(testMethodAttribute)))
                    return false;
            }

            return !IsAwaitable(methodSymbol)
                && !methodSymbol.IsOverride
                && methodSymbol.Name.EndsWith("Async");
        }
    }
}