using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Threading;

namespace Newtonsoft.Json.Utilities
{
  internal interface IWrappedDictionary : IDictionary
  {
    object UnderlyingDictionary { get; }
  }

  internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, IWrappedDictionary
  {
    private readonly IDictionary _dictionary;
    private readonly IDictionary<TKey, TValue> _genericDictionary;
    private object _syncRoot;

    public DictionaryWrapper(IDictionary dictionary)
    {
      ValidationUtils.ArgumentNotNull(dictionary, "dictionary");

      _dictionary = dictionary;
    }

    public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
    {
      ValidationUtils.ArgumentNotNull(dictionary, "dictionary");

      _genericDictionary = dictionary;
    }

    public void Add(TKey key, TValue value)
    {
      if (_genericDictionary != null)
        _genericDictionary.Add(key, value);
      else
        _dictionary.Add(key, value);
    }

    public bool ContainsKey(TKey key)
    {
      if (_genericDictionary != null)
        return _genericDictionary.ContainsKey(key);
      else
        return _dictionary.Contains(key);
    }

    public ICollection<TKey> Keys
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary.Keys;
        else
          return _dictionary.Keys.Cast<TKey>().ToList();
      }
    }

    public bool Remove(TKey key)
    {
      if (_genericDictionary != null)
      {
        return _genericDictionary.Remove(key);
      }
      else
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
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
      if (_genericDictionary != null)
      {
        return _genericDictionary.TryGetValue(key, out value);
      }
      else
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
    }

    public ICollection<TValue> Values
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary.Values;
        else
          return _dictionary.Values.Cast<TValue>().ToList();
      }
    }

    public TValue this[TKey key]
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary[key];
        else
          return (TValue)_dictionary[key];
      }
      set
      {
        if (_genericDictionary != null)
          _genericDictionary[key] = value;
        else
          _dictionary[key] = value;
      }
    }

    public void Add(KeyValuePair<TKey, TValue> item)
    {
      if (_genericDictionary != null)
        _genericDictionary.Add(item);
      else
        ((IList)_dictionary).Add(item);
    }

    public void Clear()
    {
      if (_genericDictionary != null)
        _genericDictionary.Clear();
      else
        _dictionary.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
      if (_genericDictionary != null)
        return _genericDictionary.Contains(item);
      else
        return ((IList)_dictionary).Contains(item);
    }

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
      if (_genericDictionary != null)
      {
        _genericDictionary.CopyTo(array, arrayIndex);
      }
      else
      {
        foreach (DictionaryEntry item in _dictionary)
        {
          array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)item.Key, (TValue)item.Value);
        }
      }
    }

    public int Count
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary.Count;
        else
          return _dictionary.Count;
      }
    }

    public bool IsReadOnly
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary.IsReadOnly;
        else
          return _dictionary.IsReadOnly;
      }
    }

    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
      if (_genericDictionary != null)
      {
        return _genericDictionary.Remove(item);
      }
      else
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
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
      if (_genericDictionary != null)
        return _genericDictionary.GetEnumerator();
      else
        return _dictionary.Cast<DictionaryEntry>().Select(de => new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    void IDictionary.Add(object key, object value)
    {
      if (_genericDictionary != null)
        _genericDictionary.Add((TKey)key, (TValue)value);
      else
        _dictionary.Add(key, value);
    }

    bool IDictionary.Contains(object key)
    {
      if (_genericDictionary != null)
        return _genericDictionary.ContainsKey((TKey)key);
      else
        return _dictionary.Contains(key);
    }

    private struct DictionaryEnumerator : IDictionaryEnumerator
    {
      private IEnumerator _e;

      public DictionaryEnumerator(IEnumerator e)
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
        get { return _e.Current; }
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
      if (_genericDictionary != null)
        return new DictionaryEnumerator(_genericDictionary.GetEnumerator());
      else
        return _dictionary.GetEnumerator();
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

    public void Remove(object key)
    {
      if (_genericDictionary != null)
        _genericDictionary.Remove((TKey)key);
      else
        _dictionary.Remove(key);
    }

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

    object IDictionary.this[object key]
    {
      get
      {
        if (_genericDictionary != null)
          return _genericDictionary[(TKey)key];
        else
          return _dictionary[key];
      }
      set
      {
        if (_genericDictionary != null)
          _genericDictionary[(TKey)key] = (TValue)value;
        else
          _dictionary[key] = value;
      }
    }

    void ICollection.CopyTo(Array array, int index)
    {
      if (_genericDictionary != null)
        _genericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
      else
        _dictionary.CopyTo(array, index);
    }

    bool ICollection.IsSynchronized
    {
      get
      {
        if (_genericDictionary != null)
          return false;
        else
          return _dictionary.IsSynchronized;
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
        if (_genericDictionary != null)
          return _genericDictionary;
        else
          return _dictionary;
      }
    }
  }
}
