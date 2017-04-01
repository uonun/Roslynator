// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Simplification;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator.Extensions
{
    public static class SyntaxExtensions
    {
        #region SyntaxNode
        public static IEnumerable<SyntaxTrivia> GetLeadingTrailingTrivia(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.GetLeadingTrivia().Concat(node.GetTrailingTrivia());
        }

        public static TNode PrependToLeadingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return node.WithLeadingTrivia(trivia.Concat(node.GetLeadingTrivia()));
        }

        public static TNode PrependToLeadingTrivia<TNode>(this TNode node, SyntaxTrivia trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithLeadingTrivia(node.GetLeadingTrivia().Insert(0, trivia));
        }

        public static TNode AppendToTrailingTrivia<TNode>(this TNode node, IEnumerable<SyntaxTrivia> trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return node.WithTrailingTrivia(node.GetTrailingTrivia().AddRange(trivia));
        }

        public static TNode AppendToTrailingTrivia<TNode>(this TNode node, SyntaxTrivia trivia) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithTrailingTrivia(node.GetTrailingTrivia().Add(trivia));
        }

        public static bool SpanContainsDirectives(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && !node.GetLeadingTrivia().Any(f => f.IsDirective)
                && !node.GetTrailingTrivia().Any(f => f.IsDirective);
        }

        internal static bool SpanOrLeadingTriviaContainsDirectives(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && !node.GetTrailingTrivia().Any(f => f.IsDirective);
        }

        internal static bool SpanOrTrailingTriviaContainsDirectives(this SyntaxNode node)
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.ContainsDirectives
                && !node.GetLeadingTrivia().Any(f => f.IsDirective);
        }

        public static bool ContainsDirectives(this SyntaxNode node, TextSpan span)
        {
            return node.ContainsDirectives
                && node.DescendantTrivia(span).Any(f => f.IsDirective);
        }

        public static TNode WithTriviaFrom<TNode>(this TNode node, SyntaxToken token) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node
                .WithLeadingTrivia(token.LeadingTrivia)
                .WithTrailingTrivia(token.TrailingTrivia);
        }

        public static int GetSpanStartLine(this SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.SyntaxTree != null)
            {
                return node.SyntaxTree.GetLineSpan(node.Span, cancellationToken).StartLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetFullSpanStartLine(
            this SyntaxNode node,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.SyntaxTree != null)
            {
                return node.SyntaxTree.GetLineSpan(node.FullSpan, cancellationToken).StartLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetSpanEndLine(this SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.SyntaxTree != null)
            {
                return node.SyntaxTree.GetLineSpan(node.Span, cancellationToken).EndLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetFullSpanEndLine(
            this SyntaxNode node,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            if (node.SyntaxTree != null)
            {
                return node.SyntaxTree.GetLineSpan(node.FullSpan, cancellationToken).EndLine();
            }
            else
            {
                return -1;
            }
        }

        public static TNode FirstAncestor<TNode>(
            this SyntaxNode node,
            Func<TNode, bool> predicate = null,
            bool ascendOutOfTrivia = true) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.Parent?.FirstAncestorOrSelf(predicate, ascendOutOfTrivia);
        }

        public static TNode WithFormatterAnnotation<TNode>(this TNode node) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithAdditionalAnnotations(Formatter.Annotation);
        }

        public static TNode WithSimplifierAnnotation<TNode>(this TNode node) where TNode : SyntaxNode
        {
            if (node == null)
                throw new ArgumentNullException(nameof(node));

            return node.WithAdditionalAnnotations(Simplifier.Annotation);
        }

        internal static string ToString(this SyntaxNode node, TextSpan span)
        {
            return GetSubstring(node, node.ToString(), span);
        }

        internal static string ToFullString(this SyntaxNode node, TextSpan span)
        {
            return GetSubstring(node, node.ToFullString(), span);
        }

        private static string GetSubstring(SyntaxNode node, string s, TextSpan span)
        {
            return s.Substring(span.Start - node.SpanStart, span.Length);
        }
        #endregion

        #region SyntaxNodeOrToken
        public static SyntaxNodeOrToken WithoutLeadingTrivia(this SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                return nodeOrToken.AsNode().WithoutLeadingTrivia();
            }
            else
            {
                return nodeOrToken.AsToken().WithoutLeadingTrivia();
            }
        }

        public static SyntaxNodeOrToken WithoutTrailingTrivia(this SyntaxNodeOrToken nodeOrToken)
        {
            if (nodeOrToken.IsNode)
            {
                return nodeOrToken.AsNode().WithoutTrailingTrivia();
            }
            else
            {
                return nodeOrToken.AsToken().WithoutTrailingTrivia();
            }
        }
        #endregion

        #region SyntaxToken
        public static SyntaxToken PrependToLeadingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return token.WithLeadingTrivia(trivia.Concat(token.LeadingTrivia));
        }

        public static SyntaxToken PrependToLeadingTrivia(this SyntaxToken token, SyntaxTrivia trivia)
        {
            return token.WithLeadingTrivia(token.LeadingTrivia.Insert(0, trivia));
        }

        public static SyntaxToken AppendToTrailingTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia)
        {
            if (trivia == null)
                throw new ArgumentNullException(nameof(trivia));

            return token.WithTrailingTrivia(token.TrailingTrivia.AddRange(trivia));
        }

        public static SyntaxToken AppendToTrailingTrivia(this SyntaxToken token, SyntaxTrivia trivia)
        {
            return token.WithTrailingTrivia(token.TrailingTrivia.Add(trivia));
        }

        public static IEnumerable<SyntaxTrivia> GetLeadingTrailingTrivia(this SyntaxToken token)
        {
            return token.LeadingTrivia.Concat(token.TrailingTrivia);
        }

        public static int GetSpanStartLine(this SyntaxToken token, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (token.SyntaxTree != null)
            {
                return token.SyntaxTree.GetLineSpan(token.Span, cancellationToken).StartLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetFullSpanStartLine(this SyntaxToken token, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (token.SyntaxTree != null)
            {
                return token.SyntaxTree.GetLineSpan(token.FullSpan, cancellationToken).StartLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetSpanEndLine(this SyntaxToken token, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (token.SyntaxTree != null)
            {
                return token.SyntaxTree.GetLineSpan(token.Span, cancellationToken).EndLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetFullSpanEndLine(this SyntaxToken token, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (token.SyntaxTree != null)
            {
                return token.SyntaxTree.GetLineSpan(token.FullSpan, cancellationToken).EndLine();
            }
            else
            {
                return -1;
            }
        }

        public static SyntaxToken WithoutLeadingTrivia(this SyntaxToken token)
        {
            return token.WithLeadingTrivia(default(SyntaxTriviaList));
        }

        public static SyntaxToken WithoutTrailingTrivia(this SyntaxToken token)
        {
            return token.WithTrailingTrivia(default(SyntaxTriviaList));
        }

        public static SyntaxToken WithFormatterAnnotation(this SyntaxToken token)
        {
            return token.WithAdditionalAnnotations(Formatter.Annotation);
        }

        public static SyntaxToken WithSimplifierAnnotation(this SyntaxToken token)
        {
            return token.WithAdditionalAnnotations(Simplifier.Annotation);
        }

        public static SyntaxToken WithRenameAnnotation(this SyntaxToken token)
        {
            return token.WithAdditionalAnnotations(RenameAnnotation.Create());
        }
        #endregion

        #region SyntaxTokenList
        public static SyntaxTokenList ReplaceAt(this SyntaxTokenList tokenList, int index, SyntaxToken newToken)
        {
            return tokenList.Replace(tokenList[index], newToken);
        }
        #endregion

        #region SyntaxTriviaList
        public static SyntaxTriviaList GetContainingList(this SyntaxTrivia trivia)
        {
            SyntaxToken token = trivia.Token;

            SyntaxTriviaList leadingTrivia = token.LeadingTrivia;

            int index = leadingTrivia.IndexOf(trivia);

            if (index != -1)
                return token.LeadingTrivia;

            SyntaxTriviaList trailingTrivia = token.TrailingTrivia;

            index = trailingTrivia.IndexOf(trivia);

            if (index != -1)
                return token.TrailingTrivia;

            Debug.Assert(false, "containing trivia list not found");

            return default(SyntaxTriviaList);
        }

        public static int GetSpanStartLine(this SyntaxTrivia trivia, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (trivia.SyntaxTree != null)
            {
                return trivia.SyntaxTree.GetLineSpan(trivia.Span, cancellationToken).StartLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetFullSpanStartLine(this SyntaxTrivia trivia, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (trivia.SyntaxTree != null)
            {
                return trivia.SyntaxTree.GetLineSpan(trivia.FullSpan, cancellationToken).StartLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetSpanEndLine(this SyntaxTrivia trivia, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (trivia.SyntaxTree != null)
            {
                return trivia.SyntaxTree.GetLineSpan(trivia.Span, cancellationToken).EndLine();
            }
            else
            {
                return -1;
            }
        }

        public static int GetFullSpanEndLine(this SyntaxTrivia trivia, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (trivia.SyntaxTree != null)
            {
                return trivia.SyntaxTree.GetLineSpan(trivia.FullSpan, cancellationToken).EndLine();
            }
            else
            {
                return -1;
            }
        }
        #endregion
    }
}
