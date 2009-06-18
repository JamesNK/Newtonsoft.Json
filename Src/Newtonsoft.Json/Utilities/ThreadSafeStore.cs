using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Utilities
{
  internal class ThreadSafeStore<TKey, TValue>
  {
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
      lock (this)
      {
        TValue value;

        if (_store == null)
        {
          _store = new Dictionary<TKey, TValue>();
          value = _creator(key);
          _store[key] = value;
        }
        else
        {
          // double check locking
          if (_store.TryGetValue(key, out value))
            return value;

          Dictionary<TKey, TValue> newStore = new Dictionary<TKey, TValue>(_store);
          value = _creator(key);
          newStore[key] = value;

          _store = newStore;
        }

        return value;
      }
    }
  }
}