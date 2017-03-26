// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.CSharp.Extensions;
using Roslynator.Extensions;

namespace Roslynator.CSharp
{
    public static class CSharpUtility
    {
        public static bool IsNamespaceInScope(
            SyntaxNode node,
            INamespaceSymbol namespaceSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (namespaceSymbol == null)
                throw new ArgumentNullException(nameof(namespaceSymbol));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            foreach (NameSyntax name in NamespacesInScope(node))
            {
                ISymbol symbol = semanticModel.GetSymbol(name, cancellationToken);

                if (symbol?.IsNamespace() == true)
                {
                    string namespaceText = namespaceSymbol.ToString();

                    if (string.Equals(namespaceText, symbol.ToString(), StringComparison.Ordinal))
                    {
                        return true;
                    }
                    else if (name.IsParentKind(SyntaxKind.NamespaceDeclaration))
                    {
                        foreach (INamespaceSymbol containingNamespace in symbol.ContainingNamespaces())
                        {
                            if (string.Equals(namespaceText, containingNamespace.ToString(), StringComparison.Ordinal))
                                return true;
                        }
                    }
                }
            }

            return false;
        }

        private static IEnumerable<NameSyntax> NamespacesInScope(SyntaxNode node)
        {
            foreach (SyntaxNode ancestor in node.Ancestors())
            {
                SyntaxKind kind = ancestor.Kind();

                if (kind == SyntaxKind.NamespaceDeclaration)
                {
                    var namespaceDeclaration = (NamespaceDeclarationSyntax)ancestor;

                    NameSyntax namespaceName = namespaceDeclaration.Name;

                    if (namespaceName != null)
                        yield return namespaceName;

                    foreach (NameSyntax name in namespaceDeclaration.Usings.Namespaces())
                        yield return name;
                }
                else if (kind == SyntaxKind.CompilationUnit)
                {
                    var compilationUnit = (CompilationUnitSyntax)ancestor;

                    foreach (NameSyntax name in compilationUnit.Usings.Namespaces())
                        yield return name;
                }
            }
        }

        public static bool IsStaticClassInScope(
            SyntaxNode node,
            INamedTypeSymbol staticClassSymbol,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (staticClassSymbol == null)
                throw new ArgumentNullException(nameof(staticClassSymbol));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            foreach (NameSyntax name in StaticClassesInScope(node))
            {
                ISymbol symbol = semanticModel.GetSymbol(name, cancellationToken);

                if (symbol?.Equals(staticClassSymbol) == true)
                    return true;
            }

            return false;
        }

        private static IEnumerable<NameSyntax> StaticClassesInScope(SyntaxNode node)
        {
            foreach (SyntaxNode ancestor in node.Ancestors())
            {
                SyntaxKind kind = ancestor.Kind();

                if (kind == SyntaxKind.NamespaceDeclaration)
                {
                    var namespaceDeclaration = (NamespaceDeclarationSyntax)ancestor;

                    foreach (NameSyntax name in namespaceDeclaration.Usings.StaticClasses())
                        yield return name;
                }
                else if (kind == SyntaxKind.CompilationUnit)
                {
                    var compilationUnit = (CompilationUnitSyntax)ancestor;

                    foreach (NameSyntax name in compilationUnit.Usings.StaticClasses())
                        yield return name;
                }
            }
        }

        private static IEnumerable<UsingDirectiveSyntax> RegularUsings(this SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (UsingDirectiveSyntax usingDirective in usings)
            {
                if (!usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword)
                    && usingDirective.Alias == null)
                {
                    yield return usingDirective;
                }
            }
        }

        private static IEnumerable<NameSyntax> Namespaces(this SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (UsingDirectiveSyntax usingDirective in usings.RegularUsings())
            {
                NameSyntax name = usingDirective.Name;

                if (name != null)
                    yield return name;
            }
        }

