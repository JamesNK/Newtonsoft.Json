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
#if !(NET20 || NET35 || SILVERLIGHT)
using System.Threading.Tasks;
#endif
using System.Xml;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public class JsonConvertTest : TestFixtureBase
  {
    [Test]
    public void DefaultSettings()
    {
      try
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(new { test = new[] { 1, 2, 3 } });

        Assert.AreEqual(@"{
  ""test"": [
    1,
    2,
    3
  ]
}", json);
      }
      finally
      {
        JsonConvert.DefaultSettings = null;
      }
    }

    [Test]
    public void DefaultSettings_Example()
    {
      try
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented,
          ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        Employee e = new Employee
        {
          FirstName = "Eric",
          LastName = "Example",
          BirthDate = new DateTime(1980, 4, 20, 0, 0, 0, DateTimeKind.Utc),
          Department = "IT",
          JobTitle = "Web Dude"
        };

        string json = JsonConvert.SerializeObject(e);
        // {
        //   "firstName": "Eric",
        //   "lastName": "Example",
        //   "birthDate": "1980-04-20T00:00:00Z",
        //   "department": "IT",
        //   "jobTitle": "Web Dude"
        // }

        Assert.AreEqual(@"{
  ""firstName"": ""Eric"",
  ""lastName"": ""Example"",
  ""birthDate"": ""1980-04-20T00:00:00Z"",
  ""department"": ""IT"",
  ""jobTitle"": ""Web Dude""
}", json);
      }
      finally
      {
        JsonConvert.DefaultSettings = null;
      }
    }

    [Test]
    public void DefaultSettings_Override()
    {
      try
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented
        };

        string json = JsonConvert.SerializeObject(new { test = new[] { 1, 2, 3 } }, new JsonSerializerSettings
        {
          Formatting = Formatting.None
        });

        Assert.AreEqual(@"{""test"":[1,2,3]}", json);
      }
      finally
      {
        JsonConvert.DefaultSettings = null;
      }
    }

    [Test]
    public void DefaultSettings_Override_JsonConverterOrder()
    {
      try
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented,
          Converters = { new IsoDateTimeConverter { DateTimeFormat = "yyyy" } }
        };

        string json = JsonConvert.SerializeObject(new[] { new DateTime(2000, 12, 12, 4, 2, 4, DateTimeKind.Utc) }, new JsonSerializerSettings
        {
          Formatting = Formatting.None,
          Converters =
          {
            // should take precedence
            new JavaScriptDateTimeConverter(),
            new IsoDateTimeConverter { DateTimeFormat = "dd" }
          }
        });

        Assert.AreEqual(@"[new Date(976593724000)]", json);
      }
      finally
      {
        JsonConvert.DefaultSettings = null;
      }
    }

    [Test]
    public void DefaultSettings_Create()
    {
      try
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented
        };

        IList<int> l = new List<int> { 1, 2, 3 };

        StringWriter sw = new StringWriter();
        JsonSerializer serializer = JsonSerializer.CreateDefault();
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[
  1,
  2,
  3
]", sw.ToString());

        sw = new StringWriter();
        serializer.Formatting = Formatting.None;
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[1,2,3]", sw.ToString());

        sw = new StringWriter();
        serializer = new JsonSerializer();
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[1,2,3]", sw.ToString());

        sw = new StringWriter();
        serializer = JsonSerializer.Create();
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[1,2,3]", sw.ToString());
      }
      finally
      {
        JsonConvert.DefaultSettings = null;
      }
    }

    [Test]
    public void DefaultSettings_CreateWithSettings()
    {
      try
      {
        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
          Formatting = Formatting.Indented
        };

        IList<int> l = new List<int> { 1, 2, 3 };

        StringWriter sw = new StringWriter();
        JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
          {
            Converters = { new IntConverter() }
          });
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[
  2,
  4,
  6
]", sw.ToString());

        sw = new StringWriter();
        serializer.Converters.Clear();
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[
  1,
  2,
  3
]", sw.ToString());

        sw = new StringWriter();
        serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });
        serializer.Serialize(sw, l);

        Assert.AreEqual(@"[
  1,
  2,
  3
]", sw.ToString());
      }
      finally
      {
        JsonConvert.DefaultSettings = null;
      }
    }

    public class IntConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        int i = (int)value;
        writer.WriteValue(i * 2);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        throw new NotImplementedException();
      }

      public override bool CanConvert(Type objectType)
      {
        return objectType == typeof(int);
      }
    }

    [Test]
    public void DeserializeObject_EmptyString()
    {
      object result = JsonConvert.DeserializeObject(string.Empty);
      Assert.IsNull(result);
    }

    [Test]
    public void DeserializeObject_Integer()
    {
      object result = JsonConvert.DeserializeObject("1");
      Assert.AreEqual(1L, result);
    }

    [Test]
    public void DeserializeObject_Integer_EmptyString()
    {
      int? value = JsonConvert.DeserializeObject<int?>("");
      Assert.IsNull(value);
    }

    [Test]
    public void DeserializeObject_Decimal_EmptyString()
    {
      decimal? value = JsonConvert.DeserializeObject<decimal?>("");
      Assert.IsNull(value);
    }

    [Test]
    public void DeserializeObject_DateTime_EmptyString()
    {
      DateTime? value = JsonConvert.DeserializeObject<DateTime?>("");
      Assert.IsNull(value);
    }

    [Test]
    public void EscapeJavaScriptString()
    {
      string result;

      result = JavaScriptUtils.ToEscapedJavaScriptString("How now brown cow?", '"', true);
      Assert.AreEqual(@"""How now brown cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("How now 'brown' cow?", '"', true);
      Assert.AreEqual(@"""How now 'brown' cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("How now <brown> cow?", '"', true);
      Assert.AreEqual(@"""How now <brown> cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"How 
now brown cow?", '"', true);
      Assert.AreEqual(@"""How \r\nnow brown cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007", '"', true);
      Assert.AreEqual(@"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007""", result);

      result =
        JavaScriptUtils.ToEscapedJavaScriptString("\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", '"', true);
      Assert.AreEqual(@"""\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013""", result);

      result =
        JavaScriptUtils.ToEscapedJavaScriptString(
          "\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", '"', true);
      Assert.AreEqual(@"""\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f """, result);

      result =
        JavaScriptUtils.ToEscapedJavaScriptString(
          "!\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", '"', true);
      Assert.AreEqual(@"""!\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("^_`abcdefghijklmnopqrstuvwxyz{|}~", '"', true);
      Assert.AreEqual(@"""^_`abcdefghijklmnopqrstuvwxyz{|}~""", result);

      string data =
        "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
      string expected =
        @"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~""";

      result = JavaScriptUtils.ToEscapedJavaScriptString(data, '"', true);
      Assert.AreEqual(expected, result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("Fred's cat.", '\'', true);
      Assert.AreEqual(result, @"'Fred\'s cat.'");

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"""How are you gentlemen?"" said Cats.", '"', true);
      Assert.AreEqual(result, @"""\""How are you gentlemen?\"" said Cats.""");

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"""How are' you gentlemen?"" said Cats.", '"', true);
      Assert.AreEqual(result, @"""\""How are' you gentlemen?\"" said Cats.""");

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"Fred's ""cat"".", '\'', true);
      Assert.AreEqual(result, @"'Fred\'s ""cat"".'");

      result = JavaScriptUtils.ToEscapedJavaScriptString("\u001farray\u003caddress", '"', true);
      Assert.AreEqual(result, @"""\u001farray<address""");
    }

    [Test]
    public void EscapeJavaScriptString_UnicodeLinefeeds()
    {
      string result;

      result = JavaScriptUtils.ToEscapedJavaScriptString("before" + '\u0085' + "after", '"', true);
      Assert.AreEqual(@"""before\u0085after""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("before" + '\u2028' + "after", '"', true);
      Assert.AreEqual(@"""before\u2028after""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("before" + '\u2029' + "after", '"', true);
      Assert.AreEqual(@"""before\u2029after""", result);
    }

    [Test]
    public void ToStringInvalid()
    {
      ExceptionAssert.Throws<ArgumentException>("Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation.",
      () =>
      {
        JsonConvert.ToString(new Version(1, 0));
      });
    }

    [Test]
    public void GuidToString()
    {
      Guid guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");
      string json = JsonConvert.ToString(guid);
      Assert.AreEqual(@"""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""", json);
    }

    [Test]
    public void EnumToString()
    {
      string json = JsonConvert.ToString(StringComparison.CurrentCultureIgnoreCase);
      Assert.AreEqual("1", json);
    }

    [Test]
    public void ObjectToString()
    {
      object value;

      value = 1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = 1.1;
      Assert.AreEqual("1.1", JsonConvert.ToString(value));

      value = 1.1m;
      Assert.AreEqual("1.1", JsonConvert.ToString(value));

      value = (float)1.1;
      Assert.AreEqual("1.1", JsonConvert.ToString(value));

      value = (short)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = (long)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = (byte)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = (uint)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = (ushort)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = (sbyte)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = (ulong)1;
      Assert.AreEqual("1", JsonConvert.ToString(value));

      value = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      Assert.AreEqual(@"""1970-01-01T00:00:00Z""", JsonConvert.ToString(value));

      value = new DateTime(DateTimeUtils.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      Assert.AreEqual(@"""\/Date(0)\/""", JsonConvert.ToString((DateTime)value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.RoundtripKind));

#if !NET20
      value = new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero);
      Assert.AreEqual(@"""1970-01-01T00:00:00+00:00""", JsonConvert.ToString(value));

      value = new DateTimeOffset(DateTimeUtils.InitialJavaScriptDateTicks, TimeSpan.Zero);
      Assert.AreEqual(@"""\/Date(0+0000)\/""", JsonConvert.ToString((DateTimeOffset)value, DateFormatHandling.MicrosoftDateFormat));
#endif

      value = null;
      Assert.AreEqual("null", JsonConvert.ToString(value));

#if !(NETFX_CORE || PORTABLE || PORTABLE40)
      value = DBNull.Value;
      Assert.AreEqual("null", JsonConvert.ToString(value));
#endif

      value = "I am a string";
      Assert.AreEqual(@"""I am a string""", JsonConvert.ToString(value));

      value = true;
      Assert.AreEqual("true", JsonConvert.ToString(value));

      value = 'c';
      Assert.AreEqual(@"""c""", JsonConvert.ToString(value));
    }

    [Test]
    public void TestInvalidStrings()
    {
      ExceptionAssert.Throws<JsonReaderException>("Additional text encountered after finished reading JSON content: t. Path '', line 1, position 19.",
      () =>
      {
        string orig = @"this is a string ""that has quotes"" ";

        string serialized = JsonConvert.SerializeObject(orig);

        // *** Make string invalid by stripping \" \"
        serialized = serialized.Replace(@"\""", "\"");

        JsonConvert.DeserializeObject<string>(serialized);
      });
    }

    [Test]
    public void DeserializeValueObjects()
    {
      int i = JsonConvert.DeserializeObject<int>("1");
      Assert.AreEqual(1, i);

#if !NET20
      DateTimeOffset d = JsonConvert.DeserializeObject<DateTimeOffset>(@"""\/Date(-59011455539000+0000)\/""");
      Assert.AreEqual(new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)), d);
#endif

      bool b = JsonConvert.DeserializeObject<bool>("true");
      Assert.AreEqual(true, b);

      object n = JsonConvert.DeserializeObject<object>("null");
      Assert.AreEqual(null, n);

      object u = JsonConvert.DeserializeObject<object>("undefined");
      Assert.AreEqual(null, u);
    }

    [Test]
    public void FloatToString()
    {
      Assert.AreEqual("1.1", JsonConvert.ToString(1.1));
      Assert.AreEqual("1.11", JsonConvert.ToString(1.11));
      Assert.AreEqual("1.111", JsonConvert.ToString(1.111));
      Assert.AreEqual("1.1111", JsonConvert.ToString(1.1111));
      Assert.AreEqual("1.11111", JsonConvert.ToString(1.11111));
      Assert.AreEqual("1.111111", JsonConvert.ToString(1.111111));
      Assert.AreEqual("1.0", JsonConvert.ToString(1.0));
      Assert.AreEqual("1.0", JsonConvert.ToString(1d));
      Assert.AreEqual("-1.0", JsonConvert.ToString(-1d)); 
      Assert.AreEqual("1.01", JsonConvert.ToString(1.01));
      Assert.AreEqual("1.001", JsonConvert.ToString(1.001));
      Assert.AreEqual(JsonConvert.PositiveInfinity, JsonConvert.ToString(Double.PositiveInfinity));
      Assert.AreEqual(JsonConvert.NegativeInfinity, JsonConvert.ToString(Double.NegativeInfinity));
      Assert.AreEqual(JsonConvert.NaN, JsonConvert.ToString(Double.NaN));
    }

    [Test]
    public void DecimalToString()
    {
      Assert.AreEqual("1.1", JsonConvert.ToString(1.1m));
      Assert.AreEqual("1.11", JsonConvert.ToString(1.11m));
      Assert.AreEqual("1.111", JsonConvert.ToString(1.111m));
      Assert.AreEqual("1.1111", JsonConvert.ToString(1.1111m));
      Assert.AreEqual("1.11111", JsonConvert.ToString(1.11111m));
      Assert.AreEqual("1.111111", JsonConvert.ToString(1.111111m));
      Assert.AreEqual("1.0", JsonConvert.ToString(1.0m));
      Assert.AreEqual("-1.0", JsonConvert.ToString(-1.0m));
      Assert.AreEqual("-1.0", JsonConvert.ToString(-1m));
      Assert.AreEqual("1.0", JsonConvert.ToString(1m));
      Assert.AreEqual("1.01", JsonConvert.ToString(1.01m));
      Assert.AreEqual("1.001", JsonConvert.ToString(1.001m));
      Assert.AreEqual("79228162514264337593543950335.0", JsonConvert.ToString(Decimal.MaxValue));
      Assert.AreEqual("-79228162514264337593543950335.0", JsonConvert.ToString(Decimal.MinValue));
    }

    [Test]
    public void StringEscaping()
    {
      string v = @"It's a good day
""sunshine""";

      string json = JsonConvert.ToString(v);
      Assert.AreEqual(@"""It's a good day\r\n\""sunshine\""""", json);
    }

    [Test]
    public void WriteDateTime()
    {
      DateTimeResult result = null;

      result = TestDateTime("DateTime Max", DateTime.MaxValue);
      Assert.AreEqual("9999-12-31T23:59:59.9999999", result.IsoDateRoundtrip);
      Assert.AreEqual("9999-12-31T23:59:59.9999999" + GetOffset(DateTime.MaxValue, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("9999-12-31T23:59:59.9999999", result.IsoDateUnspecified);
      Assert.AreEqual("9999-12-31T23:59:59.9999999Z", result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(253402300799999)\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(253402300799999" + GetOffset(DateTime.MaxValue, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(253402300799999)\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(253402300799999)\/", result.MsDateUtc);

      DateTime year2000local = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Local);
      string localToUtcDate = year2000local.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");

      result = TestDateTime("DateTime Local", year2000local);
      Assert.AreEqual("2000-01-01T01:01:01" + GetOffset(year2000local, DateFormatHandling.IsoDateFormat), result.IsoDateRoundtrip);
      Assert.AreEqual("2000-01-01T01:01:01" + GetOffset(year2000local, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("2000-01-01T01:01:01", result.IsoDateUnspecified);
      Assert.AreEqual(localToUtcDate, result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + GetOffset(year2000local, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + GetOffset(year2000local, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + GetOffset(year2000local, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000local) + @")\/", result.MsDateUtc);

      DateTime millisecondsLocal = new DateTime(2000, 1, 1, 1, 1, 1, 999, DateTimeKind.Local);
      localToUtcDate = millisecondsLocal.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");

      result = TestDateTime("DateTime Local with milliseconds", millisecondsLocal);
      Assert.AreEqual("2000-01-01T01:01:01.999" + GetOffset(millisecondsLocal, DateFormatHandling.IsoDateFormat), result.IsoDateRoundtrip);
      Assert.AreEqual("2000-01-01T01:01:01.999" + GetOffset(millisecondsLocal, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("2000-01-01T01:01:01.999", result.IsoDateUnspecified);
      Assert.AreEqual(localToUtcDate, result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + GetOffset(millisecondsLocal, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + GetOffset(millisecondsLocal, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + GetOffset(millisecondsLocal, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(millisecondsLocal) + @")\/", result.MsDateUtc);

      DateTime ticksLocal = new DateTime(634663873826822481, DateTimeKind.Local);
      localToUtcDate = ticksLocal.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFK");
      
      result = TestDateTime("DateTime Local with ticks", ticksLocal);
      Assert.AreEqual("2012-03-03T16:03:02.6822481" + GetOffset(ticksLocal, DateFormatHandling.IsoDateFormat), result.IsoDateRoundtrip);
      Assert.AreEqual("2012-03-03T16:03:02.6822481" + GetOffset(ticksLocal, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("2012-03-03T16:03:02.6822481", result.IsoDateUnspecified);
      Assert.AreEqual(localToUtcDate, result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + GetOffset(ticksLocal, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + GetOffset(ticksLocal, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + GetOffset(ticksLocal, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(ticksLocal) + @")\/", result.MsDateUtc);

      DateTime year2000Unspecified = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified);

      result = TestDateTime("DateTime Unspecified", year2000Unspecified);
      Assert.AreEqual("2000-01-01T01:01:01", result.IsoDateRoundtrip);
      Assert.AreEqual("2000-01-01T01:01:01" + GetOffset(year2000Unspecified, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("2000-01-01T01:01:01", result.IsoDateUnspecified);
      Assert.AreEqual("2000-01-01T01:01:01Z", result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified) + GetOffset(year2000Unspecified, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified) + GetOffset(year2000Unspecified, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified) + GetOffset(year2000Unspecified, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(year2000Unspecified.ToLocalTime()) + @")\/", result.MsDateUtc);

      DateTime year2000Utc = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
      string utcTolocalDate = year2000Utc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss");

      result = TestDateTime("DateTime Utc", year2000Utc);
      Assert.AreEqual("2000-01-01T01:01:01Z", result.IsoDateRoundtrip);
      Assert.AreEqual(utcTolocalDate + GetOffset(year2000Utc, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("2000-01-01T01:01:01", result.IsoDateUnspecified);
      Assert.AreEqual("2000-01-01T01:01:01Z", result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(946688461000)\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(946688461000" + GetOffset(year2000Utc, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(DateTime.SpecifyKind(year2000Utc, DateTimeKind.Unspecified)) + GetOffset(year2000Utc, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(946688461000)\/", result.MsDateUtc);

      DateTime unixEpoc = new DateTime(621355968000000000, DateTimeKind.Utc);
      utcTolocalDate = unixEpoc.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss");

      result = TestDateTime("DateTime Unix Epoc", unixEpoc);
      Assert.AreEqual("1970-01-01T00:00:00Z", result.IsoDateRoundtrip);
      Assert.AreEqual(utcTolocalDate + GetOffset(unixEpoc, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("1970-01-01T00:00:00", result.IsoDateUnspecified);
      Assert.AreEqual("1970-01-01T00:00:00Z", result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(0)\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(0" + GetOffset(unixEpoc, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(" + DateTimeUtils.ConvertDateTimeToJavaScriptTicks(DateTime.SpecifyKind(unixEpoc, DateTimeKind.Unspecified)) + GetOffset(unixEpoc, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(0)\/", result.MsDateUtc);

      result = TestDateTime("DateTime Min", DateTime.MinValue);
      Assert.AreEqual("0001-01-01T00:00:00", result.IsoDateRoundtrip);
      Assert.AreEqual("0001-01-01T00:00:00" + GetOffset(DateTime.MinValue, DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("0001-01-01T00:00:00", result.IsoDateUnspecified);
      Assert.AreEqual("0001-01-01T00:00:00Z", result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(-62135596800000)\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(-62135596800000" + GetOffset(DateTime.MinValue, DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(-62135596800000)\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(-62135596800000)\/", result.MsDateUtc);

      result = TestDateTime("DateTime Default", default(DateTime));
      Assert.AreEqual("0001-01-01T00:00:00", result.IsoDateRoundtrip);
      Assert.AreEqual("0001-01-01T00:00:00" + GetOffset(default(DateTime), DateFormatHandling.IsoDateFormat), result.IsoDateLocal);
      Assert.AreEqual("0001-01-01T00:00:00", result.IsoDateUnspecified);
      Assert.AreEqual("0001-01-01T00:00:00Z", result.IsoDateUtc);
      Assert.AreEqual(@"\/Date(-62135596800000)\/", result.MsDateRoundtrip);
      Assert.AreEqual(@"\/Date(-62135596800000" + GetOffset(default(DateTime), DateFormatHandling.MicrosoftDateFormat) + @")\/", result.MsDateLocal);
      Assert.AreEqual(@"\/Date(-62135596800000)\/", result.MsDateUnspecified);
      Assert.AreEqual(@"\/Date(-62135596800000)\/", result.MsDateUtc);

#if !NET20
      result = TestDateTime("DateTimeOffset TimeSpan Zero", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero));
      Assert.AreEqual("2000-01-01T01:01:01+00:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(946688461000+0000)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset TimeSpan 1 hour", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(1)));
      Assert.AreEqual("2000-01-01T01:01:01+01:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(946684861000+0100)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset TimeSpan 1.5 hour", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(1.5)));
      Assert.AreEqual("2000-01-01T01:01:01+01:30", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(946683061000+0130)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset TimeSpan 13 hour", new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)));
      Assert.AreEqual("2000-01-01T01:01:01+13:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(946641661000+1300)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset TimeSpan with ticks", new DateTimeOffset(634663873826822481, TimeSpan.Zero));
      Assert.AreEqual("2012-03-03T16:03:02.6822481+00:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(1330790582682+0000)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset Min", DateTimeOffset.MinValue);
      Assert.AreEqual("0001-01-01T00:00:00+00:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(-62135596800000+0000)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset Max", DateTimeOffset.MaxValue);
      Assert.AreEqual("9999-12-31T23:59:59.9999999+00:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(253402300799999+0000)\/", result.MsDateRoundtrip);

      result = TestDateTime("DateTimeOffset Default", default(DateTimeOffset));
      Assert.AreEqual("0001-01-01T00:00:00+00:00", result.IsoDateRoundtrip);
      Assert.AreEqual(@"\/Date(-62135596800000+0000)\/", result.MsDateRoundtrip);
#endif
    }

    public class DateTimeResult
    {
      public string IsoDateRoundtrip { get; set; }
      public string IsoDateLocal { get; set; }
      public string IsoDateUnspecified { get; set; }
      public string IsoDateUtc { get; set; }

      public string MsDateRoundtrip { get; set; }
      public string MsDateLocal { get; set; }
      public string MsDateUnspecified { get; set; }
      public string MsDateUtc { get; set; }
    }

    private DateTimeResult TestDateTime<T>(string name, T value)
    {
      Console.WriteLine(name);

      DateTimeResult result = new DateTimeResult();

      result.IsoDateRoundtrip = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
      if (value is DateTime)
      {
        result.IsoDateLocal = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.Local);
        result.IsoDateUnspecified = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.Unspecified);
        result.IsoDateUtc = TestDateTimeFormat(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.Utc);
      }

      result.MsDateRoundtrip = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.RoundtripKind);
      if (value is DateTime)
      {
        result.MsDateLocal = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Local);
        result.MsDateUnspecified = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Unspecified);
        result.MsDateUtc = TestDateTimeFormat(value, DateFormatHandling.MicrosoftDateFormat, DateTimeZoneHandling.Utc);
      }

      TestDateTimeFormat(value, new IsoDateTimeConverter());

#if !NETFX_CORE
      if (value is DateTime)
      {
        Console.WriteLine(XmlConvert.ToString((DateTime)(object)value, XmlDateTimeSerializationMode.RoundtripKind));
      }
      else
      {
        Console.WriteLine(XmlConvert.ToString((DateTimeOffset)(object)value));
      }
#endif

#if !NET20
      MemoryStream ms = new MemoryStream();
      DataContractSerializer s = new DataContractSerializer(typeof(T));
      s.WriteObject(ms, value);
      string json = Encoding.UTF8.GetString(ms.ToArray(), 0, Convert.ToInt32(ms.Length));
      Console.WriteLine(json);
#endif

      Console.WriteLine();

      return result;
    }

    private static string TestDateTimeFormat<T>(T value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
    {
      string date = null;

      if (value is DateTime)
      {
        date = JsonConvert.ToString((DateTime)(object)value, format, timeZoneHandling);
      }
      else
      {
#if !NET20
        date = JsonConvert.ToString((DateTimeOffset)(object)value, format);
#endif
      }

      Console.WriteLine(format.ToString("g") + "-" + timeZoneHandling.ToString("g") + ": " + date);

      if (timeZoneHandling == DateTimeZoneHandling.RoundtripKind)
      {
        T parsed = JsonConvert.DeserializeObject<T>(date);
        try
        {
          Assert.AreEqual(value, parsed);
        }
        catch (Exception)
        {
          long valueTicks = GetTicks(value);
          long parsedTicks = GetTicks(parsed);

          valueTicks = (valueTicks/10000)*10000;

          Assert.AreEqual(valueTicks, parsedTicks);
        }
      }

      return date.Trim('"');
    }

    private static void TestDateTimeFormat<T>(T value, JsonConverter converter)
    {
      string date = Write(value, converter);

      Console.WriteLine(converter.GetType().Name + ": " + date);

      T parsed = Read<T>(date, converter);

      try
      {
        Assert.AreEqual(value, parsed);
      }
      catch (Exception)
      {
        // JavaScript ticks aren't as precise, recheck after rounding
        long valueTicks = GetTicks(value);
        long parsedTicks = GetTicks(parsed);

        valueTicks = (valueTicks / 10000) * 10000;

        Assert.AreEqual(valueTicks, parsedTicks);
      }
    }

    public static long GetTicks(object value)
    {
      return (value is DateTime) ? ((DateTime)value).Ticks : ((DateTimeOffset)value).Ticks;
    }

    public static string Write(object value, JsonConverter converter)
    {
      StringWriter sw = new StringWriter();
      JsonTextWriter writer = new JsonTextWriter(sw);
      converter.WriteJson(writer, value, null);

      writer.Flush();
      return sw.ToString();
    }

    public static T Read<T>(string text, JsonConverter converter)
    {
      JsonTextReader reader = new JsonTextReader(new StringReader(text));
      reader.ReadAsString();

      return (T)converter.ReadJson(reader, typeof(T), null, null);
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40)
    [Test]
    public void Async()
    {
      Task<string> task = null;
      
      task = JsonConvert.SerializeObjectAsync(42);
      task.Wait();

      Assert.AreEqual("42", task.Result);

      task = JsonConvert.SerializeObjectAsync(new[] {1, 2, 3, 4, 5}, Formatting.Indented);
      task.Wait();

      Assert.AreEqual(@"[
  1,
  2,
  3,
  4,
  5
]", task.Result);

      task = JsonConvert.SerializeObjectAsync(DateTime.MaxValue, Formatting.None, new JsonSerializerSettings
        {
          DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        });
      task.Wait();

      Assert.AreEqual(@"""\/Date(253402300799999)\/""", task.Result);

      var taskObject = JsonConvert.DeserializeObjectAsync("[]");
      taskObject.Wait();

      CollectionAssert.AreEquivalent(new JArray(), (JArray)taskObject.Result);

      Task<object> taskVersionArray = JsonConvert.DeserializeObjectAsync("['2.0']", typeof(Version[]), new JsonSerializerSettings
        {
          Converters = {new VersionConverter()}
        });
      taskVersionArray.Wait();

      Version[] versionArray = (Version[])taskVersionArray.Result;

      Assert.AreEqual(1, versionArray.Length);
      Assert.AreEqual(2, versionArray[0].Major);

      Task<int> taskInt = JsonConvert.DeserializeObjectAsync<int>("5");
      taskInt.Wait();

      Assert.AreEqual(5, taskInt.Result);

      var taskVersion = JsonConvert.DeserializeObjectAsync<Version>("'2.0'", new JsonSerializerSettings
        {
          Converters = {new VersionConverter()}
        });
      taskVersion.Wait();

      Assert.AreEqual(2, taskVersion.Result.Major);

      Movie p = new Movie();
      p.Name = "Existing,";

      Task taskVoid = JsonConvert.PopulateObjectAsync("{'Name':'Appended'}", p, new JsonSerializerSettings
      {
        Converters = new List<JsonConverter> { new JsonSerializerTest.StringAppenderConverter() }
      });

      taskVoid.Wait();

      Assert.AreEqual("Existing,Appended", p.Name);
    }
#endif

    [Test]
    public void SerializeObjectDateTimeZoneHandling()
    {
      string json = JsonConvert.SerializeObject(
        new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified),
        new JsonSerializerSettings
        {
          DateTimeZoneHandling = DateTimeZoneHandling.Utc
        });

      Assert.AreEqual(@"""2000-01-01T01:01:01Z""", json);
    }

    //[Test]
    public void StackOverflowTest()
    {
      StringBuilder sb = new StringBuilder();

      int depth = 900;
      for (int i = 0; i < depth; i++)
      {
        sb.Append("{'A':");
      }

      // invalid json
      sb.Append("{***}");
      for (int i = 0; i < depth; i++)
      {
        sb.Append("}");
      }

      string json = sb.ToString();
      JsonSerializer serializer = new JsonSerializer() { };
      serializer.Deserialize<Nest>(new JsonTextReader(new StringReader(json)));
    }

    public class Nest
    {
      public Nest A { get; set; }
    }
  }
}