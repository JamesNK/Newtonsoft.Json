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
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    internal class JPropertyKeyedCollection : Collection<JToken>
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.Ordinal;

        private Dictionary<string, JToken>? _dictionary;

        public JPropertyKeyedCollection() : base(new List<JToken>())
        {
        }

        private void AddKey(string key, JToken item)
        {
            EnsureDictionary();
            _dictionary![key] = item;
        }

        protected void ChangeItemKey(JToken item, string newKey)
        {
            if (!ContainsItem(item))
            {
                throw new ArgumentException("The specified item does not exist in this KeyedCollection.");
            }

            string keyForItem = GetKeyForItem(item);
            if (!Comparer.Equals(keyForItem, newKey))
            {
                if (newKey != null)
                {
                    AddKey(newKey, item);
                }

                if (keyForItem != null)
                {
                    RemoveKey(keyForItem);
                }
            }
        }

        protected override void ClearItems()
        {
            base.ClearItems();

            _dictionary?.Clear();
        }

        public bool Contains(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_dictionary != null)
            {
                return _dictionary.ContainsKey(key);
            }

            return false;
        }

        private bool ContainsItem(JToken item)
        {
            if (_dictionary == null)
            {
                return false;
            }

            string key = GetKeyForItem(item);
            return _dictionary.TryGetValue(key, out _);
        }

        private void EnsureDictionary()
        {
            if (_dictionary == null)
            {
                _dictionary = new Dictionary<string, JToken>(Comparer);
            }
        }

        private string GetKeyForItem(JToken item)
        {
            return ((JProperty)item).Name;
        }

        protected override void InsertItem(int index, JToken item)
        {
            AddKey(GetKeyForItem(item), item);
            base.InsertItem(index, item);
        }

        public bool Remove(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (_dictionary != null)
            {
                return _dictionary.TryGetValue(key, out JToken value) && Remove(value);
            }

            return false;
        }

        protected override void RemoveItem(int index)
        {
            string keyForItem = GetKeyForItem(Items[index]);
            RemoveKey(keyForItem);
            base.RemoveItem(index);
        }

        private void RemoveKey(string key)
        {
            _dictionary?.Remove(key);
        }

        protected override void SetItem(int index, JToken item)
        {
            string keyForItem = GetKeyForItem(item);
            string keyAtIndex = GetKeyForItem(Items[index]);

            if (Comparer.Equals(keyAtIndex, keyForItem))
            {
                if (_dictionary != null)
                {
                    _dictionary[keyForItem] = item;
                }
            }
            else
            {
                AddKey(keyForItem, item);

                if (keyAtIndex != null)
                {
                    RemoveKey(keyAtIndex);
                }
            }
            base.SetItem(index, item);
        }

        public JToken this[string key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                if (_dictionary != null)
                {
                    return _dictionary[key];
                }

                throw new KeyNotFoundException();
            }
        }

        public bool TryGetValue(string key, [NotNullWhen(true)]out JToken? value)
        {
            if (_dictionary == null)
            {
                value = null;
                return false;
            }

            return _dictionary.TryGetValue(key, out value);
        }

        public ICollection<string> Keys
        {
            get
            {
                EnsureDictionary();
                return _dictionary!.Keys;
            }
        }

        public ICollection<JToken> Values
        {
            get
            {
                EnsureDictionary();
                return _dictionary!.Values;
            }
        }

        public int IndexOfReference(JToken t)
        {
            return ((List<JToken>)Items).IndexOfReference(t);
        }

        public bool Compare(JPropertyKeyedCollection other)
        {
            if (this == other)
            {
                return true;
            }

            // dictionaries in JavaScript aren't ordered
            // ignore order when comparing properties
            Dictionary<string, JToken>? d1 = _dictionary;
            Dictionary<string, JToken>? d2 = other._dictionary;

            if (d1 == null && d2 == null)
            {
                return true;
            }

            if (d1 == null)
            {
                return (d2!.Count == 0);
            }

            if (d2 == null)
            {
                return (d1.Count == 0);
            }

            if (d1.Count != d2.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, JToken> keyAndProperty in d1)
            {
                if (!d2.TryGetValue(keyAndProperty.Key, out JToken secondValue))
                {
                    return false;
                }

                JProperty p1 = (JProperty)keyAndProperty.Value;
                JProperty p2 = (JProperty)secondValue;

                if (p1.Value == null)
                {
                    return (p2.Value == null);
                }

                if (!p1.Value.DeepEquals(p2.Value))
                {
                    return false;
                }
            }

            return true;
        }
    }
}