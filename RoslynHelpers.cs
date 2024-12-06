using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace Ledgerscope.CodeGen.Decorators
{
    internal static class RoslynHelpers
    {
        public static IEnumerable<ISymbol> GetAllInterfaceMembers(this ITypeSymbol typeSymbol)
        {
            foreach (var member in typeSymbol.GetMembers())
                yield return member;

            foreach (var @interface in typeSymbol.AllInterfaces)
                foreach (var member in @interface.GetMembers())
                    yield return member;
        }

        public static TypeSyntax ToTypeSyntax(this ITypeSymbol typeSymbol)
        {
            return SyntaxFactory.ParseTypeName(typeSymbol.ToDisplayString());
        }
    }
}
