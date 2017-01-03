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
#if !PORTABLE40
using System.Collections.Specialized;
#endif
using System.Threading;
using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Globalization;
using System.ComponentModel;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a token that can contain other tokens.
    /// </summary>
    public abstract class JContainer : JToken, IList<JToken>
#if !(DOTNET || PORTABLE || PORTABLE40)
        , ITypedList, IBindingList
#endif
        , IList
#if !(NET20 || NET35 || PORTABLE40)
        , INotifyCollectionChanged
#endif
    {
#if !(DOTNET || PORTABLE40 || PORTABLE)
        internal ListChangedEventHandler _listChanged;
        internal AddingNewEventHandler _addingNew;

        /// <summary>
        /// Occurs when the list changes or an item in the list changes.
        /// </summary>
        public event ListChangedEventHandler ListChanged
        {
            add { _listChanged += value; }
            remove { _listChanged -= value; }
        }

        /// <summary>
        /// Occurs before an item is added to the collection.
        /// </summary>
        public event AddingNewEventHandler AddingNew
        {
            add { _addingNew += value; }
            remove { _addingNew -= value; }
        }
#endif
#if !(NET20 || NET35 || PORTABLE40)
        internal NotifyCollectionChangedEventHandler _collectionChanged;

        /// <summary>
        /// Occurs when the items list of the collection has changed, or the collection is reset.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChanged += value; }
            remove { _collectionChanged -= value; }
        }
#endif

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected abstract IList<JToken> ChildrenTokens { get; }

        private object _syncRoot;
#if !(PORTABLE40)
        private bool _busy;
#endif

        internal JContainer()
        {
        }

        internal JContainer(JContainer other)
            : this()
        {
            ValidationUtils.ArgumentNotNull(other, nameof(other));

            int i = 0;
            foreach (JToken child in other)
            {
                AddInternal(i, child, false);
                i++;
            }
        }

        internal void CheckReentrancy()
        {
#if !(PORTABLE40)
            if (_busy)
            {
                throw new InvalidOperationException("Cannot change {0} during a collection change event.".FormatWith(CultureInfo.InvariantCulture, GetType()));
            }
#endif
        }

        internal virtual IList<JToken> CreateChildrenCollection()
        {
            return new List<JToken>();
        }

#if !(DOTNET || PORTABLE40 || PORTABLE)
        /// <summary>
        /// Raises the <see cref="AddingNew"/> event.
        /// </summary>
        /// <param name="e">The <see cref="AddingNewEventArgs"/> instance containing the event data.</param>
        protected virtual void OnAddingNew(AddingNewEventArgs e)
        {
            _addingNew?.Invoke(this, e);
        }

        /// <summary>
        /// Raises the <see cref="ListChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ListChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnListChanged(ListChangedEventArgs e)
        {
            ListChangedEventHandler handler = _listChanged;

            if (handler != null)
            {
                _busy = true;
                try
                {
                    handler(this, e);
                }
                finally
                {
                    _busy = false;
                }
            }
        }
#endif
#if !(NET20 || NET35 || PORTABLE40)
        /// <summary>
        /// Raises the <see cref="CollectionChanged"/> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            NotifyCollectionChangedEventHandler handler = _collectionChanged;

            if (handler != null)
            {
                _busy = true;
                try
                {
                    handler(this, e);
                }
                finally
                {
                    _busy = false;
                }
            }
        }
