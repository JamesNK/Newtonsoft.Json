using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class SerializeRawJson
  {
    public class JavaScriptSettings
    {
      public JRaw OnLoadFunction { get; set; }
      public JRaw OnUnloadFunction { get; set; }
    }

    public void Example()
    {
      JavaScriptSettings settings = new JavaScriptSettings
        {
          OnLoadFunction = new JRaw("OnLoad"),
          OnUnloadFunction = new JRaw("function(e) { alert(e); }")
        };

      string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "OnLoadFunction": OnLoad,
      //   "OnUnloadFunction": function(e) { alert(e); }
      // }
    }
  }
}