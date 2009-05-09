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
using System.Text;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Newtonsoft.Json.Tests
{
  public class JsonConvertTest : TestFixtureBase
  {
#if Entities
    [Test]
    public void EntitiesTest()
    {
      Purchase purchase = new Purchase() { Id = 1 };
      purchase.PurchaseLine.Add(new PurchaseLine() { Id = 1, Purchase = purchase });
      purchase.PurchaseLine.Add(new PurchaseLine() { Id = 2, Purchase = purchase });

      StringWriter sw = new StringWriter();
      JsonSerializer serializer = new JsonSerializer();
      serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

      using (JsonWriter jw = new JsonTextWriter(sw))
      {
        jw.Formatting = Formatting.Indented;

        serializer.Serialize(jw, purchase);
      }

      string json = sw.ToString();

      Assert.AreEqual(@"{
  ""Id"": 1,
  ""PurchaseLine"": [
    {
      ""Id"": 1,
      ""PurchaseReference"": {
        ""EntityKey"": null,
        ""RelationshipName"": ""EntityDataModel.PurchasePurchaseLine"",
        ""SourceRoleName"": ""PurchaseLine"",
        ""TargetRoleName"": ""Purchase"",
        ""RelationshipSet"": null,
        ""IsLoaded"": false
      },
      ""EntityState"": 1,
      ""EntityKey"": null
    },
    {
      ""Id"": 2,
      ""PurchaseReference"": {
        ""EntityKey"": null,
        ""RelationshipName"": ""EntityDataModel.PurchasePurchaseLine"",
        ""SourceRoleName"": ""PurchaseLine"",
        ""TargetRoleName"": ""Purchase"",
        ""RelationshipSet"": null,
        ""IsLoaded"": false
      },
      ""EntityState"": 1,
      ""EntityKey"": null
    }
  ],
  ""EntityState"": 1,
  ""EntityKey"": null
}", json);

      Purchase newPurchase = JsonConvert.DeserializeObject<Purchase>(json);
      Assert.AreEqual(1, newPurchase.Id);

      Assert.AreEqual(2, newPurchase.PurchaseLine.Count);
      Assert.AreEqual(1, newPurchase.PurchaseLine.ElementAt(0).Id);
      Assert.AreEqual(2, newPurchase.PurchaseLine.ElementAt(1).Id);
    }
#endif

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
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation.")]
    public void ToStringInvalid()
    {
      JsonConvert.ToString(new Version(1, 0));
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

      value = new DateTime(JsonConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      Assert.AreEqual(@"""\/Date(0)\/""", JsonConvert.ToString(value));

      value = new DateTimeOffset(JsonConvert.InitialJavaScriptDateTicks, TimeSpan.Zero);
      Assert.AreEqual(@"""\/Date(0+0000)\/""", JsonConvert.ToString(value));

      value = null;
      Assert.AreEqual("null", JsonConvert.ToString(value));

      value = DBNull.Value;
      Assert.AreEqual("null", JsonConvert.ToString(value));

      value = "I am a string";
      Assert.AreEqual(@"""I am a string""", JsonConvert.ToString(value));

      value = true;
      Assert.AreEqual("true", JsonConvert.ToString(value));

      value = 'c';
      Assert.AreEqual(@"""c""", JsonConvert.ToString(value));
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "After parsing a value an unexpected character was encoutered: t. Line 1, position 20.")]
    public void TestInvalidStrings()
    {
      string orig = @"this is a string ""that has quotes"" ";

      string serialized = JsonConvert.SerializeObject(orig);

      // *** Make string invalid by stripping \" \"
      serialized = serialized.Replace(@"\""", "\"");

      string result = JsonConvert.DeserializeObject<string>(serialized);
    }

    [Test]
    public void DeserializeValueObjects()
    {
      int i = JsonConvert.DeserializeObject<int>("1");
      Assert.AreEqual(1, i);

#if !PocketPC
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
      Assert.AreEqual("1.01", JsonConvert.ToString(1.01));
      Assert.AreEqual("1.001", JsonConvert.ToString(1.001));
      Assert.AreEqual(JsonConvert.PositiveInfinity, JsonConvert.ToString(double.PositiveInfinity));
      Assert.AreEqual(JsonConvert.NegativeInfinity, JsonConvert.ToString(double.NegativeInfinity));
      Assert.AreEqual(JsonConvert.NaN, JsonConvert.ToString(double.NaN));
    }

    [Test]
    public void StringEscaping()
    {
      string v = @"It's a good day
""sunshine""";

      string json = JsonConvert.ToString(v);
      Assert.AreEqual(@"""It's a good day\r\n\""sunshine\""""", json);
    }
  }
}