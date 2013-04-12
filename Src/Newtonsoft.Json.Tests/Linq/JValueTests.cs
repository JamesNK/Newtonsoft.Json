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
#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE)
using System.Numerics;
#endif
using System.Text;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Linq;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests.Linq
{
  [TestFixture]
  public class JValueTests : TestFixtureBase
  {
    [Test]
    public void FloatParseHandling()
    {
      JValue v = (JValue) JToken.ReadFrom(
        new JsonTextReader(new StringReader("9.9"))
          {
            FloatParseHandling = Json.FloatParseHandling.Decimal
          });

      Assert.AreEqual(9.9m, v.Value);
      Assert.AreEqual(typeof(decimal), v.Value.GetType());
    }

    [Test]
    public void ChangeValue()
    {
      JValue v = new JValue(true);
      Assert.AreEqual(true, v.Value);
      Assert.AreEqual(JTokenType.Boolean, v.Type);

      v.Value = "Pie";
      Assert.AreEqual("Pie", v.Value);
      Assert.AreEqual(JTokenType.String, v.Type);

      v.Value = null;
      Assert.AreEqual(null, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);

      v.Value = (int?) null;
      Assert.AreEqual(null, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);

      v.Value = "Pie";
      Assert.AreEqual("Pie", v.Value);
      Assert.AreEqual(JTokenType.String, v.Type);

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
      v.Value = DBNull.Value;
      Assert.AreEqual(DBNull.Value, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);
#endif

      byte[] data = new byte[0];
      v.Value = data;

      Assert.AreEqual(data, v.Value);
      Assert.AreEqual(JTokenType.Bytes, v.Type);

      v.Value = StringComparison.OrdinalIgnoreCase;
      Assert.AreEqual(StringComparison.OrdinalIgnoreCase, v.Value);
      Assert.AreEqual(JTokenType.Integer, v.Type);

      v.Value = new Uri("http://json.codeplex.com/");
      Assert.AreEqual(new Uri("http://json.codeplex.com/"), v.Value);
      Assert.AreEqual(JTokenType.Uri, v.Type);

      v.Value = TimeSpan.FromDays(1);
      Assert.AreEqual(TimeSpan.FromDays(1), v.Value);
      Assert.AreEqual(JTokenType.TimeSpan, v.Type);

      Guid g = Guid.NewGuid();
      v.Value = g;
      Assert.AreEqual(g, v.Value);
      Assert.AreEqual(JTokenType.Guid, v.Type);

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE || PORTABLE40)
      BigInteger i = BigInteger.Parse("123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990");
      v.Value = i;
      Assert.AreEqual(i, v.Value);
      Assert.AreEqual(JTokenType.Integer, v.Type);
#endif
    }

    [Test]
    public void CreateComment()
    {
      JValue commentValue = JValue.CreateComment(null);
      Assert.AreEqual(null, commentValue.Value);
      Assert.AreEqual(JTokenType.Comment, commentValue.Type);

      commentValue.Value = "Comment";
      Assert.AreEqual("Comment", commentValue.Value);
      Assert.AreEqual(JTokenType.Comment, commentValue.Type);
    }

    [Test]
    public void CreateString()
    {
      JValue stringValue = JValue.CreateString(null);
      Assert.AreEqual(null, stringValue.Value);
      Assert.AreEqual(JTokenType.String, stringValue.Type);
    }

    [Test]
    public void JValueToString()
    {
      JValue v;

      v = new JValue(true);
      Assert.AreEqual("True", v.ToString());

      v = new JValue(Encoding.UTF8.GetBytes("Blah"));
      Assert.AreEqual("System.Byte[]", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue("I am a string!");
      Assert.AreEqual("I am a string!", v.ToString());

      v = new JValue(null, JTokenType.Null);
      Assert.AreEqual("", v.ToString());

      v = new JValue(null, JTokenType.Null);
      Assert.AreEqual("", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(new DateTime(2000, 12, 12, 20, 59, 59, DateTimeKind.Utc), JTokenType.Date);
      Assert.AreEqual("12/12/2000 20:59:59", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(new Uri("http://json.codeplex.com/"));
      Assert.AreEqual("http://json.codeplex.com/", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(TimeSpan.FromDays(1));
      Assert.AreEqual("1.00:00:00", v.ToString(null, CultureInfo.InvariantCulture));

      v = new JValue(new Guid("B282ADE7-C520-496C-A448-4084F6803DE5"));
      Assert.AreEqual("b282ade7-c520-496c-a448-4084f6803de5", v.ToString(null, CultureInfo.InvariantCulture));

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE || PORTABLE40)
      v = new JValue(BigInteger.Parse("123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990"));
      Assert.AreEqual("123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990", v.ToString(null, CultureInfo.InvariantCulture));
#endif
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE || PORTABLE40)
    [Test]
    public void JValueParse()
    {
      JValue v = (JValue)JToken.Parse("123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990");
      
      Assert.AreEqual(JTokenType.Integer, v.Type);
      Assert.AreEqual(BigInteger.Parse("123456789999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999999990"), v.Value);
    }
#endif

    [Test]
    public void Last()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
        () =>
          {
            JValue v = new JValue(true);
            JToken last = v.Last;
          });
    }

    [Test]
    public void Children()
    {
      JValue v = new JValue(true);
      var c = v.Children();
      Assert.AreEqual(JEnumerable<JToken>.Empty, c);
    }

    [Test]
    public void First()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
        () =>
          {
            JValue v = new JValue(true);
            JToken first = v.First;
          });
    }

    [Test]
    public void Item()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
        () =>
          {
            JValue v = new JValue(true);
            JToken first = v[0];
          });
    }

    [Test]
    public void Values()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot access child value on Newtonsoft.Json.Linq.JValue.",
        () =>
          {
            JValue v = new JValue(true);
            v.Values<int>();
          });
    }

    [Test]
    public void RemoveParentNull()
    {
      ExceptionAssert.Throws<InvalidOperationException>("The parent is missing.",
        () =>
          {
            JValue v = new JValue(true);
            v.Remove();
          });
    }

    [Test]
    public void Root()
    {
      JValue v = new JValue(true);
      Assert.AreEqual(v, v.Root);
    }

    [Test]
    public void Previous()
    {
      JValue v = new JValue(true);
      Assert.IsNull(v.Previous);
    }

    [Test]
    public void Next()
    {
      JValue v = new JValue(true);
      Assert.IsNull(v.Next);
    }

    [Test]
    public void DeepEquals()
    {
      Assert.IsTrue(JToken.DeepEquals(new JValue(5L), new JValue(5)));
      Assert.IsFalse(JToken.DeepEquals(new JValue(5M), new JValue(5)));
      Assert.IsTrue(JToken.DeepEquals(new JValue((ulong) long.MaxValue), new JValue(long.MaxValue)));
    }

    [Test]
    public void HasValues()
    {
      Assert.IsFalse((new JValue(5L)).HasValues);
    }

    [Test]
    public void SetValue()
    {
      ExceptionAssert.Throws<InvalidOperationException>("Cannot set child value on Newtonsoft.Json.Linq.JValue.",
        () =>
          {
            JToken t = new JValue(5L);
            t[0] = new JValue(3);
          });
    }

    [Test]
    public void CastNullValueToNonNullable()
    {
      ExceptionAssert.Throws<ArgumentException>("Can not convert Null to Int32.",
        () =>
          {
            JValue v = new JValue((object) null);
            int i = (int) v;
          });
    }

    [Test]
    public void ConvertValueToCompatibleType()
    {
      IComparable c = (new JValue(1).Value<IComparable>());
      Assert.AreEqual(1L, c);
    }

    [Test]
    public void ConvertValueToFormattableType()
    {
      IFormattable f = (new JValue(1).Value<IFormattable>());
      Assert.AreEqual(1L, f);

      Assert.AreEqual("01", f.ToString("00", CultureInfo.InvariantCulture));
    }

    [Test]
    public void Ordering()
    {
      JObject o = new JObject(
        new JProperty("Integer", new JValue(1)),
        new JProperty("Float", new JValue(1.2d)),
        new JProperty("Decimal", new JValue(1.1m))
        );

      IList<object> orderedValues = o.Values().Cast<JValue>().OrderBy(v => v).Select(v => v.Value).ToList();

      Assert.AreEqual(1L, orderedValues[0]);
      Assert.AreEqual(1.1m, orderedValues[1]);
      Assert.AreEqual(1.2d, orderedValues[2]);
    }

    [Test]
    public void WriteSingle()
    {
      float f = 5.2f;
      JValue value = new JValue(f);

      string json = value.ToString(Formatting.None);

      Assert.AreEqual("5.2", json);
    }

    public class Rate
    {
      public decimal Compoundings { get; set; }
    }

    private readonly Rate rate = new Rate {Compoundings = 12.166666666666666666666666667m};

    [Test]
    public void WriteFullDecimalPrecision()
    {
      var jTokenWriter = new JTokenWriter();
      new JsonSerializer().Serialize(jTokenWriter, rate);
      string json = jTokenWriter.Token.ToString();
      Assert.AreEqual(@"{
  ""Compoundings"": 12.166666666666666666666666667
}", json);
    }

    [Test]
    public void RoundTripDecimal()
    {
      var jTokenWriter = new JTokenWriter();
      new JsonSerializer().Serialize(jTokenWriter, rate);
      var rate2 = new JsonSerializer().Deserialize<Rate>(new JTokenReader(jTokenWriter.Token));

      Assert.AreEqual(rate.Compoundings, rate2.Compoundings);
    }

