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
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#if DNXCORE50
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

        [Flags]
        public enum NegativeFlagsEnum
        {
            NegativeFour = -4,
            NegativeTwo = -2,
            NegativeOne = -1,
            Zero = 0,
            One = 1,
            Two = 2,
            Four = 4
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

        public enum NamedEnumWithComma
        {
            [EnumMember(Value = "@first")]
            First,

            [EnumMember(Value = "@second")]
            Second,

            [EnumMember(Value = ",third")]
            Third,

            [EnumMember(Value = ",")]
            JustComma
        }
#endif

        public class NegativeEnumClass
        {
            public NegativeEnum Value1 { get; set; }
            public NegativeEnum Value2 { get; set; }
        }

        public class NegativeFlagsEnumClass
        {
            public NegativeFlagsEnum Value1 { get; set; }
            public NegativeFlagsEnum Value2 { get; set; }
        }

        [JsonConverter(typeof(StringEnumConverter), true)]
        public enum CamelCaseEnumObsolete
        {
            This,
            Is,
            CamelCase
        }

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public enum CamelCaseEnumNew
        {
            This,
            Is,
            CamelCase
        }

        [JsonConverter(typeof(StringEnumConverter), typeof(SnakeCaseNamingStrategy))]
        public enum SnakeCaseEnumNew
        {
            This,
            Is,
            SnakeCase
        }

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy), new object[0], false)]
        public enum NotAllowIntegerValuesEnum
        {
            Foo = 0,
            Bar = 1
        }

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy))]
        public enum AllowIntegerValuesEnum
        {
            Foo = 0,
            Bar = 1
        }

        [JsonConverter(typeof(StringEnumConverter), typeof(CamelCaseNamingStrategy), null)]
        public enum NullArgumentInAttribute
        {
            Foo = 0,
            Bar = 1
        }

        [Test]
        public void Serialize_CamelCaseFromAttribute_Obsolete()
        {
            string json = JsonConvert.SerializeObject(CamelCaseEnumObsolete.CamelCase);
            Assert.AreEqual(@"""camelCase""", json);
        }

        [Test]
        public void NamingStrategyAndCamelCaseText()
        {
            StringEnumConverter converter = new StringEnumConverter();
            Assert.IsNull(converter.NamingStrategy);

#pragma warning disable CS0618 // Type or member is obsolete
            converter.CamelCaseText = true;
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.IsNotNull(converter.NamingStrategy);
            Assert.AreEqual(typeof(CamelCaseNamingStrategy), converter.NamingStrategy.GetType());

            var camelCaseInstance = converter.NamingStrategy;
#pragma warning disable CS0618 // Type or member is obsolete
            converter.CamelCaseText = true;
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.AreEqual(camelCaseInstance, converter.NamingStrategy);

            converter.NamingStrategy = null;
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsFalse(converter.CamelCaseText);
#pragma warning restore CS0618 // Type or member is obsolete

            converter.NamingStrategy = new CamelCaseNamingStrategy();
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsTrue(converter.CamelCaseText);
#pragma warning restore CS0618 // Type or member is obsolete

            converter.NamingStrategy = new SnakeCaseNamingStrategy();
#pragma warning disable CS0618 // Type or member is obsolete
            Assert.IsFalse(converter.CamelCaseText);
#pragma warning restore CS0618 // Type or member is obsolete

#pragma warning disable CS0618 // Type or member is obsolete
            converter.CamelCaseText = false;
#pragma warning restore CS0618 // Type or member is obsolete
            Assert.IsNotNull(converter.NamingStrategy);
            Assert.AreEqual(typeof(SnakeCaseNamingStrategy), converter.NamingStrategy.GetType());
        }

        [Test]
        public void StringEnumConverter_CamelCaseTextCtor()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            StringEnumConverter converter = new StringEnumConverter(true);
