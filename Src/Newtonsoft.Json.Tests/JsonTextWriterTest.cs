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
using System.Diagnostics;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public class JsonTextWriterTest : TestFixtureBase
  {
    [Test]
    public void CloseOutput()
    {
      MemoryStream ms = new MemoryStream();
      JsonTextWriter writer = new JsonTextWriter(new StreamWriter(ms));

      Assert.IsTrue(ms.CanRead);
      writer.Close();
      Assert.IsFalse(ms.CanRead);

      ms = new MemoryStream();
      writer = new JsonTextWriter(new StreamWriter(ms)) { CloseOutput = false };

      Assert.IsTrue(ms.CanRead);
      writer.Close();
      Assert.IsTrue(ms.CanRead);
    }

#if !(PORTABLE || NETFX_CORE)
    [Test]
    public void WriteIConvertable()
    {
      var sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      writer.WriteValue(new ConvertibleInt(1));

      Assert.AreEqual("1", sw.ToString());
    }
#endif
    
    [Test]
    public void ValueFormatting()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue('@');
        jsonWriter.WriteValue("\r\n\t\f\b?{\\r\\n\"\'");
        jsonWriter.WriteValue(true);
        jsonWriter.WriteValue(10);
        jsonWriter.WriteValue(10.99);
        jsonWriter.WriteValue(0.99);
        jsonWriter.WriteValue(0.000000000000000001d);
        jsonWriter.WriteValue(0.000000000000000001m);
        jsonWriter.WriteValue((string)null);
        jsonWriter.WriteValue((object)null);
        jsonWriter.WriteValue("This is a string.");
        jsonWriter.WriteNull();
        jsonWriter.WriteUndefined();
        jsonWriter.WriteEndArray();
      }

      string expected = @"[""@"",""\r\n\t\f\b?{\\r\\n\""'"",true,10,10.99,0.99,1E-18,0.000000000000000001,null,null,""This is a string."",null,undefined]";
      string result = sb.ToString();

      Console.WriteLine("ValueFormatting");
      Console.WriteLine(result);

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void NullableValueFormatting()
    {
      StringWriter sw = new StringWriter();
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue((char?)null);
        jsonWriter.WriteValue((char?)'c');
        jsonWriter.WriteValue((bool?)null);
        jsonWriter.WriteValue((bool?)true);
        jsonWriter.WriteValue((byte?)null);
        jsonWriter.WriteValue((byte?)1);
        jsonWriter.WriteValue((sbyte?)null);
        jsonWriter.WriteValue((sbyte?)1);
        jsonWriter.WriteValue((short?)null);
        jsonWriter.WriteValue((short?)1);
        jsonWriter.WriteValue((ushort?)null);
        jsonWriter.WriteValue((ushort?)1);
        jsonWriter.WriteValue((int?)null);
        jsonWriter.WriteValue((int?)1);
        jsonWriter.WriteValue((uint?)null);
        jsonWriter.WriteValue((uint?)1);
        jsonWriter.WriteValue((long?)null);
        jsonWriter.WriteValue((long?)1);
        jsonWriter.WriteValue((ulong?)null);
        jsonWriter.WriteValue((ulong?)1);
        jsonWriter.WriteValue((double?)null);
        jsonWriter.WriteValue((double?)1.1);
        jsonWriter.WriteValue((float?)null);
        jsonWriter.WriteValue((float?)1.1);
        jsonWriter.WriteValue((decimal?)null);
        jsonWriter.WriteValue((decimal?)1.1m);
        jsonWriter.WriteValue((DateTime?)null);
        jsonWriter.WriteValue((DateTime?)new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc));
#if !NET20
        jsonWriter.WriteValue((DateTimeOffset?)null);
        jsonWriter.WriteValue((DateTimeOffset?)new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero));
#endif
        jsonWriter.WriteEndArray();
      }

      string json = sw.ToString();
      string expected;

#if !NET20
      expected = @"[null,""c"",null,true,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1.1,null,1.1,null,1.1,null,""1970-01-01T00:00:00Z"",null,""1970-01-01T00:00:00+00:00""]";
