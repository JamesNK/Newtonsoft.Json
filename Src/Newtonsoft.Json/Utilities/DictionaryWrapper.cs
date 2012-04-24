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
using System.Collections;
using System.Threading;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Utilities
{
#if (NETFX_CORE)
  internal interface IDictionary : ICollection
  {
    object this[object key] { get; set; }
    void Add(object key, object value);
    new IDictionaryEnumerator GetEnumerator();
  }

  internal interface IDictionaryEnumerator : IEnumerator
  {
    DictionaryEntry Entry { get; }
    object Key { get; }
    object Value { get; }
  }

  internal struct DictionaryEntry
  {
    private readonly object _key;
    private readonly object _value;

    public DictionaryEntry(object key, object value)
    {
      _key = key;
      _value = value;
    }

    public object Key
    {
      get { return _key; }
    }

    public object Value
    {
      get { return _value; }
    }
  }
#endif

  internal interface IWrappedDictionary
    : IDictionary
  {
    object UnderlyingDictionary { get; }
  }

  internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, IWrappedDictionary
  {
#if !(NETFX_CORE)
    private readonly IDictionary _dictionary;
#endif
    private readonly IDictionary<TKey, TValue> _genericDictionary;
    private object _syncRoot;

#if !(NETFX_CORE)
    public DictionaryWrapper(IDictionary dictionary)
    {
      ValidationUtils.ArgumentNotNull(dictionary, "dictionary");

      _dictionary = dictionary;
    }
#endif

    public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
    {
      ValidationUtils.ArgumentNotNull(dictionary, "dictionary");

      _genericDictionary = dictionary;
    }

    public void Add(TKey key, TValue value)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        _dictionary.Add(key, value);
      else
#endif
      _genericDictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        return _dictionary.Contains(key);
      else
#endif
      return _genericDictionary.ContainsKey(key);
    }

    public ICollection<TKey> Keys
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary.Keys.Cast<TKey>().ToList();
        else
#endif
        return _genericDictionary.Keys;
      }
    }

    public bool Remove(TKey key)
    {
#if !NETFX_CORE
      if (_dictionary != null)
      {
        if (_dictionary.Contains(key))
        {
          _dictionary.Remove(key);
          return true;
        }
        else
        {
          return false;
        }
      }
#endif

      return _genericDictionary.Remove(key);
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
#if !NETFX_CORE
      if (_dictionary != null)
      {
        if (!_dictionary.Contains(key))
        {
          value = default(TValue);
          return false;
        }
        else
        {
          value = (TValue)_dictionary[key];
          return true;
        }
      }
#endif

      return _genericDictionary.TryGetValue(key, out value);
    }

    public ICollection<TValue> Values
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary.Values.Cast<TValue>().ToList();
        else
#endif
          return _genericDictionary.Values;
      }
    }

    public TValue this[TKey key]
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return (TValue)_dictionary[key];
#endif
        return _genericDictionary[key];
      }
      set
      {
#if !NETFX_CORE
        if (_dictionary != null)
          _dictionary[key] = value;
        else
#endif
        _genericDictionary[key] = value;
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        ((IList)_dictionary).Add(item);
      else
#endif
      _genericDictionary.Add(item);
    }

    public void Clear()
    {
#if !NETFX_CORE
      if (_dictionary != null)
        _dictionary.Clear();
      else
#endif
      _genericDictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        return ((IList)_dictionary).Contains(item);
      else
#endif
      return _genericDictionary.Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
#if !NETFX_CORE
      if (_dictionary != null)
      {
        foreach (DictionaryEntry item in _dictionary)
        {
          array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)item.Key, (TValue)item.Value);
        }
      }
      else
#endif
      {
        _genericDictionary.CopyTo(array, arrayIndex);
      }
    }

    public int Count
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary.Count;
        else
#endif
        return _genericDictionary.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary.IsReadOnly;
        else
#endif
        return _genericDictionary.IsReadOnly;
      }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
#if !NETFX_CORE
      if (_dictionary != null)
      {
        if (_dictionary.Contains(item.Key))
        {
          object value = _dictionary[item.Key];

          if (object.Equals(value, item.Value))
          {
            _dictionary.Remove(item.Key);
            return true;
          }
          else
          {
            return false;
          }
        }
        else
        {
          return true;
        }
      }
      else
#endif
      {
        return _genericDictionary.Remove(item);
      }
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
#if !NETFX_CORE
      if (_dictionary != null)
        return _dictionary.Cast<DictionaryEntry>().Select(de => new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
      else
#endif
      return _genericDictionary.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void IDictionary.Add(object key, object value)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        _dictionary.Add(key, value);
      else
#endif
      _genericDictionary.Add((TKey)key, (TValue)value);
    }

    object IDictionary.this[object key]
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary[key];
        else
#endif
        return _genericDictionary[(TKey)key];
      }
      set
      {
#if !NETFX_CORE
        if (_dictionary != null)
          _dictionary[key] = value;
        else
#endif
        _genericDictionary[(TKey)key] = (TValue)value;
      }
    }

    private struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IDictionaryEnumerator
    {
      private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

      public DictionaryEnumerator(IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
      {
        ValidationUtils.ArgumentNotNull(e, "e");
        _e = e;
      }

      public DictionaryEntry Entry
      {
        get { return (DictionaryEntry)Current; }
      }

      public object Key
      {
        get { return Entry.Key; }
      }

      public object Value
      {
        get { return Entry.Value; }
      }

      public object Current
      {
        get { return new DictionaryEntry(_e.Current.Key, _e.Current.Value); }
      }

      public bool MoveNext()
      {
        return _e.MoveNext();
      }

      public void Reset()
      {
        _e.Reset();
      }
    }

    IDictionaryEnumerator IDictionary.GetEnumerator()
    {
#if !NETFX_CORE
      if (_dictionary != null)
        return _dictionary.GetEnumerator();
      else
#endif
      return new DictionaryEnumerator<TKey, TValue>(_genericDictionary.GetEnumerator());
    }

#if !NETFX_CORE
    bool IDictionary.Contains(object key)
    {
      if (_genericDictionary != null)
        return _genericDictionary.ContainsKey((TKey)key);
      else
        return _dictionary.Contains(key);
    }

    bool IDictionary.IsFixedSize
    {
      get
      {
        if (_genericDictionary != null)
          return false;
        else
          return _dictionary.IsFixedSize;
      }
    }

    ICollection IDictionary.Keys
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary.Keys.ToList();
        else
          return _dictionary.Keys;
      }
    }
#endif

    public void Remove(object key)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        _dictionary.Remove(key);
      else
#endif
      _genericDictionary.Remove((TKey)key);
    }

#if !NETFX_CORE
    ICollection IDictionary.Values
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary.Values.ToList();
        else
          return _dictionary.Values;
      }
    }
#endif

    void ICollection.CopyTo(Array array, int index)
    {
#if !NETFX_CORE
      if (_dictionary != null)
        _dictionary.CopyTo(array, index);
      else
#endif
      _genericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
    }

    bool ICollection.IsSynchronized
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary.IsSynchronized;
        else
#endif
        return false;
      }
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

    public object UnderlyingDictionary
    {
      get
      {
#if !NETFX_CORE
        if (_dictionary != null)
          return _dictionary;
        else
#endif
        return _genericDictionary;
      }
    }
  }
}
