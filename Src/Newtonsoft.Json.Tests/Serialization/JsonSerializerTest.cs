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
#if !SILVERLIGHT && !PocketPC && !NET20
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Web.Script.Serialization;
#endif
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json;
using System.IO;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json.Converters;
#if !PocketPC && !NET20
using System.Runtime.Serialization.Json;
#endif
using Newtonsoft.Json.Tests.TestObjects;
using System.Runtime.Serialization;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Reflection;
#if !NET20 && !SILVERLIGHT
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Linq.Expressions;
#endif
#if !(NET35 || NET20 || SILVERLIGHT)
using System.Dynamic;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
  public class JsonSerializerTest : TestFixtureBase
  {
    [Test]
    public void PersonTypedObjectDeserialization()
    {
      Store store = new Store();

      string jsonText = JsonConvert.SerializeObject(store);

      Store deserializedStore = (Store)JsonConvert.DeserializeObject(jsonText, typeof(Store));

      Assert.AreEqual(store.Establised, deserializedStore.Establised);
      Assert.AreEqual(store.product.Count, deserializedStore.product.Count);

      Console.WriteLine(jsonText);
    }

    [Test]
    public void TypedObjectDeserialization()
    {
      Product product = new Product();

      product.Name = "Apple";
      product.ExpiryDate = new DateTime(2008, 12, 28);
      product.Price = 3.99M;
      product.Sizes = new string[] { "Small", "Medium", "Large" };

      string output = JsonConvert.SerializeObject(product);
      //{
      //  "Name": "Apple",
      //  "ExpiryDate": "\/Date(1230375600000+1300)\/",
      //  "Price": 3.99,
      //  "Sizes": [
      //    "Small",
      //    "Medium",
      //    "Large"
      //  ]
      //}

      Product deserializedProduct = (Product)JsonConvert.DeserializeObject(output, typeof(Product));

      Assert.AreEqual("Apple", deserializedProduct.Name);
      Assert.AreEqual(new DateTime(2008, 12, 28), deserializedProduct.ExpiryDate);
      Assert.AreEqual(3.99, deserializedProduct.Price);
      Assert.AreEqual("Small", deserializedProduct.Sizes[0]);
      Assert.AreEqual("Medium", deserializedProduct.Sizes[1]);
      Assert.AreEqual("Large", deserializedProduct.Sizes[2]);
    }

    //[Test]
    //public void Advanced()
    //{
    //  Product product = new Product();
    //  product.ExpiryDate = new DateTime(2008, 12, 28);

    //  JsonSerializer serializer = new JsonSerializer();
    //  serializer.Converters.Add(new JavaScriptDateTimeConverter());
    //  serializer.NullValueHandling = NullValueHandling.Ignore;

    //  using (StreamWriter sw = new StreamWriter(@"c:\json.txt"))
    //  using (JsonWriter writer = new JsonTextWriter(sw))
    //  {
    //    serializer.Serialize(writer, product);
    //    // {"ExpiryDate":new Date(1230375600000),"Price":0}
    //  }
    //}

    [Test]
    public void JsonConvertSerializer()
    {
      string value = @"{""Name"":""Orange"", ""Price"":3.99, ""ExpiryDate"":""01/24/2010 12:00:00""}";

      Product p = JsonConvert.DeserializeObject(value, typeof(Product)) as Product;

      Assert.AreEqual("Orange", p.Name);
      Assert.AreEqual(new DateTime(2010, 1, 24, 12, 0, 0), p.ExpiryDate);
      Assert.AreEqual(3.99, p.Price);
    }

    [Test]
    public void DeserializeJavaScriptDate()
    {
      DateTime dateValue = new DateTime(2010, 3, 30);
      Dictionary<string, object> testDictionary = new Dictionary<string, object>();
      testDictionary["date"] = dateValue;

      string jsonText = JsonConvert.SerializeObject(testDictionary);

#if !PocketPC && !NET20
      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(Dictionary<string, object>));
      serializer.WriteObject(ms, testDictionary);

      byte[] data = ms.ToArray();
      string output = Encoding.UTF8.GetString(data, 0, data.Length);
#endif

      Dictionary<string, object> deserializedDictionary = (Dictionary<string, object>)JsonConvert.DeserializeObject(jsonText, typeof(Dictionary<string, object>));
      DateTime deserializedDate = (DateTime)deserializedDictionary["date"];

      Assert.AreEqual(dateValue, deserializedDate);

      Console.WriteLine("DeserializeJavaScriptDate");
      Console.WriteLine(jsonText);
      Console.WriteLine();
      Console.WriteLine(jsonText);
    }

    [Test]
    public void TestMethodExecutorObject()
    {
      MethodExecutorObject executorObject = new MethodExecutorObject();
      executorObject.serverClassName = "BanSubs";
      executorObject.serverMethodParams = new object[] { "21321546", "101", "1236", "D:\\1.txt" };
      executorObject.clientGetResultFunction = "ClientBanSubsCB";

      string output = JsonConvert.SerializeObject(executorObject);

      MethodExecutorObject executorObject2 = JsonConvert.DeserializeObject(output, typeof(MethodExecutorObject)) as MethodExecutorObject;

      Assert.AreNotSame(executorObject, executorObject2);
      Assert.AreEqual(executorObject2.serverClassName, "BanSubs");
      Assert.AreEqual(executorObject2.serverMethodParams.Length, 4);
      Assert.Contains("101", executorObject2.serverMethodParams);
      Assert.AreEqual(executorObject2.clientGetResultFunction, "ClientBanSubsCB");
    }

#if !SILVERLIGHT
    [Test]
    public void HashtableDeserialization()
    {
      string value = @"{""Name"":""Orange"", ""Price"":3.99, ""ExpiryDate"":""01/24/2010 12:00:00""}";

      Hashtable p = JsonConvert.DeserializeObject(value, typeof(Hashtable)) as Hashtable;

      Assert.AreEqual("Orange", p["Name"].ToString());
    }

    [Test]
    public void TypedHashtableDeserialization()
    {
      string value = @"{""Name"":""Orange"", ""Hash"":{""ExpiryDate"":""01/24/2010 12:00:00"",""UntypedArray"":[""01/24/2010 12:00:00""]}}";

      TypedSubHashtable p = JsonConvert.DeserializeObject(value, typeof(TypedSubHashtable)) as TypedSubHashtable;

      Assert.AreEqual("01/24/2010 12:00:00", p.Hash["ExpiryDate"].ToString());
      Assert.AreEqual(@"[
  ""01/24/2010 12:00:00""
]", p.Hash["UntypedArray"].ToString());
    }
#endif

    [Test]
    public void SerializeDeserializeGetOnlyProperty()
    {
      string value = JsonConvert.SerializeObject(new GetOnlyPropertyClass());

      GetOnlyPropertyClass c = JsonConvert.DeserializeObject<GetOnlyPropertyClass>(value);

      Assert.AreEqual(c.Field, "Field");
      Assert.AreEqual(c.GetOnlyProperty, "GetOnlyProperty");
    }

    [Test]
    public void SerializeDeserializeSetOnlyProperty()
    {
      string value = JsonConvert.SerializeObject(new SetOnlyPropertyClass());

      SetOnlyPropertyClass c = JsonConvert.DeserializeObject<SetOnlyPropertyClass>(value);

      Assert.AreEqual(c.Field, "Field");
    }

    [Test]
    public void JsonIgnoreAttributeTest()
    {
      string json = JsonConvert.SerializeObject(new JsonIgnoreAttributeTestClass());

      Assert.AreEqual(@"{""Field"":0,""Property"":21}", json);

      JsonIgnoreAttributeTestClass c = JsonConvert.DeserializeObject<JsonIgnoreAttributeTestClass>(@"{""Field"":99,""Property"":-1,""IgnoredField"":-1,""IgnoredObject"":[1,2,3,4,5]}");

      Assert.AreEqual(0, c.IgnoredField);
      Assert.AreEqual(99, c.Field);
    }

    [Test]
    public void GoogleSearchAPI()
    {
      string json = @"{
    results:
        [
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://www.google.com/"",
                url : ""http://www.google.com/"",
                visibleUrl : ""www.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:zhool8dxBV4J:www.google.com"",
                title : ""Google"",
                titleNoFormatting : ""Google"",
                content : ""Enables users to search the Web, Usenet, and 
images. Features include PageRank,   caching and translation of 
results, and an option to find similar pages.""
            },
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://news.google.com/"",
                url : ""http://news.google.com/"",
                visibleUrl : ""news.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:Va_XShOz_twJ:news.google.com"",
                title : ""Google News"",
                titleNoFormatting : ""Google News"",
                content : ""Aggregated headlines and a search engine of many of the world's news sources.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://groups.google.com/"",
                url : ""http://groups.google.com/"",
                visibleUrl : ""groups.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:x2uPD3hfkn0J:groups.google.com"",
                title : ""Google Groups"",
                titleNoFormatting : ""Google Groups"",
                content : ""Enables users to search and browse the Usenet 
archives which consist of over 700   million messages, and post new 
comments.""
            },
            
            {
                GsearchResultClass:""GwebSearch"",
                unescapedUrl : ""http://maps.google.com/"",
                url : ""http://maps.google.com/"",
                visibleUrl : ""maps.google.com"",
                cacheUrl : 
""http://www.google.com/search?q=cache:dkf5u2twBXIJ:maps.google.com"",
                title : ""Google Maps"",
                titleNoFormatting : ""Google Maps"",
                content : ""Provides directions, interactive maps, and 
satellite/aerial imagery of the United   States. Can also search by 
keyword such as type of business.""
            }
        ],
        
    adResults:
        [
            {
                GsearchResultClass:""GwebSearch.ad"",
                title : ""Gartner Symposium/ITxpo"",
                content1 : ""Meet brilliant Gartner IT analysts"",
                content2 : ""20-23 May 2007- Barcelona, Spain"",
                url : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                impressionUrl : 
""http://www.google.com/uds/css/ad-indicator-on.gif?ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB"", 

                unescapedUrl : 
""http://www.google.com/url?sa=L&ai=BVualExYGRo3hD5ianAPJvejjD8-s6ye7kdTwArbI4gTAlrECEAEYASDXtMMFOAFQubWAjvr_____AWDXw_4EiAEBmAEAyAEBgAIB&num=1&q=http://www.gartner.com/it/sym/2007/spr8/spr8.jsp%3Fsrc%3D_spain_07_%26WT.srch%3D1&usg=__CxRH06E4Xvm9Muq13S4MgMtnziY="", 

                visibleUrl : ""www.gartner.com""
            }
        ]
}
";
      object o = JsonConvert.DeserializeObject(json);
      string s = string.Empty;
      s += s;
    }

    [Test]
    public void TorrentDeserializeTest()
    {
      string jsonText = @"{
"""":"""",
""label"": [
       [""SomeName"",6]
],
""torrents"": [
       [""192D99A5C943555CB7F00A852821CF6D6DB3008A"",201,""filename.avi"",178311826,1000,178311826,72815250,408,1603,7,121430,""NameOfLabelPrevioslyDefined"",3,6,0,8,128954,-1,0],
],
""torrentc"": ""1816000723""
}";

      JObject o = (JObject)JsonConvert.DeserializeObject(jsonText);
      Assert.AreEqual(4, o.Children().Count());

      JToken torrentsArray = (JToken)o["torrents"];
      JToken nestedTorrentsArray = (JToken)torrentsArray[0];
      Assert.AreEqual(nestedTorrentsArray.Children().Count(), 19);
    }

    [Test]
    public void JsonPropertyClassSerialize()
    {
      JsonPropertyClass test = new JsonPropertyClass();
      test.Pie = "Delicious";
      test.SweetCakesCount = int.MaxValue;

      string jsonText = JsonConvert.SerializeObject(test);

      Assert.AreEqual(@"{""pie"":""Delicious"",""pie1"":""PieChart!"",""sweet_cakes_count"":2147483647}", jsonText);

      JsonPropertyClass test2 = JsonConvert.DeserializeObject<JsonPropertyClass>(jsonText);

      Assert.AreEqual(test.Pie, test2.Pie);
      Assert.AreEqual(test.SweetCakesCount, test2.SweetCakesCount);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"A member with the name 'pie' already exists on 'Newtonsoft.Json.Tests.TestObjects.BadJsonPropertyClass'. Use the JsonPropertyAttribute to specify another name.")]
    public void BadJsonPropertyClassSerialize()
    {
      JsonConvert.SerializeObject(new BadJsonPropertyClass());
    }

    [Test]
    public void InheritedListSerialize()
    {
      Article a1 = new Article("a1");
      Article a2 = new Article("a2");

      ArticleCollection articles1 = new ArticleCollection();
      articles1.Add(a1);
      articles1.Add(a2);

      string jsonText = JsonConvert.SerializeObject(articles1);

      ArticleCollection articles2 = JsonConvert.DeserializeObject<ArticleCollection>(jsonText);

      Assert.AreEqual(articles1.Count, articles2.Count);
      Assert.AreEqual(articles1[0].Name, articles2[0].Name);
    }

    [Test]
    public void ReadOnlyCollectionSerialize()
    {
      ReadOnlyCollection<int> r1 = new ReadOnlyCollection<int>(new int[] { 0, 1, 2, 3, 4 });

      string jsonText = JsonConvert.SerializeObject(r1);

      ReadOnlyCollection<int> r2 = JsonConvert.DeserializeObject<ReadOnlyCollection<int>>(jsonText);

      CollectionAssert.AreEqual(r1, r2);
    }

#if !PocketPC && !NET20
    [Test]
    public void Unicode()
    {
      string json = @"[""PRE\u003cPOST""]";

      DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<string>));
      List<string> dataContractResult = (List<string>)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));

      List<string> jsonNetResult = JsonConvert.DeserializeObject<List<string>>(json);

      Assert.AreEqual(1, jsonNetResult.Count);
      Assert.AreEqual(dataContractResult[0], jsonNetResult[0]);
    }

    [Test]
    public void BackslashEqivilence()
    {
      string json = @"[""vvv\/vvv\tvvv\""vvv\bvvv\nvvv\rvvv\\vvv\fvvv""]";

#if !SILVERLIGHT
      JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
      List<string> javaScriptSerializerResult = javaScriptSerializer.Deserialize<List<string>>(json);
#endif

      DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<string>));
      List<string> dataContractResult = (List<string>)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));

      List<string> jsonNetResult = JsonConvert.DeserializeObject<List<string>>(json);

      Assert.AreEqual(1, jsonNetResult.Count);
      Assert.AreEqual(dataContractResult[0], jsonNetResult[0]);
