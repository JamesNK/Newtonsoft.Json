#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if HAVE_BENCHMARKS

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Utilities;
#if !PORTABLE || NETSTANDARD2_0
using MemberTypes = System.Reflection.MemberTypes;
using BindingFlags = System.Reflection.BindingFlags;
#else
using MemberTypes = Newtonsoft.Json.Utilities.MemberTypes;
using BindingFlags = Newtonsoft.Json.Utilities.BindingFlags;
#endif

namespace Newtonsoft.Json.Tests.Benchmarks
{
    public class LowLevelBenchmarks
    {
        private const string FloatText = "123.123";
        private static readonly char[] FloatChars = FloatText.ToCharArray();

        private static readonly Dictionary<string, object> NormalDictionary = new Dictionary<string, object>();

        private static readonly ConcurrentDictionary<string, object> ConcurrentDictionary = new ConcurrentDictionary<string, object>();

        static LowLevelBenchmarks()
        {
            for (int i = 0; i < 10; i++)
            {
                string key = i.ToString();
                object value = new object();

                NormalDictionary.Add(key, value);
                ConcurrentDictionary.TryAdd(key, value);
            }
        }

        [Benchmark]
        public void DictionaryGet()
        {
            NormalDictionary.TryGetValue("1", out object _);
        }

        [Benchmark]
        public void ConcurrentDictionaryGet()
        {
            ConcurrentDictionary.TryGetValue("1", out object _);
        }

        [Benchmark]
        public void ConcurrentDictionaryGetOrCreate()
        {
            ConcurrentDictionary.GetOrAdd("1", Dummy);
        }

        private object Dummy(string arg)
        {
            throw new Exception("Should never get here.");
        }

        [Benchmark]
        public void DecimalTryParseString()
        {
            decimal value;
            decimal.TryParse(FloatText, NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture, out value);
        }

        [Benchmark]
        public void GetMemberWithMemberTypeAndBindingFlags()
        {
            typeof(LowLevelBenchmarks).GetMember("AName", MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [Benchmark]
        public void GetPropertyGetField()
        {
            typeof(LowLevelBenchmarks).GetProperty("AName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            typeof(LowLevelBenchmarks).GetField("AName", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }

        [Benchmark]
        public void DecimalTryParseChars()
        {
            decimal value;
            ConvertUtils.DecimalTryParse(FloatChars, 0, FloatChars.Length, out value);
        }

        [Benchmark]
        public void WriteEscapedJavaScriptString()
        {
            string text = @"The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.

The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.

The general form of an HTML element is therefore: <tag attribute1=""value1"" attribute2=""value2"">content</tag>.
Some HTML elements are defined as empty elements and take the form <tag attribute1=""value1"" attribute2=""value2"" >.
Empty elements may enclose no content, for instance, the BR tag or the inline IMG tag.
The name of an HTML element is the name used in the tags.
Note that the end tag's name is preceded by a slash character, ""/"", and that in empty elements the end tag is neither required nor allowed.
If attributes are not mentioned, default values are used in each case.
";

            using (StringWriter w = StringUtils.CreateStringWriter(text.Length))
            {
                char[] buffer = null;
                JavaScriptUtils.WriteEscapedJavaScriptString(w, text, '"', true, JavaScriptUtils.DoubleQuoteCharEscapeFlags, StringEscapeHandling.Default, null, ref buffer);
            }
        }
    }
}

#endif