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
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Collections.ObjectModel;
using Newtonsoft.Json.Linq;
using System.Linq;
using Newtonsoft.Json.Converters;
#if !PocketPC
using System.Runtime.Serialization.Json;
#endif
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests
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

      Console.WriteLine(jsonText);
    }

    [Test]
    public void TypedObjectDeserialization()
    {
      Product product = new Product();

      product.Name = "Apple";
      product.Expiry = new DateTime(2008, 12, 28);
      product.Price = 3.99M;
      product.Sizes = new string[] { "Small", "Medium", "Large" };

      string output = JsonConvert.SerializeObject(product);
      //{
      //  "Name": "Apple",
      //  "Expiry": "\/Date(1230375600000+1300)\/",
      //  "Price": 3.99,
      //  "Sizes": [
      //    "Small",
      //    "Medium",
      //    "Large"
      //  ]
      //}

      Product deserializedProduct = (Product)JsonConvert.DeserializeObject(output, typeof(Product));

      Assert.AreEqual("Apple", deserializedProduct.Name);
      Assert.AreEqual(new DateTime(2008, 12, 28), deserializedProduct.Expiry);
      Assert.AreEqual(3.99, deserializedProduct.Price);
      Assert.AreEqual("Small", deserializedProduct.Sizes[0]);
      Assert.AreEqual("Medium", deserializedProduct.Sizes[1]);
      Assert.AreEqual("Large", deserializedProduct.Sizes[2]);
    }

    //[Test]
    //public void Advanced()
    //{
    //  Product product = new Product();
    //  product.Expiry = new DateTime(2008, 12, 28);

    //  JsonSerializer serializer = new JsonSerializer();
    //  serializer.Converters.Add(new JavaScriptDateTimeConverter());
    //  serializer.NullValueHandling = NullValueHandling.Ignore;

    //  using (StreamWriter sw = new StreamWriter(@"c:\json.txt"))
    //  using (JsonWriter writer = new JsonTextWriter(sw))
    //  {
    //    serializer.Serialize(writer, product);
    //    // {"Expiry":new Date(1230375600000),"Price":0}
    //  }
    //}

    [Test]
    public void JsonConvertSerializer()
    {
      string value = @"{""Name"":""Orange"", ""Price"":3.99, ""Expiry"":""01/24/2010 12:00:00""}";

      Product p = JsonConvert.DeserializeObject(value, typeof(Product)) as Product;

      Assert.AreEqual("Orange", p.Name);
      Assert.AreEqual(new DateTime(2010, 1, 24, 12, 0, 0), p.Expiry);
      Assert.AreEqual(3.99, p.Price);
    }

    [Test]
    public void DeserializeJavaScriptDate()
    {
      DateTime dateValue = new DateTime(2000, 3, 30);
      Dictionary<string, object> testDictionary = new Dictionary<string, object>();
      testDictionary["date"] = dateValue;

      string jsonText = JsonConvert.SerializeObject(testDictionary);

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
      string value = @"{""Name"":""Orange"", ""Price"":3.99, ""Expiry"":""01/24/2010 12:00:00""}";

      Hashtable p = JsonConvert.DeserializeObject(value, typeof(Hashtable)) as Hashtable;

      Assert.AreEqual("Orange", p["Name"].ToString());
    }

    [Test]
    public void TypedHashtableDeserialization()
    {
      string value = @"{""Name"":""Orange"", ""Hash"":{""Expiry"":""01/24/2010 12:00:00"",""UntypedArray"":[""01/24/2010 12:00:00""]}}";

      TypedSubHashtable p = JsonConvert.DeserializeObject(value, typeof(TypedSubHashtable)) as TypedSubHashtable;

      Assert.AreEqual("01/24/2010 12:00:00", p.Hash["Expiry"].ToString());
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

    public class JsonPropertyClass
    {
      [JsonProperty("pie")]
      public string Pie = "Yum";

      [JsonIgnore]
      public string pie = "No pie for you!";

      public string pie1 = "PieChart!";

      private int _sweetCakesCount;

      [JsonProperty("sweet_cakes_count")]
      public int SweetCakesCount
      {
        get { return _sweetCakesCount; }
        set { _sweetCakesCount = value; }
      }
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
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"A member with the name 'pie' already exists on Newtonsoft.Json.Tests.TestObjects.BadJsonPropertyClass. Use the JsonPropertyAttribute to specify another name.")]
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

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Could not find member 'Price' on object of type 'ProductShort'")]
    public void MissingMemberDeserialize()
    {
      Product product = new Product();

      product.Name = "Apple";
      product.Expiry = new DateTime(2008, 12, 28);
      product.Price = 3.99M;
      product.Sizes = new string[] { "Small", "Medium", "Large" };

      string output = JsonConvert.SerializeObject(product);
      //{
      //  "Name": "Apple",
      //  "Expiry": new Date(1230422400000),
      //  "Price": 3.99,
      //  "Sizes": [
      //    "Small",
      //    "Medium",
      //    "Large"
      //  ]
      //}

      ProductShort deserializedProductShort = (ProductShort)JsonConvert.DeserializeObject(output, typeof(ProductShort), new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
    }

    [Test]
    public void MissingMemberDeserializeOkay()
    {
      Product product = new Product();

      product.Name = "Apple";
      product.Expiry = new DateTime(2008, 12, 28);
      product.Price = 3.99M;
      product.Sizes = new string[] { "Small", "Medium", "Large" };

      string output = JsonConvert.SerializeObject(product);
      //{
      //  "Name": "Apple",
      //  "Expiry": new Date(1230422400000),
      //  "Price": 3.99,
      //  "Sizes": [
      //    "Small",
      //    "Medium",
      //    "Large"
      //  ]
      //}

      JsonSerializer jsonSerializer = new JsonSerializer();
      jsonSerializer.MissingMemberHandling = MissingMemberHandling.Ignore;

      object deserializedValue;

      using (JsonReader jsonReader = new JsonTextReader(new StringReader(output)))
      {
        deserializedValue = jsonSerializer.Deserialize(jsonReader, typeof(ProductShort));
      }

      ProductShort deserializedProductShort = (ProductShort)deserializedValue;

      Assert.AreEqual("Apple", deserializedProductShort.Name);
      Assert.AreEqual(new DateTime(2008, 12, 28), deserializedProductShort.Expiry);
      Assert.AreEqual("Small", deserializedProductShort.Sizes[0]);
      Assert.AreEqual("Medium", deserializedProductShort.Sizes[1]);
      Assert.AreEqual("Large", deserializedProductShort.Sizes[2]);
    }

#if !PocketPC
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
    public void DateTime()
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
#endif

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
      Assert.AreEqual(@"{""StringValue"":""I am a string"",""IntValue"":2147483647,""NestedAnonymous"":{""NestedValue"":255},""NestedArray"":[1,2],""Product"":{""Name"":""TestProduct"",""Expiry"":""\/Date(946684800000)\/"",""Price"":0,""Sizes"":null}}", json);

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

      Assert.AreEqual(@"[{""Name"":""Test1"",""Expiry"":""\/Date(946684800000)\/"",""Price"":0,""Sizes"":null},{""Name"":""Test2"",""Expiry"":""\/Date(946684800000)\/"",""Price"":0,""Sizes"":null},{""Name"":""Test3"",""Expiry"":""\/Date(946684800000)\/"",""Price"":0,""Sizes"":null}]",
        sw.GetStringBuilder().ToString());

      ProductCollection collectionNew = (ProductCollection)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(sw.GetStringBuilder().ToString())), typeof(ProductCollection));

      CollectionAssert.AreEqual(collection, collectionNew);
    }

    [Test]
    public void NullValueHandlingSerialization()
    {
      Store s1 = new Store();

      JsonSerializer jsonSerializer = new JsonSerializer();
      jsonSerializer.NullValueHandling = NullValueHandling.Ignore;

      StringWriter sw = new StringWriter();
      jsonSerializer.Serialize(sw, s1);

      //JsonConvert.ConvertDateTimeToJavaScriptTicks(s1.Establised.DateTime)

      Assert.AreEqual(@"{""Color"":2,""Establised"":""\/Date(1264122061000+0000)\/"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""Expiry"":""\/Date(949532490000)\/"",""Price"":0},{""Name"":""Alien"",""Expiry"":""\/Date(946684800000)\/"",""Price"":0}]}", sw.GetStringBuilder().ToString());

      Store s2 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader("{}")), typeof(Store));
      Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", s2.Escape);

      Store s3 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(@"{""Escape"":null}")), typeof(Store));
      Assert.AreEqual("\r\n\t\f\b?{\\r\\n\"\'", s3.Escape);

      Store s4 = (Store)jsonSerializer.Deserialize(new JsonTextReader(new StringReader(@"{""Color"":2,""Establised"":""\/Date(1264071600000+1300)\/"",""Width"":1.1,""Employees"":999,""RoomsPerFloor"":[1,2,3,4,5,6,7,8,9],""Open"":false,""Symbol"":""@"",""Mottos"":[""Hello World"",""öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~"",null,"" ""],""Cost"":100980.1,""Escape"":""\r\n\t\f\b?{\\r\\n\""'"",""product"":[{""Name"":""Rocket"",""Expiry"":""\/Date(949485690000+1300)\/"",""Price"":0},{""Name"":""Alien"",""Expiry"":""\/Date(946638000000)\/"",""Price"":0}]}")), typeof(Store));
      Assert.AreEqual(s1.Establised, s3.Establised);
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
      string jsonText = @"[3, ""somestring"",[1,2,3]]";
      ArrayList o = JsonConvert.DeserializeObject<ArrayList>(jsonText);

      Assert.AreEqual(3, o.Count);
      Assert.AreEqual(3, ((JArray)o[2]).Count);
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
      Assert.AreEqual(@"{""DefaultConverter"":""\/Date(0)\/"",""MemberConverter"":""1970-01-01T00:00:00.0000000Z""}", json);

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
      Assert.AreEqual(@"{""DefaultConverter"":new Date(0),""MemberConverter"":""1970-01-01T00:00:00.0000000Z""}", json);

      MemberConverterClass m2 = JsonConvert.DeserializeObject<MemberConverterClass>(json, new JavaScriptDateTimeConverter());

      Assert.AreEqual(testDate, m2.DefaultConverter);
      Assert.AreEqual(testDate, m2.MemberConverter);
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
    public void DefaultValueAttributeTest()
    {
      string json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass(),
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{""TestField1"":0,""TestProperty1"":null}", json);

      json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = int.MinValue, TestProperty1 = "NotDefault" },
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{""TestField1"":-2147483648,""TestProperty1"":""NotDefault""}", json);

      json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = 21, TestProperty1 = "NotDefault" },
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{""TestProperty1"":""NotDefault""}", json);

      json = JsonConvert.SerializeObject(new DefaultValueAttributeTestClass { TestField1 = 21, TestProperty1 = "TestProperty1Value" },
        Formatting.None, new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });
      Assert.AreEqual(@"{}", json);
    }

    [Test]
    public void SerializeInvoice()
    {
      Invoice invoice = new Invoice
                        {
                          Company = "Acme Ltd.",
                          Amount = 50.0m,
                          Paid = false
                        };

      string json = JsonConvert.SerializeObject(invoice);

      Console.WriteLine(json);
      // {"Company":"Acme Ltd.","Amount":50.0,"Paid":false,"PaidDate":null}

      json = JsonConvert.SerializeObject(invoice,
        Formatting.None,
        new JsonSerializerSettings { DefaultValueHandling = DefaultValueHandling.Ignore });

      Console.WriteLine(json);
      // {"Company":"Acme Ltd.","Amount":50.0}
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
        RawContent = new JsonRaw("[1,2,3,4,5]"),
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
      Assert.AreEqual("[1,2,3,4,5]", personRaw.RawContent.Content);
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
    public void MissingMemberIgnoreComplexValue()
    {
      JsonSerializer serializer = new JsonSerializer { MissingMemberHandling = MissingMemberHandling.Ignore };
      serializer.Converters.Add(new JavaScriptDateTimeConverter());

      string response = @"{""PreProperty"":1,""DateProperty"":new Date(1225962698973),""PostProperty"":2}";

      MyClass myClass = (MyClass)serializer.Deserialize(new StringReader(response), typeof(MyClass));

      Assert.AreEqual(1, myClass.PreProperty);
      Assert.AreEqual(2, myClass.PostProperty);
    }

    [Test]
    public void DeserializeInt64ToNullableDouble()
    {
      string json = @"{""Height"":1}";

      DoubleClass c = JsonConvert.DeserializeObject<DoubleClass>(json);
      Assert.AreEqual(1, c.Height);
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Could not find member 'Missing' on object of type 'DoubleClass'")]
    public void MissingMemeber()
    {
      string json = @"{""Missing"":1}";

      JsonConvert.DeserializeObject<DoubleClass>(json, new JsonSerializerSettings { MissingMemberHandling = MissingMemberHandling.Error });
    }

    [Test]
    public void SerializeTypeProperty()
    {
      TypeClass typeClass = new TypeClass {TypeProperty = typeof (bool)};

      string json = JsonConvert.SerializeObject(typeClass);
      Assert.AreEqual(@"{""TypeProperty"":""System.Boolean""}", json);

      TypeClass typeClass2 = JsonConvert.DeserializeObject<TypeClass>(json);
      Assert.AreEqual(typeof(bool), typeClass2.TypeProperty);
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

    public class GoogleMapGeocoderStructure
    {
      public string Name;
      public Status Status;
      public List<Placemark> Placemark;
    }

    public class Status
    {
      public string Request;
      public string Code;
    }

    public class Placemark
    {
      public string Address;
      public AddressDetails AddressDetails;
      public Point Point;
    }

    public class AddressDetails
    {
      public int Accuracy;
      public Country Country;
    }

    public class Country
    {
      public string CountryNameCode;
      public AdministrativeArea AdministrativeArea;
    }

    public class AdministrativeArea
    {
      public string AdministrativeAreaName;
      public SubAdministrativeArea SubAdministrativeArea;
    }

    public class SubAdministrativeArea
    {
      public string SubAdministrativeAreaName;
      public Locality Locality;
    }

    public class Locality
    {
      public string LocalityName;
      public Thoroughfare Thoroughfare;
      public PostalCode PostalCode;
    }

    public class Thoroughfare
    {
      public string ThoroughfareName;
    }

    public class PostalCode
    {
      public string PostalCodeNumber;
    }

    public class Point
    {
      public List<decimal> Coordinates;
    }

    [Test]
    public  void DeserializeGoogleGeoCode()
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

      //JavaScriptSerializer serializer = new JavaScriptSerializer();
      //GoogleMapGeocoderStructure jsonGoogleMapGeocoder = serializer.Deserialize<GoogleMapGeocoderStructure>(json);

      GoogleMapGeocoderStructure jsonGoogleMapGeocoder = JsonConvert.DeserializeObject<GoogleMapGeocoderStructure>(json);
    }

    public class Co : ICo
    {
      public String Name { get; set; }
    }

    public interface ICo
    {
      String Name { get; set; }
    }

    public interface ITest
    {
      ICo co { get; set; }
    }

    public class Test : ITest
    {
      public ICo co { get; set; }

      public Test()
      {
      }
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Could not create an instance of type Newtonsoft.Json.Tests.JsonSerializerTest+ICo. Type is an interface or abstract class and cannot be instantated.")]
    public void DeserializeInterfaceProperty()
    {
      Test testClass = new Test();
      testClass.co = new Co();
      String strFromTest = JsonConvert.SerializeObject(testClass);
      Test testFromDe = (Test)JsonConvert.DeserializeObject(strFromTest, typeof(Test));
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

      using (FileStream fs = File.Open(@"c:\person.json", FileMode.CreateNew))
      using (StreamWriter sw = new StreamWriter(fs))
      using (JsonWriter jw = new JsonTextWriter(sw))
      {
        jw.Formatting = Formatting.Indented;

        JsonSerializer serializer = new JsonSerializer();
        serializer.Serialize(jw, person);
      }
    }

    public class LogEntry
    {
      public string Details { get; set; }
      public DateTime LogDate { get; set; }
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

    public class PropertyCase
    {
      public string firstName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string lastName { get; set; }
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

    public class SuperKlass
    {
      public string SuperProp { get; set; }
      public SuperKlass() { SuperProp = "default superprop"; }
    }

    public class SubKlass : SuperKlass
    {
      public string SubProp { get; set; }
      public SubKlass(string subprop) { SubProp = subprop; }
    }

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
  }
}