#if !SILVERLIGHT
      Assert.AreEqual(javaScriptSerializerResult[0], jsonNetResult[0]);
#endif
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = @"Bad JSON escape sequence: \j. Line 1, position 7.")]
    public void InvalidBackslash()
    {
      string json = @"[""vvv\jvvv""]";

      JsonConvert.DeserializeObject<List<string>>(json);
    }

    [Test]
    public void DateTimeTest()
    {
      List<DateTime> testDates = new List<DateTime> {
        new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Local),
        new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Unspecified),
        new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc),
        new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Local),
        new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified),
        new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc),
      };
      string result;


      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<DateTime>));
      s.WriteObject(ms, testDates);
      ms.Seek(0, SeekOrigin.Begin);
      StreamReader sr = new StreamReader(ms);

      string expected = sr.ReadToEnd();

      result = JsonConvert.SerializeObject(testDates);
      Assert.AreEqual(expected, result);
    }

    [Test]
    public void DateTimeOffset()
    {
      List<DateTimeOffset> testDates = new List<DateTimeOffset> {
        new DateTimeOffset(new DateTime(100, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
        new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero),
        new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13)),
        new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(-3.5)),
      };

      string result = JsonConvert.SerializeObject(testDates);
      Assert.AreEqual(@"[""\/Date(-59011455539000+0000)\/"",""\/Date(946688461000+0000)\/"",""\/Date(946641661000+1300)\/"",""\/Date(946701061000-0330)\/""]", result);
    }
#endif

    [Test]
    public void NonStringKeyDictionary()
    {
      Dictionary<int, int> values = new Dictionary<int, int>();
      values.Add(-5, 6);
      values.Add(int.MinValue, int.MaxValue);

      string json = JsonConvert.SerializeObject(values);

      Assert.AreEqual(@"{""-5"":6,""-2147483648"":2147483647}", json);

      Dictionary<int, int> newValues = JsonConvert.DeserializeObject<Dictionary<int, int>>(json);

      CollectionAssert.AreEqual(values, newValues);
    }

    [Test]
    public void AnonymousObjectSerialization()
    {
      var anonymous =
        new
        {
          StringValue = "I am a string",
          IntValue = int.MaxValue,
          NestedAnonymous = new { NestedValue = byte.MaxValue },
          NestedArray = new[] { 1, 2 },
          Product = new Product() { Name = "TestProduct" }
        };

      string json = JsonConvert.SerializeObject(anonymous);
      Assert.AreEqual(@"{""StringValue"":""I am a string"",""IntValue"":2147483647,""NestedAnonymous"":{""NestedValue"":255},""NestedArray"":[1,2],""Product"":{""Name"":""TestProduct"",""ExpiryDate"":""\/Date(946684800000)\/"",""Price"":0.0,""Sizes"":null}}", json);

      anonymous = JsonConvert.DeserializeAnonymousType(json, anonymous);
      Assert.AreEqual("I am a string", anonymous.StringValue);
      Assert.AreEqual(int.MaxValue, anonymous.IntValue);
      Assert.AreEqual(255, anonymous.NestedAnonymous.NestedValue);
      Assert.AreEqual(2, anonymous.NestedArray.Length);
      Assert.AreEqual(1, anonymous.NestedArray[0]);
      Assert.AreEqual(2, anonymous.NestedArray[1]);
      Assert.AreEqual("TestProduct", anonymous.Product.Name);
    }

    [Test]
    public void CustomCollectionSerialization()
    {
      ProductCollection collection = new ProductCollection()
      {
        new Product() { Name = "Test1" },
        new Product() { Name = "Test2" },
        new Product() { Name = "Test3" }
      };

      JsonSerializer jsonSerializer = new JsonSerializer();
      jsonSerializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

      StringWriter sw = new StringWriter();

      jsonSerializer.Serialize(sw, collection);

      Assert.AreEqual(@"[{""Name"":""Test1"",""ExpiryDate"":""\/Date(946684800000)\/"",""Price"":0.0,""Sizes"":null},{""Name"":""Test2"",""ExpiryDate"":""\/Date(946684800000)\/"",""Price"":0.0,""Sizes"":null},{""Name"":""Test3"",""ExpiryDate"":""\/Date(946684800000)\/"",""Price"":0.0,""Sizes"":null}]",
        sw.GetStringBuilder().ToString());

      ProductCollection collectionNew = (ProductCollection)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(sw.GetStringBuilder().ToString())), typeof(ProductCollection));

      CollectionAssert.AreEqual(collection, collectionNew);
    }

    [Test]
    public void SerializeObject()
    {
      string json = JsonConvert.SerializeObject(new object());
      Assert.AreEqual("{}", json);
    }

    [Test]
    public void SerializeNull()
    {
      string json = JsonConvert.SerializeObject(null);
      Assert.AreEqual("null", json);
    }

    [Test]
    public void CanDeserializeIntArrayWhenNotFirstPropertyInJson()
    {
      string json = "{foo:'hello',bar:[1,2,3]}";
      ClassWithArray wibble = JsonConvert.DeserializeObject<ClassWithArray>(json);
      Assert.AreEqual("hello", wibble.Foo);

      Assert.AreEqual(4, wibble.Bar.Count);
      Assert.AreEqual(int.MaxValue, wibble.Bar[0]);
      Assert.AreEqual(1, wibble.Bar[1]);
      Assert.AreEqual(2, wibble.Bar[2]);
      Assert.AreEqual(3, wibble.Bar[3]);
    }

    [Test]
    public void CanDeserializeIntArray_WhenArrayIsFirstPropertyInJson()
    {
      string json = "{bar:[1,2,3], foo:'hello'}";
      ClassWithArray wibble = JsonConvert.DeserializeObject<ClassWithArray>(json);
      Assert.AreEqual("hello", wibble.Foo);

      Assert.AreEqual(4, wibble.Bar.Count);
      Assert.AreEqual(int.MaxValue, wibble.Bar[0]);
      Assert.AreEqual(1, wibble.Bar[1]);
      Assert.AreEqual(2, wibble.Bar[2]);
      Assert.AreEqual(3, wibble.Bar[3]);
    }

    [Test]
    public void ObjectCreationHandlingReplace()
    {
      string json = "{bar:[1,2,3], foo:'hello'}";

      JsonSerializer s = new JsonSerializer();
      s.ObjectCreationHandling = ObjectCreationHandling.Replace;

      ClassWithArray wibble = (ClassWithArray)s.Deserialize(new StringReader(json), typeof(ClassWithArray));

      Assert.AreEqual("hello", wibble.Foo);

      Assert.AreEqual(1, wibble.Bar.Count);
    }

    [Test]
    public void CanDeserializeSerializedJson()
    {
      ClassWithArray wibble = new ClassWithArray();
      wibble.Foo = "hello";
      wibble.Bar.Add(1);
      wibble.Bar.Add(2);
      wibble.Bar.Add(3);
      string json = JsonConvert.SerializeObject(wibble);

      ClassWithArray wibbleOut = JsonConvert.DeserializeObject<ClassWithArray>(json);
      Assert.AreEqual("hello", wibbleOut.Foo);

      Assert.AreEqual(5, wibbleOut.Bar.Count);
      Assert.AreEqual(int.MaxValue, wibbleOut.Bar[0]);
      Assert.AreEqual(int.MaxValue, wibbleOut.Bar[1]);
      Assert.AreEqual(1, wibbleOut.Bar[2]);
      Assert.AreEqual(2, wibbleOut.Bar[3]);
      Assert.AreEqual(3, wibbleOut.Bar[4]);
    }

    [Test]
    public void SerializeConverableObjects()
    {
      string json = JsonConvert.SerializeObject(new ConverableMembers());

      Assert.AreEqual(@"{""String"":""string"",""Int32"":2147483647,""UInt32"":4294967295,""Byte"":255,""SByte"":127,""Short"":32767,""UShort"":65535,""Long"":9223372036854775807,""ULong"":9223372036854775807,""Double"":1.7976931348623157E+308,""Float"":3.40282347E+38,""DBNull"":null,""Bool"":true,""Char"":""\u0000""}", json);

      ConverableMembers c = JsonConvert.DeserializeObject<ConverableMembers>(json);
      Assert.AreEqual("string", c.String);
      Assert.AreEqual(double.MaxValue, c.Double);
      Assert.AreEqual(DBNull.Value, c.DBNull);
    }

    [Test]
    public void SerializeStack()
    {
      Stack<object> s = new Stack<object>();
      s.Push(1);
      s.Push(2);
      s.Push(3);

      string json = JsonConvert.SerializeObject(s);
      Assert.AreEqual("[3,2,1]", json);
    }

    [Test]
    public void GuidTest()
    {
      Guid guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");

      string json = JsonConvert.SerializeObject(new ClassWithGuid { GuidField = guid });
      Assert.AreEqual(@"{""GuidField"":""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""}", json);

      ClassWithGuid c = JsonConvert.DeserializeObject<ClassWithGuid>(json);
      Assert.AreEqual(guid, c.GuidField);
    }

    [Test]
    public void EnumTest()
    {
      string json = JsonConvert.SerializeObject(StringComparison.CurrentCultureIgnoreCase);
      Assert.AreEqual(@"1", json);

      StringComparison s = JsonConvert.DeserializeObject<StringComparison>(json);
      Assert.AreEqual(StringComparison.CurrentCultureIgnoreCase, s);
    }

    public class ClassWithTimeSpan
    {
      public TimeSpan TimeSpanField;
    }

    [Test]
    public void TimeSpanTest()
    {
      TimeSpan ts = new TimeSpan(00, 23, 59, 1);

      string json = JsonConvert.SerializeObject(new ClassWithTimeSpan { TimeSpanField = ts }, Formatting.Indented);
      Assert.AreEqual(@"{
  ""TimeSpanField"": ""23:59:01""
}", json);

      ClassWithTimeSpan c = JsonConvert.DeserializeObject<ClassWithTimeSpan>(json);
      Assert.AreEqual(ts, c.TimeSpanField);
    }

    [Test]
    public void JsonIgnoreAttributeOnClassTest()
    {
      string json = JsonConvert.SerializeObject(new JsonIgnoreAttributeOnClassTestClass());

      Assert.AreEqual(@"{""TheField"":0,""Property"":21}", json);

      JsonIgnoreAttributeOnClassTestClass c = JsonConvert.DeserializeObject<JsonIgnoreAttributeOnClassTestClass>(@"{""TheField"":99,""Property"":-1,""IgnoredField"":-1}");

      Assert.AreEqual(0, c.IgnoredField);
      Assert.AreEqual(99, c.Field);
    }

#if !SILVERLIGHT
    [Test]
    public void SerializeArrayAsArrayList()
    {
      string jsonText = @"[3, ""somestring"",[1,2,3],{}]";
      ArrayList o = JsonConvert.DeserializeObject<ArrayList>(jsonText);

      Assert.AreEqual(4, o.Count);
      Assert.AreEqual(3, ((JArray)o[2]).Count);
      Assert.AreEqual(0, ((JObject)o[3]).Count);
    }