#else
      expected = @"[null,""c"",null,true,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1.1,null,1.1,null,1.1,null,""1970-01-01T00:00:00Z""]";
#endif

      Assert.AreEqual(expected, json);
    }

    [Test]
    public void WriteValueObjectWithNullable()
    {
      StringWriter sw = new StringWriter();
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        char? value = 'c';

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue((object)value);
        jsonWriter.WriteEndArray();
      }

      string json = sw.ToString();
      string expected = @"[""c""]";

      Assert.AreEqual(expected, json);
    }

    [Test]
    public void WriteValueObjectWithUnsupportedValue()
    {
      ExceptionAssert.Throws<JsonWriterException>(
        @"Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation. Path ''.",
        () =>
        {
          StringWriter sw = new StringWriter();
          using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
          {
            jsonWriter.WriteStartArray();
            jsonWriter.WriteValue(new Version(1, 1, 1, 1));
            jsonWriter.WriteEndArray();
          }
        });
    }

    [Test]
    public void StringEscaping()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(@"""These pretzels are making me thirsty!""");
        jsonWriter.WriteValue("Jeff's house was burninated.");
        jsonWriter.WriteValue(@"1. You don't talk about fight club.
2. You don't talk about fight club.");
        jsonWriter.WriteValue("35% of\t statistics\n are made\r up.");
        jsonWriter.WriteEndArray();
      }

      string expected = @"[""\""These pretzels are making me thirsty!\"""",""Jeff's house was burninated."",""1. You don't talk about fight club.\r\n2. You don't talk about fight club."",""35% of\t statistics\n are made\r up.""]";
      string result = sb.ToString();

      Console.WriteLine("StringEscaping");
      Console.WriteLine(result);

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void WriteEnd()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("CPU");
        jsonWriter.WriteValue("Intel");
        jsonWriter.WritePropertyName("PSU");
        jsonWriter.WriteValue("500W");
        jsonWriter.WritePropertyName("Drives");
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue("DVD read/writer");
        jsonWriter.WriteComment("(broken)");
        jsonWriter.WriteValue("500 gigabyte hard drive");
        jsonWriter.WriteValue("200 gigabype hard drive");
        jsonWriter.WriteEndObject();
        Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
      }

      string expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabype hard drive""
  ]
}";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void CloseWithRemainingContent()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("CPU");
        jsonWriter.WriteValue("Intel");
        jsonWriter.WritePropertyName("PSU");
        jsonWriter.WriteValue("500W");
        jsonWriter.WritePropertyName("Drives");
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue("DVD read/writer");
        jsonWriter.WriteComment("(broken)");
        jsonWriter.WriteValue("500 gigabyte hard drive");
        jsonWriter.WriteValue("200 gigabype hard drive");
        jsonWriter.Close();
      }

      string expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabype hard drive""
  ]
}";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void Indenting()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("CPU");
        jsonWriter.WriteValue("Intel");
        jsonWriter.WritePropertyName("PSU");
        jsonWriter.WriteValue("500W");
        jsonWriter.WritePropertyName("Drives");
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue("DVD read/writer");
        jsonWriter.WriteComment("(broken)");
        jsonWriter.WriteValue("500 gigabyte hard drive");
        jsonWriter.WriteValue("200 gigabype hard drive");
        jsonWriter.WriteEnd();
        jsonWriter.WriteEndObject();
        Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
      }

      // {
      //   "CPU": "Intel",
      //   "PSU": "500W",
      //   "Drives": [
      //     "DVD read/writer"
      //     /*(broken)*/,
      //     "500 gigabyte hard drive",
      //     "200 gigabype hard drive"
      //   ]
      // }

      string expected = @"{
  ""CPU"": ""Intel"",
  ""PSU"": ""500W"",
  ""Drives"": [
    ""DVD read/writer""
    /*(broken)*/,
    ""500 gigabyte hard drive"",
    ""200 gigabype hard drive""
  ]
}";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void State()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);

        jsonWriter.WriteStartObject();
        Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
        Assert.AreEqual("", jsonWriter.Path);

        jsonWriter.WritePropertyName("CPU");
        Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);
        Assert.AreEqual("CPU", jsonWriter.Path);

        jsonWriter.WriteValue("Intel");
        Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
        Assert.AreEqual("CPU", jsonWriter.Path);

        jsonWriter.WritePropertyName("Drives");
        Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);
        Assert.AreEqual("Drives", jsonWriter.Path);

        jsonWriter.WriteStartArray();
        Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteValue("DVD read/writer");
        Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);
        Assert.AreEqual("Drives[0]", jsonWriter.Path);

        jsonWriter.WriteEnd();
        Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);
        Assert.AreEqual("Drives", jsonWriter.Path);

        jsonWriter.WriteEndObject();
        Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
        Assert.AreEqual("", jsonWriter.Path);
      }
    }

    [Test]
    public void FloatingPointNonFiniteNumbers_Symbol()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteValue(double.PositiveInfinity);
        jsonWriter.WriteValue(double.NegativeInfinity);
        jsonWriter.WriteValue(float.NaN);
        jsonWriter.WriteValue(float.PositiveInfinity);
        jsonWriter.WriteValue(float.NegativeInfinity);
        jsonWriter.WriteEndArray();

        jsonWriter.Flush();
      }

      string expected = @"[
  NaN,
  Infinity,
  -Infinity,
  NaN,
  Infinity,
  -Infinity
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void FloatingPointNonFiniteNumbers_Zero()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.DefaultValue;

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteValue(double.PositiveInfinity);
        jsonWriter.WriteValue(double.NegativeInfinity);
        jsonWriter.WriteValue(float.NaN);
        jsonWriter.WriteValue(float.PositiveInfinity);
        jsonWriter.WriteValue(float.NegativeInfinity);
        jsonWriter.WriteValue((double?)double.NaN);
        jsonWriter.WriteValue((double?)double.PositiveInfinity);
        jsonWriter.WriteValue((double?)double.NegativeInfinity);
        jsonWriter.WriteValue((float?)float.NaN);
        jsonWriter.WriteValue((float?)float.PositiveInfinity);
        jsonWriter.WriteValue((float?)float.NegativeInfinity);
        jsonWriter.WriteEndArray();

        jsonWriter.Flush();
      }

      string expected = @"[
  0.0,
  0.0,
  0.0,
  0.0,
  0.0,
  0.0,
  null,
  null,
  null,
  null,
  null,
  null
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void FloatingPointNonFiniteNumbers_String()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.String;

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteValue(double.PositiveInfinity);
        jsonWriter.WriteValue(double.NegativeInfinity);
        jsonWriter.WriteValue(float.NaN);
        jsonWriter.WriteValue(float.PositiveInfinity);
        jsonWriter.WriteValue(float.NegativeInfinity);
        jsonWriter.WriteEndArray();

        jsonWriter.Flush();
      }

      string expected = @"[
  ""NaN"",
  ""Infinity"",
  ""-Infinity"",
  ""NaN"",
  ""Infinity"",
  ""-Infinity""
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void FloatingPointNonFiniteNumbers_QuoteChar()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.String;
        jsonWriter.QuoteChar = '\'';

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteValue(double.PositiveInfinity);
        jsonWriter.WriteValue(double.NegativeInfinity);
        jsonWriter.WriteValue(float.NaN);
        jsonWriter.WriteValue(float.PositiveInfinity);
        jsonWriter.WriteValue(float.NegativeInfinity);
        jsonWriter.WriteEndArray();

        jsonWriter.Flush();
      }

      string expected = @"[
  'NaN',
  'Infinity',
  '-Infinity',
  'NaN',
  'Infinity',
  '-Infinity'
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void WriteRawInStart()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

        jsonWriter.WriteRaw("[1,2,3,4,5]");
        jsonWriter.WriteWhitespace("  ");
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteEndArray();
      }

      string expected = @"[1,2,3,4,5]  [
  NaN
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void WriteRawInArray()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteRaw(",[1,2,3,4,5]");
        jsonWriter.WriteRaw(",[1,2,3,4,5]");
        jsonWriter.WriteValue(float.NaN);
        jsonWriter.WriteEndArray();
      }

      string expected = @"[
  NaN,[1,2,3,4,5],[1,2,3,4,5],
  NaN
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void WriteRawInObject()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

        jsonWriter.WriteStartObject();
        jsonWriter.WriteRaw(@"""PropertyName"":[1,2,3,4,5]");
        jsonWriter.WriteEnd();
      }

      string expected = @"{""PropertyName"":[1,2,3,4,5]}";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void WriteToken()
    {
      JsonTextReader reader = new JsonTextReader(new StringReader("[1,2,3,4,5]"));
      reader.Read();
      reader.Read();

      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      writer.WriteToken(reader);

      Assert.AreEqual("1", sw.ToString());
    }

    [Test]
    public void WriteRawValue()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        int i = 0;
        string rawJson = "[1,2]";

        jsonWriter.WriteStartObject();

        while (i < 3)
        {
          jsonWriter.WritePropertyName("d" + i);
          jsonWriter.WriteRawValue(rawJson);

          i++;
        }

        jsonWriter.WriteEndObject();
      }

      Assert.AreEqual(@"{""d0"":[1,2],""d1"":[1,2],""d2"":[1,2]}", sb.ToString());
    }

    [Test]
    public void WriteObjectNestedInConstructor()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("con");

        jsonWriter.WriteStartConstructor("Ext.data.JsonStore");
        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("aa");
        jsonWriter.WriteValue("aa");
        jsonWriter.WriteEndObject();
        jsonWriter.WriteEndConstructor();

        jsonWriter.WriteEndObject();
      }

      Assert.AreEqual(@"{""con"":new Ext.data.JsonStore({""aa"":""aa""})}", sb.ToString());
    }

    [Test]
    public void WriteFloatingPointNumber()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;
        
        jsonWriter.WriteStartArray();

        jsonWriter.WriteValue(0.0);
        jsonWriter.WriteValue(0f);
        jsonWriter.WriteValue(0.1);
        jsonWriter.WriteValue(1.0);
        jsonWriter.WriteValue(1.000001);
        jsonWriter.WriteValue(0.000001);
        jsonWriter.WriteValue(double.Epsilon);
        jsonWriter.WriteValue(double.PositiveInfinity);
        jsonWriter.WriteValue(double.NegativeInfinity);
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteValue(double.MaxValue);
        jsonWriter.WriteValue(double.MinValue);
        jsonWriter.WriteValue(float.PositiveInfinity);
        jsonWriter.WriteValue(float.NegativeInfinity);
        jsonWriter.WriteValue(float.NaN);

        jsonWriter.WriteEndArray();
      }

      Assert.AreEqual(@"[0.0,0.0,0.1,1.0,1.000001,1E-06,4.94065645841247E-324,Infinity,-Infinity,NaN,1.7976931348623157E+308,-1.7976931348623157E+308,Infinity,-Infinity,NaN]", sb.ToString());
    }

    [Test]
    public void WriteIntegerNumber()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw) { Formatting = Formatting.Indented })
      {
        jsonWriter.WriteStartArray();

        jsonWriter.WriteValue(int.MaxValue);
        jsonWriter.WriteValue(int.MinValue);
        jsonWriter.WriteValue(0);
        jsonWriter.WriteValue(-0);
        jsonWriter.WriteValue(9L);
        jsonWriter.WriteValue(9UL);
        jsonWriter.WriteValue(long.MaxValue);
        jsonWriter.WriteValue(long.MinValue);
        jsonWriter.WriteValue(ulong.MaxValue);
        jsonWriter.WriteValue(ulong.MinValue);

        jsonWriter.WriteEndArray();
      }

      Console.WriteLine(sb.ToString());

      Assert.AreEqual(@"[
  2147483647,
  -2147483648,
  0,
  0,
  9,
  9,
  9223372036854775807,
  -9223372036854775808,
  18446744073709551615,
  0
]", sb.ToString());
    }

    [Test]
    public void BadWriteEndArray()
    {
      ExceptionAssert.Throws<JsonWriterException>(
        "No token to close. Path ''.",
        () =>
        {
          StringBuilder sb = new StringBuilder();
          StringWriter sw = new StringWriter(sb);

          using (JsonWriter jsonWriter = new JsonTextWriter(sw))
          {
            jsonWriter.WriteStartArray();

            jsonWriter.WriteValue(0.0);

            jsonWriter.WriteEndArray();
            jsonWriter.WriteEndArray();
          }
        });
    }

    [Test]
    public void InvalidQuoteChar()
    {
      ExceptionAssert.Throws<ArgumentException>(
        @"Invalid JavaScript string quote character. Valid quote characters are ' and "".",
        () =>
        {
          StringBuilder sb = new StringBuilder();
          StringWriter sw = new StringWriter(sb);

          using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
          {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.QuoteChar = '*';
          }
        });
    }

    [Test]
    public void Indentation()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        jsonWriter.FloatFormatHandling = FloatFormatHandling.Symbol;

        Assert.AreEqual(Formatting.Indented, jsonWriter.Formatting);

        jsonWriter.Indentation = 5;
        Assert.AreEqual(5, jsonWriter.Indentation);
        jsonWriter.IndentChar = '_';
        Assert.AreEqual('_', jsonWriter.IndentChar);
        jsonWriter.QuoteName = true;
        Assert.AreEqual(true, jsonWriter.QuoteName);
        jsonWriter.QuoteChar = '\'';
        Assert.AreEqual('\'', jsonWriter.QuoteChar);

        jsonWriter.WriteStartObject();
        jsonWriter.WritePropertyName("propertyName");
        jsonWriter.WriteValue(double.NaN);
        jsonWriter.WriteEndObject();
      }

      string expected = @"{