#if !NET20
    public class ObjectWithDateTimeOffset
    {
      public DateTimeOffset DateTimeOffset { get; set; }
    }

    [Test]
    public void SetDateTimeOffsetProperty()
    {
      var dateTimeOffset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(3));
      var json = JsonConvert.SerializeObject(
        new ObjectWithDateTimeOffset
          {
            DateTimeOffset = dateTimeOffset
          });

      var o = JObject.Parse(json);
      o.Property("DateTimeOffset").Value = dateTimeOffset;
    }

    public void ParseAndConvertDateTimeOffset()
    {
      var json = @"{ d: ""\/Date(0+0100)\/"" }";

      using (var stringReader = new StringReader(json))
      using (var jsonReader = new JsonTextReader(stringReader))
      {
        jsonReader.DateParseHandling = DateParseHandling.DateTimeOffset;

        var obj = JObject.Load(jsonReader);
        var d = (JValue)obj["d"];

        CustomAssert.IsInstanceOfType(typeof(DateTimeOffset), d.Value);
        TimeSpan offset = ((DateTimeOffset)d.Value).Offset;
        Assert.AreEqual(TimeSpan.FromHours(1), offset);

        DateTimeOffset dateTimeOffset = (DateTimeOffset) d;
        Assert.AreEqual(TimeSpan.FromHours(1), dateTimeOffset.Offset);
      }
    }

    public void ReadDatesAsDateTimeOffsetViaJsonConvert()
    {
      var content = @"{""startDateTime"":""2012-07-19T14:30:00+09:30""}";

      var jsonSerializerSettings = new JsonSerializerSettings() { DateFormatHandling = DateFormatHandling.IsoDateFormat, DateParseHandling = DateParseHandling.DateTimeOffset, DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind };
      JObject obj = (JObject)JsonConvert.DeserializeObject(content, jsonSerializerSettings);

      object startDateTime = obj["startDateTime"];

      CustomAssert.IsInstanceOfType(typeof(DateTimeOffset), startDateTime);
    }
