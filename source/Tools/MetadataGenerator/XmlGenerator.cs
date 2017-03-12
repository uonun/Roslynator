// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Roslynator.Metadata;

namespace MetadataGenerator
{
    internal static class XmlGenerator
    {
        public static string CreateDefaultConfigFile(IEnumerable<RefactoringDescriptor> refactorings)
        {
            var doc = new XDocument(
                new XElement("Roslynator",
                    new XElement("Settings",
                        new XElement("General",
                            new XElement("PrefixFieldIdentifierWithUnderscore", new XAttribute("IsEnabled", true))),
                        new XElement("Refactorings",
                            refactorings
                                .OrderBy(f => f.Id)
                                .Select(f =>
                                {
                                    return new XNode[] {
                                        new XElement("Refactoring",
                                        new XAttribute("Id", f.Id),
                                        new XAttribute("IsEnabled", f.IsEnabledByDefault)),
                                        new XComment($" {f.Identifier} ")
                                    };
                                })
                        )
                    )
                )
            );

            var xmlWriterSettings = new XmlWriterSettings()
            {
                OmitXmlDeclaration = false,
                NewLineChars = "\r\n",
                IndentChars = "  ",
                Indent = true
            };

            using (var sw = new Utf8StringWriter())
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(sw, xmlWriterSettings))
                    doc.WriteTo(xmlWriter);

                string s = sw.ToString();

                s = _regex.Replace(s, "${refactoring} ${comment}");

                return s;
            }
        }

        private static readonly Regex _regex = new Regex(@"
            (?<refactoring><Refactoring\ Id=""RR[0-9]{4}""\ IsEnabled=""(true|false)""\ />)
            \s+
            (?<comment><!--\ [a-zA-Z0-9]+\ -->)
            ", RegexOptions.IgnorePatternWhitespace);
    }
}
