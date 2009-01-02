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
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;

namespace Newtonsoft.Json.Tests
{
  public class JsonTextWriterTest : TestFixtureBase
  {
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
        jsonWriter.WriteValue((DateTime?)new DateTime(JsonConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc));
        jsonWriter.WriteValue((DateTimeOffset?)null);
        jsonWriter.WriteValue((DateTimeOffset?)new DateTimeOffset(JsonConvert.InitialJavaScriptDateTicks, TimeSpan.Zero));
        jsonWriter.WriteEndArray();
      }

      string json = sw.ToString();
      string expected = @"[null,""c"",null,true,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1,null,1.1,null,1.1,null,1.1,null,""\/Date(0)\/"",null,""\/Date(0+0000)\/""]";

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
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation.")]
    public void WriteValueObjectWithUnsupportedValue()
    {
      StringWriter sw = new StringWriter();
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.WriteStartArray();
        jsonWriter.WriteValue(new Version(1, 1, 1, 1));
        jsonWriter.WriteEndArray();
      }
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

      Console.WriteLine("Indenting");
      Console.WriteLine(result);

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

        jsonWriter.WritePropertyName("CPU");
        Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);

        jsonWriter.WriteValue("Intel");
        Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

        jsonWriter.WritePropertyName("Drives");
        Assert.AreEqual(WriteState.Property, jsonWriter.WriteState);

        jsonWriter.WriteStartArray();
        Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteValue("DVD read/writer");
        Assert.AreEqual(WriteState.Array, jsonWriter.WriteState);

        jsonWriter.WriteEnd();
        Assert.AreEqual(WriteState.Object, jsonWriter.WriteState);

        jsonWriter.WriteEndObject();
        Assert.AreEqual(WriteState.Start, jsonWriter.WriteState);
      }
    }

    [Test]
    public void FloatingPointNonFiniteNumbers()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

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
    public void WriteRawInStart()
    {
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = Formatting.Indented;

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
  }
}