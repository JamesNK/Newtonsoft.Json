using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Json
{
  public class ReadJsonWithJsonTextReader
  {
    public void Example()
    {
      #region Usage
      string json = @"{
         'CPU': 'Intel',
         'PSU': '500W',
         'Drives': [
           'DVD read/writer'
           /*(broken)*/,
           '500 gigabyte hard drive',
           '200 gigabype hard drive'
         ]
      }";

      JsonTextReader reader = new JsonTextReader(new StringReader(json));
      while (reader.Read())
      {
        if (reader.Value != null)
          Console.WriteLine("Token: {0}, Value: {1}", reader.TokenType, reader.Value);
        else
          Console.WriteLine("Token: {0}", reader.TokenType);
      }

      // Token: StartObject
      // Token: PropertyName, Value: CPU
      // Token: String, Value: Intel
      // Token: PropertyName, Value: PSU
      // Token: String, Value: 500W
      // Token: PropertyName, Value: Drives
      // Token: StartArray
      // Token: String, Value: DVD read/writer
      // Token: Comment, Value: (broken)
      // Token: String, Value: 500 gigabyte hard drive
      // Token: String, Value: 200 gigabype hard drive
      // Token: EndArray
      // Token: EndObject
      #endregion
    }
  }
}