#pragma warning restore CS0618 // Type or member is obsolete

            Assert.IsNotNull(converter.NamingStrategy);
            Assert.AreEqual(typeof(CamelCaseNamingStrategy), converter.NamingStrategy.GetType());
            Assert.AreEqual(true, converter.AllowIntegerValues);
        }

        [Test]
        public void StringEnumConverter_NamingStrategyTypeCtor()
        {
            StringEnumConverter converter = new StringEnumConverter(typeof(CamelCaseNamingStrategy), new object[] { true, true, true }, false);

            Assert.IsNotNull(converter.NamingStrategy);
            Assert.AreEqual(typeof(CamelCaseNamingStrategy), converter.NamingStrategy.GetType());
            Assert.AreEqual(false, converter.AllowIntegerValues);
            Assert.AreEqual(true, converter.NamingStrategy.OverrideSpecifiedNames);
            Assert.AreEqual(true, converter.NamingStrategy.ProcessDictionaryKeys);
            Assert.AreEqual(true, converter.NamingStrategy.ProcessExtensionDataNames);
        }

        [Test]
        public void StringEnumConverter_NamingStrategyTypeCtor_Null()
        {
            ExceptionAssert.Throws<ArgumentNullException>(
                () => new StringEnumConverter(null),
                @"Value cannot be null.
Parameter name: namingStrategyType");
        }

        [Test]
        public void StringEnumConverter_NamingStrategyTypeWithArgsCtor_Null()
        {
            ExceptionAssert.Throws<ArgumentNullException>(
                () => new StringEnumConverter(null, new object[] { true, true, true }, false),
                @"Value cannot be null.
Parameter name: namingStrategyType");
        }

        [Test]
        public void Deserialize_CamelCaseFromAttribute_Obsolete()
        {
            CamelCaseEnumObsolete e = JsonConvert.DeserializeObject<CamelCaseEnumObsolete>(@"""camelCase""");
            Assert.AreEqual(CamelCaseEnumObsolete.CamelCase, e);
        }

        [Test]
        public void Serialize_CamelCaseFromAttribute()
        {
            string json = JsonConvert.SerializeObject(CamelCaseEnumNew.CamelCase);
            Assert.AreEqual(@"""camelCase""", json);
        }

        [Test]
        public void Deserialize_CamelCaseFromAttribute()
        {
            CamelCaseEnumNew e = JsonConvert.DeserializeObject<CamelCaseEnumNew>(@"""camelCase""");
            Assert.AreEqual(CamelCaseEnumNew.CamelCase, e);
        }

        [Test]
        public void Serialize_SnakeCaseFromAttribute()
        {
            string json = JsonConvert.SerializeObject(SnakeCaseEnumNew.SnakeCase);
            Assert.AreEqual(@"""snake_case""", json);
        }

        [Test]
        public void Deserialize_SnakeCaseFromAttribute()
        {
            SnakeCaseEnumNew e = JsonConvert.DeserializeObject<SnakeCaseEnumNew>(@"""snake_case""");
            Assert.AreEqual(SnakeCaseEnumNew.SnakeCase, e);
        }

        [Test]
        public void Deserialize_NotAllowIntegerValuesFromAttribute()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                NotAllowIntegerValuesEnum e = JsonConvert.DeserializeObject<NotAllowIntegerValuesEnum>(@"""9""");
            });
        }

        [Test]
        public void CannotPassNullArgumentToConverter()
        {
            var ex = ExceptionAssert.Throws<JsonException>(() =>
            {
                JsonConvert.DeserializeObject<NullArgumentInAttribute>(@"""9""");
            });

            Assert.AreEqual("Cannot pass a null parameter to the constructor.", ex.InnerException.Message);
        }

        [Test]
        public void Deserialize_AllowIntegerValuesAttribute()
        {
            AllowIntegerValuesEnum e = JsonConvert.DeserializeObject<AllowIntegerValuesEnum>(@"""9""");
            Assert.AreEqual(9, (int)e);
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
        public void NamedEnumCommaTest()
        {
            EnumContainer<NamedEnumWithComma> c = new EnumContainer<NamedEnumWithComma>
            {
                Enum = NamedEnumWithComma.Third
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
            StringAssert.AreEqual(@"{
  ""Enum"": "",third""
}", json);

            EnumContainer<NamedEnumWithComma> c2 = JsonConvert.DeserializeObject<EnumContainer<NamedEnumWithComma>>(json, new StringEnumConverter());
            Assert.AreEqual(NamedEnumWithComma.Third, c2.Enum);
        }

        [Test]
        public void NamedEnumCommaTest2()
        {
            EnumContainer<NamedEnumWithComma> c = new EnumContainer<NamedEnumWithComma>
            {
                Enum = NamedEnumWithComma.JustComma
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
            StringAssert.AreEqual(@"{
  ""Enum"": "",""
}", json);

            EnumContainer<NamedEnumWithComma> c2 = JsonConvert.DeserializeObject<EnumContainer<NamedEnumWithComma>>(json, new StringEnumConverter());
            Assert.AreEqual(NamedEnumWithComma.JustComma, c2.Enum);
        }

        [Test]
        public void NamedEnumCommaCaseInsensitiveTest()
        {
            EnumContainer<NamedEnumWithComma> c2 = JsonConvert.DeserializeObject<EnumContainer<NamedEnumWithComma>>(@"{""Enum"":"",THIRD""}", new StringEnumConverter());
            Assert.AreEqual(NamedEnumWithComma.Third, c2.Enum);
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
            EnumClass enumClass = new EnumClass()
            {
                StoreColor = StoreColor.Red,
                NullableStoreColor1 = StoreColor.White,
                NullableStoreColor2 = null
            };

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
            EnumClass enumClass = new EnumClass()
            {
                StoreColor = StoreColor.Red,
                NullableStoreColor1 = StoreColor.DarkGoldenrod,
                NullableStoreColor2 = null
            };

#pragma warning disable CS0618 // Type or member is obsolete
            string json = JsonConvert.SerializeObject(enumClass, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });
#pragma warning restore CS0618 // Type or member is obsolete

            StringAssert.AreEqual(@"{
  ""StoreColor"": ""red"",
  ""NullableStoreColor1"": ""darkGoldenrod"",
  ""NullableStoreColor2"": null
}", json);
        }

        [Test]
        public void SerializeEnumClassUndefined()
        {
            EnumClass enumClass = new EnumClass()
            {
                StoreColor = (StoreColor)1000,
                NullableStoreColor1 = (StoreColor)1000,
                NullableStoreColor2 = null
            };

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
            EnumClass enumClass = new EnumClass()
            {
                StoreColor = StoreColor.Red | StoreColor.White,
                NullableStoreColor1 = StoreColor.White & StoreColor.Yellow,
                NullableStoreColor2 = StoreColor.Red | StoreColor.White | StoreColor.Black
            };

            string json = JsonConvert.SerializeObject(enumClass, Formatting.Indented, new StringEnumConverter());

            StringAssert.AreEqual(@"{
  ""StoreColor"": ""Red, White"",
  ""NullableStoreColor1"": 0,
  ""NullableStoreColor2"": ""Black, Red, White""
}", json);
        }

        [Test]
        public void SerializeNegativeFlagsEnum()
        {
            NegativeFlagsEnumClass negativeEnumClass = new NegativeFlagsEnumClass();
            negativeEnumClass.Value1 = NegativeFlagsEnum.NegativeFour | NegativeFlagsEnum.NegativeTwo;
            negativeEnumClass.Value2 = NegativeFlagsEnum.Two | NegativeFlagsEnum.Four;

            string json = JsonConvert.SerializeObject(negativeEnumClass, Formatting.Indented, new StringEnumConverter());

            StringAssert.AreEqual(@"{
  ""Value1"": ""NegativeTwo"",
  ""Value2"": ""Two, Four""
}", json);
        }

        [Test]
        public void DeserializeNegativeFlagsEnum()
        {
            string json = @"{
  ""Value1"": ""NegativeFour,NegativeTwo"",
  ""Value2"": ""NegativeFour,Four""
}";

            NegativeFlagsEnumClass negativeEnumClass = JsonConvert.DeserializeObject<NegativeFlagsEnumClass>(json, new StringEnumConverter());

            Assert.AreEqual(NegativeFlagsEnum.NegativeFour | NegativeFlagsEnum.NegativeTwo, negativeEnumClass.Value1);
            Assert.AreEqual(NegativeFlagsEnum.NegativeFour | NegativeFlagsEnum.Four, negativeEnumClass.Value2);
        }

        [Test]
        public void SerializeNegativeEnum()
        {
            NegativeEnumClass negativeEnumClass = new NegativeEnumClass()
            {
                Value1 = NegativeEnum.Negative,
                Value2 = (NegativeEnum)int.MinValue
            };

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

#pragma warning disable CS0618 // Type or member is obsolete
            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });
