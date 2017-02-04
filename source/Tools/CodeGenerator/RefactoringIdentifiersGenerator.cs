// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
        public CompilationUnitSyntax Generate(IEnumerable<RefactoringInfo> refactorings)
        {
            CompilationUnitSyntax compilationUnit = CompilationUnit()
                .WithUsings(List(new UsingDirectiveSyntax[] {
                    UsingDirective(ParseName(MetadataNames.System_Collections_Generic)) }))
                .WithMembers(
                    NamespaceDeclaration(DefaultNamespace)
                        .WithMembers(
                            ClassDeclaration("RefactoringIdentifiers")
                                .WithModifiers(ModifierFactory.PublicStatic())
                                .WithMembers(
                                    CreateMembers(refactorings))));

            compilationUnit = compilationUnit.NormalizeWhitespace();

            var classDeclaration = (ClassDeclarationSyntax)compilationUnit
                .DescendantNodes()
                .FirstOrDefault(f => f.IsKind(SyntaxKind.ClassDeclaration));

            ClassDeclarationSyntax newClassDeclaration = AddEmptyLineBetweenMembers(classDeclaration);

            newClassDeclaration = FormatInitializer(newClassDeclaration);

            return compilationUnit.ReplaceNode(classDeclaration, newClassDeclaration);
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

        private static ClassDeclarationSyntax FormatInitializer(ClassDeclarationSyntax newClassDeclaration)
        {
            var initializer = (InitializerExpressionSyntax)newClassDeclaration
                .DescendantNodes()
                .FirstOrDefault(f => f.IsKind(SyntaxKind.ObjectInitializerExpression));

            InitializerExpressionSyntax newInitializer = initializer
                .WithOpenBraceToken(initializer.OpenBraceToken.WithTrailingTrivia(NewLineTrivia()))
                .WithCloseBraceToken(initializer.CloseBraceToken.WithLeadingTrivia(ParseLeadingTrivia("        ")).PrependToLeadingTrivia(NewLineTrivia()))
                .WithExpressions(initializer.Expressions.Select(f => f.WithLeadingTrivia(ParseLeadingTrivia("            "))).ToSeparatedSyntaxList());

            newInitializer = newInitializer.ReplaceTokens(newInitializer.ChildTokens().Where(f => f.IsKind(SyntaxKind.CommaToken)), (f, g) => f.WithTrailingTrivia(NewLineTrivia()));

            newClassDeclaration = newClassDeclaration.ReplaceNode(initializer, newInitializer);
            return newClassDeclaration;
        }

        private static IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<RefactoringInfo> refactorings)
        {
            foreach (RefactoringInfo refactoring in refactorings)
                yield return CreateConstantDeclaration(refactoring.Identifier);

            TypeSyntax type = ParseTypeName("Dictionary<string, string>");

            yield return FieldDeclaration(
                ModifierFactory.PrivateStaticReadOnly(),
                type,
                Identifier("_map"),
                ObjectCreationExpression(
                    type,
                    ArgumentList(),
                    ObjectInitializerExpression(
                        refactorings
                            .Select(refactoring =>
                            {
                                return SimpleAssignmentExpression(
                                    ImplicitElementAccess(StringLiteralExpression(refactoring.Id)),
                                    StringLiteralExpression(refactoring.Identifier));
                            })
                            .ToSeparatedSyntaxList<ExpressionSyntax>()
                    )
                )
            );

            yield return MethodDeclaration(
                ModifierFactory.PublicStatic(),
                BoolType(),
                Identifier("TryGetIdentifier"),
                ParameterList(
                    Parameter(StringType(), Identifier("id")),
                    Parameter(ModifierFactory.Out(), StringType(), Identifier("identifier"))),
                Block(
                    ReturnStatement(
                        SimpleMemberInvocationExpression(
                            IdentifierName("_map"),
                            "TryGetValue",
                            ArgumentList(
                                Argument(IdentifierName("id")),
                                Argument(default(NameColonSyntax), OutKeyword(), IdentifierName("identifier")))))));
        }

        private static MemberDeclarationSyntax CreateConstantDeclaration(string name)
        {
            return FieldDeclaration(ModifierFactory.PublicConst(), StringType(), name, StringLiteralExpression(name));
        }
    }
}
