// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
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
                new XElement("roslynator",
                    new XElement("settings",
                        new XElement("general",
                            new XElement("prefixFieldIdentifierWithUnderscore", new XAttribute("isEnabled", true))),
                        new XElement("refactorings",
                            refactorings.Select(f =>
                            {
                                return new XElement("refactoring",
                                    new XAttribute("id", f.Id),
                                    new XAttribute("isEnabled", f.IsEnabledByDefault));
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

                return sw.ToString();
            }
        }
    }
}
