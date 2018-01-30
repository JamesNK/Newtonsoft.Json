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
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Utilities
{
    internal interface IWrappedDictionary
        : IDictionary
    {
        object UnderlyingDictionary { get; }
    }

    internal class DictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>, IWrappedDictionary
    {
        private readonly IDictionary _dictionary;
        private readonly IDictionary<TKey, TValue> _genericDictionary;
#if HAVE_READ_ONLY_COLLECTIONS
        private readonly IReadOnlyDictionary<TKey, TValue> _readOnlyDictionary;
#endif
        private object _syncRoot;

        public DictionaryWrapper(IDictionary dictionary)
        {
            ValidationUtils.ArgumentNotNull(dictionary, nameof(dictionary));

            _dictionary = dictionary;
        }

        public DictionaryWrapper(IDictionary<TKey, TValue> dictionary)
        {
            ValidationUtils.ArgumentNotNull(dictionary, nameof(dictionary));

            _genericDictionary = dictionary;
        }

#if HAVE_READ_ONLY_COLLECTIONS
        public DictionaryWrapper(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            ValidationUtils.ArgumentNotNull(dictionary, nameof(dictionary));

            _readOnlyDictionary = dictionary;
        }
#endif

        public void Add(TKey key, TValue value)
        {
            if (_dictionary != null)
            {
                _dictionary.Add(key, value);
            }
            else if (_genericDictionary != null)
            {
                _genericDictionary.Add(key, value);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public bool ContainsKey(TKey key)
        {
            if (_dictionary != null)
            {
                return _dictionary.Contains(key);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.ContainsKey(key);
            }
#endif
            else
            {
                return _genericDictionary.ContainsKey(key);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary.Keys.Cast<TKey>().ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Keys.ToList();
                }
#endif
                else
                {
                    return _genericDictionary.Keys;
                }
            }
        }

        public bool Remove(TKey key)
        {
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
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                return _genericDictionary.Remove(key);
            }
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
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
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                return _genericDictionary.TryGetValue(key, out value);
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary.Values.Cast<TValue>().ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Values.ToList();
                }
#endif
                else
                {
                    return _genericDictionary.Values;
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                if (_dictionary != null)
                {
                    return (TValue)_dictionary[key];
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary[key];
                }
#endif
                else
                {
                    return _genericDictionary[key];
                }
            }
            set
            {
                if (_dictionary != null)
                {
                    _dictionary[key] = value;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    throw new NotSupportedException();
                }
#endif
                else
                {
                    _genericDictionary[key] = value;
                }
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary != null)
            {
                ((IList)_dictionary).Add(item);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                _genericDictionary?.Add(item);
            }
        }

        public void Clear()
        {
            if (_dictionary != null)
            {
                _dictionary.Clear();
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                _genericDictionary.Clear();
            }
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary != null)
            {
                return ((IList)_dictionary).Contains(item);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.Contains(item);
            }
#endif
            else
            {
                return _genericDictionary.Contains(item);
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (_dictionary != null)
            {
                // Manual use of IDictionaryEnumerator instead of foreach to avoid DictionaryEntry box allocations.
                IDictionaryEnumerator e = _dictionary.GetEnumerator();
                try
                {
                    while (e.MoveNext())
                    {
                        DictionaryEntry entry = e.Entry;
                        array[arrayIndex++] = new KeyValuePair<TKey, TValue>((TKey)entry.Key, (TValue)entry.Value);
                    }
                }
                finally
                {
                    (e as IDisposable)?.Dispose();
                }
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                _genericDictionary.CopyTo(array, arrayIndex);
            }
        }

        public int Count
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary.Count;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Count;
                }
#endif
                else
                {
                    return _genericDictionary.Count;
                }
            }
        }

        public bool IsReadOnly
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary.IsReadOnly;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return true;
                }
