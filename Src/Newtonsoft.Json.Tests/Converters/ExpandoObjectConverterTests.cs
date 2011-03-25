#if !(NET35 || NET20 || WINDOWS_PHONE)

using System;
using System.Collections.Generic;
#if !SILVERLIGHT && !PocketPC && !NET20
using System.Data.Linq;
#endif
#if !SILVERLIGHT
using System.Data.SqlTypes;
#endif
using System.Dynamic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
  public class ExpandoObjectConverterTests : TestFixtureBase
  {
    public class ExpandoContainer
    {
      public string Before { get; set; }
      public ExpandoObject Expando { get; set; }
      public string After { get; set; }
    }

    [Test]
    public void SerializeExpandoObject()
    {
      ExpandoContainer d = new ExpandoContainer
        {
          Before = "Before!",
          Expando = new ExpandoObject(),
          After = "After!"
        };

      dynamic o = d.Expando;

      o.String = "String!";
      o.Integer = 234;
      o.Float = 1.23d;
      o.List = new List<string> {"First", "Second", "Third"};
      o.Object = new Dictionary<string, object>
        {
          {"First", 1}
        };

      string json = JsonConvert.SerializeObject(d, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Before"": ""Before!"",
  ""Expando"": {
    ""String"": ""String!"",
    ""Integer"": 234,
    ""Float"": 1.23,
    ""List"": [
      ""First"",
      ""Second"",
      ""Third""
    ],
    ""Object"": {
      ""First"": 1
    }
  },
  ""After"": ""After!""
}", json);
    }

    [Test]
    public void SerializeNullExpandoObject()
    {
      ExpandoContainer d = new ExpandoContainer();

      string json = JsonConvert.SerializeObject(d, Formatting.Indented);

      Assert.AreEqual(@"{
  ""Before"": null,
  ""Expando"": null,
  ""After"": null
}", json);
    }

    [Test]
    public void DeserializeExpandoObject()
    {
      string json = @"{
  ""Before"": ""Before!"",
  ""Expando"": {
    ""String"": ""String!"",
    ""Integer"": 234,
    ""Float"": 1.23,
    ""List"": [
      ""First"",
      ""Second"",
      ""Third""
    ],
    ""Object"": {
      ""First"": 1
    }
  },
  ""After"": ""After!""
}";

      ExpandoContainer o = JsonConvert.DeserializeObject<ExpandoContainer>(json);

      Assert.AreEqual(o.Before, "Before!");
      Assert.AreEqual(o.After, "After!");
      Assert.IsNotNull(o.Expando);

      dynamic d = o.Expando;
      Assert.IsInstanceOfType(typeof(ExpandoObject), d);

      Assert.AreEqual("String!", d.String);
      Assert.IsInstanceOfType(typeof(string), d.String);

      Assert.AreEqual(234, d.Integer);
      Assert.IsInstanceOfType(typeof(long), d.Integer);

      Assert.AreEqual(1.23, d.Float);
      Assert.IsInstanceOfType(typeof(double), d.Float);

      Assert.IsNotNull(d.List);
      Assert.AreEqual(3, d.List.Count);
      Assert.IsInstanceOfType(typeof(List<object>), d.List);

      Assert.AreEqual("First", d.List[0]);
      Assert.IsInstanceOfType(typeof(string), d.List[0]);

      Assert.AreEqual("Second", d.List[1]);
      Assert.AreEqual("Third", d.List[2]);

      Assert.IsNotNull(d.Object);
      Assert.IsInstanceOfType(typeof(ExpandoObject), d.Object);

      Assert.AreEqual(1, d.Object.First);
      Assert.IsInstanceOfType(typeof(long), d.Object.First);
    }

    [Test]
    public void DeserializeNullExpandoObject()
    {
      string json = @"{
  ""Before"": null,
  ""Expando"": null,
  ""After"": null
}";

      ExpandoContainer c = JsonConvert.DeserializeObject<ExpandoContainer>(json);

      Assert.AreEqual(null, c.Expando);
    }

  }
}

#endif