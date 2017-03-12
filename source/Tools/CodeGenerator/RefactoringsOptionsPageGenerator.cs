// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator;
using Roslynator.CSharp;
using Roslynator.CSharp.Extensions;
using Roslynator.Metadata;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace CodeGenerator
{
    public class OptionsPagePropertiesGenerator : Generator
    {
        public StringComparer InvariantComparer { get; } = StringComparer.InvariantCulture;

        public OptionsPagePropertiesGenerator()
        {
            DefaultNamespace = "Roslynator.VisualStudio";
        }

        public CompilationUnitSyntax Generate(IEnumerable<RefactoringDescriptor> refactorings)
        {
            return CompilationUnit()
                .WithUsings(List(new UsingDirectiveSyntax[] {
                    UsingDirective(ParseName(MetadataNames.System_Collections_Generic)),
                    UsingDirective(ParseName(MetadataNames.System_ComponentModel)),
                    UsingDirective(ParseName(MetadataNames.System_Linq)),
                    UsingDirective(ParseName("Roslynator.CSharp.Refactorings")),
                    UsingDirective(ParseName("Roslynator.VisualStudio.TypeConverters"))}))
                .WithMembers(
                    NamespaceDeclaration(DefaultNamespace)
                        .WithMembers(
                            ClassDeclaration("RefactoringsOptionsPage")
                                .WithModifiers(Modifiers.PublicPartial())
                                .WithMembers(
                                    CreateMembers(refactorings))));
        }

        private IEnumerable<MemberDeclarationSyntax> CreateMembers(IEnumerable<RefactoringDescriptor> refactorings)
        {
            yield return ConstructorDeclaration("RefactoringsOptionsPage")
                .WithModifiers(Modifiers.Public())
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return SimpleAssignmentStatement(
                                IdentifierName(refactoring.Id),
                                (refactoring.IsEnabledByDefault) ? TrueLiteralExpression() : FalseLiteralExpression());
                        })));

            yield return MethodDeclaration(VoidType(), "MigrateValuesFromIdentifierPropertiesToIdProperties")
                .WithModifiers(Modifiers.Public())
                .WithParameterList(ParameterList())
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                ParseExpression($"{refactoring.Id} = {refactoring.Identifier}"));
                        })));

            yield return MethodDeclaration(VoidType(), "SetRefactoringsDisabledByDefault")
                .WithModifiers(Modifiers.PublicStatic())
                .WithParameterList(ParameterList(Parameter(IdentifierName("RefactoringSettings"), Identifier("settings"))))
                .WithBody(
                    Block(refactorings
                        .Where(f => !f.IsEnabledByDefault)
                        .OrderBy(f => f.Identifier, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                ParseExpression($"settings.DisableRefactoring(RefactoringIdentifiers.{refactoring.Identifier})"));
                        })));

            yield return MethodDeclaration(VoidType(), "SaveValuesToView")
                .WithModifiers(Modifiers.Public())
                .WithParameterList(ParameterList(Parameter(ParseTypeName("ICollection<RefactoringModel>"), Identifier("refactorings"))))
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                ParseExpression($"refactorings.Add(new RefactoringModel(\"{refactoring.Id}\", \"{StringUtility.EscapeQuote(refactoring.Title)}\", {refactoring.Id}))"));
                        })));

            yield return MethodDeclaration(VoidType(), "LoadValuesFromView")
                .WithModifiers(Modifiers.Public())
                .WithParameterList(ParameterList(Parameter(ParseTypeName("ICollection<RefactoringModel>"), Identifier("refactorings"))))
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Id, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                ParseExpression($"{refactoring.Id} = refactorings.FirstOrDefault(f => f.Id == \"{refactoring.Id}\").Enabled"));
                        })));

            yield return MethodDeclaration(VoidType(), "Apply")
                .WithModifiers(Modifiers.Public())
                .WithBody(
                    Block(refactorings
                        .OrderBy(f => f.Identifier, InvariantComparer)
                        .Select(refactoring =>
                        {
                            return ExpressionStatement(
                                InvocationExpression(
                                    IdentifierName("SetIsEnabled"),
                                    ArgumentList(
                                        Argument(
                                            SimpleMemberAccessExpression(
                                                IdentifierName("RefactoringIdentifiers"),
                                                IdentifierName(refactoring.Identifier))),
                                        Argument(IdentifierName(refactoring.Id)))));
                        })));

            foreach (RefactoringDescriptor info in refactorings.OrderBy(f => f.Id, InvariantComparer))
                yield return PropertyDeclaration(BoolType(), info.Id)
                    .WithAttributeLists(
                        SingletonAttributeList(Attribute(IdentifierName("Category"), IdentifierName("RefactoringCategory"))),
                        SingletonAttributeList(Attribute(IdentifierName("DisplayName"), StringLiteralExpression(info.Title))),
                        SingletonAttributeList(Attribute(IdentifierName("Description"), StringLiteralExpression(CreateDescription(info)))),
                        SingletonAttributeList(Attribute(IdentifierName("TypeConverter"), TypeOfExpression(IdentifierName("EnabledDisabledConverter")))))
                    .WithModifiers(Modifiers.Public())
                    .WithAccessorList(
                        AccessorList(
                            AutoGetAccessorDeclaration(),
                            AutoSetAccessorDeclaration()));

            foreach (RefactoringDescriptor info in refactorings.OrderBy(f => f.Identifier, InvariantComparer))
                yield return PropertyDeclaration(BoolType(), info.Identifier)
                    .WithAttributeLists(
                        SingletonAttributeList(Attribute(IdentifierName("Browsable"), FalseLiteralExpression())),
                        SingletonAttributeList(Attribute(IdentifierName("Category"), IdentifierName("RefactoringCategory"))),
                        SingletonAttributeList(Attribute(IdentifierName("TypeConverter"), TypeOfExpression(IdentifierName("EnabledDisabledConverter")))))
                    .WithModifiers(Modifiers.Public())
                    .WithAccessorList(
                        AccessorList(
                            AutoGetAccessorDeclaration(),
                            AutoSetAccessorDeclaration()));
        }

        private static string CreateDescription(RefactoringDescriptor refactoring)
        {
            string s = "";

            if (refactoring.Syntaxes.Count > 0)
                s = "Syntax: " + string.Join(", ", refactoring.Syntaxes.Select(f => f.Name));

            if (!string.IsNullOrEmpty(refactoring.Scope))
            {
                if (!string.IsNullOrEmpty(s))
                    s += "\r\n";

                s += "Scope: " + refactoring.Scope;
            }

            return s;
        }
    }
}
