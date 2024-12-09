using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public static TypeParameterConstraintClauseSyntax[] GetTypeParameterConstraintClauses(this ITypeParameterSymbol typeParameterSymbol)
        {
            var clauses = new List<TypeParameterConstraintSyntax>();

            foreach (var constraintType in typeParameterSymbol.ConstraintTypes)
            {
                clauses.Add(SyntaxFactory.TypeConstraint(constraintType.ToTypeSyntax()));
            }

            if (typeParameterSymbol.HasReferenceTypeConstraint)
            {
                clauses.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.ClassConstraint));
            }

            if (typeParameterSymbol.HasConstructorConstraint)
            {
                clauses.Add(SyntaxFactory.ConstructorConstraint());
            }

            if (typeParameterSymbol.HasValueTypeConstraint)
            {
                clauses.Add(SyntaxFactory.ClassOrStructConstraint(SyntaxKind.StructConstraint));
            }

            if (clauses.Count != 0)
                return new[] { SyntaxFactory.TypeParameterConstraintClause(typeParameterSymbol.Name).AddConstraints(clauses.ToArray()) };

            return Array.Empty<TypeParameterConstraintClauseSyntax>();
        }
    }
}
