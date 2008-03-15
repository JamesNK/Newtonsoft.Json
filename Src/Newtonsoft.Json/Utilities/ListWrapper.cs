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

namespace Newtonsoft.Json.Utilities
{
  public interface IWrappedList : IList
  {
    object UnderlyingList { get; }
  }

  public class ListWrapper<T> : IList<T>, IWrappedList
  {
    private readonly IList _list;
    private readonly IList<T> _genericList;
    private object _syncRoot;

    public ListWrapper(IList list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      _list = list;
    }

    public ListWrapper(IList<T> list)
    {
      ValidationUtils.ArgumentNotNull(list, "list");

      _genericList = list;
    }

    public int IndexOf(T item)
    {
      if (_genericList != null)
        return _genericList.IndexOf(item);
      else
        return _list.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
      if (_genericList != null)
        _genericList.Insert(index, item);
      else
        _list.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
      if (_genericList != null)
        _genericList.RemoveAt(index);
      else
        _list.RemoveAt(index);
    }

    public T this[int index]
    {
      get
      {
        if (_genericList != null)
          return _genericList[index];
        else
          return (T)_list[index];
      }
      set
      {
        if (_genericList != null)
          _genericList[index] = value;
        else
          _list[index] = value;
      }
    }

    public void Add(T item)
    {
      if (_genericList != null)
        _genericList.Add(item);
      else
        _list.Add(item);
    }

    public void Clear()
    {
      if (_genericList != null)
        _genericList.Clear();
      else
        _list.Clear();
    }

    public bool Contains(T item)
    {
      if (_genericList != null)
        return _genericList.Contains(item);
      else
        return _list.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
      if (_genericList != null)
        _genericList.CopyTo(array, arrayIndex);
      else
        _list.CopyTo(array, arrayIndex);
    }

    public int Count
    {
      get
      {
        if (_genericList != null)
          return _genericList.Count;
        else
          return _list.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        if (_genericList != null)
          return _genericList.IsReadOnly;
        else
          return _list.IsReadOnly;
      }
    }

    public bool Remove(T item)
    {
      if (_genericList != null)
      {
        return _genericList.Remove(item);
      }
      else
      {
        bool contains = _list.Contains(item);

        if (contains)
          _list.Remove(item);

        return contains;
      }
    }

    public IEnumerator<T> GetEnumerator()
    {
      if (_genericList != null)
        return _genericList.GetEnumerator();

      return _list.Cast<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      if (_genericList != null)
        return _genericList.GetEnumerator();
      else
        return _list.GetEnumerator();
    }

    int IList.Add(object value)
    {
      VerifyValueType(value);
      Add((T)value);

      return (Count - 1);
    }

    bool IList.Contains(object value)
    {
      if (IsCompatibleObject(value))
        return Contains((T)value);

      return false;
    }

    int IList.IndexOf(object value)
    {
      if (IsCompatibleObject(value))
        return IndexOf((T)value);

      return -1;
    }

    void IList.Insert(int index, object value)
    {
      VerifyValueType(value);
      Insert(index, (T)value);
    }

    bool IList.IsFixedSize
    {
      get { return false; }
    }

    void IList.Remove(object value)
    {
      if (IsCompatibleObject(value))
        Remove((T)value);
    }

    object IList.this[int index]
    {
      get { return this[index]; }
      set
      {
        VerifyValueType(value);
        this[index] = (T)value;
      }
    }

    void ICollection.CopyTo(Array array, int arrayIndex)
    {
      CopyTo((T[])array, arrayIndex);
    }

    bool ICollection.IsSynchronized
    {
      get { return false; }
    }

    object ICollection.SyncRoot
    {
      get
      {
        if (_syncRoot == null)
          Interlocked.CompareExchange(ref _syncRoot, new object(), null);

        return _syncRoot;
      }
    }

    private static void VerifyValueType(object value)
    {
      if (!IsCompatibleObject(value))
        throw new ArgumentException(string.Format("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.", value, typeof(T)), "value");
    }

    private static bool IsCompatibleObject(object value)
    {
      if (!(value is T) && (value != null || typeof(T).IsValueType))
        return false;

      return true;
    }

    public object UnderlyingList
    {
      get
      {
        if (_genericList != null)
          return _genericList;
        else
          return _list;
      }
    }
  }
}