using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace AsyncMethodNameFixer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(AsyncMethodNameFixerCodeFixProvider)), Shared]
    public class AsyncMethodNameFixerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Rename '{0}' to '{1}'";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId, AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().First();
                string finalTitle = string.Empty;
                if (diagnostic.Descriptor.Id == AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId)
                {
                    finalTitle = string.Format(title, declaration.Identifier.Text, declaration.Identifier.Text + "Async");
                }
                else if (diagnostic.Descriptor.Id == AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId)
                {
                    var newTitle = declaration.Identifier.Text.Substring(0, declaration.Identifier.Text.Length - 5);
                    finalTitle = string.Format(title, declaration.Identifier.Text, newTitle);
                }

                context.RegisterCodeFix(CodeAction.Create(
                   title: finalTitle,
                   createChangedSolution: c => RenameMethod(diagnostic.Descriptor.Id, context.Document, declaration, c),
                   equivalenceKey: finalTitle), diagnostic);

            }

        }

        private async Task<Solution> RenameMethod(string diagnosticDescriptorId, Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            // Compute new uppercase name.            
            var identifierToken = methodDeclaration.Identifier;

            string newName = string.Empty;
            if (diagnosticDescriptorId.Equals(AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId))
                newName = identifierToken.Text + "Async";
            else if (diagnosticDescriptorId.Equals(AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId))
                newName = identifierToken.Text.Substring(0, identifierToken.Text.Length - 5);

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}
