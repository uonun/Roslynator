// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Roslynator.VisualStudio.Settings
{
    public sealed class ApplicationSettings
    {
        public const string FileName = "roslynator.config";

        public ApplicationSettings()
        {
            Refactorings = new Dictionary<string, bool>(StringComparer.Ordinal);
        }

        public bool PrefixFieldIdentifierWithUnderscore { get; set; } = true;

        public Dictionary<string, bool> Refactorings { get; set; }

        public static ApplicationSettings Load(string uri)
        {
            var settings = new ApplicationSettings();

            XDocument doc = XDocument.Load(uri);

            XElement root = doc.Element("roslynator");

            if (root != null)
            {
                foreach (XElement element in root.Elements())
                {
                    XName name = element.Name;

                    if (name == "settings")
                        LoadSettingsElement(element, settings);
                }
            }

            return settings;
        }

        private static void LoadSettingsElement(XElement element, ApplicationSettings settings)
        {
            foreach (XElement child in element.Elements())
            {
                XName name = child.Name;

                if (name == "general")
                {
                    LoadPrefixFieldIdentifierWithUnderscore(child, settings);
                }
                else if (name == "refactorings")
                {
                    LoadRefactorings(child, settings);
                }
            }
        }

        private static void LoadPrefixFieldIdentifierWithUnderscore(XElement parent, ApplicationSettings settings)
        {
            XElement element = parent.Element("prefixFieldIdentifierWithUnderscore");

            if (element != null)
            {
                bool isEnabled;
                if (element.TryGetAttributeValueAsBoolean("isEnabled", out isEnabled))
                    settings.PrefixFieldIdentifierWithUnderscore = isEnabled;
            }
        }

        private static void LoadRefactorings(XElement element, ApplicationSettings settings)
        {
            foreach (XElement child in element.Elements("refactoring"))
                LoadRefactoring(child, settings);
        }

        private static void LoadRefactoring(XElement element, ApplicationSettings settings)
        {
            string id;
            if (element.TryGetAttributeValueAsString("id", out id))
            {
                bool isEnabled;
                if (element.TryGetAttributeValueAsBoolean("isEnabled", out isEnabled))
                    settings.Refactorings[id] = isEnabled;
            }
        }
    }
}
