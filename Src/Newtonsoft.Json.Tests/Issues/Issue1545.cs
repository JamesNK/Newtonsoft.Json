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
        public void Test_Populate()
        {
            string json = @"{
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

            Simple s = JsonConvert.DeserializeObject<Simple>(json);
            Assert.AreEqual(2, s.Array.Length);
            Assert.AreEqual("item1", s.Array[0].Value);
            Assert.AreEqual("item2", s.Array[1].Value);
        }

        [Test]
        public void Test_Multidimensional()
        {
            string json = @"[
                /* comment0 */
                [1,2,3],
                /* comment1 */
                [
                    /* comment2 */
                    4,
                    /* comment3 */
                    5,
                    /* comment4 */
                    6
                ]
                /* comment5 */
            ]";

            int[,] s = JsonConvert.DeserializeObject<int[,]>(json);
            Assert.AreEqual(6, s.Length);
            Assert.AreEqual(1, s[0, 0]);
            Assert.AreEqual(2, s[0, 1]);
            Assert.AreEqual(3, s[0, 2]);
            Assert.AreEqual(4, s[1, 0]);
            Assert.AreEqual(5, s[1, 1]);
            Assert.AreEqual(6, s[1, 2]);
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

            JsonLineInfo lineInfoObject = Activator.CreateInstance(objectType) as JsonLineInfo;
            serializer.Populate(reader, lineInfoObject);

            IJsonLineInfo jsonLineInfo = reader as IJsonLineInfo;
            if (jsonLineInfo != null && jsonLineInfo.HasLineInfo())
            {
                lineInfoObject.LineNumber = jsonLineInfo.LineNumber;
                lineInfoObject.LinePosition = jsonLineInfo.LinePosition;
            }

            return lineInfoObject;
        }
    }
}
