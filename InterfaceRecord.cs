using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Ledgerscope.CodeGen.Decorators
{
    internal class InterfaceRecord : IEquatable<InterfaceRecord>
    {
        public InterfaceDeclarationSyntax Syntax { get; set; }
        public SemanticModel SemanticModel { get; internal set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as InterfaceRecord);
        }

        public bool Equals(InterfaceRecord other)
        {
            return other is not null &&
                   EqualityComparer<InterfaceDeclarationSyntax>.Default.Equals(Syntax, other.Syntax);
        }

        public override int GetHashCode()
        {
            return 860707778 + EqualityComparer<InterfaceDeclarationSyntax>.Default.GetHashCode(Syntax);
        }
    }
}
