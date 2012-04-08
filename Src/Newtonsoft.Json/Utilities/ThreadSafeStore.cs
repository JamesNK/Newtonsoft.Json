using System;
using System.Collections.Generic;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
  internal class ThreadSafeStore<TKey, TValue>
  {
    private readonly object _lock = new object();
    private Dictionary<TKey, TValue> _store;
    private readonly Func<TKey, TValue> _creator;

    public ThreadSafeStore(Func<TKey, TValue> creator)
    {
      if (creator == null)
        throw new ArgumentNullException("creator");

      _creator = creator;
    }

    public TValue Get(TKey key)
    {
      if (_store == null)
        return AddValue(key);

      TValue value;
      if (!_store.TryGetValue(key, out value))
        return AddValue(key);

      return value;
    }

    private TValue AddValue(TKey key)
    {
      TValue value = _creator(key);

      lock (_lock)
      {
        if (_store == null)
        {
          _store = new Dictionary<TKey, TValue>();
          _store[key] = value;
        }
        else
        {
          // double check locking
          TValue checkValue;
          if (_store.TryGetValue(key, out checkValue))
            return checkValue;

          Dictionary<TKey, TValue> newStore = new Dictionary<TKey, TValue>(_store);
          newStore[key] = value;

          _store = newStore;
        }

        return value;
      }
    }
  }
}