#endif

        /// <summary>
        /// Gets a value indicating whether this token has child tokens.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
        /// </value>
        public override bool HasValues
        {
            get { return ChildrenTokens.Count > 0; }
        }

        internal bool ContentsEqual(JContainer container)
        {
            if (container == this)
            {
                return true;
            }

            IList<JToken> t1 = ChildrenTokens;
            IList<JToken> t2 = container.ChildrenTokens;

            if (t1.Count != t2.Count)
            {
                return false;
            }

            for (int i = 0; i < t1.Count; i++)
            {
                if (!t1[i].DeepEquals(t2[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Get the first child token of this token.
        /// </summary>
        /// <value>
        /// A <see cref="JToken"/> containing the first child token of the <see cref="JToken"/>.
        /// </value>
        public override JToken First
        {
            get
            {
                IList<JToken> children = ChildrenTokens;
                return (children.Count > 0) ? children[0] : null;
            }
        }

        /// <summary>
        /// Get the last child token of this token.
        /// </summary>
        /// <value>
        /// A <see cref="JToken"/> containing the last child token of the <see cref="JToken"/>.
        /// </value>
        public override JToken Last
        {
            get
            {
                IList<JToken> children = ChildrenTokens;
                int count = children.Count;
                return (count > 0) ? children[count - 1] : null;
            }
        }

        /// <summary>
        /// Returns a collection of the child tokens of this token, in document order.
        /// </summary>
        /// <returns>
        /// An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.
        /// </returns>
        public override JEnumerable<JToken> Children()
        {
            return new JEnumerable<JToken>(ChildrenTokens);
        }

        /// <summary>
        /// Returns a collection of the child values of this token, in document order.
        /// </summary>
        /// <typeparam name="T">The type to convert the values to.</typeparam>
        /// <returns>
        /// A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="JToken"/>, in document order.
        /// </returns>
        public override IEnumerable<T> Values<T>()
        {
            return ChildrenTokens.Convert<JToken, T>();
        }

        /// <summary>
        /// Returns a collection of the descendant tokens for this token in document order.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the descendant tokens of the <see cref="JToken"/>.</returns>
        public IEnumerable<JToken> Descendants()
        {
            return GetDescendants(false);
        }

        /// <summary>
        /// Returns a collection of the tokens that contain this token, and all descendant tokens of this token, in document order.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing this token, and all the descendant tokens of the <see cref="JToken"/>.</returns>
        public IEnumerable<JToken> DescendantsAndSelf()
        {
            return GetDescendants(true);
        }

        internal IEnumerable<JToken> GetDescendants(bool self)
        {
            if (self)
            {
                yield return this;
            }

            foreach (JToken o in ChildrenTokens)
            {
                yield return o;
                JContainer c = o as JContainer;
                if (c != null)
                {
                    foreach (JToken d in c.Descendants())
                    {
                        yield return d;
                    }
                }
            }
        }

        internal bool IsMultiContent(object content)
        {
            return (content is IEnumerable && !(content is string) && !(content is JToken) && !(content is byte[]));
        }

        internal JToken EnsureParentToken(JToken item, bool skipParentCheck)
        {
            if (item == null)
            {
                return JValue.CreateNull();
            }

            if (skipParentCheck)
            {
                return item;
            }

            // to avoid a token having multiple parents or creating a recursive loop, create a copy if...
            // the item already has a parent
            // the item is being added to itself
            // the item is being added to the root parent of itself
            if (item.Parent != null || item == this || (item.HasValues && Root == item))
            {
                item = item.CloneToken();
            }

            return item;
        }

        internal abstract int IndexOfItem(JToken item);

        internal virtual void InsertItem(int index, JToken item, bool skipParentCheck)
        {
            IList<JToken> children = ChildrenTokens;

            if (index > children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be within the bounds of the List.");
            }

            CheckReentrancy();

            item = EnsureParentToken(item, skipParentCheck);

            JToken previous = (index == 0) ? null : children[index - 1];
            // haven't inserted new token yet so next token is still at the inserting index
            JToken next = (index == children.Count) ? null : children[index];

            ValidateToken(item, null);

            item.Parent = this;

            item.Previous = previous;
            if (previous != null)
            {
                previous.Next = item;
            }

            item.Next = next;
            if (next != null)
            {
                next.Previous = item;
            }

            children.Insert(index, item);

#if !(DOTNET || PORTABLE40 || PORTABLE)
            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, index));
            }
#endif
#if !(NET20 || NET35 || PORTABLE40)
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
            }
#endif
        }

        internal virtual void RemoveItemAt(int index)
        {
            IList<JToken> children = ChildrenTokens;

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is less than 0.");
            }
            if (index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is equal to or greater than Count.");
            }

            CheckReentrancy();

            JToken item = children[index];
            JToken previous = (index == 0) ? null : children[index - 1];
            JToken next = (index == children.Count - 1) ? null : children[index + 1];

            if (previous != null)
            {
                previous.Next = next;
            }
            if (next != null)
            {
                next.Previous = previous;
            }

            item.Parent = null;
            item.Previous = null;
            item.Next = null;

            children.RemoveAt(index);

#if !(DOTNET || PORTABLE40 || PORTABLE)
            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
            }
