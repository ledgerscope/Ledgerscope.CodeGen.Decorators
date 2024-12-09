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
        private static readonly string attributeName = typeof(DecorateAttribute).FullName;

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var classDeclarations = context.SyntaxProvider.ForAttributeWithMetadataName(attributeName,
                predicate: static (_, _) => true,
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null);

            static NamespaceDeclarationSyntax GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
            {
                var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.TargetNode;
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