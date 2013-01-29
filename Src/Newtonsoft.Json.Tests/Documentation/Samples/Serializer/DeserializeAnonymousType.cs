using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class DeserializeAnonymousType
  {
    public void Example()
    {
      #region Usage
      var definition = new {Name = ""};

      string json1 = @"{'Name':'James'}";
      var customer1 = JsonConvert.DeserializeAnonymousType(json1, definition);

      Console.WriteLine(customer1.Name);
      // James

      string json2 = @"{'Name':'Mike'}";
      var customer2 = JsonConvert.DeserializeAnonymousType(json2, definition);

      Console.WriteLine(customer2.Name);
      // Mike
      #endregion
    }
  }
}