#pragma warning restore CS0618 // Type or member is obsolete
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

#pragma warning disable CS0618 // Type or member is obsolete
            EnumContainer<FlagsTestEnum> c = JsonConvert.DeserializeObject<EnumContainer<FlagsTestEnum>>(json, new StringEnumConverter { CamelCaseText = true });
#pragma warning restore CS0618 // Type or member is obsolete
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

#pragma warning disable CS0618 // Type or member is obsolete
            string json1 = JsonConvert.SerializeObject(lfoo, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });
#pragma warning restore CS0618 // Type or member is obsolete

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

#pragma warning disable CS0618 // Type or member is obsolete
            string json2 = JsonConvert.SerializeObject(lbar, Formatting.Indented, new StringEnumConverter { CamelCaseText = true });
#pragma warning restore CS0618 // Type or member is obsolete

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

        [Test]
        public void EnumMemberWithNumbers()
        {
            StringEnumConverter converter = new StringEnumConverter();

            NumberNamesEnum e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"1\"", converter);

            Assert.AreEqual(NumberNamesEnum.second, e);

            e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"2\"", converter);

            Assert.AreEqual(NumberNamesEnum.first, e);

            e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"3\"", converter);

            Assert.AreEqual(NumberNamesEnum.third, e);

            e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"-4\"", converter);

            Assert.AreEqual(NumberNamesEnum.fourth, e);
        }

        [Test]
        public void EnumMemberWithNumbers_NoIntegerValues()
        {
            StringEnumConverter converter = new StringEnumConverter { AllowIntegerValues = false };

            NumberNamesEnum e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"1\"", converter);

            Assert.AreEqual(NumberNamesEnum.second, e);

            e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"2\"", converter);

            Assert.AreEqual(NumberNamesEnum.first, e);

            e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"3\"", converter);

            Assert.AreEqual(NumberNamesEnum.third, e);

            e = JsonConvert.DeserializeObject<NumberNamesEnum>("\"-4\"", converter);

            Assert.AreEqual(NumberNamesEnum.fourth, e);
        }
