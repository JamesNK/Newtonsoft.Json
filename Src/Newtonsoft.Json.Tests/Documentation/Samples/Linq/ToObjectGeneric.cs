using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Linq
{
  public class ToObjectGeneric
  {
    public void Example()
    {
      #region Usage
      JValue v1 = new JValue(true);

      bool b = v1.ToObject<bool>();

      Console.WriteLine(b);
      // true

      int i = v1.ToObject<int>();

      Console.WriteLine(i);
      // 1

      string s = v1.ToObject<string>();

      Console.WriteLine(s);
      // "True"
      #endregion
    }
  }
}