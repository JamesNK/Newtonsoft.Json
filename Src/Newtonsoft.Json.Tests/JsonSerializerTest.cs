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
using System.Runtime.Serialization.Json;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests
{
  public class Product
  {
    public string Name;
    public DateTime Expiry = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public decimal Price;
    public string[] Sizes;

    public override bool Equals(object obj)
    {
      if (obj is Product)
      {
        Product p = (Product)obj;

        return (p.Name == Name && p.Expiry == Expiry && p.Price == Price);
      }

      return base.Equals(obj);
    }

    public override int GetHashCode()
    {
      return (Name ?? string.Empty).GetHashCode();
    }
  }

  public class ProductCollection : List<Product>
  {
  }

  public class ProductShort
  {
    public string Name;
    public DateTime Expiry;
    //public decimal Price;
    public string[] Sizes;
  }

  public class Store
  {
    public StoreColor Color = StoreColor.Yellow;
    public DateTimeOffset Establised = new DateTimeOffset(2010, 1, 22, 1, 1, 1, TimeSpan.Zero);
    public double Width = 1.1;
    public int Employees = 999;
    public int[] RoomsPerFloor = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
    public bool Open = false;
    public char Symbol = '@';
    public List<string> Mottos = new List<string>();
    public decimal Cost = 100980.1M;
    public string Escape = "\r\n\t\f\b?{\\r\\n\"\'";
    public List<Product> product = new List<Product>();

    public Store()
    {
      Mottos.Add("Hello World");
      Mottos.Add("öäüÖÄÜ\\'{new Date(12345);}[222]_µ@²³~");
      Mottos.Add(null);
      Mottos.Add(" ");

      Product rocket = new Product();
      rocket.Name = "Rocket";
      rocket.Expiry = new DateTime(2000, 2, 2, 23, 1, 30, DateTimeKind.Utc);
      Product alien = new Product();
      alien.Name = "Alien";

      product.Add(rocket);
      product.Add(alien);
    }
  }

  public enum StoreColor
  {
    Black,
    Red,
    Yellow,
    White
  }

  public class ClassWithGuid
  {
    public Guid GuidField;
  }

  public class ClassWithArray
  {
    private readonly IList<long> bar;
    private string foo;

    public ClassWithArray()
    {
      bar = new List<Int64>() { int.MaxValue };
    }

    [JsonProperty("foo")]
    public string Foo
    {
      get { return foo; }
      set { foo = value; }
    }

    [JsonProperty(PropertyName = "bar")]
    public IList<long> Bar
    {
      get { return bar; }
    }
  }

  [JsonObject]
  public class ConverableMembers
  {
    public string String = "string";
    public int Int32 = int.MaxValue;
    public uint UInt32 = uint.MaxValue;
    public byte Byte = byte.MaxValue;
    public sbyte SByte = sbyte.MaxValue;
    public short Short = short.MaxValue;
    public ushort UShort = ushort.MaxValue;
    public long Long = long.MaxValue;
    public ulong ULong = long.MaxValue;
    public double Double = double.MaxValue;
    public float Float = float.MaxValue;
    public DBNull DBNull = DBNull.Value;
    public bool Bool = true;
    public char Char = '\0';
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class JsonIgnoreAttributeOnClassTestClass
  {
    private int _property = 21;
    private int _ignoredProperty = 12;

    [JsonProperty("TheField")]
    public int Field;

    [JsonProperty]
    public int Property
    {
      get { return _property; }
    }

    public int IgnoredField;

    [JsonProperty]
    [JsonIgnore] // JsonIgnore should take priority
    public int IgnoredProperty
    {
      get { return _ignoredProperty; }
    }
  }

  [JsonObject(MemberSerialization.OptIn)]
  public class Person
  {
    [JsonProperty]
    public string Name { get; set; }

    [JsonProperty]
    public DateTime BirthDate { get; set; }

    // not serialized
    public string Department { get; set; }
  }

  public class JsonSerializerTest : TestFixtureBase
  {
    [Test]
    public void PersonTypedObjectDeserialization()
    {
      Store store = new Store();

      string jsonText = JavaScriptConvert.SerializeObject(store);

      Store deserializedStore = (Store)JavaScriptConvert.DeserializeObject(jsonText, typeof(Store));

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

      string output = JavaScriptConvert.SerializeObject(product);
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

      Product deserializedProduct = (Product)JavaScriptConvert.DeserializeObject(output, typeof(Product));

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
    public void JavaScriptConvertSerializer()
    {
      string value = @"{""Name"":""Orange"", ""Price"":3.99, ""Expiry"":""01/24/2010 12:00:00""}";

      Product p = JavaScriptConvert.DeserializeObject(value, typeof(Product)) as Product;

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

      string jsonText = JavaScriptConvert.SerializeObject(testDictionary);

      Dictionary<string, object> deserializedDictionary = (Dictionary<string, object>)JavaScriptConvert.DeserializeObject(jsonText, typeof(Dictionary<string, object>));
      DateTime deserializedDate = (DateTime)deserializedDictionary["date"];

      Assert.AreEqual(dateValue, deserializedDate);

      Console.WriteLine("DeserializeJavaScriptDate");
      Console.WriteLine(jsonText);
      Console.WriteLine();
      Console.WriteLine(jsonText);
    }

    public class MethodExecutorObject
    {
      public string serverClassName;
      public object[] serverMethodParams;
      public string clientGetResultFunction;
    }

    [Test]
    public void TestMethodExecutorObject()
    {
      MethodExecutorObject executorObject = new MethodExecutorObject();
      executorObject.serverClassName = "BanSubs";
      executorObject.serverMethodParams = new object[] { "21321546", "101", "1236", "D:\\1.txt" };
      executorObject.clientGetResultFunction = "ClientBanSubsCB";

      string output = JavaScriptConvert.SerializeObject(executorObject);

      MethodExecutorObject executorObject2 = JavaScriptConvert.DeserializeObject(output, typeof(MethodExecutorObject)) as MethodExecutorObject;

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

      Hashtable p = JavaScriptConvert.DeserializeObject(value, typeof(Hashtable)) as Hashtable;

      Assert.AreEqual("Orange", p["Name"].ToString());
    }

    public class TypedSubHashtable
    {
      public string Name;
      public Hashtable Hash;
    }

    [Test]
    public void TypedHashtableDeserialization()
    {
      string value = @"{""Name"":""Orange"", ""Hash"":{""Expiry"":""01/24/2010 12:00:00"",""UntypedArray"":[""01/24/2010 12:00:00""]}}";

      TypedSubHashtable p = JavaScriptConvert.DeserializeObject(value, typeof(TypedSubHashtable)) as TypedSubHashtable;

      Assert.AreEqual("01/24/2010 12:00:00", p.Hash["Expiry"].ToString());
      Assert.AreEqual(@"[
  ""01/24/2010 12:00:00""
]", p.Hash["UntypedArray"].ToString());
    }
#endif

    public class GetOnlyPropertyClass
    {
      public string Field = "Field";

      public string GetOnlyProperty
      {
        get { return "GetOnlyProperty"; }
      }
    }

    [Test]
    public void SerializeDeserializeGetOnlyProperty()
    {
      string value = JavaScriptConvert.SerializeObject(new GetOnlyPropertyClass());

      GetOnlyPropertyClass c = JavaScriptConvert.DeserializeObject<GetOnlyPropertyClass>(value);

      Assert.AreEqual(c.Field, "Field");
      Assert.AreEqual(c.GetOnlyProperty, "GetOnlyProperty");
    }

    public class SetOnlyPropertyClass
    {
      public string Field = "Field";

      public string SetOnlyProperty
      {
        set { }
      }
    }

    [Test]
    public void SerializeDeserializeSetOnlyProperty()
    {
      string value = JavaScriptConvert.SerializeObject(new SetOnlyPropertyClass());

      SetOnlyPropertyClass c = JavaScriptConvert.DeserializeObject<SetOnlyPropertyClass>(value);

      Assert.AreEqual(c.Field, "Field");
    }

    public class JsonIgnoreAttributeTestClass
    {
      private int _property = 21;
      private int _ignoredProperty = 12;

      public int Field;
      public int Property
      {
        get { return _property; }
      }

      [JsonIgnore]
      public int IgnoredField;

      [JsonIgnore]
      public int IgnoredProperty
      {
        get { return _ignoredProperty; }
      }

      [JsonIgnore]
      public Product IgnoredObject = new Product();
    }

    [Test]
    public void JsonIgnoreAttributeTest()
    {
      string json = JavaScriptConvert.SerializeObject(new JsonIgnoreAttributeTestClass());

      Assert.AreEqual(@"{""Field"":0,""Property"":21}", json);

      JsonIgnoreAttributeTestClass c = JavaScriptConvert.DeserializeObject<JsonIgnoreAttributeTestClass>(@"{""Field"":99,""Property"":-1,""IgnoredField"":-1,""IgnoredObject"":[1,2,3,4,5]}");

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
      object o = JavaScriptConvert.DeserializeObject(json);
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

      JObject o = (JObject)JavaScriptConvert.DeserializeObject(jsonText);
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

      string jsonText = JavaScriptConvert.SerializeObject(test);

      Assert.AreEqual(@"{""pie"":""Delicious"",""pie1"":""PieChart!"",""sweet_cakes_count"":2147483647}", jsonText);

      JsonPropertyClass test2 = JavaScriptConvert.DeserializeObject<JsonPropertyClass>(jsonText);

      Assert.AreEqual(test.Pie, test2.Pie);
      Assert.AreEqual(test.SweetCakesCount, test2.SweetCakesCount);
    }

    public class BadJsonPropertyClass
    {
      [JsonProperty("pie")]
      public string Pie = "Yum";

      public string pie = "PieChart!";
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"A member with the name 'pie' already exists on Newtonsoft.Json.Tests.JsonSerializerTest+BadJsonPropertyClass. Use the JsonPropertyAttribute to specify another name.")]
    public void BadJsonPropertyClassSerialize()
    {
      JavaScriptConvert.SerializeObject(new BadJsonPropertyClass());
    }

    public class Article
    {
      public string Name;

      public Article()
      {
      }

      public Article(string name)
      {
        Name = name;
      }
    }

    public class ArticleCollection : List<Article>
    {
    }

    [Test]
    public void InheritedListSerialize()
    {
      Article a1 = new Article("a1");
      Article a2 = new Article("a2");

      ArticleCollection articles1 = new ArticleCollection();
      articles1.Add(a1);
      articles1.Add(a2);

      string jsonText = JavaScriptConvert.SerializeObject(articles1);

      ArticleCollection articles2 = JavaScriptConvert.DeserializeObject<ArticleCollection>(jsonText);

      Assert.AreEqual(articles1.Count, articles2.Count);
      Assert.AreEqual(articles1[0].Name, articles2[0].Name);
    }

    [Test]
    public void ReadOnlyCollectionSerialize()
    {
      ReadOnlyCollection<int> r1 = new ReadOnlyCollection<int>(new int[] { 0, 1, 2, 3, 4 });

      string jsonText = JavaScriptConvert.SerializeObject(r1);

      ReadOnlyCollection<int> r2 = JavaScriptConvert.DeserializeObject<ReadOnlyCollection<int>>(jsonText);

      CollectionAssert.AreEqual(r1, r2);
    }

    public class Person
    {
      private Guid _internalId;
      private string _firstName;

      [JsonIgnore]
      public Guid InternalId
      {
        get { return _internalId; }
        set { _internalId = value; }
      }

      [JsonProperty("first_name")]
      public string FirstName
      {
        get { return _firstName; }
        set { _firstName = value; }
      }
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = @"Could not find member 'Price' on object of type 'RuntimeType'")]
    public void MissingMemberDeserialize()
    {
      Product product = new Product();

      product.Name = "Apple";
      product.Expiry = new DateTime(2008, 12, 28);
      product.Price = 3.99M;
      product.Sizes = new string[] { "Small", "Medium", "Large" };

      string output = JavaScriptConvert.SerializeObject(product);
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

      ProductShort deserializedProductShort = (ProductShort)JavaScriptConvert.DeserializeObject(output, typeof(ProductShort));
    }

    [Test]
    public void MissingMemberDeserializeOkay()
    {
      Product product = new Product();

      product.Name = "Apple";
      product.Expiry = new DateTime(2008, 12, 28);
      product.Price = 3.99M;
      product.Sizes = new string[] { "Small", "Medium", "Large" };

      string output = JavaScriptConvert.SerializeObject(product);
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

    [Test]
    public void Unicode()
    {
      string json = @"[""PRE\u003cPOST""]";

      DataContractJsonSerializer s = new DataContractJsonSerializer(typeof(List<string>));
      List<string> dataContractResult = (List<string>)s.ReadObject(new MemoryStream(Encoding.UTF8.GetBytes(json)));

      List<string> jsonNetResult = JavaScriptConvert.DeserializeObject<List<string>>(json);

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

      result = JavaScriptConvert.SerializeObject(testDates);
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

      string result = JavaScriptConvert.SerializeObject(testDates);
      Assert.AreEqual(@"[""\/Date(-59011455539000+0000)\/"",""\/Date(946688461000+0000)\/"",""\/Date(946641661000+1300)\/"",""\/Date(946701061000-0330)\/""]", result);
    }

    [Test]
    public void NonStringKeyDictionary()
    {
      Dictionary<int, int> values = new Dictionary<int, int>();
      values.Add(-5, 6);
      values.Add(int.MinValue, int.MaxValue);

      string json = JavaScriptConvert.SerializeObject(values);

      Assert.AreEqual(@"{""-5"":6,""-2147483648"":2147483647}", json);

      Dictionary<int, int> newValues = JavaScriptConvert.DeserializeObject<Dictionary<int, int>>(json);

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

      string json = JavaScriptConvert.SerializeObject(anonymous);
      Assert.AreEqual(@"{""StringValue"":""I am a string"",""IntValue"":2147483647,""NestedAnonymous"":{""NestedValue"":255},""NestedArray"":[1,2],""Product"":{""Name"":""TestProduct"",""Expiry"":""\/Date(946684800000)\/"",""Price"":0,""Sizes"":null}}", json);

      anonymous = JavaScriptConvert.DeserializeAnonymousType(json, anonymous);
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

      //JavaScriptConvert.ConvertDateTimeToJavaScriptTicks(s1.Establised.DateTime)

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
      string json = JavaScriptConvert.SerializeObject(new object());
      Assert.AreEqual("{}", json);
    }

    [Test]
    public void SerializeNull()
    {
      string json = JavaScriptConvert.SerializeObject(null);
      Assert.AreEqual("null", json);
    }

    [Test]
    public void CanDeserializeIntArrayWhenNotFirstPropertyInJson()
    {
      string json = "{foo:'hello',bar:[1,2,3]}";
      ClassWithArray wibble = JavaScriptConvert.DeserializeObject<ClassWithArray>(json);
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
      ClassWithArray wibble = JavaScriptConvert.DeserializeObject<ClassWithArray>(json);
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
      string json = JavaScriptConvert.SerializeObject(wibble);

      ClassWithArray wibbleOut = JavaScriptConvert.DeserializeObject<ClassWithArray>(json);
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
      string json = JavaScriptConvert.SerializeObject(new ConverableMembers());

      Assert.AreEqual(@"{""String"":""string"",""Int32"":2147483647,""UInt32"":4294967295,""Byte"":255,""SByte"":127,""Short"":32767,""UShort"":65535,""Long"":9223372036854775807,""ULong"":9223372036854775807,""Double"":1.7976931348623157E+308,""Float"":3.40282347E+38,""DBNull"":null,""Bool"":true,""Char"":""\u0000""}", json);

      ConverableMembers c = JavaScriptConvert.DeserializeObject<ConverableMembers>(json);
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

      string json = JavaScriptConvert.SerializeObject(s);
      Assert.AreEqual("[3,2,1]", json);
    }

    [Test]
    public void GuidTest()
    {
      Guid guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");

      string json = JavaScriptConvert.SerializeObject(new ClassWithGuid { GuidField = guid });
      Assert.AreEqual(@"{""GuidField"":""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""}", json);

      ClassWithGuid c = JavaScriptConvert.DeserializeObject<ClassWithGuid>(json);
      Assert.AreEqual(guid, c.GuidField);
    }

    [Test]
    public void EnumTest()
    {
      string json = JavaScriptConvert.SerializeObject(StringComparison.CurrentCultureIgnoreCase);
      Assert.AreEqual(@"1", json);

      StringComparison s = JavaScriptConvert.DeserializeObject<StringComparison>(json);
      Assert.AreEqual(StringComparison.CurrentCultureIgnoreCase, s);
    }

    [Test]
    public void JsonIgnoreAttributeOnClassTest()
    {
      string json = JavaScriptConvert.SerializeObject(new JsonIgnoreAttributeOnClassTestClass());

      Assert.AreEqual(@"{""TheField"":0,""Property"":21}", json);

      JsonIgnoreAttributeOnClassTestClass c = JavaScriptConvert.DeserializeObject<JsonIgnoreAttributeOnClassTestClass>(@"{""TheField"":99,""Property"":-1,""IgnoredField"":-1}");

      Assert.AreEqual(0, c.IgnoredField);
      Assert.AreEqual(99, c.Field);
    }

#if !SILVERLIGHT
    [Test]
    public void SerializeArrayAsArrayList()
    {
      string jsonText = @"[3, ""somestring"",[1,2,3]]";
      ArrayList o = JavaScriptConvert.DeserializeObject<ArrayList>(jsonText);

      Assert.AreEqual(3, o.Count);
      Assert.AreEqual(3, ((JArray)o[2]).Count);
    }
#endif

    public class Name
    {
      public string personsName;

      public List<PhoneNumber> pNumbers = new List<PhoneNumber>();

      public Name(string personsName)
      {
        this.personsName = personsName;
      }
    }

    public class PhoneNumber
    {
      public string phoneNumber;

      public PhoneNumber(string phoneNumber)
      {
        this.phoneNumber = phoneNumber;
      }
    }
    
    [Test]
    public void SerializeMemberGenericList()
    {
      Name name = new Name("The Idiot in Next To Me");

      PhoneNumber p1 = new PhoneNumber("555-1212");
      PhoneNumber p2 = new PhoneNumber("444-1212");

      name.pNumbers.Add(p1);
      name.pNumbers.Add(p2);

      string json = JavaScriptConvert.SerializeObject(name);

      Name newName = JavaScriptConvert.DeserializeObject<Name>(json);

      Assert.AreEqual("The Idiot in Next To Me", newName.personsName);

      // not passed in as part of the constructor, values not deserialized
      Assert.AreEqual(0, newName.pNumbers.Count);
    }

    public class ConstructorCaseSensitivityClass
    {
      public string param1 { get; set; }
      public string Param1 { get; set; }
      public string Param2 { get; set; }

      public ConstructorCaseSensitivityClass(string param1, string Param1, string param2)
      {
        this.param1 = param1;
        this.Param1 = Param1;
        this.Param2 = param2;
      }
    }

    [Test]
    public void ConstructorCaseSensitivity()
    {
      ConstructorCaseSensitivityClass c = new ConstructorCaseSensitivityClass("param1", "Param1", "Param2");

      string json = JavaScriptConvert.SerializeObject(c);

      ConstructorCaseSensitivityClass deserialized = JavaScriptConvert.DeserializeObject<ConstructorCaseSensitivityClass>(json);

      Assert.AreEqual("param1", deserialized.param1);
      Assert.AreEqual("Param1", deserialized.Param1);
      Assert.AreEqual("Param2", deserialized.Param2);
    }

    public class MemberConverterPrecedenceClassConverter : ConverterPrecedenceClassConverter
    {
      public override string ConverterType
      {
        get { return "Member"; }
      }
    }

    public class ClassConverterPrecedenceClassConverter : ConverterPrecedenceClassConverter
    {
      public override string ConverterType
      {
        get { return "Class"; }
      }
    }

    public class ArgumentConverterPrecedenceClassConverter : ConverterPrecedenceClassConverter
    {
      public override string ConverterType
      {
        get { return "Argument"; }
      }
    }

    public abstract class ConverterPrecedenceClassConverter : JsonConverter
    {
      public abstract string ConverterType { get; }

      public override void WriteJson(JsonWriter writer, object value)
      {
        ConverterPrecedenceClass c = (ConverterPrecedenceClass)value;

        JToken j = new JArray(ConverterType, c.TestValue);

        j.WriteTo(writer);
      }

      public override object ReadJson(JsonReader reader, Type objectType)
      {
        JToken j = JArray.Load(reader);

        string converter = (string)j[0];
        if (converter != ConverterType)
          throw new Exception("Serialize converter {0} and deserialize converter {1} do not match.".FormatWith(CultureInfo.InvariantCulture, converter, ConverterType));

        string testValue = (string)j[1];
        return new ConverterPrecedenceClass(testValue);
      }

      public override bool CanConvert(Type objectType)
      {
        return (objectType == typeof(ConverterPrecedenceClass));
      }
    }

    [JsonConverter(typeof(ClassConverterPrecedenceClassConverter))]
    public class ConverterPrecedenceClass
    {
      public string TestValue { get; set; }

      public ConverterPrecedenceClass(string testValue)
      {
        TestValue = testValue;
      }
    }

    [Test]
    public void SerializerShouldUseClassConverter()
    {
      ConverterPrecedenceClass c1 = new ConverterPrecedenceClass("!Test!");

      string json = JavaScriptConvert.SerializeObject(c1);
      Assert.AreEqual(@"[""Class"",""!Test!""]", json);

      ConverterPrecedenceClass c2 = JavaScriptConvert.DeserializeObject<ConverterPrecedenceClass>(json);

      Assert.AreEqual("!Test!", c2.TestValue);
    }

    [Test]
    public void SerializerShouldUseClassConverterOverArgumentConverter()
    {
      ConverterPrecedenceClass c1 = new ConverterPrecedenceClass("!Test!");

      string json = JavaScriptConvert.SerializeObject(c1, new ArgumentConverterPrecedenceClassConverter());
      Assert.AreEqual(@"[""Class"",""!Test!""]", json);

      ConverterPrecedenceClass c2 = JavaScriptConvert.DeserializeObject<ConverterPrecedenceClass>(json, new ArgumentConverterPrecedenceClassConverter());

      Assert.AreEqual("!Test!", c2.TestValue);
    }

    public class MemberConverterClass
    {
      public DateTime DefaultConverter { get; set; }
      [JsonConverter(typeof(IsoDateTimeConverter))]
      public DateTime MemberConverter { get; set; }
    }

    [Test]
    public void SerializerShouldUseMemberConverter()
    {
      DateTime testDate = new DateTime(JavaScriptConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

      string json = JavaScriptConvert.SerializeObject(m1);
      Assert.AreEqual(@"{""DefaultConverter"":""\/Date(0)\/"",""MemberConverter"":""1970-01-01T00:00:00.0000000Z""}", json);

      MemberConverterClass m2 = JavaScriptConvert.DeserializeObject<MemberConverterClass>(json);

      Assert.AreEqual(testDate, m2.DefaultConverter);
      Assert.AreEqual(testDate, m2.MemberConverter);
    }

    [Test]
    public void SerializerShouldUseMemberConverterOverArgumentConverter()
    {
      DateTime testDate = new DateTime(JavaScriptConvert.InitialJavaScriptDateTicks, DateTimeKind.Utc);
      MemberConverterClass m1 = new MemberConverterClass { DefaultConverter = testDate, MemberConverter = testDate };

      string json = JavaScriptConvert.SerializeObject(m1, new JavaScriptDateTimeConverter());
      Assert.AreEqual(@"{""DefaultConverter"":new Date(0),""MemberConverter"":""1970-01-01T00:00:00.0000000Z""}", json);

      MemberConverterClass m2 = JavaScriptConvert.DeserializeObject<MemberConverterClass>(json, new JavaScriptDateTimeConverter());

      Assert.AreEqual(testDate, m2.DefaultConverter);
      Assert.AreEqual(testDate, m2.MemberConverter);
    }

    public class ClassAndMemberConverterClass
    {
      public ConverterPrecedenceClass DefaultConverter { get; set; }
      [JsonConverter(typeof(MemberConverterPrecedenceClassConverter))]
      public ConverterPrecedenceClass MemberConverter { get; set; }
    }

    [Test]
    public void SerializerShouldUseMemberConverterOverClassAndArgumentConverter()
    {
      ClassAndMemberConverterClass c1 = new ClassAndMemberConverterClass();
      c1.DefaultConverter = new ConverterPrecedenceClass("DefaultConverterValue");
      c1.MemberConverter = new ConverterPrecedenceClass("MemberConverterValue");

      string json = JavaScriptConvert.SerializeObject(c1, new ArgumentConverterPrecedenceClassConverter());
      Assert.AreEqual(@"{""DefaultConverter"":[""Class"",""DefaultConverterValue""],""MemberConverter"":[""Member"",""MemberConverterValue""]}", json);

      ClassAndMemberConverterClass c2 = JavaScriptConvert.DeserializeObject<ClassAndMemberConverterClass>(json, new ArgumentConverterPrecedenceClassConverter());

      Assert.AreEqual("DefaultConverterValue", c2.DefaultConverter.TestValue);
      Assert.AreEqual("MemberConverterValue", c2.MemberConverter.TestValue);
    }

    [JsonConverter(typeof(IsoDateTimeConverter))]
    public class IncompatibleJsonAttributeClass
    {
    }

    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "JsonConverter IsoDateTimeConverter on Newtonsoft.Json.Tests.JsonSerializerTest+IncompatibleJsonAttributeClass is not compatible with member type IncompatibleJsonAttributeClass.")]
    public void IncompatibleJsonAttributeShouldThrow()
    {
      IncompatibleJsonAttributeClass c = new IncompatibleJsonAttributeClass();
      JavaScriptConvert.SerializeObject(c);
    }
  }
}