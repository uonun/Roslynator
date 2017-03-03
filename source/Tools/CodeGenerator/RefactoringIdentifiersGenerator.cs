// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator;
using Roslynator.CSharp;
using Roslynator.CSharp.Extensions;
using Roslynator.Extensions;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace CodeGenerator
{
    public class RefactoringIdentifiersGenerator : Generator
    {
        public StringComparer InvariantComparer { get; } = StringComparer.InvariantCulture;

        public CompilationUnitSyntax Generate(IEnumerable<RefactoringDescriptor> refactorings)
        {
            return CompilationUnit()
                .WithUsings(List(new UsingDirectiveSyntax[] {
                    UsingDirective(ParseName(MetadataNames.System_Collections_Generic)) }))
                .WithMembers(
                    NamespaceDeclaration(DefaultNamespace)
                        .WithMembers(
                            ClassDeclaration("RefactoringIdentifiers")
                                .WithModifiers(ModifierFactory.PublicStatic())
                                .WithMembers(
                                    CreateMembers(refactorings.OrderBy(f => f.Identifier, InvariantComparer)))));
        }

        private static ClassDeclarationSyntax AddEmptyLineBetweenMembers(ClassDeclarationSyntax classDeclaration)
        {
            MemberDeclarationSyntax[] newMembers = classDeclaration.Members.ToArray();

            for (int i = 1; i < newMembers.Length; i++)
            {
                if (newMembers[i].Kind() != newMembers[i - 1].Kind())
                {
                    newMembers[i - 1] = newMembers[i - 1].AppendToTrailingTrivia(NewLineTrivia());
                }
                else if (newMembers[i].IsKind(SyntaxKind.FieldDeclaration)
                    && newMembers[i - 1].IsKind(SyntaxKind.FieldDeclaration)
                    && ((FieldDeclarationSyntax)newMembers[i]).IsConst() != ((FieldDeclarationSyntax)newMembers[i - 1]).IsConst())
                {
                    newMembers[i - 1] = newMembers[i - 1].AppendToTrailingTrivia(NewLineTrivia());
                }
            }

            return classDeclaration.WithMembers(List(newMembers));
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<RefactoringDescriptor> refactorings)
        {
            foreach (RefactoringDescriptor refactoring in refactorings)
                yield return FieldDeclaration(ModifierFactory.PublicConst(), StringType(), refactoring.Identifier, StringLiteralExpression(refactoring.Id));
        }
    }
}
