﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Roslynator.Diagnostics.Extensions;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings
{
    internal static class RemoveEmptyDestructorRefactoring
    {
        public static void Analyze(SyntaxNodeAnalysisContext context, DestructorDeclarationSyntax destructor)
        {
            if (destructor.Body?.Statements.Count == 0
                && !destructor.SpanContainsDirectives())
            {
                context.ReportDiagnostic(DiagnosticDescriptors.RemoveEmptyDestructor, destructor);
            }
        }

        public static Task<Document> RefactorAsync(
            Document document,
            DestructorDeclarationSyntax destructorDeclaration,
            CancellationToken cancellationToken)
        {
            return Remover.RemoveMemberAsync(document, destructorDeclaration, cancellationToken);
        }
    }
}