        private static IEnumerable<UsingDirectiveSyntax> StaticUsings(this SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (UsingDirectiveSyntax usingDirective in usings)
            {
                if (usingDirective.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
                    yield return usingDirective;
            }
        }

        private static IEnumerable<NameSyntax> StaticClasses(this SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (UsingDirectiveSyntax usingDirective in usings.StaticUsings())
            {
                NameSyntax name = usingDirective.Name;

                if (name != null)
                    yield return name;
            }
        }

        private static IEnumerable<UsingDirectiveSyntax> AliasUsings(this SyntaxList<UsingDirectiveSyntax> usings)
        {
            foreach (UsingDirectiveSyntax usingDirective in usings)
            {
                if (usingDirective.Alias != null)
                    yield return usingDirective;
            }
        }

        public static int GetOperatorPrecedence(ExpressionSyntax expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            return GetOperatorPrecedence(expression.Kind());
        }

        public static int GetOperatorPrecedence(SyntaxKind kind)
        {
            switch (kind)
            {
                case SyntaxKind.ParenthesizedExpression:
                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.ConditionalAccessExpression:
                case SyntaxKind.InvocationExpression:
                case SyntaxKind.ElementAccessExpression:
                case SyntaxKind.PostIncrementExpression:
                case SyntaxKind.PostDecrementExpression:
                case SyntaxKind.ObjectCreationExpression:
                case SyntaxKind.AnonymousObjectCreationExpression:
                case SyntaxKind.ArrayCreationExpression:
                case SyntaxKind.ImplicitArrayCreationExpression:
                case SyntaxKind.StackAllocArrayCreationExpression:
                case SyntaxKind.TypeOfExpression:
                case SyntaxKind.CheckedExpression:
                case SyntaxKind.UncheckedExpression:
                case SyntaxKind.DefaultExpression:
                case SyntaxKind.AwaitExpression:
                case SyntaxKind.AnonymousMethodExpression:
                    return 1;
                case SyntaxKind.UnaryPlusExpression:
                case SyntaxKind.UnaryMinusExpression:
                case SyntaxKind.LogicalNotExpression:
                case SyntaxKind.BitwiseNotExpression:
                case SyntaxKind.PreIncrementExpression:
                case SyntaxKind.PreDecrementExpression:
                case SyntaxKind.CastExpression:
                case SyntaxKind.PointerIndirectionExpression:
                case SyntaxKind.AddressOfExpression:
                    return 2;
                case SyntaxKind.MultiplyExpression:
                case SyntaxKind.DivideExpression:
                case SyntaxKind.ModuloExpression:
                    return 3;
                case SyntaxKind.AddExpression:
                case SyntaxKind.SubtractExpression:
                    return 4;
                case SyntaxKind.LeftShiftExpression:
                case SyntaxKind.RightShiftExpression:
                    return 5;
                case SyntaxKind.LessThanExpression:
                case SyntaxKind.GreaterThanExpression:
                case SyntaxKind.LessThanOrEqualExpression:
                case SyntaxKind.GreaterThanOrEqualExpression:
                case SyntaxKind.IsExpression:
                case SyntaxKind.AsExpression:
                    return 6;
                case SyntaxKind.EqualsExpression:
                case SyntaxKind.NotEqualsExpression:
                    return 7;
                case SyntaxKind.BitwiseAndExpression:
                    return 8;
                case SyntaxKind.ExclusiveOrExpression:
                    return 9;
                case SyntaxKind.BitwiseOrExpression:
                    return 10;
                case SyntaxKind.LogicalAndExpression:
                    return 11;
                case SyntaxKind.LogicalOrExpression:
                    return 12;
                case SyntaxKind.CoalesceExpression:
                    return 13;
                case SyntaxKind.ConditionalExpression:
                    return 14;
                case SyntaxKind.SimpleAssignmentExpression:
                case SyntaxKind.AddAssignmentExpression:
                case SyntaxKind.SubtractAssignmentExpression:
                case SyntaxKind.MultiplyAssignmentExpression:
                case SyntaxKind.DivideAssignmentExpression:
                case SyntaxKind.ModuloAssignmentExpression:
                case SyntaxKind.AndAssignmentExpression:
                case SyntaxKind.ExclusiveOrAssignmentExpression:
                case SyntaxKind.OrAssignmentExpression:
                case SyntaxKind.LeftShiftAssignmentExpression:
                case SyntaxKind.RightShiftAssignmentExpression:
                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:
                    return 15;
                default:
                    return 0;
            }
        }

        public static bool IsEmptyString(
            ExpressionSyntax expression,
            SemanticModel semanticModel,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            if (semanticModel == null)
                throw new ArgumentNullException(nameof(semanticModel));

            SyntaxKind kind = expression.Kind();

            if (kind == SyntaxKind.StringLiteralExpression)
            {
                return ((LiteralExpressionSyntax)expression).Token.ValueText.Length == 0;
            }
            else if (kind == SyntaxKind.SimpleMemberAccessExpression)
            {
                var memberAccess = (MemberAccessExpressionSyntax)expression;

                if (memberAccess.Name?.Identifier.ValueText == "Empty")
                {
                    ISymbol symbol = semanticModel.GetSymbol(memberAccess, cancellationToken);

                    if (symbol?.IsField() == true)
                    {
                        var fieldSymbol = (IFieldSymbol)symbol;

                        if (string.Equals(fieldSymbol.Name, "Empty", StringComparison.Ordinal)
                            && fieldSymbol.ContainingType?.IsString() == true
                            && fieldSymbol.IsPublic()
                            && fieldSymbol.IsStatic
                            && fieldSymbol.IsReadOnly
                            && fieldSymbol.Type.IsString())
                        {
                            return true;
                        }
                    }
                }
            }

            Optional<object> optional = semanticModel.GetConstantValue(expression, cancellationToken);

            if (optional.HasValue)
            {
                var value = optional.Value as string;

                return value?.Length == 0;
            }

            return false;
        }
    }
}
