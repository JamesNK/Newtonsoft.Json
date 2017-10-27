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
#if !(NET20 || DNXCORE50)
using System.Data.Linq;
#endif
#if !DNXCORE50
using System.Data.SqlTypes;
#endif
using System.Text;
using Newtonsoft.Json.Converters;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class GenericJsonConverterTests : TestFixtureBase
    {
        public class TestGenericConverter : JsonConverter<string>
        {
            public override void WriteJson(JsonWriter writer, string value, JsonSerializer serializer)
            {
                writer.WriteValue(value);
            }

            public override string ReadJson(JsonReader reader, Type objectType, string existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                return (string)reader.Value + existingValue;
            }
        }

        [Test]
        public void WriteJsonObject()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            TestGenericConverter converter = new TestGenericConverter();
            converter.WriteJson(jsonWriter, (object)"String!", null);

            Assert.AreEqual(@"""String!""", sw.ToString());
        }

        [Test]
        public void WriteJsonGeneric()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            TestGenericConverter converter = new TestGenericConverter();
            converter.WriteJson(jsonWriter, "String!", null);

            Assert.AreEqual(@"""String!""", sw.ToString());
        }

        [Test]
        public void WriteJsonBadType()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(sw);

            TestGenericConverter converter = new TestGenericConverter();

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                converter.WriteJson(jsonWriter, 123, null);
            }, "Converter cannot write specified value to JSON. System.String is required.");
        }

        [Test]
        public void ReadJsonGenericExistingValueNull()
        {
            StringReader sr = new StringReader("'String!'");
            JsonTextReader jsonReader = new JsonTextReader(sr);
            jsonReader.Read();

            TestGenericConverter converter = new TestGenericConverter();
            string s = converter.ReadJson(jsonReader, typeof(string), null, false, null);

            Assert.AreEqual(@"String!", s);
        }

        [Test]
        public void ReadJsonGenericExistingValueString()
        {
            StringReader sr = new StringReader("'String!'");
            JsonTextReader jsonReader = new JsonTextReader(sr);
            jsonReader.Read();

            TestGenericConverter converter = new TestGenericConverter();
            string s = converter.ReadJson(jsonReader, typeof(string), "Existing!", true, null);

            Assert.AreEqual(@"String!Existing!", s);
        }

        [Test]
        public void ReadJsonObjectExistingValueNull()
        {
            StringReader sr = new StringReader("'String!'");
            JsonTextReader jsonReader = new JsonTextReader(sr);
            jsonReader.Read();

            TestGenericConverter converter = new TestGenericConverter();
            string s = (string)converter.ReadJson(jsonReader, typeof(string), null, null);

            Assert.AreEqual(@"String!", s);
        }

        [Test]
        public void ReadJsonObjectExistingValueWrongType()
        {
            StringReader sr = new StringReader("'String!'");
            JsonTextReader jsonReader = new JsonTextReader(sr);
            jsonReader.Read();

            TestGenericConverter converter = new TestGenericConverter();

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                converter.ReadJson(jsonReader, typeof(string), 12345, null);
            }, "Converter cannot read JSON with the specified existing value. System.String is required.");
        }
    }
}