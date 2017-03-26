// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings.EnumWithFlagsAttribute
{
    internal static class GenerateEnumMemberRefactoring
    {
        public static async Task ComputeRefactoringAsync(RefactoringContext context, EnumDeclarationSyntax enumDeclaration)
        {
            SemanticModel semanticModel = await context.GetSemanticModelAsync().ConfigureAwait(false);

            INamedTypeSymbol enumSymbol = semanticModel.GetDeclaredSymbol(enumDeclaration, context.CancellationToken);

            if (enumSymbol.IsEnumWithFlagsAttribute(semanticModel))
            {
                object[] values = EnumHelper.GetValues(enumSymbol).ToArray();

                SpecialType specialType = enumSymbol.EnumUnderlyingType.SpecialType;

                Optional<object> optional = EnumHelper.GetUniquePowerOfTwo(specialType, values);

                if (optional.HasValue)
                {
                    context.RegisterRefactoring(
                        "Generate enum member",
                        cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, optional.Value, cancellationToken));

                    Optional<object> optional2 = EnumHelper.GetUniquePowerOfTwo(specialType, values, startFromHighestExistingValue: true);

                    if (optional2.HasValue
                        && !optional.Value.Equals(optional2.Value))
                    {
                        context.RegisterRefactoring(
                            $"Generate enum member (with value {optional2.Value})",
                            cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, optional2.Value, cancellationToken));
                    }
                }
            }
            else
            {
                context.RegisterRefactoring(
                    "Generate enum member",
                    cancellationToken => RefactorAsync(context.Document, enumDeclaration, enumSymbol, null, cancellationToken));
            }
        }

        private static Task<Document> RefactorAsync(
            Document document,
            EnumDeclarationSyntax enumDeclaration,
            INamedTypeSymbol enumSymbol,
            object value,
            CancellationToken cancellationToken)
        {
            EnumMemberDeclarationSyntax newEnumMember = CreateEnumMember(enumSymbol, Identifier.DefaultEnumMemberName, value);

            EnumDeclarationSyntax newNode = enumDeclaration.AddMembers(newEnumMember);

            return document.ReplaceNodeAsync(enumDeclaration, newNode, cancellationToken);
        }

        private static EnumMemberDeclarationSyntax CreateEnumMember(INamedTypeSymbol enumSymbol, string name, object value)
        {
            EqualsValueClauseSyntax equalsValue = null;

            if (value != null)
                equalsValue = SyntaxFactory.EqualsValueClause(CSharpFactory.ConstantExpression(value));

            name = Identifier.EnsureUniqueEnumMemberName(enumSymbol, name);

            SyntaxToken identifier = SyntaxFactory.Identifier(name).WithRenameAnnotation();

            return SyntaxFactory.EnumMemberDeclaration(
                default(SyntaxList<AttributeListSyntax>),
                identifier,
                equalsValue);
        }
    }
}