#endif

#if !(NETFX_CORE || PORTABLE)
    [Test]
    public void ConvertsToBoolean()
    {
      Assert.AreEqual(true, Convert.ToBoolean(new JValue(true)));
    }

    [Test]
    public void ConvertsToBoolean_String()
    {
      Assert.AreEqual(true, Convert.ToBoolean(new JValue("true")));
    }

    [Test]
    public void ConvertsToInt32()
    {
      Assert.AreEqual(Int32.MaxValue, Convert.ToInt32(new JValue(Int32.MaxValue)));
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE || PORTABLE40)
    [Test]
    public void ConvertsToInt32_BigInteger()
    {
      Assert.AreEqual(123, Convert.ToInt32(new JValue(BigInteger.Parse("123"))));
    }
#endif

    [Test]
    public void ConvertsToChar()
    {
      Assert.AreEqual('c', Convert.ToChar(new JValue('c')));
    }

    [Test]
    public void ConvertsToSByte()
    {
      Assert.AreEqual(SByte.MaxValue, Convert.ToSByte(new JValue(SByte.MaxValue)));
    }

    [Test]
    public void ConvertsToByte()
    {
      Assert.AreEqual(Byte.MaxValue, Convert.ToByte(new JValue(Byte.MaxValue)));
    }

    [Test]
    public void ConvertsToInt16()
    {
      Assert.AreEqual(Int16.MaxValue, Convert.ToInt16(new JValue(Int16.MaxValue)));
    }

    [Test]
    public void ConvertsToUInt16()
    {
      Assert.AreEqual(UInt16.MaxValue, Convert.ToUInt16(new JValue(UInt16.MaxValue)));
    }

    [Test]
    public void ConvertsToUInt32()
    {
      Assert.AreEqual(UInt32.MaxValue, Convert.ToUInt32(new JValue(UInt32.MaxValue)));
    }

    [Test]
    public void ConvertsToInt64()
    {
      Assert.AreEqual(Int64.MaxValue, Convert.ToInt64(new JValue(Int64.MaxValue)));
    }

    [Test]
    public void ConvertsToUInt64()
    {
      Assert.AreEqual(UInt64.MaxValue, Convert.ToUInt64(new JValue(UInt64.MaxValue)));
    }

    [Test]
    public void ConvertsToSingle()
    {
      Assert.AreEqual(Single.MaxValue, Convert.ToSingle(new JValue(Single.MaxValue)));
    }

    [Test]
    public void ConvertsToDouble()
    {
      Assert.AreEqual(Double.MaxValue, Convert.ToDouble(new JValue(Double.MaxValue)));
    }

    [Test]
    public void ConvertsToDecimal()
    {
      Assert.AreEqual(Decimal.MaxValue, Convert.ToDecimal(new JValue(Decimal.MaxValue)));
    }

    [Test]
    public void ConvertsToDecimal_Int64()
    {
      Assert.AreEqual(123, Convert.ToDecimal(new JValue(123)));
    }

    [Test]
    public void ConvertsToString_Decimal()
    {
      Assert.AreEqual("79228162514264337593543950335", Convert.ToString(new JValue(Decimal.MaxValue)));
    }

    [Test]
    public void ConvertsToString_Uri()
    {
      Assert.AreEqual("http://www.google.com/", Convert.ToString(new JValue(new Uri("http://www.google.com"))));
    }

    [Test]
    public void ConvertsToString_Null()
    {
      Assert.AreEqual(string.Empty, Convert.ToString(new JValue((object)null)));
    }

    [Test]
    public void ConvertsToString_Guid()
    {
      Guid g = new Guid("0B5D4F85-E94C-4143-94C8-35F2AAEBB100");

      Assert.AreEqual("0b5d4f85-e94c-4143-94c8-35f2aaebb100", Convert.ToString(new JValue(g)));
    }

    [Test]
    public void ConvertsToType()
    {
      Assert.AreEqual(Int32.MaxValue, Convert.ChangeType(new JValue(Int32.MaxValue), typeof(Int32), CultureInfo.InvariantCulture));
    }

    [Test]
    public void ConvertsToDateTime()
    {
      Assert.AreEqual(new DateTime(2013, 02, 01, 01, 02, 03, 04), Convert.ToDateTime(new JValue(new DateTime(2013, 02, 01, 01, 02, 03, 04))));
    }

