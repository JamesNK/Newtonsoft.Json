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
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;
#if !SILVERLIGHT
using Newtonsoft.Json.Linq.ComponentModel;
#endif

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a token that can contain other tokens.
  /// </summary>
  public abstract class JContainer : JToken, IList<JToken>
#if !SILVERLIGHT
    , ITypedList, IBindingList
#else
    , IList
#endif
  {
#if !SILVERLIGHT
    /// <summary>
    /// Occurs when the list changes or an item in the list changes.
    /// </summary>
    public event ListChangedEventHandler ListChanged;

    /// <summary>
    /// Occurs before an item is added to the collection.
    /// </summary>
    public event AddingNewEventHandler AddingNew;
#endif

    private JToken _content;
    private object _syncRoot;
    private bool _busy;

    internal JToken Content
    {
      get { return _content; }
      set { _content = value; }
    }

    internal JContainer()
    {
    }

    internal JContainer(JContainer other)
    {
      ValidationUtils.ArgumentNotNull(other, "c");

      JToken content = other.Last;
      if (content != null)
      {
        do
        {
          content = content._next;
          Add(content.CloneToken());
        }
        while (content != other.Last);
      }
    }

    internal void CheckReentrancy()
    {
      if (_busy)
        throw new InvalidOperationException("ObservableCollection_CannotChangeObservableCollection");
    }

 #if !SILVERLIGHT
   protected virtual void OnAddingNew(AddingNewEventArgs e)
    {
      AddingNewEventHandler handler = AddingNew;
      if (handler != null)
        handler(this, e);
    }

    protected virtual void OnListChanged(ListChangedEventArgs e)
    {
      ListChangedEventHandler handler = ListChanged;

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
    /// Gets a value indicating whether this token has childen tokens.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
    /// </value>
    public override bool HasValues
    {
      get { return (_content != null); }
    }

    internal bool ContentsEqual(JContainer container)
    {
      JToken t1 = First;
      JToken t2 = container.First;

      if (t1 == t2)
        return true;

      do
      {
        if (t1 == null && t2 == null)
          return true;

        if (t1 != null && t2 != null && t1.DeepEquals(t2))
        {
          t1 = (t1 != Last) ? t1.Next : null;
          t2 = (t2 != container.Last) ? t2.Next : null;
        }
        else
        {
          return false;
        }
      }
      while (true);
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
        if (Last == null)
          return null;

        return Last._next;
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
      [DebuggerStepThrough]
      get { return _content; }
    }

    /// <summary>
    /// Returns a collection of the child tokens of this token, in document order.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.
    /// </returns>
    public override JEnumerable<JToken> Children()
    {
      return new JEnumerable<JToken>(ChildrenInternal());
    }

    private IEnumerable<JToken> ChildrenInternal()
    {
      JToken first = First;
      JToken current = first;
      if (current == null)
        yield break;

      do
      {
        yield return current;
      }
      while (current != null
        && ((current = current.Next) != null));
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
      return Children().Convert<JToken, T>();
    }

    /// <summary>
    /// Returns a collection of the descendant tokens for this token in document order.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{JToken}"/> containing the descendant tokens of the <see cref="JToken"/>.</returns>
    public IEnumerable<JToken> Descendants()
    {
      foreach (JToken o in Children())
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
      return (content is IEnumerable && !(content is string) && !(content is JToken));
    }

    internal virtual void AddItem(bool isLast, JToken previous, JToken item)
    {
      CheckReentrancy();

      ValidateToken(item, null);

      item = EnsureParentToken(item);

      JToken next = (previous != null) ? previous._next : item;

      item.Parent = this;
      item.Next = next;

      if (previous != null)
        previous.Next = item;

      if (isLast || previous == null)
        _content = item;

#if !SILVERLIGHT
      if (ListChanged != null)
        OnListChanged(new ListChangedEventArgs(ListChangedType.ItemAdded, IndexOfItem(item)));
#endif
    }

    internal JToken EnsureParentToken(JToken item)
    {
      if (item.Parent != null)
      {
        item = item.CloneToken();
      }
      else
      {
        // check whether attempting to add a token to itself
        JContainer parent = this;
        while (parent.Parent != null)
        {
          parent = parent.Parent;
        }
        if (item == parent)
        {
          item = item.CloneToken();
        }
      }
      return item;
    }

    internal void AddInternal(bool isLast, JToken previous, object content)
    {
      if (IsMultiContent(content))
      {
        IEnumerable enumerable = (IEnumerable) content;

        JToken multiPrevious = previous;
        foreach (object c in enumerable)
        {
          AddInternal(isLast, multiPrevious, c);
          multiPrevious = (multiPrevious != null) ? multiPrevious._next : Last;
        }
      }
      else
      {
        JToken item = CreateFromContent(content);

        AddItem(isLast, previous, item);
      }
    }

    internal int IndexOfItem(JToken item)
    {
      int index = 0;
      foreach (JToken token in Children())
      {
        if (token == item)
          return index;

        index++;
      }

      return -1;
    }

    internal virtual void InsertItem(int index, JToken item)
    {
      if (index == 0)
      {
        AddFirst(item);
      }
      else
      {
        JToken token = GetItem(index);
        AddInternal(false, token.Previous, item);
      }
    }

    internal virtual void RemoveItemAt(int index)
    {
      if (index < 0)
        throw new ArgumentOutOfRangeException("index", "index is less than 0.");

      CheckReentrancy();

      int currentIndex = 0;
      foreach (JToken token in Children())
      {
        if (index == currentIndex)
        {
          token.Remove();

#if !SILVERLIGHT
          OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, index));
#endif

          return;
        }

        currentIndex++;
      }

      throw new ArgumentOutOfRangeException("index", "index is equal to or greater than Count.");
    }

    internal virtual bool RemoveItem(JToken item)
    {
      if (item == null || item.Parent != this)
        return false;

      CheckReentrancy();

      JToken content = _content;

      int itemIndex = 0;
      while (content._next != item)
      {
        itemIndex++;
        content = content._next;
      }
      if (content == item)
      {
        // token is containers last child
        _content = null;
      }
      else
      {
        if (_content == item)
        {
          _content = content;
        }
        content._next = item._next;
      }
      item.Parent = null;
      item.Next = null;

#if !SILVERLIGHT
      OnListChanged(new ListChangedEventArgs(ListChangedType.ItemDeleted, itemIndex));
#endif

      return true;
    }

    internal virtual JToken GetItem(int index)
    {
      return Children().ElementAt(index);
    }

    internal virtual void SetItem(int index, JToken item)
    {
      CheckReentrancy();

      JToken token = GetItem(index);
      token.Replace(item);

#if !SILVERLIGHT
      OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, index));
