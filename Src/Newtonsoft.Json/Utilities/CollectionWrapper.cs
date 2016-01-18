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
using System.Threading;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Utilities
{
    internal interface IWrappedCollection : IList
    {
        object UnderlyingCollection { get; }
    }

    internal class CollectionWrapper<T> : ICollection<T>, IWrappedCollection
    {
        private readonly IList _list;
        private readonly ICollection<T> _genericCollection;
        private object _syncRoot;

        public CollectionWrapper(IList list)
        {
            ValidationUtils.ArgumentNotNull(list, nameof(list));

            if (list is ICollection<T>)
            {
                _genericCollection = (ICollection<T>)list;
            }
            else
            {
                _list = list;
            }
        }

        public CollectionWrapper(ICollection<T> list)
        {
            ValidationUtils.ArgumentNotNull(list, nameof(list));

            _genericCollection = list;
        }

        public virtual void Add(T item)
        {
            if (_genericCollection != null)
            {
                _genericCollection.Add(item);
            }
            else
            {
                _list.Add(item);
            }
        }

        public virtual void Clear()
        {
            if (_genericCollection != null)
            {
                _genericCollection.Clear();
            }
            else
            {
                _list.Clear();
            }
        }

        public virtual bool Contains(T item)
        {
            if (_genericCollection != null)
            {
                return _genericCollection.Contains(item);
            }
            else
            {
                return _list.Contains(item);
            }
        }

        public virtual void CopyTo(T[] array, int arrayIndex)
        {
            if (_genericCollection != null)
            {
                _genericCollection.CopyTo(array, arrayIndex);
            }
            else
            {
                _list.CopyTo(array, arrayIndex);
            }
        }

        public virtual int Count
        {
            get
            {
                if (_genericCollection != null)
                {
                    return _genericCollection.Count;
                }
                else
                {
                    return _list.Count;
                }
            }
        }

        public virtual bool IsReadOnly
        {
            get
            {
                if (_genericCollection != null)
                {
                    return _genericCollection.IsReadOnly;
                }
                else
                {
                    return _list.IsReadOnly;
                }
            }
        }

        public virtual bool Remove(T item)
        {
            if (_genericCollection != null)
            {
                return _genericCollection.Remove(item);
            }
            else
            {
                bool contains = _list.Contains(item);

                if (contains)
                {
                    _list.Remove(item);
                }

                return contains;
            }
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            if (_genericCollection != null)
            {
                return _genericCollection.GetEnumerator();
            }

            return _list.Cast<T>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_genericCollection != null)
            {
                return _genericCollection.GetEnumerator();
            }
            else
            {
                return _list.GetEnumerator();
            }
        }

        int IList.Add(object value)
        {
            VerifyValueType(value);
            Add((T)value);

            return (Count - 1);
        }

        bool IList.Contains(object value)
        {
            if (IsCompatibleObject(value))
            {
                return Contains((T)value);
            }

            return false;
        }

        int IList.IndexOf(object value)
        {
            if (_genericCollection != null)
            {
                throw new InvalidOperationException("Wrapped ICollection<T> does not support IndexOf.");
            }

            if (IsCompatibleObject(value))
            {
                return _list.IndexOf((T)value);
            }

            return -1;
        }

        void IList.RemoveAt(int index)
        {
            if (_genericCollection != null)
            {
                throw new InvalidOperationException("Wrapped ICollection<T> does not support RemoveAt.");
            }

            _list.RemoveAt(index);
        }

        void IList.Insert(int index, object value)
        {
            if (_genericCollection != null)
            {
                throw new InvalidOperationException("Wrapped ICollection<T> does not support Insert.");
            }

            VerifyValueType(value);
            _list.Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get
            {
                if (_genericCollection != null)
                {
                    // ICollection<T> only has IsReadOnly
                    return _genericCollection.IsReadOnly;
                }
                else
                {
                    return _list.IsFixedSize;
                }
            }
        }

        void IList.Remove(object value)
        {
            if (IsCompatibleObject(value))
            {
                Remove((T)value);
            }
        }

        object IList.this[int index]
        {
            get
            {
                if (_genericCollection != null)
                {
                    throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
                }

                return _list[index];
            }
            set
            {
                if (_genericCollection != null)
                {
                    throw new InvalidOperationException("Wrapped ICollection<T> does not support indexer.");
                }

                VerifyValueType(value);
                _list[index] = (T)value;
            }
        }

        void ICollection.CopyTo(Array array, int arrayIndex)
        {
            CopyTo((T[])array, arrayIndex);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
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

        private static void VerifyValueType(object value)
        {
            if (!IsCompatibleObject(value))
            {
                throw new ArgumentException("The value '{0}' is not of type '{1}' and cannot be used in this generic collection.".FormatWith(CultureInfo.InvariantCulture, value, typeof(T)), nameof(value));
            }
        }

        private static bool IsCompatibleObject(object value)
        {
            if (!(value is T) && (value != null || (typeof(T).IsValueType() && !ReflectionUtils.IsNullableType(typeof(T)))))
            {
                return false;
            }

            return true;
        }

        public object UnderlyingCollection
        {
            get
            {
                if (_genericCollection != null)
                {
                    return _genericCollection;
                }
                else
                {
                    return _list;
                }
            }
        }
    }
}