_____'propertyName': NaN
}";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void WriteSingleBytes()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      string text = "Hello world.";
      byte[] data = Encoding.UTF8.GetBytes(text);

      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        Assert.AreEqual(Formatting.Indented, jsonWriter.Formatting);

        jsonWriter.WriteValue(data);
      }

      string expected = @"""SGVsbG8gd29ybGQu""";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);

      byte[] d2 = Convert.FromBase64String(result.Trim('"'));

      Assert.AreEqual(text, Encoding.UTF8.GetString(d2, 0, d2.Length));
    }

    [Test]
    public void WriteBytesInArray()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      string text = "Hello world.";
      byte[] data = Encoding.UTF8.GetBytes(text);

      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;
        Assert.AreEqual(Formatting.Indented, jsonWriter.Formatting);

        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(data);
        jsonWriter.WriteValue(data);
        jsonWriter.WriteValue((object)data);
        jsonWriter.WriteValue((byte[])null);
        jsonWriter.WriteValue((Uri)null);
        jsonWriter.WriteEndArray();
      }

      string expected = @"[
  ""SGVsbG8gd29ybGQu"",
  ""SGVsbG8gd29ybGQu"",
  ""SGVsbG8gd29ybGQu"",
  null,
  null
]";
      string result = sb.ToString();

      Assert.AreEqual(expected, result);
    }

    [Test]
    public void Path()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      string text = "Hello world.";
      byte[] data = Encoding.UTF8.GetBytes(text);

      using (JsonTextWriter writer = new JsonTextWriter(sw))
      {
        writer.Formatting = Formatting.Indented;

        writer.WriteStartArray();
        Assert.AreEqual("", writer.Path);
        writer.WriteStartObject();
        Assert.AreEqual("[0]", writer.Path);
        writer.WritePropertyName("Property1");
        Assert.AreEqual("[0].Property1", writer.Path);
        writer.WriteStartArray();
        Assert.AreEqual("[0].Property1", writer.Path);
        writer.WriteValue(1);
        Assert.AreEqual("[0].Property1[0]", writer.Path);
        writer.WriteStartArray();
        Assert.AreEqual("[0].Property1[1]", writer.Path);
        writer.WriteStartArray();
        Assert.AreEqual("[0].Property1[1][0]", writer.Path);
        writer.WriteStartArray();
        Assert.AreEqual("[0].Property1[1][0][0]", writer.Path);
        writer.WriteEndObject();
        Assert.AreEqual("[0]", writer.Path);
        writer.WriteStartObject();
        Assert.AreEqual("[1]", writer.Path);
        writer.WritePropertyName("Property2");
        Assert.AreEqual("[1].Property2", writer.Path);
        writer.WriteStartConstructor("Constructor1");
        Assert.AreEqual("[1].Property2", writer.Path);
        writer.WriteNull();
        Assert.AreEqual("[1].Property2[0]", writer.Path);
        writer.WriteStartArray();
        Assert.AreEqual("[1].Property2[1]", writer.Path);
        writer.WriteValue(1);
        Assert.AreEqual("[1].Property2[1][0]", writer.Path);
        writer.WriteEnd();
        Assert.AreEqual("[1].Property2[1]", writer.Path);
        writer.WriteEndObject();
        Assert.AreEqual("[1]", writer.Path);
        writer.WriteEndArray();
        Assert.AreEqual("", writer.Path);
      }

      Assert.AreEqual(@"[
  {
    ""Property1"": [
      1,
      [
        [
          []
        ]
      ]
    ]
  },
  {
    ""Property2"": new Constructor1(
      null,
      [
        1
      ]
    )
  }
]", sb.ToString());
    }

    [Test]
    public void BuildStateArray()
    {
      JsonWriter.State[][] stateArray = JsonWriter.BuildStateArray();

      var valueStates = JsonWriter.StateArrayTempate[7];

      foreach (JsonToken valueToken in EnumUtils.GetValues(typeof(JsonToken)))
      {
        switch (valueToken)
        {
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.String:
          case JsonToken.Boolean:
          case JsonToken.Null:
          case JsonToken.Undefined:
          case JsonToken.Date:
          case JsonToken.Bytes:
            Assert.AreEqual(valueStates, stateArray[(int)valueToken], "Error for " + valueToken + " states.");
            break;
        }
      }
    }

    [Test]
    public void DateTimeZoneHandling()
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw)
        {
          DateTimeZoneHandling = Json.DateTimeZoneHandling.Utc
        };

      writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));

      Assert.AreEqual(@"""2000-01-01T01:01:01Z""", sw.ToString());
    }

    [Test]
    public void HtmlStringEscapeHandling()
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw)
      {
        StringEscapeHandling = StringEscapeHandling.EscapeHtml
      };

      string script = @"<script type=""text/javascript"">alert('hi');</script>";

      writer.WriteValue(script);

      string json = sw.ToString();

      Assert.AreEqual(@"""\u003cscript type=\u0022text/javascript\u0022\u003ealert(\u0027hi\u0027);\u003c/script\u003e""", json);

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      
      Assert.AreEqual(script, reader.ReadAsString());

      //Console.WriteLine(HttpUtility.HtmlEncode(script));

      //System.Web.Script.Serialization.JavaScriptSerializer s = new System.Web.Script.Serialization.JavaScriptSerializer();
      //Console.WriteLine(s.Serialize(new { html = script }));
    }

    [Test]
    public void NonAsciiStringEscapeHandling()
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw)
      {
        StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
      };

      string unicode = "\u5f20";

      writer.WriteValue(unicode);

      string json = sw.ToString();

      Assert.AreEqual(8, json.Length);
      Assert.AreEqual(@"""\u5f20""", json);

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      Assert.AreEqual(unicode, reader.ReadAsString());

      sw = new StringWriter();
      writer = new JsonTextWriter(sw)
      {
        StringEscapeHandling = StringEscapeHandling.Default
      };

      writer.WriteValue(unicode);

      json = sw.ToString();

      Assert.AreEqual(3, json.Length);
      Assert.AreEqual("\"\u5f20\"", json);
    }

    [Test]
    public void WriteEndOnProperty()
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      writer.QuoteChar = '\'';

      writer.WriteStartObject();
      writer.WritePropertyName("Blah");
      writer.WriteEnd();

      Assert.AreEqual("{'Blah':null}", sw.ToString());
    }

