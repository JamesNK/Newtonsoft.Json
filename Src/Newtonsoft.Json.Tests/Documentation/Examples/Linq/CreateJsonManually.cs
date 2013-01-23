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
      JArray array = new JArray();
      JValue text = new JValue("Manual text");
      JValue date = new JValue(new DateTime(2000, 5, 23));

      array.Add(text);
      array.Add(date);

      string json = array.ToString();

      Console.WriteLine(json);
      // [
      //   "Manual text",
      //   "2000-05-23T00:00:00"
      // ]
    }
  }
}
