using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class JValueValue
  {
    public void Example()
    {
      #region Usage
      JValue s = new JValue("A string value");

      Console.WriteLine(s.Value.GetType().Name);
      // String
      Console.WriteLine(s.Value);
      // A string value

      JValue u = new JValue(new Uri("http://www.google.com/"));

      Console.WriteLine(u.Value.GetType().Name);
      // Uri
      Console.WriteLine(u.Value);
      // http://www.google.com/
      #endregion
    }
  }
}