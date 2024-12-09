using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
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
                predicate: static (_, _) => true, // ForAttributeWithMetadataName already filters by attribute name
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx));

            static InterfaceRecord GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
            {
                var interfaceDeclarationSyntax = (InterfaceDeclarationSyntax)context.TargetNode;
                return new InterfaceRecord() { Syntax = interfaceDeclarationSyntax, SemanticModel = context.SemanticModel };
            }

            context.RegisterSourceOutput(classDeclarations, Execute);
        }

        private static void Execute(SourceProductionContext context, InterfaceRecord interfaceRecord)
        {
            var namespaceDeclaration = OutputGenerator.GenerateOutputs(interfaceRecord.SemanticModel.GetDeclaredSymbol(interfaceRecord.Syntax));

            context.AddSource("Decorator." + namespaceDeclaration.ChildNodes().OfType<ClassDeclarationSyntax>().First().Identifier.ToString() + ".g.cs", SourceText.From(namespaceDeclaration.NormalizeWhitespace().ToFullString(), Encoding.UTF8));
        }
    }
}