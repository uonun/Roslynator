// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Roslynator.CSharp.Extensions;
using Roslynator.CSharp.Helpers;
using Roslynator.CSharp.SyntaxRewriters;
using Roslynator.Extensions;

namespace Roslynator.CSharp
{
    public static class Remover
    {
        public static SyntaxRemoveOptions DefaultRemoveOptions
        {
            get { return SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives; }
        }

        public static TNode RemoveStatement<TNode>(TNode node, StatementSyntax statement) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (statement == null)
                throw new ArgumentNullException(nameof(statement));

            return node.RemoveNode(statement, GetRemoveOptions(statement));
        }

        public static TNode RemoveModifier<TNode>(TNode node, SyntaxKind modifierKind) where TNode : SyntaxNode
        {
            return RemoveModifierHelper.RemoveModifier(node, modifierKind);
        }

        public static TNode RemoveModifier<TNode>(TNode node, SyntaxToken modifier) where TNode : SyntaxNode
        {
            return RemoveModifierHelper.RemoveModifier(node, modifier);
        }

        public static TNode RemoveComments<TNode>(TNode node, CommentRemoveOptions removeOptions) where TNode : SyntaxNode
        {
            return CommentRemover.RemoveComments(node, removeOptions);
        }

        public static TNode RemoveComments<TNode>(TNode node, CommentRemoveOptions removeOptions, TextSpan span) where TNode : SyntaxNode
        {
            return CommentRemover.RemoveComments(node, removeOptions, span);
        }

        public static TNode RemoveTrivia<TNode>(TNode node, TextSpan? span = null) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return TriviaRemover.RemoveTrivia(node, span);
        }

        public static TNode RemoveWhitespaceOrEndOfLineTrivia<TNode>(TNode node, TextSpan? span = null) where TNode : SyntaxNode
        {
            return WhitespaceOrEndOfLineTriviaRemover.RemoveWhitespaceOrEndOfLineTrivia(node, span);
        }

        private static async Task<SyntaxTree> RemoveDirectivesAsync(
            SyntaxTree syntaxTree,
            IEnumerable<DirectiveTriviaSyntax> directives,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SourceText sourceText = await syntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);

            SourceText newSourceText = RemoveDirectives(sourceText, directives);

            return syntaxTree.WithChangedText(newSourceText);
        }

        private static async Task<SyntaxTree> RemoveDirectivesAsync(
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            SourceText sourceText = await syntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);

            SourceText newSourceText = RemoveDirectives(sourceText, root.DescendantDirectives());

            return syntaxTree.WithChangedText(newSourceText);
        }

        internal static SourceText RemoveDirectives(
            SourceText sourceText,
            IEnumerable<DirectiveTriviaSyntax> directives)
        {
            TextLineCollection lines = sourceText.Lines;

            var changes = new List<TextChange>();

            foreach (DirectiveTriviaSyntax directive in directives)
            {
                int startLine = directive.GetSpanStartLine();

                changes.Add(new TextChange(lines[startLine].SpanIncludingLineBreak, string.Empty));
            }

            return sourceText.WithChanges(changes);
        }

        private static async Task<SyntaxTree> RemoveRegionDirectivesAsync(
            SyntaxTree syntaxTree,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SyntaxNode root = await syntaxTree.GetRootAsync(cancellationToken).ConfigureAwait(false);

            return await RemoveDirectivesAsync(syntaxTree, root.DescendantRegionDirectives(), cancellationToken).ConfigureAwait(false);
        }

        internal static SyntaxNode RemoveEmptyNamespaces(SyntaxNode node, SyntaxRemoveOptions removeOptions)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            IEnumerable<NamespaceDeclarationSyntax> emptyNamespaces = node
                .DescendantNodes()
                .Where(f => f.IsKind(SyntaxKind.NamespaceDeclaration))
                .Cast<NamespaceDeclarationSyntax>()
                .Where(f => !f.Members.Any());

            return node.RemoveNodes(emptyNamespaces, removeOptions);
        }

        internal static SyntaxRemoveOptions GetRemoveOptions(CSharpSyntaxNode node)
        {
            SyntaxRemoveOptions removeOptions = DefaultRemoveOptions;

            if (node.GetLeadingTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                removeOptions &= ~SyntaxRemoveOptions.KeepLeadingTrivia;

            if (node.GetTrailingTrivia().All(f => f.IsWhitespaceOrEndOfLineTrivia()))
                removeOptions &= ~SyntaxRemoveOptions.KeepTrailingTrivia;

            return removeOptions;
        }
    }
}
