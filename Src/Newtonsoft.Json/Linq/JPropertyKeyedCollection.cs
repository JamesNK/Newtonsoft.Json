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
using System.Collections.ObjectModel;
using System.Diagnostics;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    internal class JPropertyKeyedCollection : Collection<JToken>
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.Ordinal;

        private KeyedMap _map = KeyedMap.Empty;

        public JPropertyKeyedCollection() : base(new List<JToken>())
        {
        }

        private void AddKey(string key, JToken item)
        {
            _map = _map.Set(key, item);
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

            _map = KeyedMap.Empty;
        }

        public bool Contains(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            return _map.TryGetValue(key, out _);
        }

        private bool ContainsItem(JToken item)
        {
            string key = GetKeyForItem(item);
            return _map.TryGetValue(key, out _);
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

            _map = _map.TryRemove(key, out bool success);
            return success;
        }

        protected override void RemoveItem(int index)
        {
            string keyForItem = GetKeyForItem(Items[index]);
            RemoveKey(keyForItem);
            base.RemoveItem(index);
        }

        private void RemoveKey(string key)
        {
            _map = _map.TryRemove(key, out _);
        }

        protected override void SetItem(int index, JToken item)
        {
            string keyForItem = GetKeyForItem(item);
            string keyAtIndex = GetKeyForItem(Items[index]);

            if (Comparer.Equals(keyAtIndex, keyForItem))
            {
                _map = _map.Set(keyForItem, item);
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

                if (_map.TryGetValue(key, out var value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
        }

        public bool TryGetValue(string key, out JToken value)
        {
            if (_map == null)
            {
                value = null;
                return false;
            }

            return _map.TryGetValue(key, out value);
        }

        public ICollection<string> Keys
        {
            get => _map.Keys;
        }

        public ICollection<JToken> Values
        {
            get => _map.Values;
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
            KeyedMap d1 = _map;
            KeyedMap d2 = other._map;

            if (d1 == null && d2 == null)
            {
                return true;
            }

            if (d1 == null)
            {
                return (d2.Count == 0);
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
                JToken secondValue;
                if (!d2.TryGetValue(keyAndProperty.Key, out secondValue))
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


        internal abstract class KeyedMap
        {
            public static KeyedMap Empty { get; } = new EmptyKeyedMap();

            public abstract int Count { get; }

            public abstract KeyedMap Set(string key, JToken value);

            public abstract KeyedMap TryRemove(string key, out bool success);

            public abstract bool TryGetValue(string key, out JToken value);

            public virtual Enumerator GetEnumerator() => new Enumerator(this);

            protected abstract bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value);

            public abstract ICollection<string> Keys { get; }

            public abstract ICollection<JToken> Values { get; }

            public struct Enumerator : IEnumerator<KeyValuePair<string, JToken>>
            {
                KeyedMap _map;
                int index;
                KeyValuePair<string, JToken> _current;

                internal Enumerator(KeyedMap map)
                {
                    _map = map;
                    index = -1;
                    _current = default(KeyValuePair<string, JToken>);
                }

                public KeyValuePair<string, JToken> Current => _current;

                public bool MoveNext() => _map.TryGetNext(ref index, out _current);

                public void Dispose()
                {
                }

                object IEnumerator.Current => throw new NotSupportedException();
                void IEnumerator.Reset() => throw new NotSupportedException();
            }

            // Instance without any key/value pairs. Used as a singleton.
            private sealed class EmptyKeyedMap : KeyedMap
            {
                public override int Count => 0;

                public override KeyedMap Set(string key, JToken value)
                {
                    // Create a new one-element map to store the key/value pair
                    return new OneElementKeyedMap(key, value);
                }

                public override bool TryGetValue(string key, out JToken value)
                {
                    // Nothing here
                    value = null;
                    return false;
                }

                public override KeyedMap TryRemove(string key, out bool success)
                {
                    // Nothing to remove
                    success = false;
                    return this;
                }

                protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value)
                {
                    value = default(KeyValuePair<string, JToken>);
                    return false;
                }

                public override ICollection<string> Keys => new string[0];

                public override ICollection<JToken> Values => new JToken[0];
            }

            // Instance with one key/value pair.
            private sealed class OneElementKeyedMap : KeyedMap
            {
                private readonly string _key1;
                private JToken _value1;

                public override int Count => 1;

                public OneElementKeyedMap(string key, JToken value)
                {
                    _key1 = key; _value1 = value;
                }

                public override KeyedMap Set(string key, JToken value)
                {
                    // If the key matches one already contained in this map update it,
                    // otherwise create a two-element map with the additional key/value.
                    if (Comparer.Equals(key, _key1))
                    {
                        _value1 = value;
                        return this;
                    }
                    else
                    {
                        return new TwoElementKeyedMap(_key1, _value1, key, value);
                    }
                }

                public override bool TryGetValue(string key, out JToken value)
                {
                    if (Comparer.Equals(key, _key1))
                    {
                        value = _value1;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }

                public override KeyedMap TryRemove(string key, out bool success)
                {
                    success = Comparer.Equals(key, _key1);
                    if (success)
                    {
                        // Return the Empty singleton
                        return Empty;
                    }
                    return this;
                }

                protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value)
                {
                    index++;
                    if (index == 0)
                    {
                        value = new KeyValuePair<string, JToken>(_key1, _value1);
                        return true;
                    }

                    value = default(KeyValuePair<string, JToken>);
                    return false;
                }

                public override ICollection<string> Keys => new []{ _key1 };
                public override ICollection<JToken> Values => new []{ _value1 };
            }

            // Instance with two key/value pairs.
            private sealed class TwoElementKeyedMap : KeyedMap
            {
                private readonly string _key1, _key2;
                private JToken _value1, _value2;

                public override int Count => 2;

                public TwoElementKeyedMap(string key1, JToken value1, string key2, JToken value2)
                {
                    _key1 = key1; _value1 = value1;
                    _key2 = key2; _value2 = value2;
                }

                public override KeyedMap Set(string key, JToken value)
                {
                    // If the key matches one already contained in this map  then update the value
                    if (Comparer.Equals(key, _key1))
                    {
                        _value1 = value;
                    }
                    else if (Comparer.Equals(key, _key2))
                    {
                        _value2 = value;
                    }
                    else
                    {
                        // Otherwise create a three-element map with the additional key/value.
                        return new ThreeElementKeyedMap(_key1, _value1, _key2, _value2, key, value);
                    }

                    return this;
                }

                public override bool TryGetValue(string key, out JToken value)
                {
                    if (Comparer.Equals(key, _key1))
                    {
                        value = _value1;
                        return true;
                    }
                    else if (Comparer.Equals(key, _key2))
                    {
                        value = _value2;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }

                public override KeyedMap TryRemove(string key, out bool success)
                {
                    // If the key exists in this map, remove it by downgrading to a one-element map without the key. 
                    if (Comparer.Equals(key, _key1))
                    {
                        success = true;
                        return new OneElementKeyedMap(_key2, _value2);
                    }
                    else if (Comparer.Equals(key, _key2))
                    {
                        success = true;
                        return new OneElementKeyedMap(_key1, _value1);
                    }
                    else
                    {
                        // Otherwise, there's nothing to add or remove, so just return this map.
                        success = false;
                        return this;
                    }
                }

                protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value)
                {
                    index++;
                    if (index == 0)
                    {
                        value = new KeyValuePair<string, JToken>(_key1, _value1);
                    }
                    else if (index == 1)
                    {
                        value = new KeyValuePair<string, JToken>(_key2, _value2);
                    }
                    else
                    {
                        value = default(KeyValuePair<string, JToken>);
                        return false;
                    }

                    return true;
                }

                public override ICollection<string> Keys => new[] { _key1, _key2 };
                public override ICollection<JToken> Values => new[] { _value1, _value2 };
            }

            // Instance with three key/value pairs.
            private sealed class ThreeElementKeyedMap : KeyedMap
            {
                private readonly string _key1, _key2, _key3;
                private JToken _value1, _value2, _value3;

                public override int Count => 3;

                public ThreeElementKeyedMap(string key1, JToken value1, string key2, JToken value2, string key3, JToken value3)
                {
                    _key1 = key1; _value1 = value1;
                    _key2 = key2; _value2 = value2;
                    _key3 = key3; _value3 = value3;
                }

                public override KeyedMap Set(string key, JToken value)
                {
                    // If the key matches one already contained in this map, then the update the value.
                    if (Comparer.Equals(key, _key1))
                    {
                        _value1 = value;
                    }
                    else if (Comparer.Equals(key, _key2))
                    {
                        _value2 = value;
                    }
                    else if (Comparer.Equals(key, _key3))
                    {
                        _value3 = value;
                    }
                    else
                    {
                        // The key doesn't exist in this map, so upgrade to a multi map that contains
                        // the additional key/value pair.
                        var multi = new MultiElementKeyedMap(4);
                        multi.UnsafeStore(0, _key1, _value1);
                        multi.UnsafeStore(1, _key2, _value2);
                        multi.UnsafeStore(2, _key3, _value3);
                        multi.UnsafeStore(3, key, value);
                        return multi;
                    }

                    return this;
                }

                public override bool TryGetValue(string key, out JToken value)
                {
                    if (Comparer.Equals(key, _key1))
                    {
                        value = _value1;
                        return true;
                    }
                    else if (Comparer.Equals(key, _key2))
                    {
                        value = _value2;
                        return true;
                    }
                    else if (Comparer.Equals(key, _key3))
                    {
                        value = _value3;
                        return true;
                    }
                    else
                    {
                        value = null;
                        return false;
                    }
                }

                public override KeyedMap TryRemove(string key, out bool success)
                {
                    // If the key exists in this map, remove it by downgrading to a two - element map without the key.
                    if (Comparer.Equals(key, _key1))
                    {
                        success = true;
                        return new TwoElementKeyedMap(_key2, _value2, _key3, _value3);
                    }
                    else if (Comparer.Equals(key, _key2))
                    {
                        success = true;
                        return new TwoElementKeyedMap(_key1, _value1, _key3, _value3);
                    }
                    else if (Comparer.Equals(key, _key3))
                    {
                        success = true;
                        return new TwoElementKeyedMap(_key1, _value1, _key2, _value2);
                    }
                    else
                    {
                        // Otherwise, there's nothing to add or remove, so just return this map.
                        success = true;
                        return this;
                    }
                }

                protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value)
                {
                    index++;
                    if (index == 0)
                    {
                        value = new KeyValuePair<string, JToken>(_key1, _value1);
                    }
                    else if (index == 1)
                    {
                        value = new KeyValuePair<string, JToken>(_key2, _value2);
                    }
                    else if (index == 1)
                    {
                        value = new KeyValuePair<string, JToken>(_key3, _value3);
                    }
                    else
                    {
                        value = default(KeyValuePair<string, JToken>);
                        return false;
                    }

                    return true;
                }

                public override ICollection<string> Keys => new[] { _key1, _key2, _key3 };
                public override ICollection<JToken> Values => new[] { _value1, _value2, _value3 };
            }

            // Instance with up to 16 key/value pairs.
            private sealed class MultiElementKeyedMap : KeyedMap
            {
                internal const int MaxMultiElements = 16;
                private KeyValuePair<string, JToken>[] _keyValues;

                public override int Count => _keyValues.Length;

                internal MultiElementKeyedMap(int count)
                {
                    Debug.Assert(count <= MaxMultiElements);
                    _keyValues = new KeyValuePair<string, JToken>[count];
                }

                internal void UnsafeStore(int index, string key, JToken value)
                {
                    Debug.Assert(index < _keyValues.Length);
                    _keyValues[index] = new KeyValuePair<string, JToken>(key, value);
                }

                public override KeyedMap Set(string key, JToken value)
                {
                    // Find the key in this map.
                    for (int i = 0; i < _keyValues.Length; i++)
                    {
                        if (Comparer.Equals(key, _keyValues[i].Key))
                        {
                            // The key is in the map. Update the value
                            _keyValues[i] = new KeyValuePair<string, JToken>(key, value);
                            return this;
                        }
                    }

                    // The key does not already exist in this map.

                    // We need to create a new map that has the additional key/value pair.
                    // If with the addition we can still fit in a multi map, create one.
                    if (_keyValues.Length < MaxMultiElements)
                    {
                        var multi = new MultiElementKeyedMap(_keyValues.Length + 1);
                        Array.Copy(_keyValues, 0, multi._keyValues, 0, _keyValues.Length);
                        multi._keyValues[_keyValues.Length] = new KeyValuePair<string, JToken>(key, value);
                        return multi;
                    }

                    // Otherwise, upgrade to a many map.
                    var many = new ManyElementKeyedMap(MaxMultiElements + 1);
                    foreach (KeyValuePair<string, JToken> pair in _keyValues)
                    {
                        many[pair.Key] = pair.Value;
                    }
                    many[key] = value;
                    return many;
                }

                public override bool TryGetValue(string key, out JToken value)
                {
                    foreach (KeyValuePair<string, JToken> pair in _keyValues)
                    {
                        if (Comparer.Equals(key, pair.Key))
                        {
                            value = pair.Value;
                            return true;
                        }
                    }
                    value = null;
                    return false;
                }

                public override KeyedMap TryRemove(string key, out bool success)
                {
                    // Find the key in this map.
                    for (int i = 0; i < _keyValues.Length; i++)
                    {
                        if (Comparer.Equals(key, _keyValues[i].Key))
                        {
                            // The key is in the map.  If the value isn't null, then create a new map of the same
                            // size that has all of the same pairs, with this new key/value pair overwriting the old.
                            if (_keyValues.Length == 4)
                            {
                                success = true;
                                // We only have four elements, one of which we're removing,
                                // so downgrade to a three-element map, without the matching element.
                                return
                                    i == 0 ? new ThreeElementKeyedMap(_keyValues[1].Key, _keyValues[1].Value, _keyValues[2].Key, _keyValues[2].Value, _keyValues[3].Key, _keyValues[3].Value) :
                                    i == 1 ? new ThreeElementKeyedMap(_keyValues[0].Key, _keyValues[0].Value, _keyValues[2].Key, _keyValues[2].Value, _keyValues[3].Key, _keyValues[3].Value) :
                                    i == 2 ? new ThreeElementKeyedMap(_keyValues[0].Key, _keyValues[0].Value, _keyValues[1].Key, _keyValues[1].Value, _keyValues[3].Key, _keyValues[3].Value) :
                                             new ThreeElementKeyedMap(_keyValues[0].Key, _keyValues[0].Value, _keyValues[1].Key, _keyValues[1].Value, _keyValues[2].Key, _keyValues[2].Value);
                            }
                            else
                            {
                                success = true;
                                // We have enough elements remaining to warrant a multi map.
                                // Create a new one and copy all of the elements from this one, except the one to be removed.
                                var multi = new MultiElementKeyedMap(_keyValues.Length - 1);
                                if (i != 0) Array.Copy(_keyValues, 0, multi._keyValues, 0, i);
                                if (i != _keyValues.Length - 1) Array.Copy(_keyValues, i + 1, multi._keyValues, i, _keyValues.Length - i - 1);
                                return multi;
                            }
                        }
                    }

                    // Key not found, nothing to remove
                    success = false;
                    return this;
                }

                protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value)
                {
                    index++;

                    if ((uint)index >= (uint)_keyValues.Length)
                    {
                        value = default(KeyValuePair<string, JToken>);
                        return false;
                    }

                    value = _keyValues[index];
                    return true;
                }

                public override ICollection<string> Keys
                {
                    get
                    {
                        var keysValues = _keyValues;
                        var keys = new string[keysValues.Length];
                        for (var i = 0; i < keysValues.Length; i++)
                        {
                            keys[i] = keysValues[i].Key;
                        }

                        return keys;
                    }
                }

                public override ICollection<JToken> Values
                {
                    get
                    {
                        var keysValues = _keyValues;
                        var values = new JToken[keysValues.Length];
                        for (var i = 0; i < keysValues.Length; i++)
                        {
                            values[i] = keysValues[i].Value;
                        }

                        return values;
                    }
                }
            }

            // Instance with any number of key/value pairs.
            private sealed class ManyElementKeyedMap : KeyedMap
            {
                private Dictionary<string, JToken> _dictionary;

                public override int Count => _dictionary.Count;

                public ManyElementKeyedMap(int capacity)
                {
                    _dictionary = new Dictionary<string, JToken>(capacity, Comparer);
                }

                public override KeyedMap Set(string key, JToken value)
                {
                    _dictionary[key] = value;
                    return this;
                }

                public override KeyedMap TryRemove(string key, out bool success)
                {
                    int count = _dictionary.Count;
                    // If the key is contained in this map, we're going to create a new map that's one pair smaller.
                    if (_dictionary.ContainsKey(key))
                    {
                        // If the new count would be within range of a multi map instead of a many map,
                        // downgrade to the many map, which uses less memory and is faster to access.
                        // Otherwise, just create a new many map that's missing this key.
                        if (count == MultiElementKeyedMap.MaxMultiElements + 1)
                        {
                            var multi = new MultiElementKeyedMap(MultiElementKeyedMap.MaxMultiElements);
                            int index = 0;
                            foreach (KeyValuePair<string, JToken> pair in _dictionary)
                            {
                                if (!Comparer.Equals(key, pair.Key))
                                {
                                    multi.UnsafeStore(index++, pair.Key, pair.Value);
                                }
                            }
                            Debug.Assert(index == MultiElementKeyedMap.MaxMultiElements);
                            success = true;
                            return multi;
                        }
                        else
                        {
                            var map = new ManyElementKeyedMap(count - 1);
                            foreach (KeyValuePair<string, JToken> pair in _dictionary)
                            {
                                if (!Comparer.Equals(key, pair.Key))
                                {
                                    map[pair.Key] = pair.Value;
                                }
                            }
                            Debug.Assert(_dictionary.Count == count - 1);
                            success = true;
                            return map;
                        }
                    }

                    // The key wasn't in the map, so there's nothing to change.
                    // Just return this instance.
                    success = false;
                    return this;
                }

                public override bool TryGetValue(string key, out JToken value) => _dictionary.TryGetValue(key, out value);

                public JToken this[string key]
                {
                    get => _dictionary[key];
                    set => _dictionary[key] = value;
                }

                protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value) => throw new NotSupportedException();

                public override Enumerator GetEnumerator() => new Enumerator(new ManyElementKeyedMapEnumerator(this));


                public override ICollection<string> Keys => _dictionary.Keys;
                public override ICollection<JToken> Values => _dictionary.Values;

                private class ManyElementKeyedMapEnumerator : KeyedMap
                {
                    private Dictionary<string, JToken>.Enumerator _enumerator;

                    public ManyElementKeyedMapEnumerator(ManyElementKeyedMap map)
                    {
                        _enumerator = map._dictionary.GetEnumerator();
                    }

                    protected override bool TryGetNext(ref int index, out KeyValuePair<string, JToken> value)
                    {
                        var success = _enumerator.MoveNext();
                        value = success ? _enumerator.Current : default(KeyValuePair<string, JToken>);

                        return success;
                    }

                    public override int Count => throw new NotSupportedException();
                    public override KeyedMap Set(string key, JToken value) => throw new NotSupportedException();
                    public override bool TryGetValue(string key, out JToken value) => throw new NotSupportedException();
                    public override KeyedMap TryRemove(string key, out bool success) => throw new NotSupportedException();
                    public override ICollection<string> Keys => throw new NotSupportedException();
                    public override ICollection<JToken> Values => throw new NotSupportedException();
                }
            }
        }
    }
}