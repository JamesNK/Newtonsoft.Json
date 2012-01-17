using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Collections;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a collection of <see cref="JToken"/> objects.
  /// </summary>
  /// <typeparam name="T">The type of token</typeparam>
  public struct JEnumerable<T> : IJEnumerable<T> where T : JToken
  {
    /// <summary>
    /// An empty collection of <see cref="JToken"/> objects.
    /// </summary>
    public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

    private readonly IEnumerable<T> _enumerable;

    /// <summary>
    /// Initializes a new instance of the <see cref="JEnumerable{T}"/> struct.
    /// </summary>
    /// <param name="enumerable">The enumerable.</param>
    public JEnumerable(IEnumerable<T> enumerable)
    {
      ValidationUtils.ArgumentNotNull(enumerable, "enumerable");

      _enumerable = enumerable;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<T> GetEnumerator()
    {
      return _enumerable.GetEnumerator();
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    /// <summary>
    /// Gets the <see cref="IJEnumerable{JToken}"/> with the specified key.
    /// </summary>
    /// <value></value>
    public IJEnumerable<JToken> this[object key]
    {
      get { return new JEnumerable<JToken>(Extensions.Values<T, JToken>(_enumerable, key)); }
    }

    /// <summary>
    /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
    /// </summary>
    /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
    /// <returns>
    /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
    /// </returns>
    public override bool Equals(object obj)
    {
      if (obj is JEnumerable<T>)
        return _enumerable.Equals(((JEnumerable<T>)obj)._enumerable);

      return false;
    }

    /// <summary>
    /// Returns a hash code for this instance.
    /// </summary>
    /// <returns>
    /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
    /// </returns>
    public override int GetHashCode()
    {
      return _enumerable.GetHashCode();
    }
  }
}
