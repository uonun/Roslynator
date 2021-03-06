﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Extensions;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings
{
    internal static class NegateBooleanLiteralRefactoring
    {
        public static Task<Document> RefactorAsync(
            Document document,
            LiteralExpressionSyntax literalExpression,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            LiteralExpressionSyntax newNode = (literalExpression.IsKind(SyntaxKind.TrueLiteralExpression))
                ? FalseLiteralExpression()
                : TrueLiteralExpression();

            newNode = newNode.WithTriviaFrom(literalExpression);

            return document.ReplaceNodeAsync(literalExpression, newNode, cancellationToken);
        }
    }
}