#endif

    [Test]
    public void SerializeMemberGenericList()
    {
      Name name = new Name("The Idiot in Next To Me");

      PhoneNumber p1 = new PhoneNumber("555-1212");
      PhoneNumber p2 = new PhoneNumber("444-1212");

      name.pNumbers.Add(p1);
      name.pNumbers.Add(p2);

      string json = JsonConvert.SerializeObject(name, Formatting.Indented);

      Assert.AreEqual(@"{
  ""personsName"": ""The Idiot in Next To Me"",
  ""pNumbers"": [
    {
      ""phoneNumber"": ""555-1212""
    },
    {
      ""phoneNumber"": ""444-1212""
    }
  ]
}", json);

      Name newName = JsonConvert.DeserializeObject<Name>(json);

      Assert.AreEqual("The Idiot in Next To Me", newName.personsName);

      // not passed in as part of the constructor but assigned to pNumbers property
      Assert.AreEqual(2, newName.pNumbers.Count);
      Assert.AreEqual("555-1212", newName.pNumbers[0].phoneNumber);
      Assert.AreEqual("444-1212", newName.pNumbers[1].phoneNumber);
    }

    [Test]
    public void ConstructorCaseSensitivity()
    {
      ConstructorCaseSensitivityClass c = new ConstructorCaseSensitivityClass("param1", "Param1", "Param2");

      string json = JsonConvert.SerializeObject(c);

      ConstructorCaseSensitivityClass deserialized = JsonConvert.DeserializeObject<ConstructorCaseSensitivityClass>(json);

      Assert.AreEqual("param1", deserialized.param1);
      Assert.AreEqual("Param1", deserialized.Param1);
      Assert.AreEqual("Param2", deserialized.Param2);
    }

    [Test]
    public void SerializerShouldUseClassConverter()
    {
      ConverterPrecedenceClass c1 = new ConverterPrecedenceClass("!Test!");

      string json = JsonConvert.SerializeObject(c1);
      Assert.AreEqual(@"[""Class"",""!Test!""]", json);

      ConverterPrecedenceClass c2 = JsonConvert.DeserializeObject<ConverterPrecedenceClass>(json);

      Assert.AreEqual("!Test!", c2.TestValue);
    }

    [Test]
    public void SerializerShouldUseClassConverterOverArgumentConverter()
    {
      ConverterPrecedenceClass c1 = new ConverterPrecedenceClass("!Test!");

      string json = JsonConvert.SerializeObject(c1, new ArgumentConverterPrecedenceClassConverter());
      Assert.AreEqual(@"[""Class"",""!Test!""]", json);

      ConverterPrecedenceClass c2 = JsonConvert.DeserializeObject<ConverterPrecedenceClass>(json, new ArgumentConverterPrecedenceClassConverter());

      Assert.AreEqual("!Test!", c2.TestValue);
    }

    [Test]
    public void SerializerShouldUseMemberConverter()
    {
      DateTime testDate = new DateTime(JsonConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

      string json = JsonConvert.SerializeObject(m1);
      Assert.AreEqual(@"{""DefaultConverter"":""\/Date(0)\/"",""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

      MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json);

      Assert.AreEqual(testDate, m2.DefaultConverter);
      Assert.AreEqual(testDate, m2.MemberConverter);
    }

    [Test]
    public void SerializerShouldUseMemberConverterOverArgumentConverter()
    {
      DateTime testDate = new DateTime(JsonConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

      string json = JsonConvert.SerializeObject(m1, new JavaScriptDateTimeConverter());
      Assert.AreEqual(@"{""DefaultConverter"":new Date(0),""MemberConverter"":""1970-01-01T00:00:00Z""}", json);

      MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json, new JavaScriptDateTimeConverter());

      Assert.AreEqual(testDate, m2.DefaultConverter);
      Assert.AreEqual(testDate, m2.MemberConverter);
    }

    [Test]
    public void ConverterAttributeExample()
    {
      DateTime date = Convert.ToDateTime("1970-01-01T00:00:00Z").ToUniversalTime();

      MemberConverterClass c = new MemberConverterClass
        {
          DefaultConverter = date,
          MemberConverter = date
        };

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Console.WriteLine(json);
      //{
      //  "DefaultConverter": "\/Date(0)\/",
      //  "MemberConverter": "1970-01-01T00:00:00Z"
      //}
    }

    [Test]
    public void SerializerShouldUseMemberConverterOverClassAndArgumentConverter()
    {
      ClassAndMemberConverterClass c1 = new ClassAndMemberConverterClass();
      c1.DefaultConverter = new ConverterPrecedenceClass("DefaultConverterValue");
      c1.MemberConverter = new ConverterPrecedenceClass("MemberConverterValue");

      string json = JsonConvert.SerializeObject(c1, new ArgumentConverterPrecedenceClassConverter());
      Assert.AreEqual(@"{""DefaultConverter"":[""Class"",""DefaultConverterValue""],""MemberConverter"":[""Member"",""MemberConverterValue""]}", json);

      ClassAndMemberConverterClass c2 = JsonConvert.DeserializeObject<ClassAndMemberConverterClass>(json, new ArgumentConverterPrecedenceClassConverter());

      Assert.AreEqual("DefaultConverterValue", c2.DefaultConverter.TestValue);
      Assert.AreEqual("MemberConverterValue", c2.MemberConverter.TestValue);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JsonConverter IsoDateTimeConverter on Newtonsoft.Json.Tests.TestObjects.IncompatibleJsonAttributeClass is not compatible with member type IncompatibleJsonAttributeClass.")]
    public void IncompatibleJsonAttributeShouldThrow()
    {
      IncompatibleJsonAttributeClass c = new IncompatibleJsonAttributeClass();
      JsonConvert.SerializeObject(c);
    }

    [Test]
    public void GenericAbstractProperty()
    {
      string json = JsonConvert.SerializeObject(new GenericImpl());
      Assert.AreEqual(@"{""Id"":0}", json);
    }

    [Test]
    public void DeserializeNullable()
    {
      string json;

      json = JsonConvert.SerializeObject((int?)null);
      Assert.AreEqual("null", json);

      json = JsonConvert.SerializeObject((int?)1);
      Assert.AreEqual("1", json);
    }

    [Test]
    public void SerializeJsonRaw()
    {
      PersonRaw personRaw = new PersonRaw
      {
        FirstName = "FirstNameValue",
        RawContent = new JRaw("[1,2,3,4,5]"),
        LastName = "LastNameValue"
      };

      string json;

      json = JsonConvert.SerializeObject(personRaw);
      Assert.AreEqual(@"{""first_name"":""FirstNameValue"",""RawContent"":[1,2,3,4,5],""last_name"":""LastNameValue""}", json);
    }

    [Test]
    public void DeserializeJsonRaw()
    {
      string json = @"{""first_name"":""FirstNameValue"",""RawContent"":[1,2,3,4,5],""last_name"":""LastNameValue""}";

      PersonRaw personRaw = JsonConvert.DeserializeObject<PersonRaw>(json);

      Assert.AreEqual("FirstNameValue", personRaw.FirstName);
      Assert.AreEqual("[1,2,3,4,5]", personRaw.RawContent.ToString());
      Assert.AreEqual("LastNameValue", personRaw.LastName);
    }


    [Test]
    public void DeserializeNullableMember()
    {
      UserNullable userNullablle = new UserNullable
                                    {
                                      Id = new Guid("AD6205E8-0DF4-465d-AEA6-8BA18E93A7E7"),
                                      FName = "FirstValue",
                                      LName = "LastValue",
                                      RoleId = 5,
                                      NullableRoleId = 6,
                                      NullRoleId = null,
                                      Active = true
                                    };

      string json = JsonConvert.SerializeObject(userNullablle);

      Assert.AreEqual(@"{""Id"":""ad6205e8-0df4-465d-aea6-8ba18e93a7e7"",""FName"":""FirstValue"",""LName"":""LastValue"",""RoleId"":5,""NullableRoleId"":6,""NullRoleId"":null,""Active"":true}", json);

      UserNullable userNullablleDeserialized = JsonConvert.DeserializeObject<UserNullable>(json);

      Assert.AreEqual(new Guid("AD6205E8-0DF4-465d-AEA6-8BA18E93A7E7"), userNullablleDeserialized.Id);
      Assert.AreEqual("FirstValue", userNullablleDeserialized.FName);
      Assert.AreEqual("LastValue", userNullablleDeserialized.LName);
      Assert.AreEqual(5, userNullablleDeserialized.RoleId);
      Assert.AreEqual(6, userNullablleDeserialized.NullableRoleId);
      Assert.AreEqual(null, userNullablleDeserialized.NullRoleId);
      Assert.AreEqual(true, userNullablleDeserialized.Active);
    }

    [Test]
    public void DeserializeInt64ToNullableDouble()
    {
      string json = @"{""Height"":1}";

      DoubleClass c = JsonConvert.DeserializeObject<DoubleClass>(json);
      Assert.AreEqual(1, c.Height);
    }

    [Test]
    public void SerializeTypeProperty()
    {
      string boolRef = typeof(bool).AssemblyQualifiedName;
      TypeClass typeClass = new TypeClass { TypeProperty = typeof(bool) };

      string json = JsonConvert.SerializeObject(typeClass);
      Assert.AreEqual(@"{""TypeProperty"":""" + boolRef + @"""}", json);

      TypeClass typeClass2 = JsonConvert.DeserializeObject<TypeClass>(json);
      Assert.AreEqual(typeof(bool), typeClass2.TypeProperty);

      string jsonSerializerTestRef = typeof(JsonSerializerTest).AssemblyQualifiedName;
      typeClass = new TypeClass { TypeProperty = typeof(JsonSerializerTest) };

      json = JsonConvert.SerializeObject(typeClass);
      Assert.AreEqual(@"{""TypeProperty"":""" + jsonSerializerTestRef + @"""}", json);

      typeClass2 = JsonConvert.DeserializeObject<TypeClass>(json);
      Assert.AreEqual(typeof(JsonSerializerTest), typeClass2.TypeProperty);
    }

    [Test]
    public void RequiredMembersClass()
    {
      RequiredMembersClass c = new RequiredMembersClass()
      {
        BirthDate = new DateTime(2000, 12, 20, 10, 55, 55, DateTimeKind.Utc),
        FirstName = "Bob",
        LastName = "Smith",
        MiddleName = "Cosmo"
      };

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"{
  ""FirstName"": ""Bob"",
  ""MiddleName"": ""Cosmo"",
  ""LastName"": ""Smith"",
  ""BirthDate"": ""\/Date(977309755000)\/""
}", json);

      RequiredMembersClass c2 = JsonConvert.DeserializeObject<RequiredMembersClass>(json);

      Assert.AreEqual("Bob", c2.FirstName);
      Assert.AreEqual(new DateTime(2000, 12, 20, 10, 55, 55, DateTimeKind.Utc), c2.BirthDate);
    }

    [Test]
    public void DeserializeRequiredMembersClassWithNullValues()
    {
      string json = @"{
  ""FirstName"": ""I can't be null bro!"",
  ""MiddleName"": null,
  ""LastName"": null,
  ""BirthDate"": ""\/Date(977309755000)\/""
}";

      RequiredMembersClass c = JsonConvert.DeserializeObject<RequiredMembersClass>(json);

      Assert.AreEqual("I can't be null bro!", c.FirstName);
      Assert.AreEqual(null, c.MiddleName);
      Assert.AreEqual(null, c.LastName);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Required property 'FirstName' expects a value but got null.")]
    public void DeserializeRequiredMembersClassNullRequiredValueProperty()
    {
      string json = @"{
  ""FirstName"": null,
  ""MiddleName"": null,
  ""LastName"": null,
  ""BirthDate"": ""\/Date(977309755000)\/""
}";

      JsonConvert.DeserializeObject<RequiredMembersClass>(json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Cannot write a null value for property 'FirstName'. Property requires a value.")]
    public void SerializeRequiredMembersClassNullRequiredValueProperty()
    {
      RequiredMembersClass requiredMembersClass = new RequiredMembersClass
        {
          FirstName = null,
          BirthDate = new DateTime(2000, 10, 10, 10, 10, 10, DateTimeKind.Utc),
          LastName = null,
          MiddleName = null
        };

      string json = JsonConvert.SerializeObject(requiredMembersClass);
      Console.WriteLine(json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Required property 'LastName' not found in JSON.")]
    public void RequiredMembersClassMissingRequiredProperty()
    {
      string json = @"{
  ""FirstName"": ""Bob""
}";

      JsonConvert.DeserializeObject<RequiredMembersClass>(json);
    }

    [Test]
    public void SerializeJaggedArray()
    {
      JaggedArray aa = new JaggedArray();
      aa.Before = "Before!";
      aa.After = "After!";
      aa.Coordinates = new[] { new[] { 1, 1 }, new[] { 1, 2 }, new[] { 2, 1 }, new[] { 2, 2 } };

      string json = JsonConvert.SerializeObject(aa);

      Assert.AreEqual(@"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}", json);
    }

    [Test]
    public void DeserializeJaggedArray()
    {
      string json = @"{""Before"":""Before!"",""Coordinates"":[[1,1],[1,2],[2,1],[2,2]],""After"":""After!""}";

      JaggedArray aa = JsonConvert.DeserializeObject<JaggedArray>(json);

      Assert.AreEqual("Before!", aa.Before);
      Assert.AreEqual("After!", aa.After);
      Assert.AreEqual(4, aa.Coordinates.Length);
      Assert.AreEqual(2, aa.Coordinates[0].Length);
      Assert.AreEqual(1, aa.Coordinates[0][0]);
      Assert.AreEqual(2, aa.Coordinates[1][1]);

      string after = JsonConvert.SerializeObject(aa);

      Assert.AreEqual(json, after);
    }

    [Test]
    public void DeserializeGoogleGeoCode()
    {
      string json = @"{
  ""name"": ""1600 Amphitheatre Parkway, Mountain View, CA, USA"",
  ""Status"": {
    ""code"": 200,
    ""request"": ""geocode""
  },
  ""Placemark"": [
    {
      ""address"": ""1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA"",
      ""AddressDetails"": {
        ""Country"": {
          ""CountryNameCode"": ""US"",
          ""AdministrativeArea"": {
            ""AdministrativeAreaName"": ""CA"",
            ""SubAdministrativeArea"": {
              ""SubAdministrativeAreaName"": ""Santa Clara"",
              ""Locality"": {
                ""LocalityName"": ""Mountain View"",
                ""Thoroughfare"": {
                  ""ThoroughfareName"": ""1600 Amphitheatre Pkwy""
                },
                ""PostalCode"": {
                  ""PostalCodeNumber"": ""94043""
                }
              }
            }
          }
        },
        ""Accuracy"": 8
      },
      ""Point"": {
        ""coordinates"": [-122.083739, 37.423021, 0]
      }
    }
  ]
}";

      GoogleMapGeocoderStructure jsonGoogleMapGeocoder = JsonConvert.DeserializeObject<GoogleMapGeocoderStructure>(json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Could not create an instance of type Newtonsoft.Json.Tests.TestObjects.ICo. Type is an interface or abstract class and cannot be instantated.")]
    public void DeserializeInterfaceProperty()
    {
      InterfacePropertyTestClass testClass = new InterfacePropertyTestClass();
      testClass.co = new Co();
      String strFromTest = JsonConvert.SerializeObject(testClass);
      InterfacePropertyTestClass testFromDe = (InterfacePropertyTestClass)JsonConvert.DeserializeObject(strFromTest, typeof(InterfacePropertyTestClass));
    }

    private Person GetPerson()
    {
      Person person = new Person
                        {
                          Name = "Mike Manager",
                          BirthDate = new DateTime(1983, 8, 3, 0, 0, 0, DateTimeKind.Utc),
                          Department = "IT",
                          LastModified = new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc)
                        };
      return person;
    }

    //[Test]
    public void WriteJsonToFile()
    {
      //Person person = GetPerson();

      //string json = JsonConvert.SerializeObject(person, Formatting.Indented);

      //File.WriteAllText(@"c:\person.json", json);

      Person person = GetPerson();

      using (FileStream fs = System.IO.File.Open(@"c:\person.json", FileMode.CreateNew))
      using (StreamWriter sw = new StreamWriter(fs))
      using (JsonWriter jw = new JsonTextWriter(sw))
      {
        jw.Formatting = Formatting.Indented;

        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(jw, person);
      }
    }

    [Test]
    public void WriteJsonDates()
    {
      LogEntry entry = new LogEntry
                         {
                           LogDate = new DateTime(2009, 2, 15, 0, 0, 0, DateTimeKind.Utc),
                           Details = "Application started."
                         };

      string defaultJson = JsonConvert.SerializeObject(entry);
      // {"Details":"Application started.","LogDate":"\/Date(1234656000000)\/"}

      string isoJson = JsonConvert.SerializeObject(entry, new IsoDateTimeConverter());
      // {"Details":"Application started.","LogDate":"2009-02-15T00:00:00.0000000Z"}

      string javascriptJson = JsonConvert.SerializeObject(entry, new JavaScriptDateTimeConverter());
      // {"Details":"Application started.","LogDate":new Date(1234656000000)}

      Console.WriteLine(defaultJson);
      Console.WriteLine(isoJson);
      Console.WriteLine(javascriptJson);
    }

    public void GenericListAndDictionaryInterfaceProperties()
    {
      GenericListAndDictionaryInterfaceProperties o = new GenericListAndDictionaryInterfaceProperties();
      o.IDictionaryProperty = new Dictionary<string, int>
                                {
                                  {"one", 1},
                                  {"two", 2},
                                  {"three", 3}
                                };
      o.IListProperty = new List<int>
                          {
                            1, 2, 3
                          };
      o.IEnumerableProperty = new List<int>
                                {
                                  4, 5, 6
                                };

      string json = JsonConvert.SerializeObject(o, Formatting.Indented);

      Assert.AreEqual(@"{
  ""IEnumerableProperty"": [
    4,
    5,
    6
  ],
  ""IListProperty"": [
    1,
    2,
    3
  ],
  ""IDictionaryProperty"": {
    ""one"": 1,
    ""two"": 2,
    ""three"": 3
  }
}", json);

      GenericListAndDictionaryInterfaceProperties deserializedObject = JsonConvert.DeserializeObject<GenericListAndDictionaryInterfaceProperties>(json);
      Assert.IsNotNull(deserializedObject);

      CollectionAssert.AreEqual(o.IListProperty.ToArray(), deserializedObject.IListProperty.ToArray());
      CollectionAssert.AreEqual(o.IEnumerableProperty.ToArray(), deserializedObject.IEnumerableProperty.ToArray());
      CollectionAssert.AreEqual(o.IDictionaryProperty.ToArray(), deserializedObject.IDictionaryProperty.ToArray());
    }

    [Test]
    public void DeserializeBestMatchPropertyCase()
    {
      string json = @"{
  ""firstName"": ""firstName"",
  ""FirstName"": ""FirstName"",
  ""LastName"": ""LastName"",
  ""lastName"": ""lastName"",
}";

      PropertyCase o = JsonConvert.DeserializeObject<PropertyCase>(json);
      Assert.IsNotNull(o);

      Assert.AreEqual("firstName", o.firstName);
      Assert.AreEqual("FirstName", o.FirstName);
      Assert.AreEqual("LastName", o.LastName);
      Assert.AreEqual("lastName", o.lastName);
    }

    [Test]
    public void DeserializePropertiesOnToNonDefaultConstructor()
    {
      SubKlass i = new SubKlass("my subprop");
      i.SuperProp = "overrided superprop";

      string json = JsonConvert.SerializeObject(i);
      Assert.AreEqual(@"{""SubProp"":""my subprop"",""SuperProp"":""overrided superprop""}", json);

      SubKlass ii = JsonConvert.DeserializeObject<SubKlass>(json);

      string newJson = JsonConvert.SerializeObject(ii);
      Assert.AreEqual(@"{""SubProp"":""my subprop"",""SuperProp"":""overrided superprop""}", newJson);
    }

    [Test]
    public void JsonPropertyWithHandlingValues()
    {
      JsonPropertyWithHandlingValues o = new JsonPropertyWithHandlingValues();
      o.DefaultValueHandlingIgnoreProperty = "Default!";
      o.DefaultValueHandlingIncludeProperty = "Default!";

      string json = JsonConvert.SerializeObject(o, Formatting.Indented);

      Assert.AreEqual(@"{
  ""DefaultValueHandlingIncludeProperty"": ""Default!"",
  ""NullValueHandlingIncludeProperty"": null,
  ""ReferenceLoopHandlingErrorProperty"": null,
  ""ReferenceLoopHandlingIgnoreProperty"": null,
  ""ReferenceLoopHandlingSerializeProperty"": null
}", json);

      json = JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

      Assert.AreEqual(@"{
  ""DefaultValueHandlingIncludeProperty"": ""Default!"",
  ""NullValueHandlingIncludeProperty"": null
}", json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Self referencing loop")]
    public void JsonPropertyWithHandlingValues_ReferenceLoopError()
    {
      JsonPropertyWithHandlingValues o = new JsonPropertyWithHandlingValues();
      o.ReferenceLoopHandlingErrorProperty = o;

      JsonConvert.SerializeObject(o, Formatting.Indented, new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
    }

    [Test]
    public void PartialClassDeserialize()
    {
      string json = @"{
    ""request"": ""ux.settings.update"",
    ""sid"": ""14c561bd-32a8-457e-b4e5-4bba0832897f"",
    ""uid"": ""30c39065-0f31-de11-9442-001e3786a8ec"",
    ""fidOrder"": [
        ""id"",
        ""andytest_name"",
        ""andytest_age"",
        ""andytest_address"",
        ""andytest_phone"",
        ""date"",
        ""title"",
        ""titleId""
    ],
    ""entityName"": ""Andy Test"",
    ""setting"": ""entity.field.order""
}";

      RequestOnly r = JsonConvert.DeserializeObject<RequestOnly>(json);
      Assert.AreEqual("ux.settings.update", r.Request);

      NonRequest n = JsonConvert.DeserializeObject<NonRequest>(json);
      Assert.AreEqual(new Guid("14c561bd-32a8-457e-b4e5-4bba0832897f"), n.Sid);
      Assert.AreEqual(new Guid("30c39065-0f31-de11-9442-001e3786a8ec"), n.Uid);
      Assert.AreEqual(8, n.FidOrder.Count);
      Assert.AreEqual("id", n.FidOrder[0]);
      Assert.AreEqual("titleId", n.FidOrder[n.FidOrder.Count - 1]);
    }

#if !SILVERLIGHT && !PocketPC && !NET20
    [MetadataType(typeof(OptInClassMetadata))]
    public class OptInClass
    {
      [DataContract]
      public class OptInClassMetadata
      {
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public int Age { get; set; }
        public string NotIncluded { get; set; }
      }

      public string Name { get; set; }
      public int Age { get; set; }
      public string NotIncluded { get; set; }
    }

    [Test]
    public void OptInClassMetadataSerialization()
    {
      OptInClass optInClass = new OptInClass();
      optInClass.Age = 26;
      optInClass.Name = "James NK";
      optInClass.NotIncluded = "Poor me :(";

      string json = JsonConvert.SerializeObject(optInClass, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Name"": ""James NK"",
  ""Age"": 26
}", json);

      OptInClass newOptInClass = JsonConvert.DeserializeObject<OptInClass>(@"{
  ""Name"": ""James NK"",
  ""NotIncluded"": ""Ignore me!"",
  ""Age"": 26
}");
      Assert.AreEqual(26, newOptInClass.Age);
      Assert.AreEqual("James NK", newOptInClass.Name);
      Assert.AreEqual(null, newOptInClass.NotIncluded);
    }
#endif

#if !PocketPC && !NET20
    [DataContract]
    public class DataContractPrivateMembers
    {
      public DataContractPrivateMembers()
      {
      }

      public DataContractPrivateMembers(string name, int age, int rank, string title)
      {
        _name = name;
        Age = age;
        Rank = rank;
        Title = title;
      }

      [DataMember]
      private string _name;
      [DataMember(Name = "_age")]
      private int Age { get; set; }
      [JsonProperty]
      private int Rank { get; set; }
      [JsonProperty(PropertyName = "JsonTitle")]
      [DataMember(Name = "DataTitle")]
      private string Title { get; set; }

      public string NotIncluded { get; set; }

      public override string ToString()
      {
        return "_name: " + _name + ", _age: " + Age + ", Rank: " + Rank + ", JsonTitle: " + Title;
      }
    }

    [Test]
    public void SerializeDataContractPrivateMembers()
    {
      DataContractPrivateMembers c = new DataContractPrivateMembers("Jeff", 26, 10, "Dr");
      c.NotIncluded = "Hi";
      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"{
  ""_name"": ""Jeff"",
  ""_age"": 26,
  ""Rank"": 10,
  ""JsonTitle"": ""Dr""
}", json);

      DataContractPrivateMembers cc = JsonConvert.DeserializeObject<DataContractPrivateMembers>(json);
      Assert.AreEqual("_name: Jeff, _age: 26, Rank: 10, JsonTitle: Dr", cc.ToString());
    }
#endif

    [Test]
    public void DeserializeDictionaryInterface()
    {
      string json = @"{
  ""Name"": ""Name!"",
  ""Dictionary"": {
    ""Item"": 11
  }
}";

      DictionaryInterfaceClass c = JsonConvert.DeserializeObject<DictionaryInterfaceClass>(json,
        new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace });
      Assert.AreEqual("Name!", c.Name);
      Assert.AreEqual(1, c.Dictionary.Count);
      Assert.AreEqual(11, c.Dictionary["Item"]);
    }

    [Test]
    public void DeserializeDictionaryInterfaceWithExistingValues()
    {
      string json = @"{
  ""Random"": {
    ""blah"": 1
  },
  ""Name"": ""Name!"",
  ""Dictionary"": {
    ""Item"": 11,
    ""Item1"": 12
  },
  ""Collection"": [
    999
  ],
  ""Employee"": {
    ""Manager"": {
      ""Name"": ""ManagerName!""
    }
  }
}";

      DictionaryInterfaceClass c = JsonConvert.DeserializeObject<DictionaryInterfaceClass>(json,
        new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Reuse });

      Assert.AreEqual("Name!", c.Name);
      Assert.AreEqual(3, c.Dictionary.Count);
      Assert.AreEqual(11, c.Dictionary["Item"]);
      Assert.AreEqual(1, c.Dictionary["existing"]);
      Assert.AreEqual(4, c.Collection.Count);
      Assert.AreEqual(1, c.Collection.ElementAt(0));
      Assert.AreEqual(999, c.Collection.ElementAt(3));
      Assert.AreEqual("EmployeeName!", c.Employee.Name);
      Assert.AreEqual("ManagerName!", c.Employee.Manager.Name);
      Assert.IsNotNull(c.Random);
    }

    [Test]
    public void TypedObjectDeserializationWithComments()
    {
      string json = @"/*comment*/ { /*comment*/
        ""Name"": /*comment*/ ""Apple"" /*comment*/, /*comment*/
        ""ExpiryDate"": ""\/Date(1230422400000)\/"",
        ""Price"": 3.99,
        ""Sizes"": /*comment*/ [ /*comment*/
          ""Small"", /*comment*/
          ""Medium"" /*comment*/,
          /*comment*/ ""Large""
        /*comment*/ ] /*comment*/
      } /*comment*/";

      Product deserializedProduct = (Product)JsonConvert.DeserializeObject(json, typeof(Product));

      Assert.AreEqual("Apple", deserializedProduct.Name);
      Assert.AreEqual(new DateTime(2008, 12, 28, 0, 0, 0, DateTimeKind.Utc), deserializedProduct.ExpiryDate);
      Assert.AreEqual(3.99, deserializedProduct.Price);
      Assert.AreEqual("Small", deserializedProduct.Sizes[0]);
      Assert.AreEqual("Medium", deserializedProduct.Sizes[1]);
      Assert.AreEqual("Large", deserializedProduct.Sizes[2]);
    }

    [Test]
    public void NestedInsideOuterObject()
    {
      string json = @"{
  ""short"": {
    ""original"": ""http://www.contrast.ie/blog/online&#45;marketing&#45;2009/"",
    ""short"": ""m2sqc6"",
    ""shortened"": ""http://short.ie/m2sqc6"",
    ""error"": {
      ""code"": 0,
      ""msg"": ""No action taken""
    }
  }
}";

      JObject o = JObject.Parse(json);

      Shortie s = JsonConvert.DeserializeObject<Shortie>(o["short"].ToString());
      Assert.IsNotNull(s);

      Assert.AreEqual(s.Original, "http://www.contrast.ie/blog/online&#45;marketing&#45;2009/");
      Assert.AreEqual(s.Short, "m2sqc6");
      Assert.AreEqual(s.Shortened, "http://short.ie/m2sqc6");
    }

    [Test]
    public void UriSerialization()
    {
      Uri uri = new Uri("http://codeplex.com");
      string json = JsonConvert.SerializeObject(uri);

      Assert.AreEqual("http://codeplex.com/", uri.ToString());

      Uri newUri = JsonConvert.DeserializeObject<Uri>(json);
      Assert.AreEqual(uri, newUri);
    }

    [Test]
    public void AnonymousPlusLinqToSql()
    {
      var value = new
        {
          bar = new JObject(new JProperty("baz", 13))
        };

      string json = JsonConvert.SerializeObject(value);

      Assert.AreEqual(@"{""bar"":{""baz"":13}}", json);
    }

    [Test]
    public void SerializeEnumerableAsObject()
    {
      Content content = new Content
        {
          Text = "Blah, blah, blah",
          Children = new List<Content>
            {
              new Content { Text = "First" },
              new Content { Text = "Second" }
            }
        };

      string json = JsonConvert.SerializeObject(content, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Children"": [
    {
      ""Children"": null,
      ""Text"": ""First""
    },
    {
      ""Children"": null,
      ""Text"": ""Second""
    }
  ],
  ""Text"": ""Blah, blah, blah""
}", json);
    }

    [Test]
    public void DeserializeEnumerableAsObject()
    {
      string json = @"{
  ""Children"": [
    {
      ""Children"": null,
      ""Text"": ""First""
    },
    {
      ""Children"": null,
      ""Text"": ""Second""
    }
  ],
  ""Text"": ""Blah, blah, blah""
}";

      Content content = JsonConvert.DeserializeObject<Content>(json);

      Assert.AreEqual("Blah, blah, blah", content.Text);
      Assert.AreEqual(2, content.Children.Count);
      Assert.AreEqual("First", content.Children[0].Text);
      Assert.AreEqual("Second", content.Children[1].Text);
    }

    [Test]
    public void RoleTransferTest()
    {
      string json = @"{""Operation"":""1"",""RoleName"":""Admin"",""Direction"":""0""}";

      RoleTransfer r = JsonConvert.DeserializeObject<RoleTransfer>(json);

      Assert.AreEqual(RoleTransferOperation.Second, r.Operation);
      Assert.AreEqual("Admin", r.RoleName);
      Assert.AreEqual(RoleTransferDirection.First, r.Direction);
    }

    [Test]
    public void PrimitiveValuesInObjectArray()
    {
      string json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",null],""type"":""rpc"",""tid"":2}";

      ObjectArrayPropertyTest o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

      Assert.AreEqual("Router", o.Action);
      Assert.AreEqual("Navigate", o.Method);
      Assert.AreEqual(2, o.Data.Length);
      Assert.AreEqual("dashboard", o.Data[0]);
      Assert.AreEqual(null, o.Data[1]);
    }

    [Test]
    public void ComplexValuesInObjectArray()
    {
      string json = @"{""action"":""Router"",""method"":""Navigate"",""data"":[""dashboard"",[""id"", 1, ""teststring"", ""test""],{""one"":1}],""type"":""rpc"",""tid"":2}";

      ObjectArrayPropertyTest o = JsonConvert.DeserializeObject<ObjectArrayPropertyTest>(json);

      Assert.AreEqual("Router", o.Action);
      Assert.AreEqual("Navigate", o.Method);
      Assert.AreEqual(3, o.Data.Length);
      Assert.AreEqual("dashboard", o.Data[0]);
      Assert.IsInstanceOfType(typeof(JArray), o.Data[1]);
      Assert.AreEqual(4, ((JArray)o.Data[1]).Count);
      Assert.IsInstanceOfType(typeof(JObject), o.Data[2]);
      Assert.AreEqual(1, ((JObject)o.Data[2]).Count);
      Assert.AreEqual(1, (int)((JObject)o.Data[2])["one"]);
    }

    [Test]
    public void DeserializeGenericDictionary()
    {
      string json = @"{""key1"":""value1"",""key2"":""value2""}";

      Dictionary<string, string> values = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

      Console.WriteLine(values.Count);
      // 2

      Console.WriteLine(values["key1"]);
      // value1

      Assert.AreEqual(2, values.Count);
      Assert.AreEqual("value1", values["key1"]);
      Assert.AreEqual("value2", values["key2"]);
    }

    [Test]
    public void SerializeGenericList()
    {
      Product p1 = new Product
        {
          Name = "Product 1",
          Price = 99.95m,
          ExpiryDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        };
      Product p2 = new Product
      {
        Name = "Product 2",
        Price = 12.50m,
        ExpiryDate = new DateTime(2009, 7, 31, 0, 0, 0, DateTimeKind.Utc),
      };

      List<Product> products = new List<Product>();
      products.Add(p1);
      products.Add(p2);

      string json = JsonConvert.SerializeObject(products, Formatting.Indented);
      //[
      //  {
      //    "Name": "Product 1",
      //    "ExpiryDate": "\/Date(978048000000)\/",
      //    "Price": 99.95,
      //    "Sizes": null
      //  },
      //  {
      //    "Name": "Product 2",
      //    "ExpiryDate": "\/Date(1248998400000)\/",
      //    "Price": 12.50,
      //    "Sizes": null
      //  }
      //]

      Assert.AreEqual(@"[
  {
    ""Name"": ""Product 1"",
    ""ExpiryDate"": ""\/Date(978048000000)\/"",
    ""Price"": 99.95,
    ""Sizes"": null
  },
  {
    ""Name"": ""Product 2"",
    ""ExpiryDate"": ""\/Date(1248998400000)\/"",
    ""Price"": 12.50,
    ""Sizes"": null
  }
]", json);
    }

    [Test]
    public void DeserializeGenericList()
    {
      string json = @"[
        {
          ""Name"": ""Product 1"",
          ""ExpiryDate"": ""\/Date(978048000000)\/"",
          ""Price"": 99.95,
          ""Sizes"": null
        },
        {
          ""Name"": ""Product 2"",
          ""ExpiryDate"": ""\/Date(1248998400000)\/"",
          ""Price"": 12.50,
          ""Sizes"": null
        }
      ]";

      List<Product> products = JsonConvert.DeserializeObject<List<Product>>(json);

      Console.WriteLine(products.Count);
      // 2

      Product p1 = products[0];

      Console.WriteLine(p1.Name);
      // Product 1

      Assert.AreEqual(2, products.Count);
      Assert.AreEqual("Product 1", products[0].Name);
    }

#if !PocketPC && !NET20
    [Test]
    public void DeserializeEmptyStringToNullableDateTime()
    {
      string json = @"{""DateTimeField"":""""}";

      NullableDateTimeTestClass c = JsonConvert.DeserializeObject<NullableDateTimeTestClass>(json);
      Assert.AreEqual(null, c.DateTimeField);
    }
#endif

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.Event. A class should either have a default constructor or only one constructor with arguments.")]
    public void FailWhenClassWithNoDefaultConstructorHasMultipleConstructorsWithArguments()
    {
      string json = @"{""sublocation"":""AlertEmailSender.Program.Main"",""userId"":0,""type"":0,""summary"":""Loading settings variables"",""details"":null,""stackTrace"":""   at System.Environment.GetStackTrace(Exception e, Boolean needFileInfo)\r\n   at System.Environment.get_StackTrace()\r\n   at mr.Logging.Event..ctor(String summary) in C:\\Projects\\MRUtils\\Logging\\Event.vb:line 71\r\n   at AlertEmailSender.Program.Main(String[] args) in C:\\Projects\\AlertEmailSender\\AlertEmailSender\\Program.cs:line 25"",""tag"":null,""time"":""\/Date(1249591032026-0400)\/""}";

      Event e = JsonConvert.DeserializeObject<Event>(json);
    }

    [Test]
    public void DeserializeObjectSetOnlyProperty()
    {
      string json = @"{'SetOnlyProperty':[1,2,3,4,5]}";

      SetOnlyPropertyClass2 setOnly = JsonConvert.DeserializeObject<SetOnlyPropertyClass2>(json);
      JArray a = (JArray)setOnly.GetValue();
      Assert.AreEqual(5, a.Count);
      Assert.AreEqual(1, (int)a[0]);
      Assert.AreEqual(5, (int)a[a.Count - 1]);
    }

    [Test]
    public void DeserializeOptInClasses()
    {
      string json = @"{id: ""12"", name: ""test"", items: [{id: ""112"", name: ""testing""}]}";

      ListTestClass l = JsonConvert.DeserializeObject<ListTestClass>(json);
    }

    [Test]
    public void DeserializeNullableListWithNulls()
    {
      List<decimal?> l = JsonConvert.DeserializeObject<List<decimal?>>("[ 3.3, null, 1.1 ] ");
      Assert.AreEqual(3, l.Count);

      Assert.AreEqual(3.3m, l[0]);
      Assert.AreEqual(null, l[1]);
      Assert.AreEqual(1.1m, l[2]);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Cannot deserialize JSON array into type 'Newtonsoft.Json.Tests.TestObjects.Person'.")]
    public void CannotDeserializeArrayIntoObject()
    {
      string json = @"[]";

      JsonConvert.DeserializeObject<Person>(json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Cannot deserialize JSON object into type 'System.Collections.Generic.List`1[Newtonsoft.Json.Tests.TestObjects.Person]'.")]
    public void CannotDeserializeObjectIntoArray()
    {
      string json = @"{}";

      JsonConvert.DeserializeObject<List<Person>>(json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Cannot populate JSON array onto type 'Newtonsoft.Json.Tests.TestObjects.Person'.")]
    public void CannotPopulateArrayIntoObject()
    {
      string json = @"[]";

      JsonConvert.PopulateObject(json, new Person());
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Cannot populate JSON object onto type 'System.Collections.Generic.List`1[Newtonsoft.Json.Tests.TestObjects.Person]'.")]
    public void CannotPopulateObjectIntoArray()
    {
      string json = @"{}";

      JsonConvert.PopulateObject(json, new List<Person>());
    }

    [Test]
    public void DeserializeEmptyString()
    {
      string json = @"{""Name"":""""}";

      Person p = JsonConvert.DeserializeObject<Person>(json);
      Assert.AreEqual("", p.Name);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Error getting value from 'ReadTimeout' on 'System.IO.MemoryStream'.")]
    public void SerializePropertyGetError()
    {
      JsonConvert.SerializeObject(new MemoryStream());
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Error setting value to 'ReadTimeout' on 'System.IO.MemoryStream'.")]
    public void DeserializePropertySetError()
    {
      JsonConvert.DeserializeObject<MemoryStream>("{ReadTimeout:0}");
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Error converting value """" to type 'System.Int32'.")]
    public void DeserializeEnsureTypeEmptyStringToIntError()
    {
      JsonConvert.DeserializeObject<MemoryStream>("{ReadTimeout:''}");
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Error converting value {null} to type 'System.Int32'.")]
    public void DeserializeEnsureTypeNullToIntError()
    {
      JsonConvert.DeserializeObject<MemoryStream>("{ReadTimeout:null}");
    }

    [Test]
    public void SerializeGenericListOfStrings()
    {
      List<String> strings = new List<String>();

      strings.Add("str_1");
      strings.Add("str_2");
      strings.Add("str_3");

      string json = JsonConvert.SerializeObject(strings);
      Assert.AreEqual(@"[""str_1"",""str_2"",""str_3""]", json);
    }

    [Test]
    public void ConstructorReadonlyFieldsTest()
    {
      ConstructorReadonlyFields c1 = new ConstructorReadonlyFields("String!", int.MaxValue);
      string json = JsonConvert.SerializeObject(c1, Formatting.Indented);
      Assert.AreEqual(@"{
  ""A"": ""String!"",
  ""B"": 2147483647
}", json);

      ConstructorReadonlyFields c2 = JsonConvert.DeserializeObject<ConstructorReadonlyFields>(json);
      Assert.AreEqual("String!", c2.A);
      Assert.AreEqual(int.MaxValue, c2.B);
    }

    [Test]
    public void SerializeStruct()
    {
      StructTest structTest = new StructTest
                                {
                                  StringProperty = "StringProperty!",
                                  StringField = "StringField",
                                  IntProperty = 5,
                                  IntField = 10
                                };

      string json = JsonConvert.SerializeObject(structTest, Formatting.Indented);
      Console.WriteLine(json);
      Assert.AreEqual(@"{
  ""StringField"": ""StringField"",
  ""IntField"": 10,
  ""StringProperty"": ""StringProperty!"",
  ""IntProperty"": 5
}", json);

      StructTest deserialized = JsonConvert.DeserializeObject<StructTest>(json);
      Assert.AreEqual(structTest.StringProperty, deserialized.StringProperty);
      Assert.AreEqual(structTest.StringField, deserialized.StringField);
      Assert.AreEqual(structTest.IntProperty, deserialized.IntProperty);
      Assert.AreEqual(structTest.IntField, deserialized.IntField);
    }

    [Test]
    public void SerializeListWithJsonConverter()
    {
      Foo f = new Foo();
      f.Bars.Add(new Bar { Id = 0 });
      f.Bars.Add(new Bar { Id = 1 });
      f.Bars.Add(new Bar { Id = 2 });

      string json = JsonConvert.SerializeObject(f, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Bars"": [
    0,
    1,
    2
  ]
}", json);

      Foo newFoo = JsonConvert.DeserializeObject<Foo>(json);
      Assert.AreEqual(3, newFoo.Bars.Count);
      Assert.AreEqual(0, newFoo.Bars[0].Id);
      Assert.AreEqual(1, newFoo.Bars[1].Id);
      Assert.AreEqual(2, newFoo.Bars[2].Id);
    }

    [Test]
    public void SerializeGuidKeyedDictionary()
    {
      Dictionary<Guid, int> dictionary = new Dictionary<Guid, int>();
      dictionary.Add(new Guid("F60EAEE0-AE47-488E-B330-59527B742D77"), 1);
      dictionary.Add(new Guid("C2594C02-EBA1-426A-AA87-8DD8871350B0"), 2);

      string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);
      Assert.AreEqual(@"{
  ""f60eaee0-ae47-488e-b330-59527b742d77"": 1,
  ""c2594c02-eba1-426a-aa87-8dd8871350b0"": 2
}", json);
    }

    [Test]
    public void SerializePersonKeyedDictionary()
    {
      Dictionary<Person, int> dictionary = new Dictionary<Person, int>();
      dictionary.Add(new Person { Name = "p1" }, 1);
      dictionary.Add(new Person { Name = "p2" }, 2);

      string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 1,
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 2
}", json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Could not convert string 'Newtonsoft.Json.Tests.TestObjects.Person' to dictionary key type 'Newtonsoft.Json.Tests.TestObjects.Person'. Create a TypeConverter to convert from the string to the key type object.")]
    public void DeserializePersonKeyedDictionary()
    {
      string json =
        @"{
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 1,
  ""Newtonsoft.Json.Tests.TestObjects.Person"": 2
}";

      JsonConvert.DeserializeObject<Dictionary<Person, int>>(json);
    }

    [Test]
    public void SerializeFragment()
    {
      string googleSearchText = @"{
        ""responseData"": {
          ""results"": [
            {
              ""GsearchResultClass"": ""GwebSearch"",
              ""unescapedUrl"": ""http://en.wikipedia.org/wiki/Paris_Hilton"",
              ""url"": ""http://en.wikipedia.org/wiki/Paris_Hilton"",
              ""visibleUrl"": ""en.wikipedia.org"",
              ""cacheUrl"": ""http://www.google.com/search?q=cache:TwrPfhd22hYJ:en.wikipedia.org"",
              ""title"": ""<b>Paris Hilton</b> - Wikipedia, the free encyclopedia"",
              ""titleNoFormatting"": ""Paris Hilton - Wikipedia, the free encyclopedia"",
              ""content"": ""[1] In 2006, she released her debut album...""
            },
            {
              ""GsearchResultClass"": ""GwebSearch"",
              ""unescapedUrl"": ""http://www.imdb.com/name/nm0385296/"",
              ""url"": ""http://www.imdb.com/name/nm0385296/"",
              ""visibleUrl"": ""www.imdb.com"",
              ""cacheUrl"": ""http://www.google.com/search?q=cache:1i34KkqnsooJ:www.imdb.com"",
              ""title"": ""<b>Paris Hilton</b>"",
              ""titleNoFormatting"": ""Paris Hilton"",
              ""content"": ""Self: Zoolander. Socialite <b>Paris Hilton</b>...""
            }
          ],
          ""cursor"": {
            ""pages"": [
              {
                ""start"": ""0"",
                ""label"": 1
              },
              {
                ""start"": ""4"",
                ""label"": 2
              },
              {
                ""start"": ""8"",
                ""label"": 3
              },
              {
                ""start"": ""12"",
                ""label"": 4
              }
            ],
            ""estimatedResultCount"": ""59600000"",
            ""currentPageIndex"": 0,
            ""moreResultsUrl"": ""http://www.google.com/search?oe=utf8&ie=utf8...""
          }
        },
        ""responseDetails"": null,
        ""responseStatus"": 200
      }";

      JObject googleSearch = JObject.Parse(googleSearchText);

      // get JSON result objects into a list
      IList<JToken> results = googleSearch["responseData"]["results"].Children().ToList();

      // serialize JSON results into .NET objects
      IList<SearchResult> searchResults = new List<SearchResult>();
      foreach (JToken result in results)
      {
        SearchResult searchResult = JsonConvert.DeserializeObject<SearchResult>(result.ToString());
        searchResults.Add(searchResult);
      }

      // Title = <b>Paris Hilton</b> - Wikipedia, the free encyclopedia
      // Content = [1] In 2006, she released her debut album...
      // Url = http://en.wikipedia.org/wiki/Paris_Hilton

      // Title = <b>Paris Hilton</b>
      // Content = Self: Zoolander. Socialite <b>Paris Hilton</b>...
      // Url = http://www.imdb.com/name/nm0385296/

      Assert.AreEqual(2, searchResults.Count);
      Assert.AreEqual("<b>Paris Hilton</b> - Wikipedia, the free encyclopedia", searchResults[0].Title);
      Assert.AreEqual("<b>Paris Hilton</b>", searchResults[1].Title);
    }

    [Test]
    public void DeserializeBaseReferenceWithDerivedValue()
    {
      PersonPropertyClass personPropertyClass = new PersonPropertyClass();
      WagePerson wagePerson = (WagePerson)personPropertyClass.Person;

      wagePerson.BirthDate = new DateTime(2000, 11, 29, 23, 59, 59, DateTimeKind.Utc);
      wagePerson.Department = "McDees";
      wagePerson.HourlyWage = 12.50m;
      wagePerson.LastModified = new DateTime(2000, 11, 29, 23, 59, 59, DateTimeKind.Utc);
      wagePerson.Name = "Jim Bob";

      string json = JsonConvert.SerializeObject(personPropertyClass, Formatting.Indented);
      Assert.AreEqual(
        @"{
  ""Person"": {
    ""HourlyWage"": 12.50,
    ""Name"": ""Jim Bob"",
    ""BirthDate"": ""\/Date(975542399000)\/"",
    ""LastModified"": ""\/Date(975542399000)\/""
  }
}",
        json);

      PersonPropertyClass newPersonPropertyClass = JsonConvert.DeserializeObject<PersonPropertyClass>(json);
      Assert.AreEqual(wagePerson.HourlyWage, ((WagePerson)newPersonPropertyClass.Person).HourlyWage);
    }

    public class ExistingValueClass
    {
      public Dictionary<string, string> Dictionary { get; set; }
      public List<string> List { get; set; }

      public ExistingValueClass()
      {
        Dictionary = new Dictionary<string, string>
                       {
                         {"existing", "yup"}
                       };
        List = new List<string>
                 {
                   "existing"
                 };
      }
    }

    [Test]
    public void DeserializePopulateDictionaryAndList()
    {
      ExistingValueClass d = JsonConvert.DeserializeObject<ExistingValueClass>(@"{'Dictionary':{appended:'appended',existing:'new'}}");

      Assert.IsNotNull(d);
      Assert.IsNotNull(d.Dictionary);
      Assert.AreEqual(typeof(Dictionary<string, string>), d.Dictionary.GetType());
      Assert.AreEqual(typeof(List<string>), d.List.GetType());
      Assert.AreEqual(2, d.Dictionary.Count);
      Assert.AreEqual("new", d.Dictionary["existing"]);
      Assert.AreEqual("appended", d.Dictionary["appended"]);
      Assert.AreEqual(1, d.List.Count);
      Assert.AreEqual("existing", d.List[0]);
    }

    public interface IKeyValueId
    {
      int Id { get; set; }
      string Key { get; set; }
      string Value { get; set; }
    }


    public class KeyValueId : IKeyValueId
    {
      public int Id { get; set; }
      public string Key { get; set; }
      public string Value { get; set; }
    }

    public class ThisGenericTest<T> where T : IKeyValueId
    {
      private Dictionary<string, T> _dict1 = new Dictionary<string, T>();

      public string MyProperty { get; set; }

      public void Add(T item)
      {
        this._dict1.Add(item.Key, item);
      }

      public T this[string key]
      {
        get { return this._dict1[key]; }
        set { this._dict1[key] = value; }
      }

      public T this[int id]
      {
        get { return this._dict1.Values.FirstOrDefault(x => x.Id == id); }
        set
        {
          var item = this[id];

          if (item == null)
            this.Add(value);
          else
            this._dict1[item.Key] = value;
        }
      }

      public string ToJson()
      {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
      }

      public T[] TheItems
      {
        get { return this._dict1.Values.ToArray<T>(); }
        set
        {
          foreach (var item in value)
            this.Add(item);
        }
      }
    }

    [Test]
    public void IgnoreIndexedProperties()
    {
      ThisGenericTest<KeyValueId> g = new ThisGenericTest<KeyValueId>();

      g.Add(new KeyValueId { Id = 1, Key = "key1", Value = "value1" });
      g.Add(new KeyValueId { Id = 2, Key = "key2", Value = "value2" });

      g.MyProperty = "some value";

      string json = g.ToJson();

      Assert.AreEqual(@"{
  ""MyProperty"": ""some value"",
  ""TheItems"": [
    {
      ""Id"": 1,
      ""Key"": ""key1"",
      ""Value"": ""value1""
    },
    {
      ""Id"": 2,
      ""Key"": ""key2"",
      ""Value"": ""value2""
    }
  ]
}", json);

      ThisGenericTest<KeyValueId> gen = JsonConvert.DeserializeObject<ThisGenericTest<KeyValueId>>(json);
      Assert.AreEqual("some value", gen.MyProperty);
    }

    public class JRawValueTestObject
    {
      public JRaw Value { get; set; }
    }

    [Test]
    public void JRawValue()
    {
      JRawValueTestObject deserialized = JsonConvert.DeserializeObject<JRawValueTestObject>("{value:3}");
      Assert.AreEqual("3", deserialized.Value.ToString());

      deserialized = JsonConvert.DeserializeObject<JRawValueTestObject>("{value:'3'}");
      Assert.AreEqual(@"""3""", deserialized.Value.ToString());
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Unable to find a default constructor to use for type Newtonsoft.Json.Tests.Serialization.JsonSerializerTest+DictionaryWithNoDefaultConstructor.")]
    public void DeserializeDictionaryWithNoDefaultConstructor()
    {
      string json = "{key1:'value',key2:'value',key3:'value'}";
      JsonConvert.DeserializeObject<DictionaryWithNoDefaultConstructor>(json);
    }

    public class DictionaryWithNoDefaultConstructor : Dictionary<string, string>
    {
      public DictionaryWithNoDefaultConstructor(IEnumerable<KeyValuePair<string, string>> initial)
      {
        foreach (KeyValuePair<string, string> pair in initial)
        {
          Add(pair.Key, pair.Value);
        }
      }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class A
    {
      [JsonProperty("A1")]
      private string _A1;
      public string A1 { get { return _A1; } set { _A1 = value; } }

      [JsonProperty("A2")]
      private string A2 { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class B : A
    {
      public string B1 { get; set; }

      [JsonProperty("B2")]
      string _B2;
      public string B2 { get { return _B2; } set { _B2 = value; } }

      [JsonProperty("B3")]
      private string B3 { get; set; }
    }

    [Test]
    public void SerializeNonPublicBaseJsonProperties()
    {
      B value = new B();
      string json = JsonConvert.SerializeObject(value, Formatting.Indented);

      Assert.AreEqual(@"{
  ""B2"": null,
  ""A1"": null,
  ""B3"": null,
  ""A2"": null
}", json);
    }

    public class TestClass
    {
      public string Key { get; set; }
      public object Value { get; set; }
    }

    [Test]
    public void DeserializeToObjectProperty()
    {
      var json = "{ Key: 'abc', Value: 123 }";
      var item = JsonConvert.DeserializeObject<TestClass>(json);

      Assert.AreEqual(123, item.Value);
    }

    public abstract class Animal
    {
      public abstract string Name { get; }
    }

    public class Human : Animal
    {
      public override string Name
      {
        get { return typeof(Human).Name; }
      }

      public string Ethnicity { get; set; }
    }

#if !NET20 && !PocketPC
    public class DataContractJsonSerializerTestClass
    {
      public TimeSpan TimeSpanProperty { get; set; }
      public Guid GuidProperty { get; set; }
      public Animal AnimalProperty { get; set; }
      public Exception ExceptionProperty { get; set; }
    }

    [Test]
    public void DataContractJsonSerializerTest()
    {
      Exception ex = new Exception("Blah blah blah");

      DataContractJsonSerializerTestClass c = new DataContractJsonSerializerTestClass();
      c.TimeSpanProperty = new TimeSpan(200, 20, 59, 30, 900);
      c.GuidProperty = new Guid("66143115-BE2A-4a59-AF0A-348E1EA15B1E");
      c.AnimalProperty = new Human() { Ethnicity = "European" };
      c.ExceptionProperty = ex;

      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer serializer = new DataContractJsonSerializer(
        typeof(DataContractJsonSerializerTestClass),
        new Type[] { typeof(Human) });
      serializer.WriteObject(ms, c);

      byte[] jsonBytes = ms.ToArray();
      string json = Encoding.UTF8.GetString(jsonBytes, 0, jsonBytes.Length);

      Console.WriteLine(JObject.Parse(json).ToString());
      Console.WriteLine();

      Console.WriteLine(JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings
                                                                          {
                                                                            //               TypeNameHandling = TypeNameHandling.Objects
                                                                          }));
    }
#endif

    public class ModelStateDictionary<T> : IDictionary<string, T>
    {

      private readonly Dictionary<string, T> _innerDictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

      public ModelStateDictionary()
      {
      }

      public ModelStateDictionary(ModelStateDictionary<T> dictionary)
      {
        if (dictionary == null)
        {
          throw new ArgumentNullException("dictionary");
        }

        foreach (var entry in dictionary)
        {
          _innerDictionary.Add(entry.Key, entry.Value);
        }
      }

      public int Count
      {
        get
        {
          return _innerDictionary.Count;
        }
      }

      public bool IsReadOnly
      {
        get
        {
          return ((IDictionary<string, T>)_innerDictionary).IsReadOnly;
        }
      }

      public ICollection<string> Keys
      {
        get
        {
          return _innerDictionary.Keys;
        }
      }

      public T this[string key]
      {
        get
        {
          T value;
          _innerDictionary.TryGetValue(key, out value);
          return value;
        }
        set
        {
          _innerDictionary[key] = value;
        }
      }

      public ICollection<T> Values
      {
        get
        {
          return _innerDictionary.Values;
        }
      }

      public void Add(KeyValuePair<string, T> item)
      {
        ((IDictionary<string, T>)_innerDictionary).Add(item);
      }

      public void Add(string key, T value)
      {
        _innerDictionary.Add(key, value);
      }

      public void Clear()
      {
        _innerDictionary.Clear();
      }

      public bool Contains(KeyValuePair<string, T> item)
      {
        return ((IDictionary<string, T>)_innerDictionary).Contains(item);
      }

      public bool ContainsKey(string key)
      {
        return _innerDictionary.ContainsKey(key);
      }

      public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
      {
        ((IDictionary<string, T>)_innerDictionary).CopyTo(array, arrayIndex);
      }

      public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
      {
        return _innerDictionary.GetEnumerator();
      }

      public void Merge(ModelStateDictionary<T> dictionary)
      {
        if (dictionary == null)
        {
          return;
        }

        foreach (var entry in dictionary)
        {
          this[entry.Key] = entry.Value;
        }
      }

      public bool Remove(KeyValuePair<string, T> item)
      {
        return ((IDictionary<string, T>)_innerDictionary).Remove(item);
      }

      public bool Remove(string key)
      {
        return _innerDictionary.Remove(key);
      }

      public bool TryGetValue(string key, out T value)
      {
        return _innerDictionary.TryGetValue(key, out value);
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
        return ((IEnumerable)_innerDictionary).GetEnumerator();
      }
    }

    [Test]
    public void SerializeNonIDictionary()
    {
      ModelStateDictionary<string> modelStateDictionary = new ModelStateDictionary<string>();
      modelStateDictionary.Add("key", "value");

      string json = JsonConvert.SerializeObject(modelStateDictionary);

      Assert.AreEqual(@"{""key"":""value""}", json);

      ModelStateDictionary<string> newModelStateDictionary = JsonConvert.DeserializeObject<ModelStateDictionary<string>>(json);
      Assert.AreEqual(1, newModelStateDictionary.Count);
      Assert.AreEqual("value", newModelStateDictionary["key"]);
    }

#if !SILVERLIGHT && !PocketPC
    public class ISerializableTestObject : ISerializable
    {
      internal string _stringValue;
      internal int _intValue;
      internal DateTimeOffset _dateTimeOffsetValue;
      internal Person _personValue;
      internal Person _nullPersonValue;
      internal int? _nullableInt;
      internal bool _booleanValue;
      internal byte _byteValue;
      internal char _charValue;
      internal DateTime _dateTimeValue;
      internal decimal _decimalValue;
      internal short _shortValue;
      internal long _longValue;
      internal sbyte _sbyteValue;
      internal float _floatValue;
      internal ushort _ushortValue;
      internal uint _uintValue;
      internal ulong _ulongValue;

      public ISerializableTestObject(string stringValue, int intValue, DateTimeOffset dateTimeOffset, Person personValue)
      {
        _stringValue = stringValue;
        _intValue = intValue;
        _dateTimeOffsetValue = dateTimeOffset;
        _personValue = personValue;
        _dateTimeValue = new DateTime(0, DateTimeKind.Utc);
      }

      protected ISerializableTestObject(SerializationInfo info, StreamingContext context)
      {
        _stringValue = info.GetString("stringValue");
        _intValue = info.GetInt32("intValue");
        _dateTimeOffsetValue = (DateTimeOffset)info.GetValue("dateTimeOffsetValue", typeof(DateTimeOffset));
        _personValue = (Person)info.GetValue("personValue", typeof(Person));
        _nullPersonValue = (Person)info.GetValue("nullPersonValue", typeof(Person));
        _nullableInt = (int?)info.GetValue("nullableInt", typeof(int?));

        _booleanValue = info.GetBoolean("booleanValue");
        _byteValue = info.GetByte("byteValue");
        _charValue = info.GetChar("charValue");
        _dateTimeValue = info.GetDateTime("dateTimeValue");
        _decimalValue = info.GetDecimal("decimalValue");
        _shortValue = info.GetInt16("shortValue");
        _longValue = info.GetInt64("longValue");
        _sbyteValue = info.GetSByte("sbyteValue");
        _floatValue = info.GetSingle("floatValue");
        _ushortValue = info.GetUInt16("ushortValue");
        _uintValue = info.GetUInt32("uintValue");
        _ulongValue = info.GetUInt64("ulongValue");
      }

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        info.AddValue("stringValue", _stringValue);
        info.AddValue("intValue", _intValue);
        info.AddValue("dateTimeOffsetValue", _dateTimeOffsetValue);
        info.AddValue("personValue", _personValue);
        info.AddValue("nullPersonValue", _nullPersonValue);
        info.AddValue("nullableInt", null);

        info.AddValue("booleanValue", _booleanValue);
        info.AddValue("byteValue", _byteValue);
        info.AddValue("charValue", _charValue);
        info.AddValue("dateTimeValue", _dateTimeValue);
        info.AddValue("decimalValue", _decimalValue);
        info.AddValue("shortValue", _shortValue);
        info.AddValue("longValue", _longValue);
        info.AddValue("sbyteValue", _sbyteValue);
        info.AddValue("floatValue", _floatValue);
        info.AddValue("ushortValue", _ushortValue);
        info.AddValue("uintValue", _uintValue);
        info.AddValue("ulongValue", _ulongValue);
      }
    }

    [Test]
    public void SerializeISerializableTestObject()
    {
      Person person = new Person();
      person.BirthDate = new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc);
      person.LastModified = person.BirthDate;
      person.Department = "Department!";
      person.Name = "Name!";

      DateTimeOffset dateTimeOffset = new DateTimeOffset(2000, 12, 20, 22, 59, 59, TimeSpan.FromHours(2));
      string dateTimeOffsetText;
#if !NET20
      dateTimeOffsetText = @"\/Date(977345999000+0200)\/";
#else
      dateTimeOffsetText = @"12/20/2000 22:59:59 +02:00";
#endif

      ISerializableTestObject o = new ISerializableTestObject("String!", int.MinValue, dateTimeOffset, person);

      string json = JsonConvert.SerializeObject(o, Formatting.Indented);
      Assert.AreEqual(@"{
  ""stringValue"": ""String!"",
  ""intValue"": -2147483648,
  ""dateTimeOffsetValue"": """ + dateTimeOffsetText + @""",
  ""personValue"": {
    ""Name"": ""Name!"",
    ""BirthDate"": ""\/Date(946688461000)\/"",
    ""LastModified"": ""\/Date(946688461000)\/""
  },
  ""nullPersonValue"": null,
  ""nullableInt"": null,
  ""booleanValue"": false,
  ""byteValue"": 0,
  ""charValue"": ""\u0000"",
  ""dateTimeValue"": ""\/Date(-62135596800000)\/"",
  ""decimalValue"": 0.0,
  ""shortValue"": 0,
  ""longValue"": 0,
  ""sbyteValue"": 0,
  ""floatValue"": 0.0,
  ""ushortValue"": 0,
  ""uintValue"": 0,
  ""ulongValue"": 0
}", json);

      ISerializableTestObject o2 = JsonConvert.DeserializeObject<ISerializableTestObject>(json);
      Assert.AreEqual("String!", o2._stringValue);
      Assert.AreEqual(int.MinValue, o2._intValue);
      Assert.AreEqual(dateTimeOffset, o2._dateTimeOffsetValue);
      Assert.AreEqual("Name!", o2._personValue.Name);
      Assert.AreEqual(null, o2._nullPersonValue);
      Assert.AreEqual(null, o2._nullableInt);
    }
#endif

    public class KVPair<TKey, TValue>
    {
      public TKey Key { get; set; }
      public TValue Value { get; set; }

      public KVPair(TKey k, TValue v)
      {
        Key = k;
        Value = v;
      }
    }

    [Test]
    public void DeserializeUsingNonDefaultConstructorWithLeftOverValues()
    {
      List<KVPair<string, string>> kvPairs =
        JsonConvert.DeserializeObject<List<KVPair<string, string>>>(
          "[{\"Key\":\"Two\",\"Value\":\"2\"},{\"Key\":\"One\",\"Value\":\"1\"}]");

      Assert.AreEqual(2, kvPairs.Count);
      Assert.AreEqual("Two", kvPairs[0].Key);
      Assert.AreEqual("2", kvPairs[0].Value);
      Assert.AreEqual("One", kvPairs[1].Key);
      Assert.AreEqual("1", kvPairs[1].Value);
    }

    [Test]
    public void SerializeClassWithInheritedProtectedMember()
    {
      AA myA = new AA(2);
      string json = JsonConvert.SerializeObject(myA, Formatting.Indented);
      Assert.AreEqual(@"{
  ""AA_field1"": 2,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2
}", json);

      BB myB = new BB(3, 4);
      json = JsonConvert.SerializeObject(myB, Formatting.Indented);
      Assert.AreEqual(@"{
  ""BB_field1"": 4,
  ""BB_field2"": 4,
  ""AA_field1"": 3,
  ""BB_property1"": 4,
  ""BB_property2"": 4,
  ""BB_property3"": 4,
  ""BB_property4"": 4,
  ""BB_property5"": 4,
  ""BB_property7"": 4,
  ""AA_property1"": 3,
  ""AA_property2"": 3,
  ""AA_property3"": 3,
  ""AA_property4"": 3
}", json);
    }

    [Test]
    public void DeserializeClassWithInheritedProtectedMember()
    {
      AA myA = JsonConvert.DeserializeObject<AA>(
          @"{
  ""AA_field1"": 2,
  ""AA_field2"": 2,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2,
  ""AA_property5"": 2,
  ""AA_property6"": 2
}");

      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field1", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field2", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property1", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property2", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property3", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property4", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property5", BindingFlags.Instance | BindingFlags.NonPublic), myA));
      Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property6", BindingFlags.Instance | BindingFlags.NonPublic), myA));

      BB myB = JsonConvert.DeserializeObject<BB>(
          @"{
  ""BB_field1"": 4,
  ""BB_field2"": 4,
  ""AA_field1"": 3,
  ""AA_field2"": 3,
  ""AA_property1"": 2,
  ""AA_property2"": 2,
  ""AA_property3"": 2,
  ""AA_property4"": 2,
  ""AA_property5"": 2,
  ""AA_property6"": 2,
  ""BB_property1"": 3,
  ""BB_property2"": 3,
  ""BB_property3"": 3,
  ""BB_property4"": 3,
  ""BB_property5"": 3,
  ""BB_property6"": 3,
  ""BB_property7"": 3,
  ""BB_property8"": 3
}");

      Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field1", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetField("AA_field2", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property1", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property2", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property3", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(2, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property4", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property5", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(0, ReflectionUtils.GetMemberValue(typeof(AA).GetProperty("AA_property6", BindingFlags.Instance | BindingFlags.NonPublic), myB));

      Assert.AreEqual(4, myB.BB_field1);
      Assert.AreEqual(4, myB.BB_field2);
      Assert.AreEqual(3, myB.BB_property1);
      Assert.AreEqual(3, myB.BB_property2);
      Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property3", BindingFlags.Instance | BindingFlags.Public), myB));
      Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property4", BindingFlags.Instance | BindingFlags.NonPublic), myB));
      Assert.AreEqual(0, myB.BB_property5);
      Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property6", BindingFlags.Instance | BindingFlags.Public), myB));
      Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property7", BindingFlags.Instance | BindingFlags.Public), myB));
      Assert.AreEqual(3, ReflectionUtils.GetMemberValue(typeof(BB).GetProperty("BB_property8", BindingFlags.Instance | BindingFlags.Public), myB));
    }

    public class AA
    {
      [JsonProperty]
      protected int AA_field1;
      protected int AA_field2;
      [JsonProperty]
      protected int AA_property1 { get; set; }
      [JsonProperty]
      protected int AA_property2 { get; private set; }
      [JsonProperty]
      protected int AA_property3 { private get; set; }
      [JsonProperty]
      private int AA_property4 { get; set; }
      protected int AA_property5 { get; private set; }
      protected int AA_property6 { private get; set; }

      public AA()
      {
      }

      public AA(int f)
      {
        AA_field1 = f;
        AA_field2 = f;
        AA_property1 = f;
        AA_property2 = f;
        AA_property3 = f;
        AA_property4 = f;
        AA_property5 = f;
        AA_property6 = f;
      }
    }

    public class BB : AA
    {
      [JsonProperty]
      public int BB_field1;
      public int BB_field2;
      [JsonProperty]
      public int BB_property1 { get; set; }
      [JsonProperty]
      public int BB_property2 { get; private set; }
      [JsonProperty]
      public int BB_property3 { private get; set; }
      [JsonProperty]
      private int BB_property4 { get; set; }
      public int BB_property5 { get; private set; }
      public int BB_property6 { private get; set; }
      [JsonProperty]
      public int BB_property7 { protected get; set; }
      public int BB_property8 { protected get; set; }

      public BB()
      {
      }

      public BB(int f, int g)
        : base(f)
      {
        BB_field1 = g;
        BB_field2 = g;
        BB_property1 = g;
        BB_property2 = g;
        BB_property3 = g;
        BB_property4 = g;
        BB_property5 = g;
        BB_property6 = g;
        BB_property7 = g;
        BB_property8 = g;
      }
    }

#if !NET20 && !SILVERLIGHT
    public class XNodeTestObject
    {
      public XDocument Document { get; set; }
      public XElement Element { get; set; }
    }
#endif

#if !SILVERLIGHT
    public class XmlNodeTestObject
    {
      public XmlDocument Document { get; set; }
    }
#endif

#if !NET20 && !SILVERLIGHT
    [Test]
    public void SerializeDeserializeXNodeProperties()
    {
      XNodeTestObject testObject = new XNodeTestObject();
      testObject.Document = XDocument.Parse("<root>hehe, root</root>");
      testObject.Element = XElement.Parse(@"<fifth xmlns:json=""http://json.org"" json:Awesome=""true"">element</fifth>");

      string json = JsonConvert.SerializeObject(testObject, Formatting.Indented);
      string expected = @"{
  ""Document"": {
    ""root"": ""hehe, root""
  },
  ""Element"": {
    ""fifth"": {
      ""@xmlns:json"": ""http://json.org"",
      ""@json:Awesome"": ""true"",
      ""#text"": ""element""
    }
  }
}";
      Assert.AreEqual(expected, json);

      XNodeTestObject newTestObject = JsonConvert.DeserializeObject<XNodeTestObject>(json);
      Assert.AreEqual(testObject.Document.ToString(), newTestObject.Document.ToString());
      Assert.AreEqual(testObject.Element.ToString(), newTestObject.Element.ToString());

      Assert.IsNull(newTestObject.Element.Parent);
    }
#endif

#if !SILVERLIGHT
    [Test]
    public void SerializeDeserializeXmlNodeProperties()
    {
      XmlNodeTestObject testObject = new XmlNodeTestObject();
      XmlDocument document = new XmlDocument();
      document.LoadXml("<root>hehe, root</root>");
      testObject.Document = document;

      string json = JsonConvert.SerializeObject(testObject, Formatting.Indented);
      string expected = @"{
  ""Document"": {
    ""root"": ""hehe, root""
  }
}";
      Assert.AreEqual(expected, json);

      XmlNodeTestObject newTestObject = JsonConvert.DeserializeObject<XmlNodeTestObject>(json);
      Assert.AreEqual(testObject.Document.InnerXml, newTestObject.Document.InnerXml);
    }
#endif

    [Test]
    public void FullClientMapSerialization()
    {
      ClientMap source = new ClientMap()
      {
        position = new Pos() { X = 100, Y = 200 },
        center = new PosDouble() { X = 251.6, Y = 361.3 }
      };

      string json = JsonConvert.SerializeObject(source, new PosConverter(), new PosDoubleConverter());
      Assert.AreEqual("{\"position\":new Pos(100,200),\"center\":new PosD(251.6,361.3)}", json);
    }

    public class ClientMap
    {
      public Pos position { get; set; }
      public PosDouble center { get; set; }
    }

    public class Pos
    {
      public int X { get; set; }
      public int Y { get; set; }
    }

    public class PosDouble
    {
      public double X { get; set; }
      public double Y { get; set; }
    }

    public class PosConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        Pos p = (Pos)value;

        if (p != null)
          writer.WriteRawValue(String.Format("new Pos({0},{1})", p.X, p.Y));
        else
          writer.WriteNull();
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        throw new NotImplementedException();
      }

      public override bool CanConvert(Type objectType)
      {
        return objectType.IsAssignableFrom(typeof(Pos));
      }
    }

    public class PosDoubleConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        PosDouble p = (PosDouble)value;

        if (p != null)
          writer.WriteRawValue(String.Format(CultureInfo.InvariantCulture, "new PosD({0},{1})", p.X, p.Y));
        else
          writer.WriteNull();
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        throw new NotImplementedException();
      }

      public override bool CanConvert(Type objectType)
      {
        return objectType.IsAssignableFrom(typeof(PosDouble));
      }
    }

    [Test]
    public void TestEscapeDictionaryStrings()
    {
      const string s = @"host\user";
      string serialized = JsonConvert.SerializeObject(s);
      Assert.AreEqual(@"""host\\user""", serialized);

      Dictionary<int, object> d1 = new Dictionary<int, object>();
      d1.Add(5, s);
      Assert.AreEqual(@"{""5"":""host\\user""}", JsonConvert.SerializeObject(d1));

      Dictionary<string, object> d2 = new Dictionary<string, object>();
      d2.Add(s, 5);
      Assert.AreEqual(@"{""host\\user"":5}", JsonConvert.SerializeObject(d2));
    }

    public class GenericListTestClass
    {
      public List<string> GenericList { get; set; }

      public GenericListTestClass()
      {
        GenericList = new List<string>();
      }
    }

    [Test]
    public void DeserializeExistingGenericList()
    {
      GenericListTestClass c = new GenericListTestClass();
      c.GenericList.Add("1");
      c.GenericList.Add("2");

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      GenericListTestClass newValue = JsonConvert.DeserializeObject<GenericListTestClass>(json);
      Assert.AreEqual(2, newValue.GenericList.Count);
      Assert.AreEqual(typeof(List<string>), newValue.GenericList.GetType());
    }

    [Test]
    public void DeserializeSimpleKeyValuePair()
    {
      List<KeyValuePair<string, string>> list = new List<KeyValuePair<string, string>>();
      list.Add(new KeyValuePair<string, string>("key1", "value1"));
      list.Add(new KeyValuePair<string, string>("key2", "value2"));

      string json = JsonConvert.SerializeObject(list);

      Assert.AreEqual(@"[{""Key"":""key1"",""Value"":""value1""},{""Key"":""key2"",""Value"":""value2""}]", json);

      List<KeyValuePair<string, string>> result = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(json);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual("key1", result[0].Key);
      Assert.AreEqual("value1", result[0].Value);
      Assert.AreEqual("key2", result[1].Key);
      Assert.AreEqual("value2", result[1].Value);
    }

    [Test]
    public void DeserializeComplexKeyValuePair()
    {
      DateTime dateTime = new DateTime(2000, 12, 1, 23, 1, 1, DateTimeKind.Utc);

      List<KeyValuePair<string, WagePerson>> list = new List<KeyValuePair<string, WagePerson>>();
      list.Add(new KeyValuePair<string, WagePerson>("key1", new WagePerson
                                                              {
                                                                BirthDate = dateTime,
                                                                Department = "Department1",
                                                                LastModified = dateTime,
                                                                HourlyWage = 1
                                                              }));
      list.Add(new KeyValuePair<string, WagePerson>("key2", new WagePerson
      {
        BirthDate = dateTime,
        Department = "Department2",
        LastModified = dateTime,
        HourlyWage = 2
      }));

      string json = JsonConvert.SerializeObject(list, Formatting.Indented);

      Assert.AreEqual(@"[
  {
    ""Key"": ""key1"",
    ""Value"": {
      ""HourlyWage"": 1.0,
      ""Name"": null,
      ""BirthDate"": ""\/Date(975711661000)\/"",
      ""LastModified"": ""\/Date(975711661000)\/""
    }
  },
  {
    ""Key"": ""key2"",
    ""Value"": {
      ""HourlyWage"": 2.0,
      ""Name"": null,
      ""BirthDate"": ""\/Date(975711661000)\/"",
      ""LastModified"": ""\/Date(975711661000)\/""
    }
  }
]", json);

      List<KeyValuePair<string, WagePerson>> result = JsonConvert.DeserializeObject<List<KeyValuePair<string, WagePerson>>>(json);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual("key1", result[0].Key);
      Assert.AreEqual(1, result[0].Value.HourlyWage);
      Assert.AreEqual("key2", result[1].Key);
      Assert.AreEqual(2, result[1].Value.HourlyWage);
    }

    public class StringListAppenderConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        writer.WriteValue(value);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        List<string> existingStrings = (List<string>)existingValue;
        List<string> newStrings = new List<string>(existingStrings);

        reader.Read();

        while (reader.TokenType != JsonToken.EndArray)
        {
          string s = (string)reader.Value;
          newStrings.Add(s);

          reader.Read();
        }

        return newStrings;
      }

      public override bool CanConvert(Type objectType)
      {
        return (objectType == typeof(List<string>));
      }
    }

    [Test]
    public void StringListAppenderConverterTest()
    {
      Movie p = new Movie();
      p.ReleaseCountries = new List<string> { "Existing" };

      JsonConvert.PopulateObject("{'ReleaseCountries':['Appended']}", p, new JsonSerializerSettings
        {
          Converters = new List<JsonConverter> { new StringListAppenderConverter() }
        });

      Assert.AreEqual(2, p.ReleaseCountries.Count);
      Assert.AreEqual("Existing", p.ReleaseCountries[0]);
      Assert.AreEqual("Appended", p.ReleaseCountries[1]);
    }

    public class StringAppenderConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        writer.WriteValue(value);
      }

      public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
      {
        string existingString = (string)existingValue;
        string newString = existingString + (string)reader.Value;

        return newString;
      }

      public override bool CanConvert(Type objectType)
      {
        return (objectType == typeof(string));
      }
    }

    [Test]
    public void StringAppenderConverterTest()
    {
      Movie p = new Movie();
      p.Name = "Existing,";

      JsonConvert.PopulateObject("{'Name':'Appended'}", p, new JsonSerializerSettings
      {
        Converters = new List<JsonConverter> { new StringAppenderConverter() }
      });

      Assert.AreEqual(p.Name, "Existing,Appended");
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Additional content found in JSON reference object. A JSON reference object should only have a $ref property.")]
    public void SerializeRefAdditionalContent()
    {
      //Additional text found in JSON string after finishing deserializing object.
      //Test 1
      var reference = new Dictionary<string, object>();
      reference.Add("$ref", "Persons");
      reference.Add("$id", 1);

      var child = new Dictionary<string, object>();
      child.Add("_id", 2);
      child.Add("Name", "Isabell");
      child.Add("Father", reference);

      var json = JsonConvert.SerializeObject(child);
      JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JSON reference $ref property must have a string value.")]
    public void SerializeRefBadType()
    {
      //Additional text found in JSON string after finishing deserializing object.
      //Test 1
      var reference = new Dictionary<string, object>();
      reference.Add("$ref", 1);
      reference.Add("$id", 1);

      var child = new Dictionary<string, object>();
      child.Add("_id", 2);
      child.Add("Name", "Isabell");
      child.Add("Father", reference);

      var json = JsonConvert.SerializeObject(child);
      JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
    }

    public class ConstructorCompexIgnoredProperty
    {
      [JsonIgnore]
      public Product Ignored { get; set; }
      public string First { get; set; }
      public int Second { get; set; }

      public ConstructorCompexIgnoredProperty(string first, int second)
      {
        First = first;
        Second = second;
      }
    }

    [Test]
    public void DeserializeIgnoredPropertyInConstructor()
    {
      string json = @"{""First"":""First"",""Second"":2,""Ignored"":{""Name"":""James""},""AdditionalContent"":{""LOL"":true}}";

      ConstructorCompexIgnoredProperty cc = JsonConvert.DeserializeObject<ConstructorCompexIgnoredProperty>(json);
      Assert.AreEqual("First", cc.First);
      Assert.AreEqual(2, cc.Second);
      Assert.AreEqual(null, cc.Ignored);
    }

    public class ShouldSerializeTestClass
    {
      internal bool _shouldSerializeName;

      public string Name { get; set; }
      public int Age { get; set; }

      public void ShouldSerializeAge()
      {
        // dummy. should never be used because it doesn't return bool
      }

      public bool ShouldSerializeName()
      {
        return _shouldSerializeName;
      }
    }

    [Test]
    public void ShouldSerializeTest()
    {
      ShouldSerializeTestClass c = new ShouldSerializeTestClass();
      c.Name = "James";
      c.Age = 27;

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Age"": 27
}", json);

      c._shouldSerializeName = true;
      json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Name"": ""James"",
  ""Age"": 27
}", json);

      ShouldSerializeTestClass deserialized = JsonConvert.DeserializeObject<ShouldSerializeTestClass>(json);
      Assert.AreEqual("James", deserialized.Name);
      Assert.AreEqual(27, deserialized.Age);
    }

    public class Employee
    {
      public string Name { get; set; }
      public Employee Manager { get; set; }

      public bool ShouldSerializeManager()
      {
        return (Manager != this);
      }
    }

    [Test]
    public void ShouldSerializeExample()
    {
      Employee joe = new Employee();
      joe.Name = "Joe Employee";
      Employee mike = new Employee();
      mike.Name = "Mike Manager";

      joe.Manager = mike;
      mike.Manager = mike;

      string json = JsonConvert.SerializeObject(new[] { joe, mike }, Formatting.Indented);
      // [
      //   {
      //     "Name": "Joe Employee",
      //     "Manager": {
      //       "Name": "Mike Manager"
      //     }
      //   },
      //   {
      //     "Name": "Mike Manager"
      //   }
      // ]

      Console.WriteLine(json);
    }

    public class DictionaryKey
    {
      public string Value { get; set; }

      public override string ToString()
      {
        return Value;
      }

      public static implicit operator DictionaryKey(string value)
      {
        return new DictionaryKey() { Value = value };
      }
    }

    [Test]
    public void SerializeDeserializeDictionaryKey()
    {
      Dictionary<DictionaryKey, string> dictionary = new Dictionary<DictionaryKey, string>();

      dictionary.Add(new DictionaryKey() { Value = "First!" }, "First");
      dictionary.Add(new DictionaryKey() { Value = "Second!" }, "Second");

      string json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

      Assert.AreEqual(@"{
  ""First!"": ""First"",
  ""Second!"": ""Second""
}", json);

      Dictionary<DictionaryKey, string> newDictionary =
        JsonConvert.DeserializeObject<Dictionary<DictionaryKey, string>>(json);

      Assert.AreEqual(2, newDictionary.Count);
    }

    [Test]
    public void SerializeNullableArray()
    {
      string jsonText = JsonConvert.SerializeObject(new double?[] { 2.4, 4.3, null }, Formatting.Indented);

      Assert.AreEqual(@"[
  2.4,
  4.3,
  null
]", jsonText);

      double?[] d = (double?[])JsonConvert.DeserializeObject(jsonText, typeof(double?[]));

      Assert.AreEqual(3, d.Length);
      Assert.AreEqual(2.4, d[0]);
      Assert.AreEqual(4.3, d[1]);
      Assert.AreEqual(null, d[2]);
    }

