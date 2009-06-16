using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  internal class ThreadSafeDictionaryWrapper<TKey, TValue>
  {
    private Dictionary<TKey, TValue> _dictionary;
    private readonly Func<TKey, TValue> _creator;

    public ThreadSafeDictionaryWrapper(Func<TKey, TValue> creator)
    {
      ValidationUtils.ArgumentNotNull(creator, "creator");

      _creator = creator;
    }

    private TValue AddValue(TKey key)
    {
      lock (this)
      {
        TValue value;

        if (_dictionary == null)
        {
          _dictionary = new Dictionary<TKey, TValue>();
          value = _creator(key);
          _dictionary[key] = value;
        }
        else
        {
          // double check locking
          if (_dictionary.TryGetValue(key, out value))
            return value;

          Dictionary<TKey, TValue> newDictionary = new Dictionary<TKey, TValue>(_dictionary);
          value = _creator(key);
          newDictionary[key] = value;

          _dictionary = newDictionary;
        }

        return value;
      }
    }

    public TValue Get(TKey key)
    {
      if (_dictionary == null)
        return AddValue(key);

      TValue value;
      if (!_dictionary.TryGetValue(key, out value))
        return AddValue(key);

      return value;
    }
  }
}