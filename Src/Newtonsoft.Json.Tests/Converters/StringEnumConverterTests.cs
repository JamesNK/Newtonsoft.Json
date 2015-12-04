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
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class StringEnumConverterTests : TestFixtureBase
    {
        public class EnumClass
        {
            public StoreColor StoreColor { get; set; }
            public StoreColor? NullableStoreColor1 { get; set; }
            public StoreColor? NullableStoreColor2 { get; set; }
        }

        public class EnumContainer<T>
        {
            public T Enum { get; set; }
        }

        [Flags]
        public enum FlagsTestEnum
        {
            Default = 0,
            First = 1,
            Second = 2
        }

        public enum NegativeEnum
        {
            Negative = -1,
            Zero = 0,
            Positive = 1
        }

#if !NET20
        public enum NamedEnum
        {
            [EnumMember(Value = "@first")]
            First,

            [EnumMember(Value = "@second")]
            Second,
            Third
        }

        public enum NamedEnumDuplicate
        {
            [EnumMember(Value = "Third")]
            First,

            [EnumMember(Value = "@second")]
            Second,
            Third
        }
#endif

        public class NegativeEnumClass
        {
            public NegativeEnum Value1 { get; set; }
            public NegativeEnum Value2 { get; set; }
        }

#if !NET20
        [Test]
        public void NamedEnumDuplicateTest()
        {
            ExceptionAssert.Throws<Exception>(() =>
            {
                EnumContainer<NamedEnumDuplicate> c = new EnumContainer<NamedEnumDuplicate>
                {
                    Enum = NamedEnumDuplicate.First
                };

                JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
            }, "Enum name 'Third' already exists on enum 'NamedEnumDuplicate'.");
        }

        [Test]
        public void SerializeNameEnumTest()
        {
            EnumContainer<NamedEnum> c = new EnumContainer<NamedEnum>
            {
                Enum = NamedEnum.First
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
            StringAssert.AreEqual(@"{
  ""Enum"": ""@first""
}", json);

            c = new EnumContainer<NamedEnum>
            {
                Enum = NamedEnum.Third
            };

            json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
            StringAssert.AreEqual(@"{
  ""Enum"": ""Third""
}", json);
        }

        [Test]
        public void DeserializeNameEnumTest()
        {
            string json = @"{
  ""Enum"": ""@first""
}";

            EnumContainer<NamedEnum> c = JsonConvert.DeserializeObject<EnumContainer<NamedEnum>>(json, new StringEnumConverter());
            Assert.AreEqual(NamedEnum.First, c.Enum);

            json = @"{
  ""Enum"": ""Third""
}";

            c = JsonConvert.DeserializeObject<EnumContainer<NamedEnum>>(json, new StringEnumConverter());
            Assert.AreEqual(NamedEnum.Third, c.Enum);
        }