#endif
    }

    internal virtual void ClearItems()
    {
      CheckReentrancy();

      while (_content != null)
      {
        JToken o = _content;

        JToken next = o._next;
        if (o != _content || next != o._next)
          throw new InvalidOperationException("This operation was corrupted by external code.");

        if (next != o)
          o._next = next._next;
        else
          _content = null;

        next.Parent = null;
        next._next = null;
      }

#if !SILVERLIGHT
      OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
#endif
    }

    internal virtual void ReplaceItem(JToken existing, JToken replacement)
    {
      if (existing == null || existing.Parent != this)
        return;

      if (IsTokenUnchanged(existing, replacement))
        return;

      CheckReentrancy();

      replacement = EnsureParentToken(replacement);

      ValidateToken(replacement, existing);

      JToken content = _content;

      int itemIndex = 0;
      while (content._next != existing)
      {
        itemIndex++;
        content = content._next;
      }

      if (content == existing)
      {
        // token is containers last child
        _content = replacement;
        replacement._next = replacement;
      }
      else
      {
        if (_content == existing)
        {
          _content = replacement;
        }
        content._next = replacement;
        replacement._next = existing._next;
      }

      replacement.Parent = this;

      existing.Parent = null;
      existing.Next = null;

#if !SILVERLIGHT
      OnListChanged(new ListChangedEventArgs(ListChangedType.ItemChanged, itemIndex));
#endif
    }

    internal virtual bool ContainsItem(JToken item)
    {
      return (IndexOfItem(item) != -1);
    }

    internal virtual void CopyItemsTo(Array array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
      if (arrayIndex >= array.Length)
        throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
      if (CountItems() > array.Length - arrayIndex)
        throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");

      int index = 0;
      foreach (JToken token in Children())
      {
        array.SetValue(token, arrayIndex + index);
        index++;
      }
    }

    internal virtual int CountItems()
    {
      return Children().Count();
    }

    internal static bool IsTokenUnchanged(JToken currentValue, JToken newValue)
    {
      JValue v1 = currentValue as JValue;
      if (v1 != null)
      {
        // null will get turned into a JValue of type null
        if (v1.Type == JTokenType.Null && newValue == null)
          return true;

        return v1.Equals(newValue);
      }

      return false;
    }

    internal virtual void ValidateToken(JToken o, JToken existing)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type == JTokenType.Property)
        throw new ArgumentException("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
    }

    /// <summary>
    /// Adds the specified content as children of this <see cref="JToken"/>.
    /// </summary>
    /// <param name="content">The content to be added.</param>
    public void Add(object content)
    {
      AddInternal(true, Last, content);
    }

    /// <summary>
    /// Adds the specified content as the first children of this <see cref="JToken"/>.
    /// </summary>
    /// <param name="content">The content to be added.</param>
    public void AddFirst(object content)
    {
      AddInternal(false, Last, content);
    }

    internal JToken CreateFromContent(object content)
    {
      if (content is JToken)
        return (JToken)content;
      
      return new JValue(content);
    }

    /// <summary>
    /// Creates an <see cref="JsonWriter"/> that can be used to add tokens to the <see cref="JToken"/>.
    /// </summary>
    /// <returns>An <see cref="JsonWriter"/> that is ready to have content written to it.</returns>
    public JsonWriter CreateWriter()
    {
      return new JTokenWriter(this);
    }

    /// <summary>
    /// Replaces the children nodes of this token with the specified content.
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

    internal abstract void ValidateObject(JToken o, JToken previous);

    internal void AddObjectSkipNotify(JToken o)
    {
      ValidateObject(o, this);

      Add(o);
    }

    internal void ReadContentFrom(JsonReader r)
    {
      ValidationUtils.ArgumentNotNull(r, "r");
      IJsonLineInfo lineInfo = r as IJsonLineInfo;

      JContainer parent = this;

      do
      {
        if (parent is JProperty && ((JProperty)parent).Value != null)
        {
          if (parent == this)
            return;

          parent = parent.Parent;
        }

        switch (r.TokenType)
        {
          case JsonToken.None:
            // new reader. move to actual content
            break;
          case JsonToken.StartArray:
            JArray a = new JArray();
            a.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(a);
            parent = a;
            break;

          case JsonToken.EndArray:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.StartObject:
            JObject o = new JObject();
            o.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(o);
            parent = o;
            break;
          case JsonToken.EndObject:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.StartConstructor:
            JConstructor constructor = new JConstructor(r.Value.ToString());
            constructor.SetLineInfo(constructor);
            parent.AddObjectSkipNotify(constructor);
            parent = constructor;
            break;
          case JsonToken.EndConstructor:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.String:
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.Date:
          case JsonToken.Boolean:
            JValue v = new JValue(r.Value);
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.Comment:
            v = JValue.CreateComment(r.Value.ToString());
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.Null:
            v = new JValue(null, JTokenType.Null);
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.Undefined:
            v = new JValue(null, JTokenType.Undefined);
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.PropertyName:
            JProperty property = new JProperty(r.Value.ToString());
            property.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(property);
            parent = property;
            break;
          default:
            throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
        }
      }
      while (r.Read());
    }

    internal int ContentsHashCode()
    {
      int hashCode = 0;
      foreach (JToken item in Children())
      {
        hashCode ^= item.GetDeepHashCode();
      }
      return hashCode;
    }