#endif

        [Test]
        public void AllowIntegerValueAndStringNumber()
        {
            JsonSerializationException ex = ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<StoreColor>("\"1\"", new StringEnumConverter { AllowIntegerValues = false });
            });

            Assert.AreEqual("Integer string '1' is not allowed.", ex.InnerException.Message);
        }

        [Test]
        public void AllowIntegerValueAndNegativeStringNumber()
        {
            JsonSerializationException ex = ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<StoreColor>("\"-1\"", new StringEnumConverter { AllowIntegerValues = false });
            });

            Assert.AreEqual("Integer string '-1' is not allowed.", ex.InnerException.Message);
        }

        [Test]
        public void AllowIntegerValueAndPositiveStringNumber()
        {
            JsonSerializationException ex = ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<StoreColor>("\"+1\"", new StringEnumConverter { AllowIntegerValues = false });
            });

            Assert.AreEqual("Integer string '+1' is not allowed.", ex.InnerException.Message);
        }

        [Test]
        public void AllowIntegerValueAndDash()
        {
            JsonSerializationException ex = ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.DeserializeObject<StoreColor>("\"-\"", new StringEnumConverter { AllowIntegerValues = false });
            });

            Assert.AreEqual("Requested value '-' was not found.", ex.InnerException.Message);
        }

        [Test]
        public void AllowIntegerValueAndNonNamedValue()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() =>
            {
                JsonConvert.SerializeObject((StoreColor)999, new StringEnumConverter { AllowIntegerValues = false });
            }, "Integer value 999 is not allowed. Path ''.");
        }

        public enum EnumWithDifferentCases
        {
            M,
            m
        }

        [Test]
        public void SerializeEnumWithDifferentCases()
        {
            string json = JsonConvert.SerializeObject(EnumWithDifferentCases.M, new StringEnumConverter());

            Assert.AreEqual(@"""M""", json);

            json = JsonConvert.SerializeObject(EnumWithDifferentCases.m, new StringEnumConverter());

            Assert.AreEqual(@"""m""", json);
        }

        [Test]
        public void DeserializeEnumWithDifferentCases()
        {
            EnumWithDifferentCases e = JsonConvert.DeserializeObject<EnumWithDifferentCases>(@"""M""", new StringEnumConverter());
            Assert.AreEqual(EnumWithDifferentCases.M, e);

            e = JsonConvert.DeserializeObject<EnumWithDifferentCases>(@"""m""", new StringEnumConverter());
            Assert.AreEqual(EnumWithDifferentCases.m, e);
        }

