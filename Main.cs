using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Ledgerscope.CodeGen.Decorators
{
    [Generator]
    public class Main : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

            static bool IsSyntaxTargetForGeneration(SyntaxNode node)
            {
                if (node is InterfaceDeclarationSyntax m)
                {
                    if (m.AttributeLists.Any(a => a.Attributes.Any(b => b.Name.ToString() == "Decorate")))
                    {
                        return true;
                    }
                    //=> node is InterfaceDeclarationSyntax m && m.AttributeLists.Any(a => a.Attributes.Any(b => b.Name.ToString() == nameof(DecorateAttribute)));
                }
                return false;
            }

            static NamespaceDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
            {
                var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.Node;
                return OutputGenerator.GenerateOutputs(context.SemanticModel.GetDeclaredSymbol(interfaceDeclarationSyntax));
            }

            IncrementalValueProvider<(Compilation, ImmutableArray<NamespaceDeclarationSyntax>)> compilationAndClasses
                = context.CompilationProvider.Combine(classDeclarations.Collect());

            context.RegisterSourceOutput(compilationAndClasses,
                static (spc, source) => Execute(source.Item1, source.Item2, spc));
        }

        private static void Execute(Compilation compilation, ImmutableArray<NamespaceDeclarationSyntax> classes, SourceProductionContext context)
        {
            if (classes.IsDefaultOrEmpty)
            {
                // nothing to do yet
                return;
            }

            var distinctClasses = classes.Distinct();

            foreach (var namespaceDeclaration in distinctClasses)
            {
                context.AddSource("Decorator." + namespaceDeclaration.ChildNodes().OfType<ClassDeclarationSyntax>().First().Identifier.ToString() + ".g.cs", SourceText.From(namespaceDeclaration.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
            }

            //var p = new Parser(compilation, context.ReportDiagnostic, context.CancellationToken);

            //IReadOnlyList<LoggerClass> logClasses = p.GetLogClasses(distinctClasses);
            //if (logClasses.Count > 0) {
            //    var e = new Emitter();
            //    string result = e.Emit(logClasses, context.CancellationToken);

            //    context.AddSource("LoggerMessage.g.cs", SourceText.From(result, Encoding.UTF8));
            //}
        }
    }
}