#if !NET20
    [Test]
    public void QuoteChar()
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      writer.Formatting = Formatting.Indented;
      writer.QuoteChar = '\'';

      writer.WriteStartArray();

      writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
      writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

      writer.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;
      writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
      writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

      writer.DateFormatString = "yyyy gg";
      writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
      writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

      writer.WriteValue(new byte[] { 1, 2, 3 });
      writer.WriteValue(TimeSpan.Zero);
      writer.WriteValue(new Uri("http://www.google.com/"));
      writer.WriteValue(Guid.Empty);

      writer.WriteEnd();

      Assert.AreEqual(@"[
  '2000-01-01T01:01:01Z',
  '2000-01-01T01:01:01+00:00',
  '\/Date(946688461000)\/',
  '\/Date(946688461000+0000)\/',
  '2000 A.D.',
  '2000 A.D.',
  'AQID',
  '00:00:00',
  'http://www.google.com/',
  '00000000-0000-0000-0000-000000000000'
]", sw.ToString());
    }

    [Test]
    public void Culture()
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      writer.Formatting = Formatting.Indented;
      writer.DateFormatString = "yyyy tt";
      writer.Culture = new CultureInfo("en-NZ");
      writer.QuoteChar = '\'';

      writer.WriteStartArray();

      writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc));
      writer.WriteValue(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));

      writer.WriteEnd();

      Assert.AreEqual(@"[
  '2000 a.m.',
  '2000 a.m.'
]", sw.ToString());
    }
