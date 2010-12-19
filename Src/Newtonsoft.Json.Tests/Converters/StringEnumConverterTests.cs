using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
  public class StringEnumConverterTests : TestFixtureBase
  {
    public class EnumClass
    {
      public StoreColor StoreColor { get; set; }
      public StoreColor? NullableStoreColor1 { get; set; }
      public StoreColor? NullableStoreColor2 { get; set; }
    }

    public enum NegativeEnum
    {
      Negative = -1,
      Zero = 0,
      Positive = 1
    }

    public class NegativeEnumClass
    {
      public NegativeEnum Value1 { get; set; }
      public NegativeEnum Value2 { get; set; }
    }

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
  }
}