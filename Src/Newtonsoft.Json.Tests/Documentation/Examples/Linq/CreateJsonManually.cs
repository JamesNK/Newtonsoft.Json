using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Linq
{
  public class CreateJsonManually
  {
    public void Example()
    {
      #region Usage
      JArray array = new JArray();
      JValue text = new JValue("Manual text");
      JValue date = new JValue(new DateTime(2000, 5, 23));

      array.Add(text);
      array.Add(date);

      JObject o = new JObject();
      o["MyArray"] = array;

      string json = o.ToString();

      Console.WriteLine(json);
      // {
      //   "MyArray": [
      //     "Manual text",
      //     "2000-05-23T00:00:00"
      //   ]
      // }
      #endregion
    }
  }
}