#if !SILVERLIGHT && !NET20 && !PocketPC
    [Test]
    public void SerializeHashSet()
    {
      string jsonText = JsonConvert.SerializeObject(new HashSet<string>()
                                                      {
                                                        "One",
                                                        "2",
                                                        "III"
                                                      }, Formatting.Indented);

      Assert.AreEqual(@"[
  ""One"",
  ""2"",
  ""III""
]", jsonText);

      HashSet<string> d = JsonConvert.DeserializeObject<HashSet<string>>(jsonText);

      Assert.AreEqual(3, d.Count);
      Assert.IsTrue(d.Contains("One"));
      Assert.IsTrue(d.Contains("2"));
      Assert.IsTrue(d.Contains("III"));
    }
#endif

    private class MyClass
    {
      public byte[] Prop1 { get; set; }

      public MyClass()
      {
        Prop1 = new byte[0];
      }
    }

    [Test]
    public void DeserializeByteArray()
    {
      JsonSerializer serializer1 = new JsonSerializer();
      serializer1.Converters.Add(new IsoDateTimeConverter());
      serializer1.NullValueHandling = NullValueHandling.Ignore;

      string json = @"[{""Prop1"":""""},{""Prop1"":""""}]";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));

      MyClass[] z = (MyClass[])serializer1.Deserialize(reader, typeof(MyClass[]));
      Assert.AreEqual(2, z.Length);
      Assert.AreEqual(0, z[0].Prop1.Length);
      Assert.AreEqual(0, z[1].Prop1.Length);
    }

