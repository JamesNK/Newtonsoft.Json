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

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class ModelStateDictionary<T> : IDictionary<string, T>
    {
        private readonly Dictionary<string, T> _innerDictionary = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);

        public ModelStateDictionary()
        {
        }

        public ModelStateDictionary(ModelStateDictionary<T> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            foreach (var entry in dictionary)
            {
                _innerDictionary.Add(entry.Key, entry.Value);
            }
        }

        public int Count
        {
            get { return _innerDictionary.Count; }
        }

        public bool IsReadOnly
        {
            get { return ((IDictionary<string, T>)_innerDictionary).IsReadOnly; }
        }

        public ICollection<string> Keys
        {
            get { return _innerDictionary.Keys; }
        }

        public T this[string key]
        {
            get
            {
                T value;
                _innerDictionary.TryGetValue(key, out value);
                return value;
            }
            set { _innerDictionary[key] = value; }
        }

        public ICollection<T> Values
        {
            get { return _innerDictionary.Values; }
        }

        public void Add(KeyValuePair<string, T> item)
        {
            ((IDictionary<string, T>)_innerDictionary).Add(item);
        }

        public void Add(string key, T value)
        {
            _innerDictionary.Add(key, value);
        }

        public void Clear()
        {
            _innerDictionary.Clear();
        }

        public bool Contains(KeyValuePair<string, T> item)
        {
            return ((IDictionary<string, T>)_innerDictionary).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return _innerDictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, T>[] array, int arrayIndex)
        {
            ((IDictionary<string, T>)_innerDictionary).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            return _innerDictionary.GetEnumerator();
        }

        public void Merge(ModelStateDictionary<T> dictionary)
        {
            if (dictionary == null)
            {
                return;
            }

            foreach (var entry in dictionary)
            {
                this[entry.Key] = entry.Value;
            }
        }

        public bool Remove(KeyValuePair<string, T> item)
        {
            return ((IDictionary<string, T>)_innerDictionary).Remove(item);
        }

        public bool Remove(string key)
        {
            return _innerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out T value)
        {
            return _innerDictionary.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_innerDictionary).GetEnumerator();
        }
    }
}