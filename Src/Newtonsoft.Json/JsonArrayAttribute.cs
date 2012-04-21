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

namespace Newtonsoft.Json
{
  /// <summary>
  /// Instructs the <see cref="JsonSerializer"/> how to serialize the collection.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
  public sealed class JsonArrayAttribute : JsonContainerAttribute
  {
    private bool _allowNullItems;

    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal NullValueHandling? _itemNullValueHandling;
    internal DefaultValueHandling? _itemDefaultValueHandling;
    internal ReferenceLoopHandling? _itemReferenceLoopHandling;
    internal ObjectCreationHandling? _itemObjectCreationHandling;
    internal TypeNameHandling? _itemTypeNameHandling;

    /// <summary>
    /// Gets or sets a value indicating whether null items are allowed in the collection.
    /// </summary>
    /// <value><c>true</c> if null items are allowed in the collection; otherwise, <c>false</c>.</value>
    public bool AllowNullItems
    {
      get { return _allowNullItems; }
      set { _allowNullItems = value; }
    }

    /// <summary>
    /// Gets or sets the null value handling used when serializing the collection's items.
    /// </summary>
    /// <value>The null value handling.</value>
    public NullValueHandling ItemNullValueHandling
    {
      get { return _itemNullValueHandling ?? default(NullValueHandling); }
      set { _itemNullValueHandling = value; }
    }

    /// <summary>
    /// Gets or sets the default value handling used when serializing the collection's items.
    /// </summary>
    /// <value>The default value handling.</value>
    public DefaultValueHandling ItemDefaultValueHandling
    {
      get { return _itemDefaultValueHandling ?? default(DefaultValueHandling); }
      set { _itemDefaultValueHandling = value; }
    }

    /// <summary>
    /// Gets or sets the reference loop handling used when serializing the collection's items.
    /// </summary>
    /// <value>The reference loop handling.</value>
    public ReferenceLoopHandling ItemReferenceLoopHandling
    {
      get { return _itemReferenceLoopHandling ?? default(ReferenceLoopHandling); }
      set { _itemReferenceLoopHandling = value; }
    }

    /// <summary>
    /// Gets or sets the object creation handling used when deserializing the collection's items.
    /// </summary>
    /// <value>The object creation handling.</value>
    public ObjectCreationHandling ItemObjectCreationHandling
    {
      get { return _itemObjectCreationHandling ?? default(ObjectCreationHandling); }
      set { _itemObjectCreationHandling = value; }
    }

    /// <summary>
    /// Gets or sets the type name handling used when serializing the collection's items.
    /// </summary>
    /// <value>The type name handling.</value>
    public TypeNameHandling ItemTypeNameHandling
    {
      get { return _itemTypeNameHandling ?? default(TypeNameHandling); }
      set { _itemTypeNameHandling = value; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonArrayAttribute"/> class.
    /// </summary>
    public JsonArrayAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonObjectAttribute"/> class with a flag indicating whether the array can contain null items
    /// </summary>
    /// <param name="allowNullItems">A flag indicating whether the array can contain null items.</param>
    public JsonArrayAttribute(bool allowNullItems)
    {
      _allowNullItems = allowNullItems;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonArrayAttribute"/> class with the specified container Id.
    /// </summary>
    /// <param name="id">The container Id.</param>
    public JsonArrayAttribute(string id)
      : base(id)
    {
    }
  }
}