#endif
#if !(NET20 || NET35 || PORTABLE40)
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            }
#endif
        }

        internal virtual bool RemoveItem(JToken item)
        {
            int index = IndexOfItem(item);
            if (index >= 0)
            {
                RemoveItemAt(index);
                return true;
            }

            return false;
        }

        internal virtual JToken GetItem(int index)
        {
            return ChildrenTokens[index];
        }

        internal virtual void SetItem(int index, JToken item)
        {
            IList<JToken> children = ChildrenTokens;

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is less than 0.");
            }
            if (index >= children.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Index is equal to or greater than Count.");
            }

            JToken existing = children[index];

            if (IsTokenUnchanged(existing, item))
            {
                return;
            }

            CheckReentrancy();

            item = EnsureParentToken(item, false);

            ValidateToken(item, existing);

            JToken previous = (index == 0) ? null : children[index - 1];
            JToken next = (index == children.Count - 1) ? null : children[index + 1];

            item.Parent = this;

            item.Previous = previous;
            if (previous != null)
            {
                previous.Next = item;
            }

            item.Next = next;
            if (next != null)
            {
                next.Previous = item;
            }

            children[index] = item;

            existing.Parent = null;
            existing.Previous = null;
            existing.Next = null;

#if !(DOTNET || PORTABLE || PORTABLE40)
            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
            }
#endif
#if !(NET20 || NET35 || PORTABLE40)
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, existing, index));
            }
