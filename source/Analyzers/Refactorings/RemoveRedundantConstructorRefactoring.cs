﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.CSharp.Extensions;
using Roslynator.Diagnostics.Extensions;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings
{
    internal static class RemoveRedundantConstructorRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, ConstructorDeclarationSyntax constructor)
        {
            if (constructor.ParameterList?.Parameters.Any() == false
                && constructor.Body?.Statements.Any() == false)
            {
                SyntaxTokenList modifiers = constructor.Modifiers;

                if (modifiers.Contains(SyntaxKind.PublicKeyword)
                    && !modifiers.Contains(SyntaxKind.StaticKeyword))
                {
                    ConstructorInitializerSyntax initializer = constructor.Initializer;

                    if (initializer == null
                        || initializer.ArgumentList?.Arguments.Any() == false)
                    {
                        if (IsSingleInstanceConstructor(constructor)
                            && constructor
                                .DescendantTrivia(constructor.Span)
                                .All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                        {
                            context.ReportDiagnostic(DiagnosticDescriptors.RemoveRedundantConstructor, constructor);
                        }
                    }
                }
            }
        }

        private static bool IsSingleInstanceConstructor(ConstructorDeclarationSyntax constructor)
        {
            var parent = constructor.Parent as MemberDeclarationSyntax;

            return parent?
                    .GetMembers()
                    .OfType<ConstructorDeclarationSyntax>()
                    .All(f => f.Equals(constructor) || f.IsStatic()) == true;
        }

        public static Task<Document> RefactorAsync(
            Document document,
            ConstructorDeclarationSyntax constructorDeclaration,
            CancellationToken cancellationToken)
        {
            return Remover.RemoveMemberAsync(document, constructorDeclaration, cancellationToken);
        }
    }
}
