// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Helpers;
using Roslynator.CSharp.SyntaxRewriters;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Extensions
{
    public static class DocumentExtensions
    {
        public static async Task<Document> RemoveMemberAsync(
            this Document document,
            MemberDeclarationSyntax member,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (member == null)
                throw new ArgumentNullException(nameof(member));

            SyntaxNode parent = member.Parent;

            if (parent?.IsKind(SyntaxKind.CompilationUnit) == true)
            {
                var compilationUnit = (CompilationUnitSyntax)parent;

                return await document.ReplaceNodeAsync(compilationUnit, compilationUnit.RemoveMember(member), cancellationToken).ConfigureAwait(false);
            }
            else
            {
                var parentMember = (MemberDeclarationSyntax)parent;

                if (parentMember != null)
                {
                    return await document.ReplaceNodeAsync(parentMember, parentMember.RemoveMember(member), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    return await document.RemoveNodeAsync(member, Remover.DefaultRemoveOptions, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        public static Task<Document> RemoveStatementAsync(this Document document, StatementSyntax statement, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            return document.RemoveNodeAsync(statement, Remover.GetRemoveOptions(statement), cancellationToken);
        }

        public static Task<Document> RemoveCommentAsync(
            this Document document,
            SyntaxTrivia comment,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return RemoveCommentHelper.RemoveCommentAsync(document, comment, cancellationToken);
        }

        public static async Task<Document> RemoveCommentsAsync(
            this Document document,
            CommentRemoveOptions removeOptions,
            TextSpan span,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode newRoot = Remover.RemoveComments(root, removeOptions, span)
                .WithFormatterAnnotation();

            return document.WithSyntaxRoot(newRoot);
        }

        public static async Task<Document> RemoveCommentsAsync(
            this Document document,
            CommentRemoveOptions removeOptions,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode newRoot = Remover.RemoveComments(root, removeOptions)
                .WithFormatterAnnotation();

            return document.WithSyntaxRoot(newRoot);
        }

        public static async Task<Document> RemoveTriviaAsync(
            this Document document,
            TextSpan span,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SyntaxNode newRoot = TriviaRemover.RemoveTrivia(root, span);

            return document.WithSyntaxRoot(newRoot);
        }

        public static async Task<Document> RemoveDirectivesAsync(
            this Document document,
            IEnumerable<DirectiveTriviaSyntax> directives,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (directives == null)
                throw new ArgumentNullException(nameof(directives));

            SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            SourceText newSourceText = Remover.RemoveDirectives(sourceText, directives);

            return document.WithText(newSourceText);
        }

        public static async Task<Document> RemoveDirectivesAsync(
            this Document document,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            SourceText newSourceText = Remover.RemoveDirectives(sourceText, root.DescendantDirectives());

            return document.WithText(newSourceText);
        }

        public static async Task<Document> RemoveRegionAsync(
            this Document document,
            RegionDirectiveTriviaSyntax regionDirective,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (regionDirective == null)
                throw new ArgumentNullException(nameof(regionDirective));

            List<DirectiveTriviaSyntax> list = regionDirective.GetRelatedDirectives();

            if (list.Count == 2
                && list[1].IsKind(SyntaxKind.EndRegionDirectiveTrivia))
            {
                var endRegionDirective = (EndRegionDirectiveTriviaSyntax)list[1];

                return await RemoveRegionAsync(document, regionDirective, endRegionDirective, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }

        public static async Task<Document> RemoveRegionAsync(
            this Document document,
            EndRegionDirectiveTriviaSyntax endRegionDirective,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (endRegionDirective == null)
                throw new ArgumentNullException(nameof(endRegionDirective));

            List<DirectiveTriviaSyntax> list = endRegionDirective.GetRelatedDirectives();

            if (list.Count == 2
                && list[0].IsKind(SyntaxKind.RegionDirectiveTrivia))
            {
                var regionDirective = (RegionDirectiveTriviaSyntax)list[0];

                return await RemoveRegionAsync(document, regionDirective, endRegionDirective, cancellationToken).ConfigureAwait(false);
            }

            return document;
        }

        private static async Task<Document> RemoveRegionAsync(Document document, RegionDirectiveTriviaSyntax regionDirective, EndRegionDirectiveTriviaSyntax endRegionDirective, CancellationToken cancellationToken)
        {
            SourceText sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);

            int startLine = regionDirective.GetSpanStartLine();
            int endLine = endRegionDirective.GetSpanEndLine();

            TextLineCollection lines = sourceText.Lines;

            TextSpan span = TextSpan.FromBounds(
                lines[startLine].Start,
                lines[endLine].EndIncludingLineBreak);

            var textChange = new TextChange(span, string.Empty);

            SourceText newSourceText = sourceText.WithChanges(textChange);

            return document.WithText(newSourceText);
        }

        public static async Task<Document> RemoveRegionDirectivesAsync(
            this Document document,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            return await RemoveDirectivesAsync(document, root.DescendantRegionDirectives(), cancellationToken).ConfigureAwait(false);
        }
    }
}
