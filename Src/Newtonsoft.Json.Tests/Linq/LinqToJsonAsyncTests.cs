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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using System.Linq;
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class LinqToJsonAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task CommentsAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(4, a.Count);
            Assert.AreEqual(JTokenType.Comment, a[0].Type);
            Assert.AreEqual(" hi", ((JValue)a[0]).Value);
        }

        [Test]
        public async Task CommentsAndReadFrom_IgnoreCommentsAsync()
        {
            StringReader textReader = new StringReader(@"[
    // hi
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)await JToken.ReadFromAsync(jsonReader);

            Assert.AreEqual(3, a.Count);
            Assert.AreEqual(JTokenType.Integer, a[0].Type);
            Assert.AreEqual(1L, ((JValue)a[0]).Value);
        }

        [Test]
        public async Task StartingCommentAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Load
            });

            Assert.AreEqual(JTokenType.Comment, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(5, lineInfo.LinePosition);
        }

        [Test]
        public async Task StartingCommentAndReadFrom_IgnoreCommentsAsync()
        {
            StringReader textReader = new StringReader(@"
// hi
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JArray a = (JArray)await JToken.ReadFromAsync(jsonReader, new JsonLoadSettings
            {
                CommentHandling = CommentHandling.Ignore
            });

            Assert.AreEqual(JTokenType.Array, a.Type);

            IJsonLineInfo lineInfo = a;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(3, lineInfo.LineNumber);
            Assert.AreEqual(1, lineInfo.LinePosition);
        }

        [Test]
        public async Task StartingUndefinedAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"
undefined
[
    1,
    2,
    3
]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            JValue v = (JValue)await JToken.ReadFromAsync(jsonReader);

            Assert.AreEqual(JTokenType.Undefined, v.Type);

            IJsonLineInfo lineInfo = v;
            Assert.AreEqual(true, lineInfo.HasLineInfo());
            Assert.AreEqual(2, lineInfo.LineNumber);
            Assert.AreEqual(9, lineInfo.LinePosition);
        }

        [Test]
        public async Task StartingEndArrayAndReadFromAsync()
        {
            StringReader textReader = new StringReader(@"[]");

            JsonTextReader jsonReader = new JsonTextReader(textReader);
            await jsonReader.ReadAsync();
            await jsonReader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => await JToken.ReadFromAsync(jsonReader), @"Error reading JToken from JsonReader. Unexpected token: EndArray. Path '', line 1, position 2.");
        }
    }
}

#endif