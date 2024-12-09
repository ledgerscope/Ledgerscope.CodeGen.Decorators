using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Linq;

namespace Ledgerscope.CodeGen.Decorators
{
    internal class InterfaceRecord : IEquatable<InterfaceRecord>
    {
        public InterfaceDeclarationSyntax Syntax { get; set; }
        public SemanticModel SemanticModel { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as InterfaceRecord);
        }

        public bool Equals(InterfaceRecord other) => Syntax.IsIncrementallyIdenticalTo(other.Syntax);

        public override int GetHashCode() => Syntax.ChildNodes().Count();
    }
}