#if !NET20 && !PocketPC && !SILVERLIGHT
    public class StringDictionaryTestClass
    {
      public StringDictionary StringDictionaryProperty { get; set; }
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Cannot create and populate list type System.Collections.Specialized.StringDictionary.")]
    public void StringDictionaryTest()
    {
      StringDictionaryTestClass s1 = new StringDictionaryTestClass()
        {
          StringDictionaryProperty = new StringDictionary()
            {
              {"1", "One"},
              {"2", "II"},
              {"3", "3"}              
            }
        };

      string json = JsonConvert.SerializeObject(s1, Formatting.Indented);

      JsonConvert.DeserializeObject<StringDictionaryTestClass>(json);
    }
#endif

    [JsonObject(MemberSerialization.OptIn)]
    public struct StructWithAttribute
    {
      public string MyString { get; set; }
      [JsonProperty]
      public int MyInt { get; set; }
    }

    [Test]
    public void SerializeStructWithJsonObjectAttribute()
    {
      StructWithAttribute testStruct = new StructWithAttribute
        {
          MyInt = int.MaxValue
        };

      string json = JsonConvert.SerializeObject(testStruct, Formatting.Indented);

      Assert.AreEqual(@"{
  ""MyInt"": 2147483647
}", json);

      StructWithAttribute newStruct = JsonConvert.DeserializeObject<StructWithAttribute>(json);

      Assert.AreEqual(int.MaxValue, newStruct.MyInt);
    }

    public class TimeZoneOffsetObject
    {
      public DateTimeOffset Offset { get; set; }
    }

#if !NET20
    [Test]
    public void ReadWriteTimeZoneOffset()
    {
      var serializeObject = JsonConvert.SerializeObject(new TimeZoneOffsetObject
      {
        Offset = new DateTimeOffset(new DateTime(2000, 1, 1), TimeSpan.FromHours(6))
      });

      Assert.AreEqual("{\"Offset\":\"\\/Date(946663200000+0600)\\/\"}", serializeObject);
      var deserializeObject = JsonConvert.DeserializeObject<TimeZoneOffsetObject>(serializeObject);
      Assert.AreEqual(TimeSpan.FromHours(6), deserializeObject.Offset.Offset);
      Assert.AreEqual(new DateTime(2000, 1, 1), deserializeObject.Offset.Date);
    }
#endif

    public abstract class LogEvent
    {
      [JsonProperty("event")]
      public abstract string EventName { get; }
    }

    public class DerivedEvent : LogEvent
    {
      public override string EventName { get { return "derived"; } }
    }

    [Test]
    public void OverridenPropertyMembers()
    {
      string json = JsonConvert.SerializeObject(new DerivedEvent(), Formatting.Indented);

      Assert.AreEqual(@"{
  ""event"": ""derived""
}", json);
    }

#if !(NET35 || NET20 || SILVERLIGHT)
   [Test]
    public void SerializeExpandoObject()
    {
      dynamic expando = new ExpandoObject();
      expando.Int = 1;
      expando.Decimal = 99.9d;
      expando.Complex = new ExpandoObject();
      expando.Complex.String = "I am a string";
      expando.Complex.DateTime = new DateTime(2000, 12, 20, 18, 55, 0, DateTimeKind.Utc);

      string json = JsonConvert.SerializeObject(expando, Formatting.Indented);
      Assert.AreEqual(@"{
  ""Int"": 1,
  ""Decimal"": 99.9,
  ""Complex"": {
    ""String"": ""I am a string"",
    ""DateTime"": ""\/Date(977338500000)\/""
  }
}", json);

      IDictionary<string, object> newExpando = JsonConvert.DeserializeObject<ExpandoObject>(json);

      Assert.IsInstanceOfType(typeof(long), newExpando["Int"]);
      Assert.AreEqual(expando.Int, newExpando["Int"]);

      Assert.IsInstanceOfType(typeof(double), newExpando["Decimal"]);
      Assert.AreEqual(expando.Decimal, newExpando["Decimal"]);

      Assert.IsInstanceOfType(typeof(JObject), newExpando["Complex"]);
      JObject o = (JObject)newExpando["Complex"];

      Assert.IsInstanceOfType(typeof(string), ((JValue)o["String"]).Value);
      Assert.AreEqual(expando.Complex.String, (string)o["String"]);

      Assert.IsInstanceOfType(typeof(DateTime), ((JValue)o["DateTime"]).Value);
      Assert.AreEqual(expando.Complex.DateTime, (DateTime)o["DateTime"]);
    }
#endif
  }
}