using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Collections;

namespace Newtonsoft.Json.Linq
{
  public struct JEnumerable<T> : IEnumerable<T> where T : JToken
  {
    public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

    private IEnumerable<T> _enumerable;

    public JEnumerable(IEnumerable<T> enumerable)
    {
      ValidationUtils.ArgumentNotNull(enumerable, "enumerable");

      _enumerable = enumerable;
    }

    public IEnumerator<T> GetEnumerator()
    {
      return _enumerable.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    public IEnumerable<JToken> this[object key]
    {
      get { return Extensions.Values<T, JToken>(_enumerable, key); }
    }
  }
}
