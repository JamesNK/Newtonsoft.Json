using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using System.IO;

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
    [ExpectedException(typeof(Exception), ExpectedMessage = "Can not add property PropertyNameValue to Newtonsoft.Json.Linq.JObject. Property with the same name already exists on object.")]
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
        RawContent = new JsonRaw("[1,2,3,4,5]"),
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
        RawContent = new JsonRaw("[1,2,3,4,5]"),
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
        RawContent = new JsonRaw("[1,2,3,4,5]"),
        LastName = "LastNameValue"
      };

      JObject o = JObject.FromObject(raw);

      JsonReader reader = new JTokenReader(o);
      JsonSerializer serializer = new JsonSerializer();
      raw = (PersonRaw)serializer.Deserialize(reader, typeof(PersonRaw));

      Assert.AreEqual("FirstNameValue", raw.FirstName);
      Assert.AreEqual("LastNameValue", raw.LastName);
      Assert.AreEqual("[1,2,3,4,5]", raw.RawContent.Content);
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
      o["val1"] = JValue.CreateRaw("1");
      o["val2"] = JValue.CreateRaw("1");

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
    public void sdfs()
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

      Assert.AreEqual("http://www.foo.com/", shortie.Original);
      Assert.AreEqual("krehqk", shortie.Short);
      Assert.AreEqual(null, shortie.Shortened);
      Assert.AreEqual(0, shortie.Error.Code);
      Assert.AreEqual("No action taken", shortie.Error.ErrorMessage);
    }
  }
}