#endif
        }

        internal virtual void ClearItems()
        {
            CheckReentrancy();

            IList<JToken> children = ChildrenTokens;

            foreach (JToken item in children)
            {
                item.Parent = null;
                item.Previous = null;
                item.Next = null;
            }

            children.Clear();

#if !(DOTNET || PORTABLE40 || PORTABLE)
            if (_listChanged != null)
            {
                OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
#endif
#if !(NET20 || NET35 || PORTABLE40)
            if (_collectionChanged != null)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
#endif
        }

        internal virtual void ReplaceItem(JToken existing, JToken replacement)
        {
            if (existing == null || existing.Parent != this)
            {
                return;
            }

            int index = IndexOfItem(existing);
            SetItem(index, replacement);
        }

        internal virtual bool ContainsItem(JToken item)
        {
            return (IndexOfItem(item) != -1);
        }

        internal virtual void CopyItemsTo(Array array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (arrayIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), "arrayIndex is less than 0.");
            }
            if (arrayIndex >= array.Length && arrayIndex != 0)
            {
                throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
            }
            if (Count > array.Length - arrayIndex)
            {
                throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");
            }

            int index = 0;
            foreach (JToken token in ChildrenTokens)
            {
                array.SetValue(token, arrayIndex + index);
                index++;
            }
        }

        internal static bool IsTokenUnchanged(JToken currentValue, JToken newValue)
        {
            JValue v1 = currentValue as JValue;
            if (v1 != null)
            {
                // null will get turned into a JValue of type null
                if (v1.Type == JTokenType.Null && newValue == null)
                {
                    return true;
                }

                return v1.Equals(newValue);
            }

            return false;
        }

        internal virtual void ValidateToken(JToken o, JToken existing)
        {
            ValidationUtils.ArgumentNotNull(o, nameof(o));

            if (o.Type == JTokenType.Property)
            {
                throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
            }
        }

        /// <summary>
        /// Adds the specified content as children of this <see cref="JToken"/>.
        /// </summary>
        /// <param name="content">The content to be added.</param>
        public virtual void Add(object content)
        {
            AddInternal(ChildrenTokens.Count, content, false);
        }

        internal void AddAndSkipParentCheck(JToken token)
        {
            AddInternal(ChildrenTokens.Count, token, true);
        }

        /// <summary>
        /// Adds the specified content as the first children of this <see cref="JToken"/>.
        /// </summary>
        /// <param name="content">The content to be added.</param>
        public void AddFirst(object content)
        {
            AddInternal(0, content, false);
        }

        internal void AddInternal(int index, object content, bool skipParentCheck)
        {
            if (IsMultiContent(content))
            {
                IEnumerable enumerable = (IEnumerable)content;

                int multiIndex = index;
                foreach (object c in enumerable)
                {
                    AddInternal(multiIndex, c, skipParentCheck);
                    multiIndex++;
                }
            }
            else
            {
                JToken item = CreateFromContent(content);

                InsertItem(index, item, skipParentCheck);
            }
        }

        internal static JToken CreateFromContent(object content)
        {
            JToken token = content as JToken;
            if (token != null)
            {
                return token;
            }

            return new JValue(content);
        }

        /// <summary>
        /// Creates a <see cref="JsonWriter"/> that can be used to add tokens to the <see cref="JToken"/>.
        /// </summary>
        /// <returns>A <see cref="JsonWriter"/> that is ready to have content written to it.</returns>
        public JsonWriter CreateWriter()
        {
            return new JTokenWriter(this);
        }

        /// <summary>
        /// Replaces the child nodes of this token with the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        public void ReplaceAll(object content)
        {
            ClearItems();
            Add(content);
        }

        /// <summary>
        /// Removes the child nodes from this token.
        /// </summary>
        public void RemoveAll()
        {
            ClearItems();
        }

        internal abstract void MergeItem(object content, JsonMergeSettings settings);

        /// <summary>
        /// Merge the specified content into this <see cref="JToken"/>.
        /// </summary>
        /// <param name="content">The content to be merged.</param>
        public void Merge(object content)
        {
            MergeItem(content, new JsonMergeSettings());
        }

        /// <summary>
        /// Merge the specified content into this <see cref="JToken"/> using <see cref="JsonMergeSettings"/>.
        /// </summary>
        /// <param name="content">The content to be merged.</param>
        /// <param name="settings">The <see cref="JsonMergeSettings"/> used to merge the content.</param>
        public void Merge(object content, JsonMergeSettings settings)
        {
            MergeItem(content, settings);
        }

        internal void ReadTokenFrom(JsonReader reader, JsonLoadSettings options)
        {
            int startDepth = reader.Depth;

            if (!reader.Read())
            {
                throw JsonReaderException.Create(reader, "Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }

            ReadContentFrom(reader, options);

            int endDepth = reader.Depth;

            if (endDepth > startDepth)
            {
                throw JsonReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }
        }

        internal void ReadContentFrom(JsonReader r, JsonLoadSettings settings)
        {
            ValidationUtils.ArgumentNotNull(r, nameof(r));
            IJsonLineInfo lineInfo = r as IJsonLineInfo;

            JContainer parent = this;

            do
            {
                if ((parent as JProperty)?.Value != null)
                {
                    if (parent == this)
                    {
                        return;
                    }

                    parent = parent.Parent;
                }

                switch (r.TokenType)
                {
                    case JsonToken.None:
                        // new reader. move to actual content
                        break;
                    case JsonToken.StartArray:
                        JArray a = new JArray();
                        a.SetLineInfo(lineInfo, settings);
                        parent.Add(a);
                        parent = a;
                        break;

                    case JsonToken.EndArray:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartObject:
                        JObject o = new JObject();
                        o.SetLineInfo(lineInfo, settings);
                        parent.Add(o);
                        parent = o;
                        break;
                    case JsonToken.EndObject:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartConstructor:
                        JConstructor constructor = new JConstructor(r.Value.ToString());
                        constructor.SetLineInfo(lineInfo, settings);
                        parent.Add(constructor);
                        parent = constructor;
                        break;
                    case JsonToken.EndConstructor:
                        if (parent == this)
                        {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Date:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                        JValue v = new JValue(r.Value);
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Comment:
                        if (settings != null && settings.CommentHandling == CommentHandling.Load)
                        {
                            v = JValue.CreateComment(r.Value.ToString());
                            v.SetLineInfo(lineInfo, settings);
                            parent.Add(v);
                        }
                        break;
                    case JsonToken.Null:
                        v = JValue.CreateNull();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Undefined:
                        v = JValue.CreateUndefined();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.PropertyName:
                        string propertyName = r.Value.ToString();
                        JProperty property = new JProperty(propertyName);
                        property.SetLineInfo(lineInfo, settings);
                        JObject parentObject = (JObject)parent;
                        // handle multiple properties with the same name in JSON
                        JProperty existingPropertyWithName = parentObject.Property(propertyName);
                        if (existingPropertyWithName == null)
                        {
                            parent.Add(property);
                        }
                        else
                        {
                            existingPropertyWithName.Replace(property);
                        }
                        parent = property;
                        break;
                    default:
                        throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
                }
            } while (r.Read());
        }

        internal int ContentsHashCode()
        {
            int hashCode = 0;
            foreach (JToken item in ChildrenTokens)
            {
                hashCode ^= item.GetDeepHashCode();
            }
            return hashCode;
        }

#if !(DOTNET || PORTABLE40 || PORTABLE)
        string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
        {
            return string.Empty;
        }

        PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
        {
            ICustomTypeDescriptor d = First as ICustomTypeDescriptor;
            return d?.GetProperties();
        }
#endif

        #region IList<JToken> Members
        int IList<JToken>.IndexOf(JToken item)
        {
            return IndexOfItem(item);
        }

        void IList<JToken>.Insert(int index, JToken item)
        {
            InsertItem(index, item, false);
        }

        void IList<JToken>.RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        JToken IList<JToken>.this[int index]
        {
            get { return GetItem(index); }
            set { SetItem(index, value); }
        }
        #endregion

        #region ICollection<JToken> Members
        void ICollection<JToken>.Add(JToken item)
        {
            Add(item);
        }

        void ICollection<JToken>.Clear()
        {
            ClearItems();
        }

        bool ICollection<JToken>.Contains(JToken item)
        {
            return ContainsItem(item);
        }

        void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
        {
            CopyItemsTo(array, arrayIndex);
        }

        bool ICollection<JToken>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<JToken>.Remove(JToken item)
        {
            return RemoveItem(item);
        }
        #endregion

        private JToken EnsureValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            JToken token = value as JToken;
            if (token != null)
            {
                return token;
            }

            throw new ArgumentException("Argument is not a JToken.");
        }

        #region IList Members
        int IList.Add(object value)
        {
            Add(EnsureValue(value));
            return Count - 1;
        }

        void IList.Clear()
        {
            ClearItems();
        }

        bool IList.Contains(object value)
        {
            return ContainsItem(EnsureValue(value));
        }

        int IList.IndexOf(object value)
        {
            return IndexOfItem(EnsureValue(value));
        }

        void IList.Insert(int index, object value)
        {
            InsertItem(index, EnsureValue(value), false);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            RemoveItem(EnsureValue(value));
        }

        void IList.RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        object IList.this[int index]
        {
            get { return GetItem(index); }
            set { SetItem(index, EnsureValue(value)); }
        }
        #endregion

        #region ICollection Members
        void ICollection.CopyTo(Array array, int index)
        {
            CopyItemsTo(array, index);
        }

        /// <summary>
        /// Gets the count of child JSON tokens.
        /// </summary>
        /// <value>The count of child JSON tokens.</value>
        public int Count
        {
            get { return ChildrenTokens.Count; }
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
        #endregion

        #region IBindingList Members
#if !(DOTNET || PORTABLE || PORTABLE40)
        void IBindingList.AddIndex(PropertyDescriptor property)
        {
        }

        object IBindingList.AddNew()
        {
            AddingNewEventArgs args = new AddingNewEventArgs();
            OnAddingNew(args);

            if (args.NewObject == null)
            {
                throw new JsonException("Could not determine new value to add to '{0}'.".FormatWith(CultureInfo.InvariantCulture, GetType()));
            }

            if (!(args.NewObject is JToken))
            {
                throw new JsonException("New item to be added to collection must be compatible with {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JToken)));
            }

            JToken newItem = (JToken)args.NewObject;
            Add(newItem);

            return newItem;
        }

        bool IBindingList.AllowEdit
        {
            get { return true; }
        }

        bool IBindingList.AllowNew
        {
            get { return true; }
        }

        bool IBindingList.AllowRemove
        {
            get { return true; }
        }

        void IBindingList.ApplySort(PropertyDescriptor property, ListSortDirection direction)
        {
            throw new NotSupportedException();
        }

        int IBindingList.Find(PropertyDescriptor property, object key)
        {
            throw new NotSupportedException();
        }

        bool IBindingList.IsSorted
        {
            get { return false; }
        }

        void IBindingList.RemoveIndex(PropertyDescriptor property)
        {
        }

        void IBindingList.RemoveSort()
        {
            throw new NotSupportedException();
        }

        ListSortDirection IBindingList.SortDirection
        {
            get { return ListSortDirection.Ascending; }
        }

        PropertyDescriptor IBindingList.SortProperty
        {
            get { return null; }
        }

        bool IBindingList.SupportsChangeNotification
        {
            get { return true; }
        }

        bool IBindingList.SupportsSearching
        {
            get { return false; }
        }

        bool IBindingList.SupportsSorting
        {
            get { return false; }
        }
#endif
        #endregion

        internal static void MergeEnumerableContent(JContainer target, IEnumerable content, JsonMergeSettings settings)
        {
            switch (settings.MergeArrayHandling)
            {
                case MergeArrayHandling.Concat:
                    foreach (JToken item in content)
                    {
                        target.Add(item);
                    }
                    break;
                case MergeArrayHandling.Union:
#if !NET20
                    HashSet<JToken> items = new HashSet<JToken>(target, EqualityComparer);

                    foreach (JToken item in content)
                    {
                        if (items.Add(item))
                        {
                            target.Add(item);
                        }
                    }
#else
                    Dictionary<JToken, bool> items = new Dictionary<JToken, bool>(EqualityComparer);
                    foreach (JToken t in target)
                    {
                        items[t] = true;
                    }

                    foreach (JToken item in content)
                    {
                        if (!items.ContainsKey(item))
                        {
                            items[item] = true;
                            target.Add(item);
                        }
                    }
#endif
                    break;
                case MergeArrayHandling.Replace:
                    target.ClearItems();
                    foreach (JToken item in content)
                    {
                        target.Add(item);
                    }
                    break;
                case MergeArrayHandling.Merge:
                    int i = 0;
                    foreach (object targetItem in content)
                    {
                        if (i < target.Count)
                        {
                            JToken sourceItem = target[i];

                            JContainer existingContainer = sourceItem as JContainer;
                            if (existingContainer != null)
                            {
                                existingContainer.Merge(targetItem, settings);
                            }
                            else
                            {
                                if (targetItem != null)
                                {
                                    JToken contentValue = CreateFromContent(targetItem);
                                    if (contentValue.Type != JTokenType.Null)
                                    {
                                        target[i] = contentValue;
                                    }
                                }
                            }
                        }
                        else
                        {
                            target.Add(targetItem);
                        }

                        i++;
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(settings), "Unexpected merge array handling when merging JSON.");
            }
        }
    }
}