#endif

    [Test]
    public void CompareNewStringEscapingWithOld()
    {
      Console.WriteLine("Started");

      char c = (char) 0;

      do
      {
        if (c % 1000 == 0)
          Console.WriteLine("Position: " + (int)c);

        StringWriter swNew = new StringWriter();
        char[] buffer = null;
        JavaScriptUtils.WriteEscapedJavaScriptString(swNew, c.ToString(), '"', true, JavaScriptUtils.DoubleQuoteCharEscapeFlags, StringEscapeHandling.Default, ref buffer);

        StringWriter swOld = new StringWriter();
        WriteEscapedJavaScriptStringOld(swOld, c.ToString(), '"', true);

        string newText = swNew.ToString();
        string oldText = swOld.ToString();

        if (newText != oldText)
          throw new Exception("Difference for char '{0}' (value {1}). Old text: {2}, New text: {3}".FormatWith(CultureInfo.InvariantCulture, c, (int) c, oldText, newText));

        c++;
      } while (c != char.MaxValue);

      Console.WriteLine("Finished");
    }

    private const string EscapedUnicodeText = "!";

    private static void WriteEscapedJavaScriptStringOld(TextWriter writer, string s, char delimiter, bool appendDelimiters)
    {
      // leading delimiter
      if (appendDelimiters)
        writer.Write(delimiter);

      if (s != null)
      {
        char[] chars = null;
        char[] unicodeBuffer = null;
        int lastWritePosition = 0;

        for (int i = 0; i < s.Length; i++)
        {
          var c = s[i];

          // don't escape standard text/numbers except '\' and the text delimiter
          if (c >= ' ' && c < 128 && c != '\\' && c != delimiter)
            continue;

          string escapedValue;

          switch (c)
          {
            case '\t':
              escapedValue = @"\t";
              break;
            case '\n':
              escapedValue = @"\n";
              break;
            case '\r':
              escapedValue = @"\r";
              break;
            case '\f':
              escapedValue = @"\f";
              break;
            case '\b':
              escapedValue = @"\b";
              break;
            case '\\':
              escapedValue = @"\\";
              break;
            case '\u0085': // Next Line
              escapedValue = @"\u0085";
              break;
            case '\u2028': // Line Separator
              escapedValue = @"\u2028";
              break;
            case '\u2029': // Paragraph Separator
              escapedValue = @"\u2029";
              break;
            case '\'':
              // this charater is being used as the delimiter
              escapedValue = @"\'";
              break;
            case '"':
              // this charater is being used as the delimiter
              escapedValue = "\\\"";
              break;
            default:
              if (c <= '\u001f')
              {
                if (unicodeBuffer == null)
                  unicodeBuffer = new char[6];

                StringUtils.ToCharAsUnicode(c, unicodeBuffer);

                // slightly hacky but it saves multiple conditions in if test
                escapedValue = EscapedUnicodeText;
              }
              else
              {
                escapedValue = null;
              }
              break;
          }

          if (escapedValue == null)
            continue;

          if (i > lastWritePosition)
          {
            if (chars == null)
              chars = s.ToCharArray();

            // write unchanged chars before writing escaped text
            writer.Write(chars, lastWritePosition, i - lastWritePosition);
          }

          lastWritePosition = i + 1;
          if (!string.Equals(escapedValue, EscapedUnicodeText))
            writer.Write(escapedValue);
          else
            writer.Write(unicodeBuffer);
        }

        if (lastWritePosition == 0)
        {
          // no escaped text, write entire string
          writer.Write(s);
        }
        else
        {
          if (chars == null)
            chars = s.ToCharArray();

          // write remaining text
          writer.Write(chars, lastWritePosition, s.Length - lastWritePosition);
        }
      }

      // trailing delimiter
      if (appendDelimiters)
        writer.Write(delimiter);
    }

    [Test]
    public void CustomJsonTextWriterTests()
    {
      StringWriter sw = new StringWriter();
      CustomJsonTextWriter writer = new CustomJsonTextWriter(sw) { Formatting = Formatting.Indented };
      writer.WriteStartObject();
      Assert.AreEqual(WriteState.Object, writer.WriteState);
      writer.WritePropertyName("Property1");
      Assert.AreEqual(WriteState.Property, writer.WriteState);
      Assert.AreEqual("Property1", writer.Path);
      writer.WriteNull();
      Assert.AreEqual(WriteState.Object, writer.WriteState);
      writer.WriteEndObject();
      Assert.AreEqual(WriteState.Start, writer.WriteState);

      Assert.AreEqual(@"{{{
  ""1ytreporP"": NULL!!!
}}}", sw.ToString());
    }
  }

  public class CustomJsonTextWriter : JsonTextWriter
  {
    private readonly TextWriter _writer;

    public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter)
    {
      _writer = textWriter;
    }

    public override void WritePropertyName(string name)
    {
      WritePropertyName(name, true);
    }

    public override void WritePropertyName(string name, bool escape)
    {
      SetWriteState(JsonToken.PropertyName, name);

      if (QuoteName)
        _writer.Write(QuoteChar);

      _writer.Write(new string(name.ToCharArray().Reverse().ToArray()));

      if (QuoteName)
        _writer.Write(QuoteChar);

      _writer.Write(':');
    }

    public override void WriteNull()
    {
      SetWriteState(JsonToken.Null, null);

      _writer.Write("NULL!!!");
    }

    public override void WriteStartObject()
    {
      SetWriteState(JsonToken.StartObject, null);

      _writer.Write("{{{");
    }

    public override void WriteEndObject()
    {
      SetWriteState(JsonToken.EndObject, null);
    }

    protected override void WriteEnd(JsonToken token)
    {
      if (token == JsonToken.EndObject)
        _writer.Write("}}}");
      else
        base.WriteEnd(token);
    }
  }

#if !(PORTABLE || NETFX_CORE)
  public struct ConvertibleInt : IConvertible
  {
    private readonly int _value;

    public ConvertibleInt(int value)
    {
      _value = value;
    }

    public TypeCode GetTypeCode()
    {
      return TypeCode.Int32;
    }

    public bool ToBoolean(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public byte ToByte(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public char ToChar(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public DateTime ToDateTime(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public decimal ToDecimal(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public double ToDouble(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public short ToInt16(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public int ToInt32(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public long ToInt64(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public sbyte ToSByte(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public float ToSingle(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public string ToString(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public object ToType(Type conversionType, IFormatProvider provider)
    {
      if (conversionType == typeof(int))
        return _value;

      throw new Exception("Type not supported: " + conversionType.FullName);
    }

    public ushort ToUInt16(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public uint ToUInt32(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }

    public ulong ToUInt64(IFormatProvider provider)
    {
      throw new NotImplementedException();
    }
  }
#endif
}