﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;

#pragma warning disable RCS1016, RCS1163

namespace Roslynator.CSharp.Analyzers.Test
{
    internal static class UseCSharp6DictionaryInitializer
    {
        public static void GetValue()
        {
            var dic = new Dictionary<int, string>() { { 0, "0" } };

            dic = new Dictionary<int, string>()
            {
                { 0, "0" },
                { 0, "1" }
            };

            dic = new Dictionary<int, string>() { [0] = null };

            var items = new List<string>() { { null } };
        }

        private class Foo<TKey, TItem> : IEnumerable<int>
        {
            public TItem this[TKey key]
            {
                get { return default(TItem); }
                //private set { }
            }

            public void Add(string key, string value)
            {
            }

            public IEnumerator<int> GetEnumerator() => null;
            IEnumerator IEnumerable.GetEnumerator() => null;
        }

        private static class Bar
        {
            public static void Method()
            {
                var q = new Foo<int, int>() { { "key", "value" } };

                var q2 = new Foo<int, string>() { { "key", "value" } };

                var q3 = new Foo<string, string>() { { "key", "value" } };
            }
        }
    }
}
