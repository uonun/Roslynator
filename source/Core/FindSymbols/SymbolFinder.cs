// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;

namespace Roslynator.FindSymbols
{
    public static class SymbolFinder
    {
        public static Task<IEnumerable<ReferencedSymbol>> FindReferencesAsync(
            ISymbol symbol,
            Document document,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            return Microsoft.CodeAnalysis.FindSymbols.SymbolFinder.FindReferencesAsync(
                symbol,
                document.Project.Solution,
                ImmutableHashSet.Create(document),
                cancellationToken);
        }

        public static async Task<ImmutableArray<SyntaxNode>> FindNodesAsync(
            ISymbol symbol,
            Document document,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (symbol == null)
                throw new ArgumentNullException(nameof(symbol));

            if (document == null)
                throw new ArgumentNullException(nameof(document));

            List<SyntaxNode> nodes = null;

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            IEnumerable<ReferencedSymbol> referencedSymbols = await FindReferencesAsync(symbol, document, cancellationToken).ConfigureAwait(false);

            foreach (Location location in referencedSymbols
                .SelectMany(f => f.Locations)
                .Select(f => f.Location)
                .Where(f => f.IsInSource))
            {
                SyntaxNode node = root.FindNode(location.SourceSpan, findInsideTrivia: true, getInnermostNodeForTie: true);

                Debug.Assert(node != null);

                if (node != null)
                    (nodes ?? (nodes = new List<SyntaxNode>())).Add(node);
            }

            if (nodes != null)
            {
                return ImmutableArray.CreateRange(nodes);
            }
            else
            {
                return ImmutableArray<SyntaxNode>.Empty;
            }
        }
    }
}
