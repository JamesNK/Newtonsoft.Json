using System;
using System.Collections.Generic;
#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
using System.Data.Linq;
#endif
#if !SILVERLIGHT && !NETFX_CORE
using System.Data.SqlTypes;
#endif
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
  [TestFixture]
  public class RegexConverterTests : TestFixtureBase
  {
    public class RegexTestClass
    {
      public Regex Regex { get; set; }
    }

    [Test]
    public void SerializeToText()
    {
      Regex regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

      string json = JsonConvert.SerializeObject(regex, Formatting.Indented, new RegexConverter());

      Assert.AreEqual(@"{
  ""Pattern"": ""abc"",
  ""Options"": 513
}", json);
    }

    [Test]
    public void SerializeToBson()
    {
      Regex regex = new Regex("abc", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);
      JsonSerializer serializer = new JsonSerializer();
      serializer.Converters.Add(new RegexConverter());

      serializer.Serialize(writer, new RegexTestClass { Regex = regex });

      string expected = "13-00-00-00-0B-52-65-67-65-78-00-61-62-63-00-69-75-00-00";
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());

      Assert.AreEqual(expected, bson);
    }

    [Test]
    public void DeserializeFromText()
    {
      string json = @"{
  ""Pattern"": ""abc"",
  ""Options"": 513
}";

      Regex newRegex = JsonConvert.DeserializeObject<Regex>(json, new RegexConverter());
      Assert.AreEqual("abc", newRegex.ToString());
      Assert.AreEqual(RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, newRegex.Options);
    }

    [Test]
    public void DeserializeFromBson()
    {
      MemoryStream ms = new MemoryStream(MiscellaneousUtils.HexToBytes("13-00-00-00-0B-52-65-67-65-78-00-61-62-63-00-69-75-00-00"));
      BsonReader reader = new BsonReader(ms);
      JsonSerializer serializer = new JsonSerializer();
      serializer.Converters.Add(new RegexConverter());

      RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

      Assert.AreEqual("abc", c.Regex.ToString());
      Assert.AreEqual(RegexOptions.IgnoreCase, c.Regex.Options);
    }

    [Test]
    public void ConvertEmptyRegexBson()
    {
      Regex regex = new Regex(string.Empty);

      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);
      JsonSerializer serializer = new JsonSerializer();
      serializer.Converters.Add(new RegexConverter());

      serializer.Serialize(writer, new RegexTestClass { Regex = regex });

      ms.Seek(0, SeekOrigin.Begin);
      BsonReader reader = new BsonReader(ms);
      serializer.Converters.Add(new RegexConverter());

      RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

      Assert.AreEqual("", c.Regex.ToString());
      Assert.AreEqual(RegexOptions.None, c.Regex.Options);
    }

    [Test]
    public void ConvertRegexWithAllOptionsBson()
    {
      Regex regex = new Regex(
        "/",
        RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture);

      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);
      JsonSerializer serializer = new JsonSerializer();
      serializer.Converters.Add(new RegexConverter());

      serializer.Serialize(writer, new RegexTestClass { Regex = regex });

      string expected = "14-00-00-00-0B-52-65-67-65-78-00-2F-00-69-6D-73-75-78-00-00";
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());

      Assert.AreEqual(expected, bson);

      ms.Seek(0, SeekOrigin.Begin);
      BsonReader reader = new BsonReader(ms);
      serializer.Converters.Add(new RegexConverter());

      RegexTestClass c = serializer.Deserialize<RegexTestClass>(reader);

      Assert.AreEqual("/", c.Regex.ToString());
      Assert.AreEqual(RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline | RegexOptions.ExplicitCapture, c.Regex.Options);
    }

    [Test]
    public void ConvertEmptyRegexJson()
    {
      Regex regex = new Regex("");

      string json = JsonConvert.SerializeObject(new RegexTestClass { Regex = regex }, Formatting.Indented, new RegexConverter());

      Assert.AreEqual(@"{
  ""Regex"": {
    ""Pattern"": """",
    ""Options"": 0
  }
}", json);

      RegexTestClass newRegex = JsonConvert.DeserializeObject<RegexTestClass>(json, new RegexConverter());
      Assert.AreEqual("", newRegex.Regex.ToString());
      Assert.AreEqual(RegexOptions.None, newRegex.Regex.Options);
    }
  }
}