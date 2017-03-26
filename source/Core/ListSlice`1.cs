// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Roslynator
{
    public class ListSlice<TNode> : IEnumerable, IEnumerable<TNode> where TNode : SyntaxNode
    {
        public ListSlice(SyntaxList<TNode> list, TextSpan span)
        {
            UnderlyingList = list;
            Span = span;

            SyntaxList<TNode>.Enumerator en = UnderlyingList.GetEnumerator();

            if (en.MoveNext())
            {
                int i = 0;

                while (Span.Start >= en.Current.FullSpan.End
                    && en.MoveNext())
                {
                    i++;
                }

                if (Span.Start >= en.Current.FullSpan.Start
                    && Span.Start <= en.Current.Span.Start)
                {
                    int j = i;

                    while (Span.End > en.Current.FullSpan.End
                        && en.MoveNext())
                    {
                        j++;
                    }

                    if (Span.End >= en.Current.Span.End
                        && Span.End <= en.Current.FullSpan.End)
                    {
                        StartIndex = i;
                        EndIndex = j;
                    }
                }
            }
        }

        public TextSpan Span { get; }

        public SyntaxList<TNode> UnderlyingList { get; }

        public int StartIndex { get; } = -1;

        public int EndIndex { get; } = -1;

        public int Count
        {
            get
            {
                if (Any())
                {
                    return EndIndex - StartIndex + 1;
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool Any()
        {
            return StartIndex != -1;
        }

        public TNode First()
        {
            return UnderlyingList[StartIndex];
        }

        public TNode FirstOrDefault()
        {
            if (Any())
            {
                return UnderlyingList[StartIndex];
            }
            else
            {
                return null;
            }
        }

        public TNode Last()
        {
            return UnderlyingList[EndIndex];
        }

        public TNode LastOrDefault()
        {
            if (Any())
            {
                return UnderlyingList[EndIndex];
            }
            else
            {
                return null;
            }
        }

        public ImmutableArray<TNode> Nodes
        {
            get
            {
                if (Any())
                {
                    ImmutableArray<TNode>.Builder builder = ImmutableArray.CreateBuilder<TNode>(Count);

                    for (int i = StartIndex; i <= EndIndex; i++)
                        builder.Add(UnderlyingList[i]);

                    return builder.ToImmutable();
                }

                return ImmutableArray<TNode>.Empty;
            }
        }

        private IEnumerable<TNode> EnumerateNodes()
        {
            if (Any())
            {
                for (int i = StartIndex; i <= EndIndex; i++)
                    yield return UnderlyingList[i];
            }
        }

        public IEnumerator<TNode> GetEnumerator()
        {
            return EnumerateNodes().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
