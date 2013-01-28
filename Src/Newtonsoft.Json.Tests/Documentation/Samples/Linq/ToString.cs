using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class ToString
  {
    public void Example()
    {
      #region Usage
      JObject o = JObject.Parse(@"{'string1':'value','integer2':99,'datetime3':'2000-05-23T00:00:00'}");

      Console.WriteLine(o.ToString());
      // {
      //   "string1": "value",
      //   "integer2": 99,
      //   "datetime3": "2000-05-23T00:00:00"
      // }

      Console.WriteLine(o.ToString(Formatting.None));
      // {"string1":"value","integer2":99,"datetime3":"2000-05-23T00:00:00"}

      Console.WriteLine(o.ToString(Formatting.None, new JavaScriptDateTimeConverter()));
      // {"string1":"value","integer2":99,"datetime3":new Date(959032800000)}
      #endregion
    }
  }
}
