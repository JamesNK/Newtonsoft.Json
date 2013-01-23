using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Linq
{
  public class ToObjectComplex
  {
    public class Person
    {
      public string Name { get; set; }
    }

    public void Example()
    {
      string json = @"{
        ""d"": [
          {
            ""Name"": ""John Smith""
          },
          {
            ""Name"": ""Mike Smith""
          }
        ]
      }";

      JObject o = JObject.Parse(json);

      JArray a = (JArray)o["d"];

      IList<Person> person = a.ToObject<IList<Person>>();

      Console.WriteLine(person[0].Name);
      // John Smith

      Console.WriteLine(person[1].Name);
      // Mike Smith
    }
  }
}