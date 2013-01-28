using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class Clone
  {
    public void Example()
    {
      #region Usage
      JObject o1 = new JObject
        {
          {"String", "A string!"},
          {"Items", new JArray(1, 2)}
        };

      Console.WriteLine(o1.ToString());
      // {
      //   "String": "A string!",
      //   "Items": [
      //     1,
      //     2
      //   ]
      // }

      JObject o2 = (JObject) o1.DeepClone();

      Console.WriteLine(o2.ToString());
      // {
      //   "String": "A string!",
      //   "Items": [
      //     1,
      //     2
      //   ]
      // }

      Console.WriteLine(JToken.DeepEquals(o1, o2));
      // true

      Console.WriteLine(Object.ReferenceEquals(o1, o2));
      // false
      #endregion
    }
  }
}
