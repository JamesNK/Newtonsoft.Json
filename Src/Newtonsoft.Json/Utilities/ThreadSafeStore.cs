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
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
#if HAVE_CONCURRENT_DICTIONARY
using System.Collections.Concurrent;
#endif
using System.Threading;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Utilities
{
    internal class ThreadSafeStore<TKey, TValue> where TKey : notnull
    {
#if HAVE_CONCURRENT_DICTIONARY
        private readonly ConcurrentDictionary<TKey, TValue> _concurrentStore;
#else
        private readonly object _lock = new object();
        private Dictionary<TKey, TValue> _store;
#endif
        private readonly Func<TKey, TValue> _creator;

        public ThreadSafeStore(Func<TKey, TValue> creator)
        {
            ValidationUtils.ArgumentNotNull(creator, nameof(creator));

            _creator = creator;
#if HAVE_CONCURRENT_DICTIONARY
            _concurrentStore = new ConcurrentDictionary<TKey, TValue>();
#else
            _store = new Dictionary<TKey, TValue>();
#endif
        }

        public TValue Get(TKey key)
        {
#if HAVE_CONCURRENT_DICTIONARY
            return _concurrentStore.GetOrAdd(key, _creator);
#else
            if (!_store.TryGetValue(key, out TValue value))
            {
                return AddValue(key);
            }

            return value;
#endif
        }

#if !HAVE_CONCURRENT_DICTIONARY
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
                    if (_store.TryGetValue(key, out TValue checkValue))
                    {
                        return checkValue;
                    }

                    Dictionary<TKey, TValue> newStore = new Dictionary<TKey, TValue>(_store);
                    newStore[key] = value;

#if HAVE_MEMORY_BARRIER
                    Thread.MemoryBarrier();
#endif
                    _store = newStore;
                }

                return value;
            }
        }
#endif
    }
}