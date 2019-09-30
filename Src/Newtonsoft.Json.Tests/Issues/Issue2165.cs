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
using System.IO;
#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json.Linq;
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
    public class Issue2165
    {
        [Test]
        public void Test_Deserializer()
        {
            ExceptionAssert.Throws<JsonWriterException>(
                () => JsonConvert.DeserializeObject<JObject>("{"),
                "Unexpected end when reading token. Path ''.");
        }

        [Test]
        public void Test()
        {
            StringWriter w = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(w);

            var jsonReader = new JsonTextReader(new StringReader("{"));
            jsonReader.Read();

            ExceptionAssert.Throws<JsonWriterException>(
                () => writer.WriteToken(jsonReader),
                "Unexpected end when reading token. Path ''.");
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40)
        [Test]
        public async Task TestAsync()
        {
            StringWriter w = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(w);

            var jsonReader = new JsonTextReader(new StringReader("{"));
            await jsonReader.ReadAsync();

            await ExceptionAssert.ThrowsAsync<JsonWriterException>(
                () => writer.WriteTokenAsync(jsonReader),
                "Unexpected end when reading token. Path ''.");
        }
#endif
    }
}
