using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal static class CachedAttributeGetter<T> where T : Attribute
  {
    private static readonly Dictionary<ICustomAttributeProvider, T> TypeAttributeCache = new Dictionary<ICustomAttributeProvider, T>();

    public static T GetAttribute(ICustomAttributeProvider type)
    {
      T attribute;

      if (TypeAttributeCache.TryGetValue(type, out attribute))
        return attribute;

      // double check locking to avoid threading issues
      lock (TypeAttributeCache)
      {
        if (TypeAttributeCache.TryGetValue(type, out attribute))
          return attribute;

        attribute = JsonTypeReflector.GetAttribute<T>(type);
        TypeAttributeCache[type] = attribute;

        return attribute;
      }
    }
  }
}
