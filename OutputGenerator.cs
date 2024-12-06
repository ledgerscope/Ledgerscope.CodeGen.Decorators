using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Ledgerscope.CodeGen.Decorators
{
    internal static class OutputGenerator
    {
        public static NamespaceDeclarationSyntax GenerateOutputs(INamedTypeSymbol type)
        {
            var interfaceName = type.Name;
            var className = type.Name.Substring(1);
            var decoratorName = className + "Decorator";

            var constructorParamName = char.ToLower(interfaceName[1]) + interfaceName.Substring(2);
            var fieldName = "_" + constructorParamName;

            var parsedType = type.ToTypeSyntax();

            var ancestorInterfaces = type.AllInterfaces;
            var ancestorMembers = ancestorInterfaces.SelectMany(a => a.GetMembers());
            var members = type.GetMembers().Concat(ancestorMembers).ToArray();

            var modifier = SyntaxFacts.GetText(type.DeclaredAccessibility);
            var accessibilityModifier = SyntaxFactory.Token(SyntaxFacts.GetKeywordKind(modifier));

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(type.ContainingNamespace.ToDisplayString()))
                .AddMembers(SyntaxFactory.ClassDeclaration(decoratorName)
                .AddModifiers(accessibilityModifier, SyntaxFactory.Token(SyntaxKind.AbstractKeyword), SyntaxFactory.Token(SyntaxKind.PartialKeyword))
                .AddBaseListTypes(SyntaxFactory.SimpleBaseType(parsedType))
                .AddMembers(getField(parsedType, fieldName))
                .AddMembers(getConstructor(decoratorName, constructorParamName, parsedType, fieldName))
                .AddMembers(getMembers(type, fieldName).ToArray()));
        }

        private static FieldDeclarationSyntax getField(TypeSyntax typeSyntax, string fieldName)
        {
            return SyntaxFactory.FieldDeclaration(SyntaxFactory.VariableDeclaration(typeSyntax)
                    .AddVariables(SyntaxFactory.VariableDeclarator(fieldName))).AddModifiers(SyntaxFactory.Token(SyntaxKind.PrivateKeyword), SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));
        }

        private static ConstructorDeclarationSyntax getConstructor(string decoratorName, string constructorParamName, TypeSyntax typeSyntax, string fieldName)
        {
            return SyntaxFactory.ConstructorDeclaration(decoratorName)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .AddParameterListParameters(SyntaxFactory.Parameter(SyntaxFactory.Identifier(constructorParamName)).WithType(typeSyntax))
                    .WithBody(
                        SyntaxFactory.Block(
                            SyntaxFactory.ExpressionStatement(
                                SyntaxFactory.AssignmentExpression(
                                    SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName(constructorParamName)))));
        }

        private static IEnumerable<MemberDeclarationSyntax> getMembers(INamedTypeSymbol type, string fieldName)
        {
            var members = type.GetAllInterfaceMembers();

            foreach (var member in members)
            {
                if (member is IMethodSymbol { MethodKind: MethodKind.Ordinary } method)
                {

                    var methodCall = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName(method.Name)))
                        .AddArgumentListArguments(method.Parameters.Select(p => SyntaxFactory.Argument(SyntaxFactory.IdentifierName(p.Name))).ToArray());

                    StatementSyntax statement;
                    if (method.ReturnType.SpecialType == SpecialType.System_Void)
                    {
                        statement = SyntaxFactory.ExpressionStatement(methodCall);
                    }
                    else
                    {
                        statement = SyntaxFactory.ReturnStatement(methodCall);
                    }

                    yield return SyntaxFactory.MethodDeclaration(method.ReturnType.ToTypeSyntax(), method.Name)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                        .AddParameterListParameters(method.Parameters.Select(p => SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name)).WithType(p.Type.ToTypeSyntax())).ToArray())
                        .WithBody(SyntaxFactory.Block(statement));

                }
                else if (member is IPropertySymbol property)
                {

                    yield return SyntaxFactory.PropertyDeclaration(property.Type.ToTypeSyntax(), property.Name)
                        .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword), SyntaxFactory.Token(SyntaxKind.VirtualKeyword))
                        .AddAccessorListAccessors(
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.ReturnStatement(SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName(property.Name))))),
                            SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                                .WithBody(SyntaxFactory.Block(SyntaxFactory.ExpressionStatement(SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.IdentifierName(fieldName), SyntaxFactory.IdentifierName(property.Name)), SyntaxFactory.IdentifierName("value"))))));
                }
            }
        }
    }
}