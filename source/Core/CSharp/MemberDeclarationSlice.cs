// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.CSharp
{
    public class MemberDeclarationSlice : ListSlice<MemberDeclarationSyntax>
    {
        private MemberDeclarationSlice(MemberDeclarationSyntax containingMember, SyntaxList<MemberDeclarationSyntax> members, TextSpan span)
             : base(members, span)
        {
            ContainingMember = containingMember;
        }

        public MemberDeclarationSyntax ContainingMember { get; }

        public static MemberDeclarationSlice Create(NamespaceDeclarationSyntax namespaceDeclaration, TextSpan span)
        {
            if (namespaceDeclaration == null)
                throw new ArgumentNullException(nameof(namespaceDeclaration));

            return new MemberDeclarationSlice(namespaceDeclaration, namespaceDeclaration.Members, span);
        }

        public static MemberDeclarationSlice Create(ClassDeclarationSyntax classDeclaration, TextSpan span)
        {
            if (classDeclaration == null)
                throw new ArgumentNullException(nameof(classDeclaration));

            return new MemberDeclarationSlice(classDeclaration, classDeclaration.Members, span);
        }

        public static MemberDeclarationSlice Create(StructDeclarationSyntax structDeclaration, TextSpan span)
        {
            if (structDeclaration == null)
                throw new ArgumentNullException(nameof(structDeclaration));

            return new MemberDeclarationSlice(structDeclaration, structDeclaration.Members, span);
        }

        public static MemberDeclarationSlice Create(InterfaceDeclarationSyntax interfaceDeclaration, TextSpan span)
        {
            if (interfaceDeclaration == null)
                throw new ArgumentNullException(nameof(interfaceDeclaration));

            return new MemberDeclarationSlice(interfaceDeclaration, interfaceDeclaration.Members, span);
        }
    }
}
