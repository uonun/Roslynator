﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Xml.Linq;

namespace Roslynator.VisualStudio.Settings
{
    internal static class XElementExtensions
    {
        public static bool TryGetAttributeValueAsString(this XElement element, XName name, out string value)
        {
            XAttribute x = element.Attributes(name).FirstOrDefault();

            if (x != null)
            {
                value = x.Value;
                return true;
            }

            value = null;
            return false;
        }

        public static bool TryGetAttributeValueAsBoolean(this XElement element, XName name, out bool value)
        {
            string s = null;

            if (TryGetAttributeValueAsString(element, name, out s))
            {
                bool result = false;
                if (bool.TryParse(s, out result))
                {
                    value = result;
                    return true;
                }
            }

            value = false;
            return false;
        }
    }
}
