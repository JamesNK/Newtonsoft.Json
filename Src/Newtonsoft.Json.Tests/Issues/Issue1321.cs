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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
#if !NET20
using System.Xml.Linq;
#endif
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1321 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                JsonConvert.DeserializeObject(
                    @"[""1"",",
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.None, MaxDepth = 1024 });
            }, "Unexpected end when reading token. Path ''.");
        }

        [Test]
        public void Test2()
        {
            JArray a = new JArray();

            var writer = a.CreateWriter();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[""1"","));

            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                writer.WriteToken(reader);
            }, "Unexpected end when reading token. Path ''.");
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40)
        [Test]
        public async Task Test2_Async()
        {
            JArray a = new JArray();

            var writer = a.CreateWriter();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[""1"","));

            await ExceptionAssert.ThrowsAsync<JsonWriterException>(async () =>
            {
                await writer.WriteTokenAsync(reader);
            }, "Unexpected end when reading token. Path ''.");
        }
#endif

        [Test]
        public void Test3()
        {
            JArray a = new JArray();

            var writer = a.CreateWriter();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[""1"","));
            reader.Read();

            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                writer.WriteToken(reader);
            }, "Unexpected end when reading token. Path ''.");
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40)
        [Test]
        public async Task Test3_Async()
        {
            JArray a = new JArray();

            var writer = a.CreateWriter();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[""1"","));
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonWriterException>(async () =>
            {
                await writer.WriteTokenAsync(reader);
            }, "Unexpected end when reading token. Path ''.");
        }
#endif

        [Test]
        public void Test4()
        {
            JArray a = new JArray();

            var writer = a.CreateWriter();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[[""1"","));
            reader.Read();
            reader.Read();

            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                writer.WriteToken(reader);
            }, "Unexpected end when reading token. Path ''.");
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40)
        [Test]
        public async Task Test4_Async()
        {
            JArray a = new JArray();

            var writer = a.CreateWriter();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[[""1"","));
            await reader.ReadAsync();
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonWriterException>(async () =>
            {
                await writer.WriteTokenAsync(reader);
            }, "Unexpected end when reading token. Path ''.");
        }
#endif

        [Test]
        public void Test5()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartArray();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[[""1"","));
            reader.Read();
            reader.Read();

            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                writer.WriteToken(reader);
            }, "Unexpected end when reading token. Path '[0]'.");
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40)
        [Test]
        public async Task Test5_Async()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartArray();

            JsonTextReader reader = new JsonTextReader(new StringReader(@"[[""1"","));
            await reader.ReadAsync();
            await reader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonWriterException>(async () =>
            {
                await writer.WriteTokenAsync(reader);
            }, "Unexpected end when reading token. Path '[0]'.");
        }
#endif
    }
}