#if !NET20
        [JsonConverter(typeof(StringEnumConverter))]
        public enum EnumMemberDoesNotMatchName
        {
            [EnumMember(Value = "first_value")]
            First
        }

        [Test]
        public void DeserializeEnumCaseIncensitive_ByEnumMemberValue_UpperCase()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberDoesNotMatchName>(@"""FIRST_VALUE""", new StringEnumConverter());
            Assert.AreEqual(EnumMemberDoesNotMatchName.First, e);
        }

        [Test]
        public void DeserializeEnumCaseIncensitive_ByEnumMemberValue_MixedCase()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberDoesNotMatchName>(@"""First_Value""", new StringEnumConverter());
            Assert.AreEqual(EnumMemberDoesNotMatchName.First, e);
        }

        [Test]
        public void DeserializeEnumCaseIncensitive_ByName_LowerCase()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberDoesNotMatchName>(@"""first""", new StringEnumConverter());
            Assert.AreEqual(EnumMemberDoesNotMatchName.First, e);
        }

        [Test]
        public void DeserializeEnumCaseIncensitive_ByName_UperCase()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberDoesNotMatchName>(@"""FIRST""", new StringEnumConverter());
            Assert.AreEqual(EnumMemberDoesNotMatchName.First, e);
        }

        [Test]
        public void DeserializeEnumCaseIncensitive_FromAttribute()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberDoesNotMatchName>(@"""FIRST_VALUE""");
            Assert.AreEqual(EnumMemberDoesNotMatchName.First, e);
        }

        [JsonConverter(typeof(StringEnumConverter))]
        public enum EnumMemberWithDiffrentCases
        {
            [EnumMember(Value = "first_value")]
            First,
            [EnumMember(Value = "second_value")]
            first
        }

        [Test]
        public void DeserializeEnumMemberWithDifferentCasing_ByEnumMemberValue_First()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberWithDiffrentCases>(@"""first_value""", new StringEnumConverter());
            Assert.AreEqual(EnumMemberWithDiffrentCases.First, e);
        }

        [Test]
        public void DeserializeEnumMemberWithDifferentCasing_ByEnumMemberValue_Second()
        {
            var e = JsonConvert.DeserializeObject<EnumMemberWithDiffrentCases>(@"""second_value""", new StringEnumConverter());
            Assert.AreEqual(EnumMemberWithDiffrentCases.first, e);
        }

        [DataContract(Name = "DateFormats")]
        public enum EnumMemberWithDifferentCases
        {
            [EnumMember(Value = "M")]
            Month,
            [EnumMember(Value = "m")]
            Minute
        }

        [Test]
        public void SerializeEnumMemberWithDifferentCases()
        {
            string json = JsonConvert.SerializeObject(EnumMemberWithDifferentCases.Month, new StringEnumConverter());

            Assert.AreEqual(@"""M""", json);

            json = JsonConvert.SerializeObject(EnumMemberWithDifferentCases.Minute, new StringEnumConverter());

            Assert.AreEqual(@"""m""", json);
        }

        [Test]
        public void DeserializeEnumMemberWithDifferentCases()
        {
            EnumMemberWithDifferentCases e = JsonConvert.DeserializeObject<EnumMemberWithDifferentCases>(@"""M""", new StringEnumConverter());

            Assert.AreEqual(EnumMemberWithDifferentCases.Month, e);

            e = JsonConvert.DeserializeObject<EnumMemberWithDifferentCases>(@"""m""", new StringEnumConverter());

            Assert.AreEqual(EnumMemberWithDifferentCases.Minute, e);
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
    public enum NumberNamesEnum
    {
        [EnumMember(Value = "2")]
        first,
        [EnumMember(Value = "1")]
        second,
        [EnumMember(Value = "3")]
        third,
        [EnumMember(Value = "-4")]
        fourth
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