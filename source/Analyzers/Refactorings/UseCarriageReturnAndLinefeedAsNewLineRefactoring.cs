﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using Roslynator.Diagnostics.Extensions;
using Roslynator.Extensions;

namespace Roslynator.CSharp.Refactorings
{
    internal static class UseCarriageReturnAndLinefeedAsNewLineRefactoring
    {
        public static void Analyze(SyntaxTreeAnalysisContext context)
        {
            SourceText sourceText;
            if (!context.Tree.TryGetText(out sourceText))
                return;

            SyntaxNode root;
            if (!context.Tree.TryGetRoot(out root))
                return;

            foreach (TextLine textLine in sourceText.Lines)
            {
                int end = textLine.End;

                if (textLine.EndIncludingLineBreak - end == 1
                    && textLine.Text[end] == '\n')
                {
                    context.ReportDiagnostic(
                        DiagnosticDescriptors.UseCarriageReturnAndLinefeedAsNewLine,
                        Location.Create(context.Tree, new TextSpan(end, 1)));
                }
            }
        }

        public static Task<Document> RefactorAsync(
            Document document,
            TextSpan span,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return document.WithTextChangeAsync(new TextChange(span, "\r\n"), cancellationToken);
        }
    }
}
