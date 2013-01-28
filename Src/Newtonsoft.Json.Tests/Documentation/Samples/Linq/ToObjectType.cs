using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class ToObjectType
  {
    public void Example()
    {
      #region Usage
      JValue v1 = new JValue(true);

      bool b = (bool)v1.ToObject(typeof(bool));

      Console.WriteLine(b);
      // true

      int i = (int)v1.ToObject(typeof(int));

      Console.WriteLine(i);
      // 1

      string s = (string)v1.ToObject(typeof(string));

      Console.WriteLine(s);
      // "True"
      #endregion
    }
  }
}