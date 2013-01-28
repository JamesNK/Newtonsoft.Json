using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class ParseJsonAny
  {
    public void Example()
    {
      #region Usage
      JToken t1 = JToken.Parse("{}");

      Console.WriteLine(t1.Type);
      // Object

      JToken t2 = JToken.Parse("[]");

      Console.WriteLine(t2.Type);
      // Array

      JToken t3 = JToken.Parse("null");

      Console.WriteLine(t3.Type);
      // Null

      JToken t4 = JToken.Parse(@"'A string!'");

      Console.WriteLine(t4.Type);
      // String
      #endregion
    }
  }
}
