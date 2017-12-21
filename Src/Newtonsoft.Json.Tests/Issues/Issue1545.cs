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
#if DNXCORE50
using System.Reflection;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1545 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            var json = @"{
                ""array"": [
                    /* comment0 */
                    {
                        ""value"": ""item1""
                    },
                    /* comment1 */
                    {
                        ""value"": ""item2""
                    }
                    /* comment2 */
                ]
            }";
            var error = false;

            try
            {
                JsonConvert.DeserializeObject<Simple>(json);
            }
            catch
            {
                error = true;
            }

            Assert.IsFalse(error);
        }
    }

    public class Simple
    {
        [JsonProperty(Required = Required.Always)]
        public SimpleObject[] Array { get; set; }
    }

    [JsonConverter(typeof(LineInfoConverter))]
    public class SimpleObject : JsonLineInfo
    {
        public string Value { get; set; }
    }

    public class JsonLineInfo
    {
        [JsonIgnore]
        public int? LineNumber { get; set; }

        [JsonIgnore]
        public int? LinePosition { get; set; }
    }

    public class LineInfoConverter : JsonConverter
    {
        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException("Converter is not writable. Method should not be invoked");
        }

        public override bool CanConvert(Type objectType)
        {
#if DNXCORE50
            return typeof(JsonLineInfo).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
#else
            return typeof(JsonLineInfo).IsAssignableFrom(objectType);
#endif
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                return null;
            }

            var lineInfoObject = Activator.CreateInstance(objectType) as JsonLineInfo;
            serializer.Populate(reader, lineInfoObject);

            var jsonLineInfo = reader as IJsonLineInfo;
            if (jsonLineInfo != null && jsonLineInfo.HasLineInfo())
            {
                lineInfoObject.LineNumber = jsonLineInfo.LineNumber;
                lineInfoObject.LinePosition = jsonLineInfo.LinePosition;
            }

            return lineInfoObject;
        }
    }
}
