using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class CreateJsonJTokenWriter
  {
    public void Example()
    {
      #region Usage
      JTokenWriter writer = new JTokenWriter();
      writer.WriteStartObject();
      writer.WritePropertyName("name1");
      writer.WriteValue("value1");
      writer.WritePropertyName("name2");
      writer.WriteStartArray();
      writer.WriteValue(1);
      writer.WriteValue(2);
      writer.WriteEndArray();
      writer.WriteEndObject();

      JObject o = (JObject)writer.Token;

      Console.WriteLine(o.ToString());
      // {
      //   "name1": "value1",
      //   "name2": [
      //     1,
      //     2
      //   ]
      // }
      #endregion
    }
  }
}