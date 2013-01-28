using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class DeepEquals
  {
    public void Example()
    {
      #region Usage
      JValue s1 = new JValue("A string");
      JValue s2 = new JValue("A string");
      JValue s3 = new JValue("A STRING");

      Console.WriteLine(JToken.DeepEquals(s1, s2));
      // true

      Console.WriteLine(JToken.DeepEquals(s2, s3));
      // false

      JObject o1 = new JObject
        {
          {"Integer", 12345},
          {"String", "A string"},
          {"Items", new JArray(1, 2)}
        };

      JObject o2 = new JObject
        {
          {"Integer", 12345},
          {"String", "A string"},
          {"Items", new JArray(1, 2)}
        };

      Console.WriteLine(JToken.DeepEquals(o1, o2));
      // true

      Console.WriteLine(JToken.DeepEquals(s1, o1["String"]));
      // true
      #endregion
    }
  }
}
