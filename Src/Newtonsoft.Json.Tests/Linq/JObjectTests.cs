using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
#if !PocketPC && !SILVERLIGHT
using System.Web.UI;
#endif

namespace Newtonsoft.Json.Tests.Linq
{
  public class JObjectTests : TestFixtureBase
  {
    [Test]
    public void TryGetValue()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      JToken t;
      Assert.AreEqual(false, o.TryGetValue("sdf", out t));
      Assert.AreEqual(null, t);

      Assert.AreEqual(false, o.TryGetValue(null, out t));
      Assert.AreEqual(null, t);

      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue(1), t));
    }

    [Test]
    public void DictionaryItemShouldSet()
    {
      JObject o = new JObject();
      o["PropertyNameValue"] = new JValue(1);
      Assert.AreEqual(1, o.Children().Count());

      JToken t;
      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue(1), t));

      o["PropertyNameValue"] = new JValue(2);
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue(2), t));

      o["PropertyNameValue"] = null;
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(true, o.TryGetValue("PropertyNameValue", out t));
      Assert.AreEqual(true, JToken.DeepEquals(new JValue((object)null), t));
    }

    [Test]
    public void Remove()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(false, o.Remove("sdf"));
      Assert.AreEqual(false, o.Remove(null));
      Assert.AreEqual(true, o.Remove("PropertyNameValue"));

      Assert.AreEqual(0, o.Children().Count());
    }

    [Test]
    public void GenericCollectionRemove()
    {
      JValue v = new JValue(1);
      JObject o = new JObject();
      o.Add("PropertyNameValue", v);
      Assert.AreEqual(1, o.Children().Count());

      Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1))));
      Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2))));
      Assert.AreEqual(false, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1))));
      Assert.AreEqual(true, ((ICollection<KeyValuePair<string, JToken>>)o).Remove(new KeyValuePair<string, JToken>("PropertyNameValue", v)));

      Assert.AreEqual(0, o.Children().Count());
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add property PropertyNameValue to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
    public void DuplicatePropertyNameShouldThrow()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", null);
      o.Add("PropertyNameValue", null);
    }

    [Test]
    public void GenericDictionaryAdd()
    {
      JObject o = new JObject();

      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, (int)o["PropertyNameValue"]);

      o.Add("PropertyNameValue1", null);
      Assert.AreEqual(null, ((JValue)o["PropertyNameValue1"]).Value);

      Assert.AreEqual(2, o.Children().Count());
    }

    [Test]
    public void GenericCollectionAdd()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string,JToken>>)o).Add(new KeyValuePair<string,JToken>("PropertyNameValue", new JValue(1)));

      Assert.AreEqual(1, (int)o["PropertyNameValue"]);
      Assert.AreEqual(1, o.Children().Count());
    }

    [Test]
    public void GenericCollectionClear()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      JProperty p = (JProperty)o.Children().ElementAt(0);

      ((ICollection<KeyValuePair<string, JToken>>)o).Clear();
      Assert.AreEqual(0, o.Children().Count());

      Assert.AreEqual(null, p.Parent);
    }

    [Test]
    public void GenericCollectionContains()
    {
      JValue v = new JValue(1);
      JObject o = new JObject();
      o.Add("PropertyNameValue", v);
      Assert.AreEqual(1, o.Children().Count());

      bool contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(1)));
      Assert.AreEqual(false, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", v));
      Assert.AreEqual(true, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue", new JValue(2)));
      Assert.AreEqual(false, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(new KeyValuePair<string, JToken>("PropertyNameValue1", new JValue(1)));
      Assert.AreEqual(false, contains);

      contains = ((ICollection<KeyValuePair<string, JToken>>)o).Contains(default(KeyValuePair<string, JToken>));
      Assert.AreEqual(false, contains);
    }

    [Test]
    public void GenericDictionaryContains()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      Assert.AreEqual(1, o.Children().Count());

      bool contains = ((IDictionary<string, JToken>)o).ContainsKey("PropertyNameValue");
      Assert.AreEqual(true, contains);
    }

    [Test]
    public void GenericCollectionCopyTo()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      o.Add("PropertyNameValue2", new JValue(2));
      o.Add("PropertyNameValue3", new JValue(3));
      Assert.AreEqual(3, o.Children().Count());

      KeyValuePair<string, JToken>[] a = new KeyValuePair<string,JToken>[5];

      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(a, 1);

      Assert.AreEqual(default(KeyValuePair<string,JToken>), a[0]);
      
      Assert.AreEqual("PropertyNameValue", a[1].Key);
      Assert.AreEqual(1, (int)a[1].Value);

      Assert.AreEqual("PropertyNameValue2", a[2].Key);
      Assert.AreEqual(2, (int)a[2].Value);

      Assert.AreEqual("PropertyNameValue3", a[3].Key);
      Assert.AreEqual(3, (int)a[3].Value);

      Assert.AreEqual(default(KeyValuePair<string, JToken>), a[4]);
    }

    [Test]
    [ExpectedException(typeof(ArgumentNullException), ExpectedMessage = @"Value cannot be null.
Parameter name: array")]
    public void GenericCollectionCopyToNullArrayShouldThrow()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(null, 0);
    }

    [Test]
    [ExpectedException(typeof(ArgumentOutOfRangeException), ExpectedMessage = @"arrayIndex is less than 0.
Parameter name: arrayIndex")]
    public void GenericCollectionCopyToNegativeArrayIndexShouldThrow()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], -1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"arrayIndex is equal to or greater than the length of array.")]
    public void GenericCollectionCopyToArrayIndexEqualGreaterToArrayLengthShouldThrow()
    {
      JObject o = new JObject();
      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[1], 1);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.")]
    public void GenericCollectionCopyToInsufficientArrayCapacity()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue", new JValue(1));
      o.Add("PropertyNameValue2", new JValue(2));
      o.Add("PropertyNameValue3", new JValue(3));

      ((ICollection<KeyValuePair<string, JToken>>)o).CopyTo(new KeyValuePair<string, JToken>[3], 1);
    }

    [Test]
    public void FromObjectRaw()
    {
      PersonRaw raw = new PersonRaw
      {
        FirstName = "FirstNameValue",
        RawContent = new JRaw("[1,2,3,4,5]"),
        LastName = "LastNameValue"
      };

      JObject o = JObject.FromObject(raw);

      Assert.AreEqual("FirstNameValue", (string)o["first_name"]);
      Assert.AreEqual(JTokenType.Raw, ((JValue)o["RawContent"]).Type);
      Assert.AreEqual("[1,2,3,4,5]", (string)o["RawContent"]);
      Assert.AreEqual("LastNameValue", (string)o["last_name"]);
    }

    [Test]
    public void JTokenReader()
    {
      PersonRaw raw = new PersonRaw
      {
        FirstName = "FirstNameValue",
        RawContent = new JRaw("[1,2,3,4,5]"),
        LastName = "LastNameValue"
      };

      JObject o = JObject.FromObject(raw);

      JsonReader reader = new JTokenReader(o);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.Raw, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.String, reader.TokenType);

      Assert.IsTrue(reader.Read());
      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      Assert.IsFalse(reader.Read());
    }

    [Test]
    public void DeserializeFromRaw()
    {
      PersonRaw raw = new PersonRaw
      {
        FirstName = "FirstNameValue",
        RawContent = new JRaw("[1,2,3,4,5]"),
        LastName = "LastNameValue"
      };

      JObject o = JObject.FromObject(raw);

      JsonReader reader = new JTokenReader(o);
      JsonSerializer serializer = new JsonSerializer();
      raw = (PersonRaw)serializer.Deserialize(reader, typeof(PersonRaw));

      Assert.AreEqual("FirstNameValue", raw.FirstName);
      Assert.AreEqual("LastNameValue", raw.LastName);
      Assert.AreEqual("[1,2,3,4,5]", raw.RawContent.Value);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Error reading JObject from JsonReader. Current JsonReader item is not an object: StartArray")]
    public void Parse_ShouldThrowOnUnexpectedToken()
    {
      string json = @"[""prop""]";
      JObject.Parse(json);
    }

    [Test]
    public void ParseJavaScriptDate()
    {
      string json = @"[new Date(1207285200000)]";

      JArray a = (JArray)JsonConvert.DeserializeObject(json, null);
      JValue v = (JValue)a[0];

      Assert.AreEqual(JsonConvert.ConvertJavaScriptTicksToDateTime(1207285200000), (DateTime)v);
    }

    [Test]
    public void GenericValueCast()
    {
      string json = @"{""foo"":true}";
      JObject o = (JObject)JsonConvert.DeserializeObject(json);
      bool? value = o.Value<bool?>("foo");
      Assert.AreEqual(true, value);

      json = @"{""foo"":null}"; 
      o = (JObject)JsonConvert.DeserializeObject(json);
      value = o.Value<bool?>("foo");
      Assert.AreEqual(null, value);
    }

    [Test]
    [ExpectedException(typeof(JsonReaderException), ExpectedMessage = "Invalid property identifier character: ]. Line 3, position 9.")]
    public void Blog()
    {
      JObject person = JObject.Parse(@"{
        ""name"": ""James"",
        ]!#$THIS IS: BAD JSON![{}}}}]
      }");

      // Invalid property identifier character: ]. Line 3, position 9.
    }

    [Test]
    public void RawChildValues()
    {
      JObject o = new JObject();
      o["val1"] = new JRaw("1");
      o["val2"] = new JRaw("1");

      string json = o.ToString();

      Assert.AreEqual(@"{
  ""val1"": 1,
  ""val2"": 1
}", json);
    }

    [Test]
    public void Iterate()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue1", new JValue(1));
      o.Add("PropertyNameValue2", new JValue(2));

      JToken t = o;

      int i = 1;
      foreach (JProperty property in t)
      {
        Assert.AreEqual("PropertyNameValue" + i, property.Name);
        Assert.AreEqual(i, (int)property.Value);

        i++;
      }
    }

    [Test]
    public void KeyValuePairIterate()
    {
      JObject o = new JObject();
      o.Add("PropertyNameValue1", new JValue(1));
      o.Add("PropertyNameValue2", new JValue(2));

      int i = 1;
      foreach (KeyValuePair<string, JToken> pair in o)
      {
        Assert.AreEqual("PropertyNameValue" + i, pair.Key);
        Assert.AreEqual(i, (int)pair.Value);

        i++;
      }
    }

    [Test]
    public void WriteObjectNullStringValue()
    {
      string s = null;
      JValue v = new JValue(s);
      Assert.AreEqual(null, v.Value);
      Assert.AreEqual(JTokenType.String, v.Type);

      JObject o = new JObject();
      o["title"] = v;

      string output = o.ToString();
      
      Assert.AreEqual(@"{
  ""title"": null
}", output);
    }

    [Test]
    public void Example()
    {
      string json = @"{
        ""Name"": ""Apple"",
        ""Expiry"": new Date(1230422400000),
        ""Price"": 3.99,
        ""Sizes"": [
          ""Small"",
          ""Medium"",
          ""Large""
        ]
      }";

      JObject o = JObject.Parse(json);

      string name = (string)o["Name"];
      // Apple

      JArray sizes = (JArray)o["Sizes"];

      string smallest = (string)sizes[0];
      // Small

      Console.WriteLine(name);
      Console.WriteLine(smallest);
    }

    [Test]
    public void DeserializeClassManually()
    {
      string jsonText = @"{
	      ""short"":{
		      ""original"":""http://www.foo.com/"",
		      ""short"":""krehqk"",
		      ""error"":{
			      ""code"":0,
			      ""msg"":""No action taken""}
		  }";

      JObject json = JObject.Parse(jsonText);

      Shortie shortie = new Shortie
                        {
                          Original = (string)json["short"]["original"],
                          Short = (string)json["short"]["short"],
                          Error = new ShortieException
                                  {
                                    Code = (int)json["short"]["error"]["code"],
                                    ErrorMessage = (string)json["short"]["error"]["msg"]
                                  }
                        };

      Console.WriteLine(shortie.Original);
      // http://www.foo.com/

      Console.WriteLine(shortie.Error.ErrorMessage);
      // No action taken

      Assert.AreEqual("http://www.foo.com/", shortie.Original);
      Assert.AreEqual("krehqk", shortie.Short);
      Assert.AreEqual(null, shortie.Shortened);
      Assert.AreEqual(0, shortie.Error.Code);
      Assert.AreEqual("No action taken", shortie.Error.ErrorMessage);
    }

    [Test]
    public void JObjectContainingHtml()
    {
      JObject o = new JObject();
      o["rc"] = new JValue(200);
      o["m"] = new JValue("");
      o["o"] = new JValue(@"<div class='s1'>
    <div class='avatar'>                    
        <a href='asdf'>asdf</a><br />
        <strong>0</strong>
    </div>
    <div class='sl'>
        <p>
            444444444
        </p>
    </div>
    <div class='clear'>
    </div>                        
</div>");

      Assert.AreEqual(@"{
  ""rc"": 200,
  ""m"": """",
  ""o"": ""<div class='s1'>\r\n    <div class='avatar'>                    \r\n        <a href='asdf'>asdf</a><br />\r\n        <strong>0</strong>\r\n    </div>\r\n    <div class='sl'>\r\n        <p>\r\n            444444444\r\n        </p>\r\n    </div>\r\n    <div class='clear'>\r\n    </div>                        \r\n</div>""
}", o.ToString());
    }

    [Test]
    public void ImplicitValueConversions()
    {
      JObject moss = new JObject();
      moss["FirstName"] = new JValue("Maurice");
      moss["LastName"] = new JValue("Moss");
      moss["BirthDate"] = new JValue(new DateTime(1977, 12, 30));
      moss["Department"] = new JValue("IT");
      moss["JobTitle"] = new JValue("Support");

      Console.WriteLine(moss.ToString());
      //{
      //  "FirstName": "Maurice",
      //  "LastName": "Moss",
      //  "BirthDate": "\/Date(252241200000+1300)\/",
      //  "Department": "IT",
      //  "JobTitle": "Support"
      //}


      JObject jen = new JObject();
      jen["FirstName"] = "Jen";
      jen["LastName"] = "Barber";
      jen["BirthDate"] = new DateTime(1978, 3, 15);
      jen["Department"] = "IT";
      jen["JobTitle"] = "Manager";

      Console.WriteLine(jen.ToString());
      //{
      //  "FirstName": "Jen",
      //  "LastName": "Barber",
      //  "BirthDate": "\/Date(258721200000+1300)\/",
      //  "Department": "IT",
      //  "JobTitle": "Manager"
      //}
    }

    [Test]
    public void ReplaceJPropertyWithJPropertyWithSameName()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");

      JObject o = new JObject(p1, p2);
      IList l = o;
      Assert.AreEqual(p1, l[0]);
      Assert.AreEqual(p2, l[1]);

      JProperty p3 = new JProperty("Test1", "III");

      p1.Replace(p3);
      Assert.AreEqual(null, p1.Parent);
      Assert.AreEqual(l, p3.Parent);

      Assert.AreEqual(p3, l[0]);
      Assert.AreEqual(p2, l[1]);

      Assert.AreEqual(2, l.Count);
      Assert.AreEqual(2, o.Properties().Count());

      JProperty p4 = new JProperty("Test4", "IV");

      p2.Replace(p4);
      Assert.AreEqual(null, p2.Parent);
      Assert.AreEqual(l, p4.Parent);

      Assert.AreEqual(p3, l[0]);
      Assert.AreEqual(p4, l[1]);
    }

#if !PocketPC && !SILVERLIGHT && !NET20
    [Test]
    public void PropertyChanging()
    {
      object changing = null;
      object changed = null;
      int changingCount = 0;
      int changedCount = 0;

      JObject o = new JObject();
      o.PropertyChanging += (sender, args) =>
        {
          JObject s = (JObject) sender;
          changing = (s[args.PropertyName] != null) ? ((JValue)s[args.PropertyName]).Value : null;
          changingCount++;
        };
      o.PropertyChanged += (sender, args) =>
      {
        JObject s = (JObject)sender;
        changed = (s[args.PropertyName] != null) ? ((JValue)s[args.PropertyName]).Value : null;
        changedCount++;
      };

      o["StringValue"] = "value1";
      Assert.AreEqual(null, changing);
      Assert.AreEqual("value1", changed);
      Assert.AreEqual("value1", (string)o["StringValue"]);
      Assert.AreEqual(1, changingCount);
      Assert.AreEqual(1, changedCount);

      o["StringValue"] = "value1";
      Assert.AreEqual(1, changingCount);
      Assert.AreEqual(1, changedCount);

      o["StringValue"] = "value2";
      Assert.AreEqual("value1", changing);
      Assert.AreEqual("value2", changed);
      Assert.AreEqual("value2", (string)o["StringValue"]);
      Assert.AreEqual(2, changingCount);
      Assert.AreEqual(2, changedCount);

      o["StringValue"] = null;
      Assert.AreEqual("value2", changing);
      Assert.AreEqual(null, changed);
      Assert.AreEqual(null, (string)o["StringValue"]);
      Assert.AreEqual(3, changingCount);
      Assert.AreEqual(3, changedCount);

      o["NullValue"] = null;
      Assert.AreEqual(null, changing);
      Assert.AreEqual(null, changed);
      Assert.AreEqual(new JValue((object)null), o["NullValue"]);
      Assert.AreEqual(4, changingCount);
      Assert.AreEqual(4, changedCount);

      o["NullValue"] = null;
      Assert.AreEqual(4, changingCount);
      Assert.AreEqual(4, changedCount);
    }
#endif

    [Test]
    public void PropertyChanged()
    {
      object changed = null;
      int changedCount = 0;

      JObject o = new JObject();
      o.PropertyChanged += (sender, args) =>
      {
        JObject s = (JObject)sender;
        changed = (s[args.PropertyName] != null) ? ((JValue)s[args.PropertyName]).Value : null;
        changedCount++;
      };

      o["StringValue"] = "value1";
      Assert.AreEqual("value1", changed);
      Assert.AreEqual("value1", (string)o["StringValue"]);
      Assert.AreEqual(1, changedCount);

      o["StringValue"] = "value1";
      Assert.AreEqual(1, changedCount);

      o["StringValue"] = "value2";
      Assert.AreEqual("value2", changed);
      Assert.AreEqual("value2", (string)o["StringValue"]);
      Assert.AreEqual(2, changedCount);

      o["StringValue"] = null;
      Assert.AreEqual(null, changed);
      Assert.AreEqual(null, (string)o["StringValue"]);
      Assert.AreEqual(3, changedCount);

      o["NullValue"] = null;
      Assert.AreEqual(null, changed);
      Assert.AreEqual(new JValue((object)null), o["NullValue"]);
      Assert.AreEqual(4, changedCount);

      o["NullValue"] = null;
      Assert.AreEqual(4, changedCount);
    }

    [Test]
    public void IListContains()
    {
      JProperty p = new JProperty("Test", 1);
      IList l = new JObject(p);

      Assert.IsTrue(l.Contains(p));
      Assert.IsFalse(l.Contains(new JProperty("Test", 1)));
    }

    [Test]
    public void IListIndexOf()
    {
      JProperty p = new JProperty("Test", 1);
      IList l = new JObject(p);

      Assert.AreEqual(0, l.IndexOf(p));
      Assert.AreEqual(-1, l.IndexOf(new JProperty("Test", 1)));
    }

    [Test]
    public void IListClear()
    {
      JProperty p = new JProperty("Test", 1);
      IList l = new JObject(p);

      Assert.AreEqual(1, l.Count);

      l.Clear();

      Assert.AreEqual(0, l.Count);
    }

    [Test]
    public void IListCopyTo()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      object[] a = new object[l.Count];

      l.CopyTo(a, 0);

      Assert.AreEqual(p1, a[0]);
      Assert.AreEqual(p2, a[1]);
    }

    [Test]
    public void IListAdd()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l.Add(p3);

      Assert.AreEqual(3, l.Count);
      Assert.AreEqual(p3, l[2]);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.")]
    public void IListAddBadToken()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      l.Add(new JValue("Bad!"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Argument is not a JToken.")]
    public void IListAddBadValue()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      l.Add("Bad!");
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add property Test2 to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
    public void IListAddPropertyWithExistingName()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test2", "II");

      l.Add(p3);
    }

    [Test]
    public void IListRemove()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      // won't do anything
      l.Remove(p3);
      Assert.AreEqual(2, l.Count);

      l.Remove(p1);
      Assert.AreEqual(1, l.Count);
      Assert.IsFalse(l.Contains(p1));
      Assert.IsTrue(l.Contains(p2));

      l.Remove(p2);
      Assert.AreEqual(0, l.Count);
      Assert.IsFalse(l.Contains(p2));
      Assert.AreEqual(null, p2.Parent);
    }

    [Test]
    public void IListRemoveAt()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      // won't do anything
      l.RemoveAt(0);

      l.Remove(p1);
      Assert.AreEqual(1, l.Count);

      l.Remove(p2);
      Assert.AreEqual(0, l.Count);
    }

    [Test]
    public void IListInsert()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l.Insert(1, p3);
      Assert.AreEqual(l, p3.Parent);

      Assert.AreEqual(p1, l[0]);
      Assert.AreEqual(p3, l[1]);
      Assert.AreEqual(p2, l[2]);
    }

    [Test]
    public void IListIsReadOnly()
    {
      IList l = new JObject();
      Assert.IsFalse(l.IsReadOnly);
    }

    [Test]
    public void IListIsFixedSize()
    {
      IList l = new JObject();
      Assert.IsFalse(l.IsFixedSize);
    }

    [Test]
    public void IListSetItem()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l[0] = p3;

      Assert.AreEqual(p3, l[0]);
      Assert.AreEqual(p2, l[1]);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add property Test3 to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
    public void IListSetItemAlreadyExists()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l[0] = p3;
      l[1] = p3;
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = @"Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.")]
    public void IListSetItemInvalid()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      l[0] = new JValue(true);
    }

    [Test]
    public void IListSyncRoot()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      Assert.IsNotNull(l.SyncRoot);
    }

    [Test]
    public void IListIsSynchronized()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList l = new JObject(p1, p2);

      Assert.IsFalse(l.IsSynchronized);
    }

    [Test]
    public void GenericListJTokenContains()
    {
      JProperty p = new JProperty("Test", 1);
      IList<JToken> l = new JObject(p);

      Assert.IsTrue(l.Contains(p));
      Assert.IsFalse(l.Contains(new JProperty("Test", 1)));
    }

    [Test]
    public void GenericListJTokenIndexOf()
    {
      JProperty p = new JProperty("Test", 1);
      IList<JToken> l = new JObject(p);

      Assert.AreEqual(0, l.IndexOf(p));
      Assert.AreEqual(-1, l.IndexOf(new JProperty("Test", 1)));
    }

    [Test]
    public void GenericListJTokenClear()
    {
      JProperty p = new JProperty("Test", 1);
      IList<JToken> l = new JObject(p);

      Assert.AreEqual(1, l.Count);

      l.Clear();

      Assert.AreEqual(0, l.Count);
    }

    [Test]
    public void GenericListJTokenCopyTo()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JToken[] a = new JToken[l.Count];

      l.CopyTo(a, 0);

      Assert.AreEqual(p1, a[0]);
      Assert.AreEqual(p2, a[1]);
    }

    [Test]
    public void GenericListJTokenAdd()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l.Add(p3);

      Assert.AreEqual(3, l.Count);
      Assert.AreEqual(p3, l[2]);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.")]
    public void GenericListJTokenAddBadToken()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      l.Add(new JValue("Bad!"));
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add Newtonsoft.Json.Linq.JValue to Newtonsoft.Json.Linq.JObject.")]
    public void GenericListJTokenAddBadValue()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      // string is implicitly converted to JValue
      l.Add("Bad!");
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add property Test2 to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
    public void GenericListJTokenAddPropertyWithExistingName()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test2", "II");

      l.Add(p3);
    }

    [Test]
    public void GenericListJTokenRemove()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      // won't do anything
      Assert.IsFalse(l.Remove(p3));
      Assert.AreEqual(2, l.Count);

      Assert.IsTrue(l.Remove(p1));
      Assert.AreEqual(1, l.Count);
      Assert.IsFalse(l.Contains(p1));
      Assert.IsTrue(l.Contains(p2));

      Assert.IsTrue(l.Remove(p2));
      Assert.AreEqual(0, l.Count);
      Assert.IsFalse(l.Contains(p2));
      Assert.AreEqual(null, p2.Parent);
    }

    [Test]
    public void GenericListJTokenRemoveAt()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      // won't do anything
      l.RemoveAt(0);

      l.Remove(p1);
      Assert.AreEqual(1, l.Count);

      l.Remove(p2);
      Assert.AreEqual(0, l.Count);
    }

    [Test]
    public void GenericListJTokenInsert()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l.Insert(1, p3);
      Assert.AreEqual(l, p3.Parent);

      Assert.AreEqual(p1, l[0]);
      Assert.AreEqual(p3, l[1]);
      Assert.AreEqual(p2, l[2]);
    }

    [Test]
    public void GenericListJTokenIsReadOnly()
    {
      IList<JToken> l = new JObject();
      Assert.IsFalse(l.IsReadOnly);
    }

    [Test]
    public void GenericListJTokenSetItem()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l[0] = p3;

      Assert.AreEqual(p3, l[0]);
      Assert.AreEqual(p2, l[1]);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not add property Test3 to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
    public void GenericListJTokenSetItemAlreadyExists()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      IList<JToken> l = new JObject(p1, p2);

      JProperty p3 = new JProperty("Test3", "III");

      l[0] = p3;
      l[1] = p3;
    }

#if !SILVERLIGHT
    [Test]
    public void IBindingListSortDirection()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(ListSortDirection.Ascending, l.SortDirection);
    }

    [Test]
    public void IBindingListSortProperty()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(null, l.SortProperty);
    }

    [Test]
    public void IBindingListSupportsChangeNotification()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(true, l.SupportsChangeNotification);
    }

    [Test]
    public void IBindingListSupportsSearching()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(false, l.SupportsSearching);
    }

    [Test]
    public void IBindingListSupportsSorting()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(false, l.SupportsSorting);
    }

    [Test]
    public void IBindingListAllowEdit()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(true, l.AllowEdit);
    }

    [Test]
    public void IBindingListAllowNew()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(true, l.AllowNew);
    }

    [Test]
    public void IBindingListAllowRemove()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(true, l.AllowRemove);
    }

    [Test]
    public void IBindingListAddIndex()
    {
      IBindingList l = new JObject();
      // do nothing
      l.AddIndex(null);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IBindingListApplySort()
    {
      IBindingList l = new JObject();
      l.ApplySort(null, ListSortDirection.Ascending);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IBindingListRemoveSort()
    {
      IBindingList l = new JObject();
      l.RemoveSort();
    }

    [Test]
    public void IBindingListRemoveIndex()
    {
      IBindingList l = new JObject();
      // do nothing
      l.RemoveIndex(null);
    }

    [Test]
    [ExpectedException(typeof(NotSupportedException))]
    public void IBindingListFind()
    {
      IBindingList l = new JObject();
      l.Find(null, null);
    }

    [Test]
    public void IBindingListIsSorted()
    {
      IBindingList l = new JObject();
      Assert.AreEqual(false, l.IsSorted);
    }

    [Test]
    [ExpectedException(typeof(Exception), ExpectedMessage = "Could not determine new value to add to 'Newtonsoft.Json.Linq.JObject'.")]
    public void IBindingListAddNew()
    {
      IBindingList l = new JObject();
      l.AddNew();
    }

    [Test]
    public void IBindingListAddNewWithEvent()
    {
      JObject o = new JObject();
      o.AddingNew += (s, e) => e.NewObject = new JProperty("Property!");

      IBindingList l = o;
      object newObject = l.AddNew();
      Assert.IsNotNull(newObject);

      JProperty p = (JProperty) newObject;
      Assert.AreEqual("Property!", p.Name);
      Assert.AreEqual(o, p.Parent);
    }

    [Test]
    public void ITypedListGetListName()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      ITypedList l = new JObject(p1, p2);

      Assert.AreEqual(string.Empty, l.GetListName(null));
    }

    [Test]
    public void ITypedListGetItemProperties()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      ITypedList l = new JObject(p1, p2);

      PropertyDescriptorCollection propertyDescriptors = l.GetItemProperties(null);
      Assert.IsNull(propertyDescriptors);
    }

    [Test]
    public void ListChanged()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      JObject o = new JObject(p1, p2);

      ListChangedType? changedType = null;
      int? index = null;
      
      o.ListChanged += (s, a) =>
        {
          changedType = a.ListChangedType;
          index = a.NewIndex;
        };

      JProperty p3 = new JProperty("Test3", "III");

      o.Add(p3);
      Assert.AreEqual(changedType, ListChangedType.ItemAdded);
      Assert.AreEqual(index, 2);
      Assert.AreEqual(p3, ((IList<JToken>)o)[index.Value]);

      JProperty p4 = new JProperty("Test4", "IV");

      ((IList<JToken>) o)[index.Value] = p4;
      Assert.AreEqual(changedType, ListChangedType.ItemChanged);
      Assert.AreEqual(index, 2);
      Assert.AreEqual(p4, ((IList<JToken>)o)[index.Value]);
      Assert.IsFalse(((IList<JToken>)o).Contains(p3));
      Assert.IsTrue(((IList<JToken>)o).Contains(p4));

      o["Test1"] = 2;
      Assert.AreEqual(changedType, ListChangedType.ItemChanged);
      Assert.AreEqual(index, 0);
      Assert.AreEqual(2, (int)o["Test1"]);
    }
#else
    [Test]
    public void ListChanged()
    {
      JProperty p1 = new JProperty("Test1", 1);
      JProperty p2 = new JProperty("Test2", "Two");
      JObject o = new JObject(p1, p2);

      NotifyCollectionChangedAction? changedType = null;
      int? index = null;

      o.CollectionChanged += (s, a) =>
      {
        changedType = a.Action;
        index = a.NewStartingIndex;
      };

      JProperty p3 = new JProperty("Test3", "III");

      o.Add(p3);
      Assert.AreEqual(changedType, NotifyCollectionChangedAction.Add);
      Assert.AreEqual(index, 2);
      Assert.AreEqual(p3, ((IList<JToken>)o)[index.Value]);

      JProperty p4 = new JProperty("Test4", "IV");

      ((IList<JToken>)o)[index.Value] = p4;
      Assert.AreEqual(changedType, NotifyCollectionChangedAction.Replace);
      Assert.AreEqual(index, 2);
      Assert.AreEqual(p4, ((IList<JToken>)o)[index.Value]);
      Assert.IsFalse(((IList<JToken>)o).Contains(p3));
      Assert.IsTrue(((IList<JToken>)o).Contains(p4));

      o["Test1"] = 2;
      Assert.AreEqual(changedType, NotifyCollectionChangedAction.Replace);
      Assert.AreEqual(index, 0);
      Assert.AreEqual(2, (int)o["Test1"]);
    }
#endif

    [Test]
    public void GetGeocodeAddress()
    {
      string json = @"{
  ""name"": ""Address: 435 North Mulford Road Rockford, IL 61107"",
  ""Status"": {
    ""code"": 200,
    ""request"": ""geocode""
  },
  ""Placemark"": [ {
    ""id"": ""p1"",
    ""address"": ""435 N Mulford Rd, Rockford, IL 61107, USA"",
    ""AddressDetails"": {
   ""Accuracy"" : 8,
   ""Country"" : {
      ""AdministrativeArea"" : {
         ""AdministrativeAreaName"" : ""IL"",
         ""SubAdministrativeArea"" : {
            ""Locality"" : {
               ""LocalityName"" : ""Rockford"",
               ""PostalCode"" : {
                  ""PostalCodeNumber"" : ""61107""
               },
               ""Thoroughfare"" : {
                  ""ThoroughfareName"" : ""435 N Mulford Rd""
               }
            },
            ""SubAdministrativeAreaName"" : ""Winnebago""
         }
      },
      ""CountryName"" : ""USA"",
      ""CountryNameCode"" : ""US""
   }
},
    ""ExtendedData"": {
      ""LatLonBox"": {
        ""north"": 42.2753076,
        ""south"": 42.2690124,
        ""east"": -88.9964645,
        ""west"": -89.0027597
      }
    },
    ""Point"": {
      ""coordinates"": [ -88.9995886, 42.2721596, 0 ]
    }
  } ]
}";

      JObject o = JObject.Parse(json);

      string searchAddress = (string)o["Placemark"][0]["AddressDetails"]["Country"]["AdministrativeArea"]["SubAdministrativeArea"]["Locality"]["Thoroughfare"]["ThoroughfareName"];
      Assert.AreEqual("435 N Mulford Rd", searchAddress);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Set JObject values with invalid key value: 0. Object property name expected.")]
    public void SetValueWithInvalidPropertyName()
    {
      JObject o = new JObject();
      o[0] = new JValue(3);
    }

    [Test]
    public void SetValue()
    {
      object key = "TestKey";

      JObject o = new JObject();
      o[key] = new JValue(3);

      Assert.AreEqual(3, (int)o[key]);
    }

    [Test]
    public void ParseMultipleProperties()
    {
      string json = @"{
        ""Name"": ""Name1"",
        ""Name"": ""Name2""
      }";

      JObject o = JObject.Parse(json);
      string value = (string)o["Name"];

      Assert.AreEqual("Name2", value);
    }

    [Test]
    public void WriteObjectNullDBNullValue()
    {
      DBNull dbNull = DBNull.Value;
      JValue v = new JValue(dbNull);
      Assert.AreEqual(DBNull.Value, v.Value);
      Assert.AreEqual(JTokenType.Null, v.Type);

      JObject o = new JObject();
      o["title"] = v;

      string output = o.ToString();
      
      Assert.AreEqual(@"{
  ""title"": null
}", output);
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not convert Object to String.")]
    public void InvalidValueCastExceptionMessage()
    {
      string json = @"{
  ""responseData"": {}, 
  ""responseDetails"": null, 
  ""responseStatus"": 200
}";

      JObject o = JObject.Parse(json);

      string name = (string)o["responseData"];
    }

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Can not convert Object to String.")]
    public void InvalidPropertyValueCastExceptionMessage()
    {
      string json = @"{
  ""responseData"": {}, 
  ""responseDetails"": null, 
  ""responseStatus"": 200
}";

      JObject o = JObject.Parse(json);

      string name = (string)o.Property("responseData");
    }
  }
}