#if !NET20
    [Test]
    public void ConvertsToDateTime_DateTimeOffset()
    {
      var offset = new DateTimeOffset(2013, 02, 01, 01, 02, 03, 04, TimeSpan.Zero);

      Assert.AreEqual(new DateTime(2013, 02, 01, 01, 02, 03, 04), Convert.ToDateTime(new JValue(offset)));
    }
#endif

    [Test]
    public void GetTypeCode()
    {
      IConvertible v = new JValue(new Guid("0B5D4F85-E94C-4143-94C8-35F2AAEBB100"));
      Assert.AreEqual(TypeCode.Object, v.GetTypeCode());

      v = new JValue(new Uri("http://www.google.com"));
      Assert.AreEqual(TypeCode.Object, v.GetTypeCode());

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE || PORTABLE40)
      v = new JValue(new BigInteger(3));
      Assert.AreEqual(TypeCode.Object, v.GetTypeCode());
#endif
    }

    [Test]
    public void ToType()
    {
      IConvertible v = new JValue(9.0m);

      int i = (int)v.ToType(typeof (int), CultureInfo.InvariantCulture);
      Assert.AreEqual(9, i);

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE)
      BigInteger bi = (BigInteger)v.ToType(typeof(BigInteger), CultureInfo.InvariantCulture);
      Assert.AreEqual(new BigInteger(9), bi);
#endif
    }
#endif

    [Test]
    public void ToStringFormat()
    {
      JValue v = new JValue(new DateTime(2013, 02, 01, 01, 02, 03, 04));

      Assert.AreEqual("2013", v.ToString("yyyy"));
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE || PORTABLE40)
    [Test]
    public void ToStringNewTypes()
    {
      JArray a = new JArray(
        new JValue(new DateTimeOffset(2013, 02, 01, 01, 02, 03, 04, TimeSpan.FromHours(1))),
        new JValue(new BigInteger(5)),
        new JValue(1.1f)
        );

      Assert.AreEqual(@"[
  ""2013-02-01T01:02:03.004+01:00"",
  5,
  1.1
]", a.ToString());
    }
#endif
  }
}