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

namespace Newtonsoft.Json.Utilities
{
  internal class BidirectionalDictionary<TFirst, TSecond>
  {
    private readonly IDictionary<TFirst, TSecond> _firstToSecond;
    private readonly IDictionary<TSecond, TFirst> _secondToFirst;

    public BidirectionalDictionary()
      : this(EqualityComparer<TFirst>.Default, EqualityComparer<TSecond>.Default)
    {
    }

    public BidirectionalDictionary(IEqualityComparer<TFirst> firstEqualityComparer, IEqualityComparer<TSecond> secondEqualityComparer)
    {
      _firstToSecond = new Dictionary<TFirst, TSecond>(firstEqualityComparer);
      _secondToFirst = new Dictionary<TSecond, TFirst>(secondEqualityComparer);
    }

    public void Add(TFirst first, TSecond second)
    {
      if (_firstToSecond.ContainsKey(first) || _secondToFirst.ContainsKey(second))
      {
        throw new ArgumentException("Duplicate first or second");
      }
      _firstToSecond.Add(first, second);
      _secondToFirst.Add(second, first);
    }

    public bool TryGetByFirst(TFirst first, out TSecond second)
    {
      return _firstToSecond.TryGetValue(first, out second);
    }

    public bool TryGetBySecond(TSecond second, out TFirst first)
    {
      return _secondToFirst.TryGetValue(second, out first);
    }
  }
}