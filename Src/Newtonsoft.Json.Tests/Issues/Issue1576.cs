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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
    public class Issue1576 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            var settings = new JsonSerializerSettings()
            {
                ContractResolver = new CustomContractResolver()
            };

            var result = JsonConvert.DeserializeObject<TestClass>("{ 'Items': '11' }", settings);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Items.Count, 1);
            Assert.AreEqual(result.Items[0], 11);
        }

        [Test]
        public void Test_WithJsonConverterAttribute()
        {
            var result = JsonConvert.DeserializeObject<TestClassWithJsonConverter>("{ 'Items': '11' }");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Items.Count, 1);
            Assert.AreEqual(result.Items[0], 11);
        }

        public class TestClass
        {
            public List<int> Items { get; } = new List<int>();
        }

        public class TestClassWithJsonConverter
        {
            [JsonConverter(typeof(OneItemListJsonConverter))]
            public List<int> Items { get; } = new List<int>();
        }

        public class CustomContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (member.Name == "Items")
                {
                    property.Converter = new OneItemListJsonConverter();
                }

                return property;
            }
        }

        public class OneItemListJsonConverter : JsonConverter
        {
            public override bool CanWrite => false;

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotSupportedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                var token = JToken.Load(reader);
                if (token.Type == JTokenType.Array)
                {
                    return token.ToObject(objectType, serializer);
                }

                var array = new JArray();
                array.Add(token);

                var list = array.ToObject(objectType, serializer) as IEnumerable;
                var existing = existingValue as IList;

                if (list != null && existing != null)
                {
                    foreach (var item in list)
                    {
                        existing.Add(item);
                    }
                }

                return list;
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(ICollection).IsAssignableFrom(objectType);
            }
        }

    }
}