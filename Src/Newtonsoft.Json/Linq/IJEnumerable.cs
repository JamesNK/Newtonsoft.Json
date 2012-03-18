using System.Collections.Generic;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a collection of <see cref="JToken"/> objects.
  /// </summary>
  /// <typeparam name="T">The type of token</typeparam>
  public interface IJEnumerable<
#if !(NET20 || NET35 || SILVERLIGHT)
    out
#endif
    T> : IEnumerable<T> where T : JToken
  {
    /// <summary>
    /// Gets the <see cref="IJEnumerable{JToken}"/> with the specified key.
    /// </summary>
    /// <value></value>
    IJEnumerable<JToken> this[object key] { get; }
  }
}