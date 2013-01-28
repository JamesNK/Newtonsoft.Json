using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Json
{
  public class WriteJsonWithJsonTextWriter
  {
    public void Example()
    {
      #region Usage
      StringBuilder sb = new StringBuilder();
      StringWriter sw = new StringWriter(sb);

      using (JsonWriter writer = new JsonTextWriter(sw))
      {
        writer.Formatting = Formatting.Indented;

        writer.WriteStartObject();
        writer.WritePropertyName("CPU");
        writer.WriteValue("Intel");
        writer.WritePropertyName("PSU");
        writer.WriteValue("500W");
        writer.WritePropertyName("Drives");
        writer.WriteStartArray();
        writer.WriteValue("DVD read/writer");
        writer.WriteComment("(broken)");
        writer.WriteValue("500 gigabyte hard drive");
        writer.WriteValue("200 gigabype hard drive");
        writer.WriteEnd();
        writer.WriteEndObject();
      }

      Console.WriteLine(sb.ToString());
      // {
      //   "CPU": "Intel",
      //   "PSU": "500W",
      //   "Drives": [
      //     "DVD read/writer"
      //     /*(broken)*/,
      //     "500 gigabyte hard drive",
      //     "200 gigabype hard drive"
      //   ]
      // }
      #endregion
    }
  }
}
