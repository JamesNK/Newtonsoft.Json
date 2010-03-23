#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
  internal class DefaultReferenceResolver : IReferenceResolver
  {
    private class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
    {
      bool IEqualityComparer<object>.Equals(object x, object y)
      {
        return ReferenceEquals(x, y);
      }

      int IEqualityComparer<object>.GetHashCode(object obj)
      {
        return -1;
      }
    }

    private int _referenceCount;
    private BidirectionalDictionary<string, object> _mappings;

    private BidirectionalDictionary<string, object> Mappings
    {
      get
      {
        // override equality comparer for object key dictionary
        // object will be modified as it deserializes and might have mutable hashcode
        if (_mappings == null)
          _mappings = new BidirectionalDictionary<string, object>(
            EqualityComparer<string>.Default,
            new ReferenceEqualsEqualityComparer());

        return _mappings;
      }
    }

    public object ResolveReference(string reference)
    {
      object value;
      Mappings.TryGetByFirst(reference, out value);
      return value;
    }

    public string GetReference(object value)
    {
      string reference;
      if (!Mappings.TryGetBySecond(value, out reference))
      {
        _referenceCount++;
        reference = _referenceCount.ToString(CultureInfo.InvariantCulture);
        Mappings.Add(reference, value);
      }

      return reference;
    }

    public void AddReference(string reference, object value)
    {
      Mappings.Add(reference, value);
    }

    public bool IsReferenced(object value)
    {
      string reference;
      return Mappings.TryGetBySecond(value, out reference);
    }
  }
}
