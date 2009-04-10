using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.IO;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JPropertyTests : TestFixtureBase
  {
    [Test]
    public void Load()
    {
      JsonReader reader = new JsonTextReader(new StringReader("{'propertyname':['value1']}"));
      reader.Read();

      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
      reader.Read();

      JProperty property = JProperty.Load(reader);
      Assert.AreEqual("propertyname", property.Name);
      Assert.IsTrue(JToken.DeepEquals(JArray.Parse("['value1']"), property.Value));

      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

      reader = new JsonTextReader(new StringReader("{'propertyname':null}"));
      reader.Read();

      Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
      reader.Read();

      property = JProperty.Load(reader);
      Assert.AreEqual("propertyname", property.Name);
      Assert.IsTrue(JToken.DeepEquals(new JValue(null, JTokenType.Null), property.Value));

      Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
    }

    [Test]
    public void MultiContentConstructor()
    {
      JProperty p = new JProperty("error", new List<string> { "one", "two" });
      JArray a = (JArray) p.Value;

      Assert.AreEqual(a.Count, 2);
      Assert.AreEqual("one", (string)a[0]);
      Assert.AreEqual("two", (string)a[1]);
    }
  }
}
