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
      string json = @"{'Name':'James'}";

      var customerDefinition = new {Name = ""};

      var customer = JsonConvert.DeserializeAnonymousType(json, customerDefinition);

      Console.WriteLine(customer.Name);
      // James
      #endregion
    }
  }
}
