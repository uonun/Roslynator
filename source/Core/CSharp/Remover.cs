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
using Roslynator.Extensions;

namespace Roslynator.CSharp
{
    internal static class Remover
    {
        public static SyntaxRemoveOptions DefaultRemoveOptions
        {
            get { return SyntaxRemoveOptions.KeepExteriorTrivia | SyntaxRemoveOptions.KeepUnbalancedDirectives; }
        }

        public static SourceText RemoveDirectives(
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

        private static async Task<SyntaxTree> RemoveDirectivesAsync(
            SyntaxTree syntaxTree,
            IEnumerable<DirectiveTriviaSyntax> directives,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            SourceText sourceText = await syntaxTree.GetTextAsync(cancellationToken).ConfigureAwait(false);

            SourceText newSourceText = RemoveDirectives(sourceText, directives);

            return syntaxTree.WithChangedText(newSourceText);
        }

        public static SyntaxNode RemoveEmptyNamespaces(SyntaxNode node, SyntaxRemoveOptions removeOptions)
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

        public static SyntaxRemoveOptions GetRemoveOptions(CSharpSyntaxNode node)
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
