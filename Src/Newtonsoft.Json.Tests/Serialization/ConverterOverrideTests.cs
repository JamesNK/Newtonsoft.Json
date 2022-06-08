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
using System.Reflection;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;


namespace Newtonsoft.Json.Tests.Serialization
{
    public class ConverterOverrideTests : TestFixtureBase
    {
        public ConverterOverrideTests()
        {
            SerializerSettings = new JsonSerializerSettings()
            {
                ContractResolver = new OverrideContractResolver(),
                Converters = new List<JsonConverter>() { new StringEnumConverter(new DefaultNamingStrategy(), allowIntegerValues: false) }
            };
        }

        internal JsonSerializerSettings SerializerSettings { get; set; }

        private enum TestEnum
        {
            Foo,
            Bar
        }

        private class TestClass
        {
            public TestEnum Enumeration { get; set; }
        }

        public class OverridingJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => true;
            public override bool CanRead => false;
            
            public override bool CanWrite => false;
            
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        public class OverrideContractResolver : DefaultContractResolver
        {
            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);

                // Simulates situational application of a converter to a property
                property.Converter = new OverridingJsonConverter();

                return property;
            }
        }

        /// <summary>
        /// <see cref="OverrideContractResolver"/> shouldn't override <see cref="TestClass"/>'s <see cref="StringEnumConverter"/> during
        /// serialization, since <see cref="OverridingJsonConverter.CanWrite"/> is false.
        /// </summary>
        [Test]
        public void ShouldNotOverrideConverterDuringSerialization()
        {
            var obj = new TestClass()
            {
                Enumeration = TestEnum.Bar
            };

            var serializedObj = JsonConvert.SerializeObject(obj, SerializerSettings);
            var targetStr = $"{{\"{nameof(TestClass.Enumeration)}\":\"{nameof(TestEnum.Bar)}\"}}";
            Assert.AreEqual(targetStr, serializedObj);
        }

        /// <summary>
        /// <see cref="OverrideContractResolver"/> shouldn't override <see cref="TestClass"/>'s <see cref="StringEnumConverter"/> during
        /// deserialization, since <see cref="OverridingJsonConverter.CanRead"/> is false.
        /// </summary>

        [Test]
        public void ShouldNotOverrideConverterDuringDeserialization()
        {
            var rawStr = $"{{\"{nameof(TestClass.Enumeration)}\":{(int)TestEnum.Bar}}}";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<TestClass>(rawStr, SerializerSettings);
            });
        }
    }
}