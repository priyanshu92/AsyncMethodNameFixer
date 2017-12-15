using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace AsyncMethodNameFixer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodNameFixerAnalyzer : DiagnosticAnalyzer
    {
        public const string AsyncDiagnosticId = "AsyncMethodDiagnostic";
        public const string NonAsyncDiagnosticId = "NonAsyncMethodDiagnostic";

        private static readonly LocalizableString AsyncTitle = new LocalizableResourceString(nameof(Resources.AsyncAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsyncMessageFormat = new LocalizableResourceString(nameof(Resources.AsyncAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString AsyncDescription = new LocalizableResourceString(nameof(Resources.AsyncAnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString NonAsyncTitle = new LocalizableResourceString(nameof(Resources.NonAsyncAnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NonAsyncMessageFormat = new LocalizableResourceString(nameof(Resources.NonAsyncAnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString NonAsyncDescription = new LocalizableResourceString(nameof(Resources.NonAsyncAnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor AsyncRule = new DiagnosticDescriptor(AsyncDiagnosticId, AsyncTitle, AsyncMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: AsyncDescription);

        private static DiagnosticDescriptor NonAsyncRule = new DiagnosticDescriptor(NonAsyncDiagnosticId, NonAsyncTitle, NonAsyncMessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: NonAsyncDescription);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(AsyncRule, NonAsyncRule); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.Method);
        }

        private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var methodSymbol = (IMethodSymbol)context.Symbol;

            if (methodSymbol.IsAsync && !methodSymbol.Name.EndsWith("Async"))
            {
                var diagnostic = Diagnostic.Create(AsyncRule, methodSymbol.Locations[0], methodSymbol.Name);

                context.ReportDiagnostic(diagnostic);
            }

            if (!methodSymbol.IsAsync && methodSymbol.Name.EndsWith("Async"))
            {
                var diagnostic = Diagnostic.Create(NonAsyncRule, methodSymbol.Locations[0], methodSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}
