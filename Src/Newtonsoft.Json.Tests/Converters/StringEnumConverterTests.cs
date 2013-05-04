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
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Converters;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
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
      ExceptionAssert.Throws<Exception>("Enum name 'Third' already exists on enum 'NamedEnumDuplicate'.",
      () =>
      {
        EnumContainer<NamedEnumDuplicate> c = new EnumContainer<NamedEnumDuplicate>
        {
          Enum = NamedEnumDuplicate.First
        };

        JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
      });
    }

    [Test]
    public void SerializeNameEnumTest()
    {
      EnumContainer<NamedEnum> c = new EnumContainer<NamedEnum>
        {
          Enum = NamedEnum.First
        };

      string json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
      Assert.AreEqual(@"{
  ""Enum"": ""@first""
}", json);

      c = new EnumContainer<NamedEnum>
      {
        Enum = NamedEnum.Third
      };

      json = JsonConvert.SerializeObject(c, Formatting.Indented, new StringEnumConverter());
      Assert.AreEqual(@"{
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

      Assert.AreEqual(@"{
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

      Assert.AreEqual(@"{
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

      Assert.AreEqual(@"{
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

      Assert.AreEqual(@"{
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
      negativeEnumClass.Value2 = (NegativeEnum) int.MinValue;

      string json = JsonConvert.SerializeObject(negativeEnumClass, Formatting.Indented, new StringEnumConverter());

      Assert.AreEqual(@"{
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
      Assert.AreEqual(@"{
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

      ExceptionAssert.Throws<JsonSerializationException>(
        @"Error converting value ""Three"" to type 'Newtonsoft.Json.Tests.Converters.StringEnumConverterTests+MyEnum'. Path 'Value', line 1, position 19.",
        () =>
          {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(new StringEnumConverter());
            serializer.Deserialize<Bucket>(new JsonTextReader(new StringReader(json)));
          });
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
  }
}