#if !SILVERLIGHT
    string ITypedList.GetListName(PropertyDescriptor[] listAccessors)
    {
      return string.Empty;
    }

    PropertyDescriptorCollection ITypedList.GetItemProperties(PropertyDescriptor[] listAccessors)
    {
      JObject o = First as JObject;
      if (o != null)
      {
        // explicitly use constructor because compact framework has no provider
        JTypeDescriptor descriptor = new JTypeDescriptor(o);
        return descriptor.GetProperties();
      }

      return null;
    }
#endif

    #region IList<JToken> Members

    int IList<JToken>.IndexOf(JToken item)
    {
      return IndexOfItem(item);
    }

    void IList<JToken>.Insert(int index, JToken item)
    {
      InsertItem(index, item);
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

    int ICollection<JToken>.Count
    {
      get { return CountItems(); }
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
        return null;

      if (value is JToken)
        return (JToken) value;

      throw new ArgumentException("Argument is not a JToken.");
    }

    #region IList Members

    int IList.Add(object value)
    {
      Add(EnsureValue(value));
      return CountItems() - 1;
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
      InsertItem(index, EnsureValue(value));
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

    int ICollection.Count
    {
      get { return CountItems(); }
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
          Interlocked.CompareExchange(ref _syncRoot, new object(), null);

        return _syncRoot;
      }

    }

    #endregion

    #region IBindingList Members

#if !SILVERLIGHT
    void IBindingList.AddIndex(PropertyDescriptor property)
    {
    }

    object IBindingList.AddNew()
    {
      AddingNewEventArgs args = new AddingNewEventArgs();
      OnAddingNew(args);

      if (args.NewObject == null)
        throw new Exception("Could not determine new value to add to '{0}'.".FormatWith(CultureInfo.InvariantCulture, GetType()));

      if (!(args.NewObject is JToken))
        throw new Exception("New item to be added to collection must be compatible with {0}.".FormatWith(CultureInfo.InvariantCulture, typeof (JToken)));

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
  }
}