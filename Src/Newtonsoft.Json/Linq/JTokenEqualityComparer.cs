using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Compares tokens to determine whether they are equal.
  /// </summary>
  public class JTokenEqualityComparer : IEqualityComparer<JToken>
  {
    /// <summary>
    /// Determines whether the specified objects are equal.
    /// </summary>
    /// <param name="x">The first object of type <paramref name="T"/> to compare.</param>
    /// <param name="y">The second object of type <paramref name="T"/> to compare.</param>
    /// <returns>
    /// true if the specified objects are equal; otherwise, false.
    /// </returns>
    public bool Equals(JToken x, JToken y)
    {
      return JToken.DeepEquals(x, y);
    }

    /// <summary>
    /// Returns a hash code for the specified object.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> for which a hash code is to be returned.</param>
    /// <returns>A hash code for the specified object.</returns>
    /// <exception cref="T:System.ArgumentNullException">The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is null.</exception>
    public int GetHashCode(JToken obj)
    {
      if (obj == null)
        return 0;

      return obj.GetDeepHashCode();
    }
  }
}