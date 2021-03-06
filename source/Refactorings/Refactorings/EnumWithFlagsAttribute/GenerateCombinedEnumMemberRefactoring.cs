﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Extensions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using static Roslynator.CSharp.CSharpFactory;

namespace Roslynator.CSharp.Refactorings.EnumWithFlagsAttribute
{
    internal static class GenerateCombinedEnumMemberRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, EnumDeclarationSyntax enumDeclaration)
        {
            EnumMemberDeclarationSyntax[] selectedMembers = GetSelectedMembers(enumDeclaration, context.Span).ToArray();

            if (selectedMembers.Length > 1)
            {
                SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

                INamedTypeSymbol enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration, context.CancellationToken);

                if (enumSymbol != null
                    && SymbolUtility.IsEnumWithFlagsAttribute(enumSymbol, semanticModel))
                {
                    IFieldSymbol[] fieldSymbols = selectedMembers
                        .Select(f => semanticModel.GetDeclaredSymbol(f, context.CancellationToken))
                        .ToArray();

                    object[] constantValues = fieldSymbols
                        .Where(f => f.HasConstantValue)
                        .Select(f => f.ConstantValue)
                        .ToArray();

                    object combinedValue = GetCombinedValue(constantValues, enumSymbol);

                    if (combinedValue != null
                        && !EnumHelper.IsValueDefined(enumSymbol, combinedValue))
                    {
                        SeparatedSyntaxList<EnumMemberDeclarationSyntax> enumMembers = enumDeclaration.Members;

                        string name = Identifier.EnsureUniqueEnumMemberName(
                            enumSymbol,
                            string.Concat(selectedMembers.Select(f => f.Identifier.ValueText)));

                        EnumMemberDeclarationSyntax newEnumMember = CreateEnumMember(name, selectedMembers);

                        int insertIndex = enumMembers.IndexOf(selectedMembers.Last()) + 1;

                        context.RegisterRefactoring(
                            $"Generate enum member '{name}'",
                            cancellationToken => RefactorAsync(context.Document, enumDeclaration, newEnumMember, insertIndex, cancellationToken));
                    }
                }
            }
        }

        private static object GetCombinedValue(IEnumerable<object> constantValues, INamedTypeSymbol enumSymbol)
        {
            switch (enumSymbol.EnumUnderlyingType.SpecialType)
            {
                case SpecialType.System_SByte:
                    {
                        sbyte[] values = constantValues.OfType<sbyte>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return values.Aggregate((f, g) => (sbyte)(f + g));
                    }
                case SpecialType.System_Byte:
                    {
                        byte[] values = constantValues.OfType<byte>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<byte>().Aggregate((f, g) => (byte)(f + g));
                    }
                case SpecialType.System_Int16:
                    {
                        short[] values = constantValues.OfType<short>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<short>().Aggregate((f, g) => (short)(f + g));
                    }
                case SpecialType.System_UInt16:
                    {
                        ushort[] values = constantValues.OfType<ushort>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<ushort>().Aggregate((f, g) => (ushort)(f + g));
                    }
                case SpecialType.System_Int32:
                    {
                        int[] values = constantValues.OfType<int>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<int>().Aggregate((f, g) => f + g);
                    }
                case SpecialType.System_UInt32:
                    {
                        uint[] values = constantValues.OfType<uint>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<uint>().Aggregate((f, g) => f + g);
                    }
                case SpecialType.System_Int64:
                    {
                        long[] values = constantValues.OfType<long>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<long>().Aggregate((f, g) => f + g);
                    }
                case SpecialType.System_UInt64:
                    {
                        ulong[] values = constantValues.OfType<ulong>().ToArray();

                        for (int i = 0; i < values.Length; i++)
                        {
                            for (int j = 0; j < values.Length; j++)
                            {
                                if (j != i
                                    && (values[i] & values[j]) != 0)
                                {
                                    return null;
                                }
                            }
                        }

                        return constantValues.OfType<ulong>().Aggregate((f, g) => f + g);
                    }
            }

            return null;
        }

        public static EnumMemberDeclarationSyntax CreateEnumMember(string name, EnumMemberDeclarationSyntax[] enumMembers)
        {
            ExpressionSyntax expression = IdentifierName(enumMembers.Last().Identifier.WithoutTrivia());

            for (int i = enumMembers.Length - 2; i >= 0; i--)
                expression = BitwiseOrExpression(IdentifierName(enumMembers[i].Identifier.WithoutTrivia()), expression);

            return EnumMemberDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                Identifier(name).WithRenameAnnotation(),
                EqualsValueClause(expression));
        }

        private static Task<Document> RefactorAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            EnumMemberDeclarationSyntax newEnumMember,
            int insertIndex,
            CancellationToken cancellationToken)
        {
            EnumDeclarationSyntax newNode = enumDeclaration.WithMembers(enumDeclaration.Members.Insert(insertIndex, newEnumMember));

            return document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken);
        }

        private static IEnumerable<EnumMemberDeclarationSyntax> GetSelectedMembers(EnumDeclarationSyntax enumDeclaration, TextSpan span)
        {
            return enumDeclaration.Members
                .SkipWhile(f => span.Start > f.Span.Start)
                .TakeWhile(f => span.End >= f.Span.End);
        }
    }
}