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
        jsonWriter.WriteValue(null);
        jsonWriter.WriteValue("This is a string.");
        jsonWriter.WriteNull();
        jsonWriter.WriteUndefined();
        jsonWriter.WriteEndArray();
      }

      string expected = @"[""@"",""\r\n\t\f\b?{\\r\\n\""'"",true,10,10.99,0.99,1E-18,0.000000000000000001,"""",""This is a string."",null,undefined]";
      string result = sb.ToString();

      Console.WriteLine("ValueFormatting");
      Console.WriteLine(result);

      Assert.AreEqual(expected, result);
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
  }
}