#endif
                else
                {
                    return _genericDictionary.IsReadOnly;
                }
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary != null)
            {
                if (_dictionary.Contains(item.Key))
                {
                    object value = _dictionary[item.Key];

                    if (Equals(value, item.Value))
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
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                return _genericDictionary.Remove(item);
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            if (_dictionary != null)
            {
                return _dictionary.Cast<DictionaryEntry>().Select(de => new KeyValuePair<TKey, TValue>((TKey)de.Key, (TValue)de.Value)).GetEnumerator();
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.GetEnumerator();
            }
#endif
            else
            {
                return _genericDictionary.GetEnumerator();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void IDictionary.Add(object key, object value)
        {
            if (_dictionary != null)
            {
                _dictionary.Add(key, value);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                _genericDictionary.Add((TKey)key, (TValue)value);
            }
        }

        object IDictionary.this[object key]
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary[key];
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary[(TKey)key];
                }
#endif
                else
                {
                    return _genericDictionary[(TKey)key];
                }
            }
            set
            {
                if (_dictionary != null)
                {
                    _dictionary[key] = value;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    throw new NotSupportedException();
                }
#endif
                else
                {
                    _genericDictionary[(TKey)key] = (TValue)value;
                }
            }
        }

        private readonly struct DictionaryEnumerator<TEnumeratorKey, TEnumeratorValue> : IDictionaryEnumerator
        {
            private readonly IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
            {
                ValidationUtils.ArgumentNotNull(e, nameof(e));
                _e = e;
            }

            public DictionaryEntry Entry => (DictionaryEntry)Current;

            public object Key => Entry.Key;

            public object Value => Entry.Value;

            public object Current => new DictionaryEntry(_e.Current.Key, _e.Current.Value);

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
            if (_dictionary != null)
            {
                return _dictionary.GetEnumerator();
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return new DictionaryEnumerator<TKey, TValue>(_readOnlyDictionary.GetEnumerator());
            }
#endif
            else
            {
                return new DictionaryEnumerator<TKey, TValue>(_genericDictionary.GetEnumerator());
            }
        }

        bool IDictionary.Contains(object key)
        {
            if (_genericDictionary != null)
            {
                return _genericDictionary.ContainsKey((TKey)key);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                return _readOnlyDictionary.ContainsKey((TKey)key);
            }
#endif
            else
            {
                return _dictionary.Contains(key);
            }
        }

        bool IDictionary.IsFixedSize
        {
            get
            {
                if (_genericDictionary != null)
                {
                    return false;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return true;
                }
#endif
                else
                {
                    return _dictionary.IsFixedSize;
                }
            }
        }

        ICollection IDictionary.Keys
        {
            get
            {
                if (_genericDictionary != null)
                {
                    return _genericDictionary.Keys.ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Keys.ToList();
                }
#endif
                else
                {
                    return _dictionary.Keys;
                }
            }
        }

        public void Remove(object key)
        {
            if (_dictionary != null)
            {
                _dictionary.Remove(key);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                _genericDictionary.Remove((TKey)key);
            }
        }

        ICollection IDictionary.Values
        {
            get
            {
                if (_genericDictionary != null)
                {
                    return _genericDictionary.Values.ToList();
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary.Values.ToList();
                }
#endif
                else
                {
                    return _dictionary.Values;
                }
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (_dictionary != null)
            {
                _dictionary.CopyTo(array, index);
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (_readOnlyDictionary != null)
            {
                throw new NotSupportedException();
            }
#endif
            else
            {
                _genericDictionary.CopyTo((KeyValuePair<TKey, TValue>[])array, index);
            }
        }

        bool ICollection.IsSynchronized
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary.IsSynchronized;
                }
                else
                {
                    return false;
                }
            }
        }

        object ICollection.SyncRoot
        {
            get
            {
                if (_syncRoot == null)
                {
                    Interlocked.CompareExchange(ref _syncRoot, new object(), null);
                }

                return _syncRoot;
            }
        }

        public object UnderlyingDictionary
        {
            get
            {
                if (_dictionary != null)
                {
                    return _dictionary;
                }
#if HAVE_READ_ONLY_COLLECTIONS
                else if (_readOnlyDictionary != null)
                {
                    return _readOnlyDictionary;
                }
#endif
                else
                {
                    return _genericDictionary;
                }
            }
        }
    }
}