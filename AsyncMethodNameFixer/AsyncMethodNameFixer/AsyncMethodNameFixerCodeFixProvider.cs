using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            foreach (var diagnostic in context.Diagnostics)
            {
                var diagnosticSpan = diagnostic.Location.SourceSpan;
                var stuff = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf();

                if (stuff.Any(x => x is MethodDeclarationSyntax))
                {
                    var declaration = stuff.OfType<MethodDeclarationSyntax>().First();
                    string finalTitle = string.Empty;
                    if (diagnostic.Descriptor.Id == AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId)
                    {
                        finalTitle = string.Format(title, declaration.Identifier.Text, declaration.Identifier.Text.AppendAsyncToString());
                    }
                    else if (diagnostic.Descriptor.Id == AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId)
                    {
                        var newTitle = declaration.Identifier.Text.RemoveAsyncFromString();
                        finalTitle = string.Format(title, declaration.Identifier.Text, newTitle);
                    }

                    context.RegisterCodeFix(CodeAction.Create(
                       title: finalTitle,
                       createChangedSolution: c => RenameMethodAsync(diagnostic.Descriptor.Id, context.Document, declaration, c),
                       equivalenceKey: finalTitle), diagnostic);
                }
                else if (stuff.Any(x => x is PropertyDeclarationSyntax))
                {
                    var declaration = stuff.OfType<PropertyDeclarationSyntax>().First();
                    string finalTitle = string.Empty;
                    if (diagnostic.Descriptor.Id == AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId)
                    {
                        finalTitle = string.Format(title, declaration.Identifier.Text, declaration.Identifier.Text.AppendAsyncToString());
                    }
                    else if (diagnostic.Descriptor.Id == AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId)
                    {
                        var newTitle = declaration.Identifier.Text.RemoveAsyncFromString();
                        finalTitle = string.Format(title, declaration.Identifier.Text, newTitle);
                    }

                    context.RegisterCodeFix(CodeAction.Create(
                       title: finalTitle,
                       createChangedSolution: c => RenamePropertyAsync(diagnostic.Descriptor.Id, context.Document, declaration, c),
                       equivalenceKey: finalTitle), diagnostic);
                }

            }
        }

        private async Task<Solution> RenameMethodAsync(string diagnosticDescriptorId, Document document, MethodDeclarationSyntax methodDeclaration, CancellationToken cancellationToken)
        {
            var identifierToken = methodDeclaration.Identifier;

            // Compute new name.
            string newName = string.Empty;
            if (diagnosticDescriptorId.Equals(AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId))
                newName = identifierToken.Text.AppendAsyncToString();
            else if (diagnosticDescriptorId.Equals(AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId))
                newName = identifierToken.Text.RemoveAsyncFromString();

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

        private async Task<Solution> RenamePropertyAsync(string diagnosticDescriptorId, Document document, PropertyDeclarationSyntax prpoertyDelcaration, CancellationToken cancellationToken)
        {
            var identifierToken = prpoertyDelcaration.Identifier;

            // Compute new name.
            string newName = string.Empty;
            if (diagnosticDescriptorId.Equals(AsyncMethodNameFixerAnalyzer.AsyncDiagnosticId))
                newName = identifierToken.Text.AppendAsyncToString();
            else if (diagnosticDescriptorId.Equals(AsyncMethodNameFixerAnalyzer.NonAsyncDiagnosticId))
                newName = identifierToken.Text.RemoveAsyncFromString();

            // Get the symbol representing the type to be renamed.
            var semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var typeSymbol = semanticModel.GetDeclaredSymbol(prpoertyDelcaration, cancellationToken);

            // Produce a new solution that has all references to that type renamed, including the declaration.
            var originalSolution = document.Project.Solution;
            var optionSet = originalSolution.Workspace.Options;
            var newSolution = await Renamer.RenameSymbolAsync(document.Project.Solution, typeSymbol, newName, optionSet, cancellationToken).ConfigureAwait(false);

            // Return the new solution with the now-uppercase type name.
            return newSolution;
        }
    }
}