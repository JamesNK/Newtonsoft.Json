using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Linq
{
  public class JTokenEqualityComparer : IEqualityComparer<JToken>
  {
    public bool Equals(JToken x, JToken y)
    {
      return JToken.DeepEquals(x, y);
    }

    public int GetHashCode(JToken obj)
    {
      if (obj == null)
        return 0;

      return obj.GetDeepHashCode();
    }
  }
}