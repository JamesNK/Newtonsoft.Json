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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json.Utilities;
using System.Linq;
using System.Globalization;

namespace Newtonsoft.Json.Utilities
{
  internal interface IWrappedList : IList
  {
    object UnderlyingList { get; }
  }

  internal class ListWrapper<T> : CollectionWrapper<T>, IList<T>, IWrappedList
  {
    private readonly IList<T> _genericList;

    public ListWrapper(IList list)
      : base(list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      if (list is IList<T>)
        _genericList = (IList<T>) list;
    }

    public ListWrapper(IList<T> list)
      : base(list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      _genericList = list;
    }

    public int IndexOf(T item)
    {
      if (_genericList != null)
        return _genericList.IndexOf(item);
      else
        return ((IList)this).IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      if (_genericList != null)
        _genericList.Insert(index, item);
      else
        ((IList)this).Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      if (_genericList != null)
        _genericList.RemoveAt(index);
      else
        ((IList)this).RemoveAt(index);
    }

    public T this[int index]
    {
      get
      {
        if (_genericList != null)
          return _genericList[index];
        else
          return (T)((IList)this)[index];
      }
      set
      {
        if (_genericList != null)
          _genericList[index] = value;
        else
          ((IList)this)[index] = value;
      }
    }

    public override void Add(T item)
    {
      if (_genericList != null)
        _genericList.Add(item);
      else
        base.Add(item);
    }

    public override void Clear()
    {
      if (_genericList != null)
        _genericList.Clear();
      else
        base.Clear();
    }

    public override bool Contains(T item)
    {
      if (_genericList != null)
        return _genericList.Contains(item);
      else
        return base.Contains(item);
    }

    public override void CopyTo(T[] array, int arrayIndex)
    {
      if (_genericList != null)
        _genericList.CopyTo(array, arrayIndex);
      else
        base.CopyTo(array, arrayIndex);
    }

    public override int Count
    {
      get
      {
        if (_genericList != null)
          return _genericList.Count;
        else
          return base.Count;
      }
    }

    public override bool IsReadOnly
    {
      get
      {
        if (_genericList != null)
          return _genericList.IsReadOnly;
        else
          return base.IsReadOnly;
      }
    }

    public override bool Remove(T item)
    {
      if (_genericList != null)
      {
        return _genericList.Remove(item);
      }
      else
      {
        bool contains = base.Contains(item);

        if (contains)
          base.Remove(item);

        return contains;
      }
    }

    public override IEnumerator<T> GetEnumerator()
    {
      if (_genericList != null)
        return _genericList.GetEnumerator();

      return base.GetEnumerator();
    }

    public object UnderlyingList
    {
      get
      {
        if (_genericList != null)
          return _genericList;
        else
          return UnderlyingCollection;
      }
    }
  }
}