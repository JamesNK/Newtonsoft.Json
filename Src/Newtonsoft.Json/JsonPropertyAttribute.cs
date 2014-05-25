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
    /// Instructs the <see cref="JsonSerializer"/> to always serialize the member with the specified name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class JsonPropertyAttribute : Attribute
    {
        // yuck. can't set nullable properties on an attribute in C#
        // have to use this approach to get an unset default state
        internal NullValueHandling? _nullValueHandling;
        internal DefaultValueHandling? _defaultValueHandling;
        internal ReferenceLoopHandling? _referenceLoopHandling;
        internal ObjectCreationHandling? _objectCreationHandling;
        internal TypeNameHandling? _typeNameHandling;
        internal bool? _isReference;
        internal int? _order;
        internal Required? _required;
        internal bool? _itemIsReference;
        internal ReferenceLoopHandling? _itemReferenceLoopHandling;
        internal TypeNameHandling? _itemTypeNameHandling;

        /// <summary>
        /// Gets or sets the converter used when serializing the property's collection items.
        /// </summary>
        /// <value>The collection's items converter.</value>
        public Type ItemConverterType { get; set; }

        /// <summary>
        /// Gets or sets the null value handling used when serializing this property.
        /// </summary>
        /// <value>The null value handling.</value>
        public NullValueHandling NullValueHandling
        {
            get { return _nullValueHandling ?? default(NullValueHandling); }
            set { _nullValueHandling = value; }
        }

        /// <summary>
        /// Gets or sets the default value handling used when serializing this property.
        /// </summary>
        /// <value>The default value handling.</value>
        public DefaultValueHandling DefaultValueHandling
        {
            get { return _defaultValueHandling ?? default(DefaultValueHandling); }
            set { _defaultValueHandling = value; }
        }

        /// <summary>
        /// Gets or sets the reference loop handling used when serializing this property.
        /// </summary>
        /// <value>The reference loop handling.</value>
        public ReferenceLoopHandling ReferenceLoopHandling
        {
            get { return _referenceLoopHandling ?? default(ReferenceLoopHandling); }
            set { _referenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the object creation handling used when deserializing this property.
        /// </summary>
        /// <value>The object creation handling.</value>
        public ObjectCreationHandling ObjectCreationHandling
        {
            get { return _objectCreationHandling ?? default(ObjectCreationHandling); }
            set { _objectCreationHandling = value; }
        }

        /// <summary>
        /// Gets or sets the type name handling used when serializing this property.
        /// </summary>
        /// <value>The type name handling.</value>
        public TypeNameHandling TypeNameHandling
        {
            get { return _typeNameHandling ?? default(TypeNameHandling); }
            set { _typeNameHandling = value; }
        }

        /// <summary>
        /// Gets or sets whether this property's value is serialized as a reference.
        /// </summary>
        /// <value>Whether this property's value is serialized as a reference.</value>
        public bool IsReference
        {
            get { return _isReference ?? default(bool); }
            set { _isReference = value; }
        }

        /// <summary>
        /// Gets or sets the order of serialization and deserialization of a member.
        /// </summary>
        /// <value>The numeric order of serialization or deserialization.</value>
        public int Order
        {
            get { return _order ?? default(int); }
            set { _order = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this property is required.
        /// </summary>
        /// <value>
        /// 	A value indicating whether this property is required.
        /// </value>
        public Required Required
        {
            get { return _required ?? Required.Default; }
            set { _required = value; }
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; set; }

        /// <summary>
        /// Gets or sets the the reference loop handling used when serializing the property's collection items.
        /// </summary>
        /// <value>The collection's items reference loop handling.</value>
        public ReferenceLoopHandling ItemReferenceLoopHandling
        {
            get { return _itemReferenceLoopHandling ?? default(ReferenceLoopHandling); }
            set { _itemReferenceLoopHandling = value; }
        }

        /// <summary>
        /// Gets or sets the the type name handling used when serializing the property's collection items.
        /// </summary>
        /// <value>The collection's items type name handling.</value>
        public TypeNameHandling ItemTypeNameHandling
        {
            get { return _itemTypeNameHandling ?? default(TypeNameHandling); }
            set { _itemTypeNameHandling = value; }
        }

        /// <summary>
        /// Gets or sets whether this property's collection items are serialized as a reference.
        /// </summary>
        /// <value>Whether this property's collection items are serialized as a reference.</value>
        public bool ItemIsReference
        {
            get { return _itemIsReference ?? default(bool); }
            set { _itemIsReference = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class.
        /// </summary>
        public JsonPropertyAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class with the specified name.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        public JsonPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}