#endif

        [Test]
        public void SerializeEnumClass()
        {
            EnumClass enumClass = new EnumClass();
            enumClass.StoreColor = StoreColor.Red;
            enumClass.NullableStoreColor1 = StoreColor.White;
            enumClass.NullableStoreColor2 = null;

            string json = JsonConvert.SerializeObject(enumClass, Formatting.Indented, new StringEnumConverter());

            StringAssert.AreEqual(@"{
  ""StoreColor"": ""Red"",
  ""NullableStoreColor1"": ""White"",
  ""NullableStoreColor2"": null
}", json);
        }

        [Test]
        public void SerializeEnumClassWithCamelCase()
        {
            EnumClass enumClass = new EnumClass();
            enumClass.StoreColor = StoreColor.Red;
            enumClass.NullableStoreColor1 = StoreColor.DarkGoldenrod;
            enumClass.NullableStoreColor2 = null;

            string json = JsonConvert.SerializeObject(enumClass, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });

            StringAssert.AreEqual(@"{
  ""StoreColor"": ""red"",
  ""NullableStoreColor1"": ""darkGoldenrod"",
  ""NullableStoreColor2"": null
}", json);
        }

        [Test]
        public void SerializeEnumClassUndefined()
        {
            EnumClass enumClass = new EnumClass();
            enumClass.StoreColor = (StoreColor)1000;
            enumClass.NullableStoreColor1 = (StoreColor)1000;
            enumClass.NullableStoreColor2 = null;

            string json = JsonConvert.SerializeObject(enumClass, Formatting.Indented, new StringEnumConverter());

            StringAssert.AreEqual(@"{
  ""StoreColor"": 1000,
  ""NullableStoreColor1"": 1000,
  ""NullableStoreColor2"": null
}", json);
        }

        [Test]
        public void SerializeFlagEnum()
        {
            EnumClass enumClass = new EnumClass();
            enumClass.StoreColor = StoreColor.Red | StoreColor.White;
            enumClass.NullableStoreColor1 = StoreColor.White & StoreColor.Yellow;
            enumClass.NullableStoreColor2 = StoreColor.Red | StoreColor.White | StoreColor.Black;

            string json = JsonConvert.SerializeObject(enumClass, Formatting.Indented, new StringEnumConverter());

            StringAssert.AreEqual(@"{
  ""StoreColor"": ""Red, White"",
  ""NullableStoreColor1"": 0,
  ""NullableStoreColor2"": ""Black, Red, White""
}", json);
        }

        [Test]
        public void SerializeNegativeEnum()
        {
            NegativeEnumClass negativeEnumClass = new NegativeEnumClass();
            negativeEnumClass.Value1 = NegativeEnum.Negative;
            negativeEnumClass.Value2 = (NegativeEnum)int.MinValue;

            string json = JsonConvert.SerializeObject(negativeEnumClass, Formatting.Indented, new StringEnumConverter());

            StringAssert.AreEqual(@"{
  ""Value1"": ""Negative"",
  ""Value2"": -2147483648
}", json);
        }

        [Test]
        public void DeserializeNegativeEnum()
        {
            string json = @"{
  ""Value1"": ""Negative"",
  ""Value2"": -2147483648
}";

            NegativeEnumClass negativeEnumClass = JsonConvert.DeserializeObject<NegativeEnumClass>(json, new StringEnumConverter());

            Assert.AreEqual(NegativeEnum.Negative, negativeEnumClass.Value1);
            Assert.AreEqual((NegativeEnum)int.MinValue, negativeEnumClass.Value2);
        }

        [Test]
        public void DeserializeFlagEnum()
        {
            string json = @"{
  ""StoreColor"": ""Red, White"",
  ""NullableStoreColor1"": 0,
  ""NullableStoreColor2"": ""black, Red, White""
}";

            EnumClass enumClass = JsonConvert.DeserializeObject<EnumClass>(json, new StringEnumConverter());

            Assert.AreEqual(StoreColor.Red | StoreColor.White, enumClass.StoreColor);
            Assert.AreEqual((StoreColor)0, enumClass.NullableStoreColor1);
            Assert.AreEqual(StoreColor.Red | StoreColor.White | StoreColor.Black, enumClass.NullableStoreColor2);
        }

        [Test]
        public void DeserializeEnumClass()
        {
            string json = @"{
  ""StoreColor"": ""Red"",
  ""NullableStoreColor1"": ""White"",
  ""NullableStoreColor2"": null
}";

            EnumClass enumClass = JsonConvert.DeserializeObject<EnumClass>(json, new StringEnumConverter());

            Assert.AreEqual(StoreColor.Red, enumClass.StoreColor);
            Assert.AreEqual(StoreColor.White, enumClass.NullableStoreColor1);
            Assert.AreEqual(null, enumClass.NullableStoreColor2);
        }

        [Test]
        public void DeserializeEnumClassUndefined()
        {
            string json = @"{
  ""StoreColor"": 1000,
  ""NullableStoreColor1"": 1000,
  ""NullableStoreColor2"": null
}";

            EnumClass enumClass = JsonConvert.DeserializeObject<EnumClass>(json, new StringEnumConverter());

            Assert.AreEqual((StoreColor)1000, enumClass.StoreColor);
            Assert.AreEqual((StoreColor)1000, enumClass.NullableStoreColor1);
            Assert.AreEqual(null, enumClass.NullableStoreColor2);
        }

        [Test]
        public void CamelCaseTextFlagEnumSerialization()
        {
            EnumContainer<FlagsTestEnum> c = new EnumContainer<FlagsTestEnum>
            {
                Enum = FlagsTestEnum.First | FlagsTestEnum.Second
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });
            StringAssert.AreEqual(@"{
  ""Enum"": ""first, second""
}", json);
        }

        [Test]
        public void CamelCaseTextFlagEnumDeserialization()
        {
            string json = @"{
  ""Enum"": ""first, second""
}";

            EnumContainer<FlagsTestEnum> c = JsonConvert.DeserializeObject<EnumContainer<FlagsTestEnum>>(json, new StringEnumConverter { CamelCaseText = true });
            Assert.AreEqual(FlagsTestEnum.First | FlagsTestEnum.Second, c.Enum);
        }

        [Test]
        public void DeserializeEmptyStringIntoNullable()
        {
            string json = @"{
  ""StoreColor"": ""Red"",
  ""NullableStoreColor1"": ""White"",
  ""NullableStoreColor2"": """"
}";

            EnumClass c = JsonConvert.DeserializeObject<EnumClass>(json, new StringEnumConverter());
            Assert.IsNull(c.NullableStoreColor2);
        }

        [Test]
        public void DeserializeInvalidString()
        {
            string json = "{ \"Value\" : \"Three\" }";

            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new StringEnumConverter());
                serializer.Deserialize<Bucket>(new JsonTextReader(new StringReader(json)));
            }, @"Error converting value ""Three"" to type 'Newtonsoft.Json.Tests.Converters.StringEnumConverterTests+MyEnum'. Path 'Value', line 1, position 19.");
        }

        public class Bucket
        {
            public MyEnum Value;
        }

        public enum MyEnum
        {
            Alpha,
            Beta,
        }

        [Test]
        public void DeserializeIntegerButNotAllowed()
        {
            string json = "{ \"Value\" : 123 }";

            try
            {
                var serializer = new JsonSerializer();
                serializer.Converters.Add(new StringEnumConverter { AllowIntegerValues = false });
                serializer.Deserialize<Bucket>(new JsonTextReader(new StringReader(json)));
            }
            catch (JsonSerializationException ex)
            {
                Assert.AreEqual("Error converting value 123 to type 'Newtonsoft.Json.Tests.Converters.StringEnumConverterTests+MyEnum'. Path 'Value', line 1, position 15.", ex.Message);
                Assert.AreEqual(@"Integer value 123 is not allowed. Path 'Value', line 1, position 15.", ex.InnerException.Message);

                return;
            }

            Assert.Fail();
        }

#if !NET20
        [Test]
        public void EnumMemberPlusFlags()
        {
            List<Foo> lfoo =
                new List<Foo>
                {
                    Foo.Bat | Foo.SerializeAsBaz,
                    Foo.FooBar,
                    Foo.Bat,
                    Foo.SerializeAsBaz,
                    Foo.FooBar | Foo.SerializeAsBaz,
                    (Foo)int.MaxValue
                };

            string json1 = JsonConvert.SerializeObject(lfoo, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });

            StringAssert.AreEqual(@"[
  ""Bat, baz"",
  ""foo_bar"",
  ""Bat"",
  ""baz"",
  ""foo_bar, baz"",
  2147483647
]", json1);

            IList<Foo> foos = JsonConvert.DeserializeObject<List<Foo>>(json1);

            Assert.AreEqual(6, foos.Count);
            Assert.AreEqual(Foo.Bat | Foo.SerializeAsBaz, foos[0]);
            Assert.AreEqual(Foo.FooBar, foos[1]);
            Assert.AreEqual(Foo.Bat, foos[2]);
            Assert.AreEqual(Foo.SerializeAsBaz, foos[3]);
            Assert.AreEqual(Foo.FooBar | Foo.SerializeAsBaz, foos[4]);
            Assert.AreEqual((Foo)int.MaxValue, foos[5]);

            List<Bar> lbar = new List<Bar>() { Bar.FooBar, Bar.Bat, Bar.SerializeAsBaz };

            string json2 = JsonConvert.SerializeObject(lbar, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });

            StringAssert.AreEqual(@"[
  ""foo_bar"",
  ""Bat"",
  ""baz""
]", json2);

            IList<Bar> bars = JsonConvert.DeserializeObject<List<Bar>>(json2);

            Assert.AreEqual(3, bars.Count);
            Assert.AreEqual(Bar.FooBar, bars[0]);
            Assert.AreEqual(Bar.Bat, bars[1]);
            Assert.AreEqual(Bar.SerializeAsBaz, bars[2]);
        }

        [Test]
        public void DuplicateNameEnumTest()
        {
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<DuplicateNameEnum>("'foo_bar'", new StringEnumConverter()),
                @"Error converting value ""foo_bar"" to type 'Newtonsoft.Json.Tests.Converters.DuplicateNameEnum'. Path '', line 1, position 9.");
        }

        // Define other methods and classes here
        [Flags]
        [JsonConverter(typeof(StringEnumConverter))]
        private enum Foo
        {
            [EnumMember(Value = "foo_bar")]
            FooBar = 0x01,
            Bat = 0x02,

            [EnumMember(Value = "baz")]
            SerializeAsBaz = 0x4,
        }

        [JsonConverter(typeof(StringEnumConverter))]
        private enum Bar
        {
            [EnumMember(Value = "foo_bar")]
            FooBar,
            Bat,

            [EnumMember(Value = "baz")]
            SerializeAsBaz
        }

        [Test]
        public void DataContractSerializerDuplicateNameEnumTest()
        {
            MemoryStream ms = new MemoryStream();
            var s = new DataContractSerializer(typeof(DuplicateEnumNameTestClass));

            ExceptionAssert.Throws<InvalidDataContractException>(() =>
            {
                s.WriteObject(ms, new DuplicateEnumNameTestClass
                {
                    Value = DuplicateNameEnum.foo_bar,
                    Value2 = DuplicateNameEnum2.foo_bar_NOT_USED
                });

                string xml = @"<DuplicateEnumNameTestClass xmlns=""http://schemas.datacontract.org/2004/07/Newtonsoft.Json.Tests.Converters"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
    <Value>foo_bar</Value>
    <Value2>foo_bar</Value2>
</DuplicateEnumNameTestClass>";

                var o = (DuplicateEnumNameTestClass)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(xml)));

                Assert.AreEqual(DuplicateNameEnum.foo_bar, o.Value);
                Assert.AreEqual(DuplicateNameEnum2.FooBar, o.Value2);
            }, "Type 'Newtonsoft.Json.Tests.Converters.DuplicateNameEnum' contains two members 'foo_bar' 'and 'FooBar' with the same name 'foo_bar'. Multiple members with the same name in one type are not supported. Consider changing one of the member names using EnumMemberAttribute attribute.");
        }
#endif
    }

#if !NET20
    [DataContract]
    public class DuplicateEnumNameTestClass
    {
        [DataMember]
        public DuplicateNameEnum Value { get; set; }

        [DataMember]
        public DuplicateNameEnum2 Value2 { get; set; }
    }

    [DataContract]
    public enum DuplicateNameEnum
    {
        [EnumMember]
        first = 0,

        [EnumMember]
        foo_bar = 1,

        [EnumMember(Value = "foo_bar")]
        FooBar = 2,

        [EnumMember]
        foo_bar_NOT_USED = 3
    }

    [DataContract]
    public enum DuplicateNameEnum2
    {
        [EnumMember]
        first = 0,

        [EnumMember(Value = "foo_bar")]
        FooBar = 1,

        [EnumMember]
        foo_bar = 2,

        [EnumMember(Value = "TEST")]
        foo_bar_NOT_USED = 3
    }
#endif
}