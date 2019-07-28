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
using Newtonsoft.Json.Utilities;
using System.IO;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a JSON array.
    /// </summary>
    /// <example>
    ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParseArray" title="Parsing a JSON Array from Text" />
    /// </example>
    public partial class JArray : JContainer, IList<JToken>
    {
        private readonly List<JToken> _values = new List<JToken>();

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected override IList<JToken> ChildrenTokens => _values;

        /// <summary>
        /// Gets the node type for this <see cref="JToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override JTokenType Type => JTokenType.Array;

        /// <summary>
        /// Initializes a new instance of the <see cref="JArray"/> class.
        /// </summary>
        public JArray()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JArray"/> class from another <see cref="JArray"/> object.
        /// </summary>
        /// <param name="other">A <see cref="JArray"/> object to copy from.</param>
        public JArray(JArray other)
            : base(other)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JArray"/> class with the specified content.
        /// </summary>
        /// <param name="content">The contents of the array.</param>
        public JArray(params object[] content)
            : this((object)content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JArray"/> class with the specified content.
        /// </summary>
        /// <param name="content">The contents of the array.</param>
        public JArray(object content)
        {
            Add(content);
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JArray t && ContentsEqual(t));
        }

        internal override JToken CloneToken()
        {
            return new JArray(this);
        }

        /// <summary>
        /// Loads an <see cref="JArray"/> from a <see cref="JsonReader"/>. 
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public new static JArray Load(JsonReader reader)
        {
            return Load(reader, null);
        }

        /// <summary>
        /// Loads an <see cref="JArray"/> from a <see cref="JsonReader"/>. 
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JArray"/>.</param>
        /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="JArray"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public new static JArray Load(JsonReader reader, JsonLoadSettings? settings)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JArray a = new JArray();
            a.SetLineInfo(reader as IJsonLineInfo, settings);

            a.ReadTokenFrom(reader, settings);

            return a;
        }

        /// <summary>
        /// Load a <see cref="JArray"/> from a string that contains JSON.
        /// </summary>
        /// <param name="json">A <see cref="String"/> that contains JSON.</param>
        /// <returns>A <see cref="JArray"/> populated from the string that contains JSON.</returns>
        /// <example>
        ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParseArray" title="Parsing a JSON Array from Text" />
        /// </example>
        public new static JArray Parse(string json)
        {
            return Parse(json, null);
        }

        /// <summary>
        /// Load a <see cref="JArray"/> from a string that contains JSON.
        /// </summary>
        /// <param name="json">A <see cref="String"/> that contains JSON.</param>
        /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="JArray"/> populated from the string that contains JSON.</returns>
        /// <example>
        ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\LinqToJsonTests.cs" region="LinqToJsonCreateParseArray" title="Parsing a JSON Array from Text" />
        /// </example>
        public new static JArray Parse(string json, JsonLoadSettings? settings)
        {
            using (JsonReader reader = new JsonTextReader(new StringReader(json)))
            {
                JArray a = Load(reader, settings);

                while (reader.Read())
                {
                    // Any content encountered here other than a comment will throw in the reader.
                }

                return a;
            }
        }

        /// <summary>
        /// Creates a <see cref="JArray"/> from an object.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <returns>A <see cref="JArray"/> with the values of the specified object.</returns>
        public new static JArray FromObject(object o)
        {
            return FromObject(o, JsonSerializer.CreateDefault());
        }

        /// <summary>
        /// Creates a <see cref="JArray"/> from an object.
        /// </summary>
        /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
        /// <param name="jsonSerializer">The <see cref="JsonSerializer"/> that will be used to read the object.</param>
        /// <returns>A <see cref="JArray"/> with the values of the specified object.</returns>
        public new static JArray FromObject(object o, JsonSerializer jsonSerializer)
        {
            JToken token = FromObjectInternal(o, jsonSerializer);

            if (token.Type != JTokenType.Array)
            {
                throw new ArgumentException("Object serialized to {0}. JArray instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));
            }

            return (JArray)token;
        }

        /// <summary>
        /// Writes this token to a <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WriteStartArray();

            for (int i = 0; i < _values.Count; i++)
            {
                _values[i].WriteTo(writer, converters);
            }

            writer.WriteEndArray();
        }

        /// <summary>
        /// Gets the <see cref="JToken"/> with the specified key.
        /// </summary>
        /// <value>The <see cref="JToken"/> with the specified key.</value>
        public override JToken? this[object key]
        {
            get
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int))
                {
                    throw new ArgumentException("Accessed JArray values with invalid key value: {0}. Int32 array index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                return GetItem((int)key);
            }
            set
            {
                ValidationUtils.ArgumentNotNull(key, nameof(key));

                if (!(key is int))
                {
                    throw new ArgumentException("Set JArray values with invalid key value: {0}. Int32 array index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));
                }

                SetItem((int)key, value);
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Newtonsoft.Json.Linq.JToken"/> at the specified index.
        /// </summary>
        /// <value></value>
        public JToken this[int index]
        {
            get => GetItem(index);
            set => SetItem(index, value);
        }

        internal override int IndexOfItem(JToken? item)
        {
            if (item == null)
            {
                return -1;
            }

            return _values.IndexOfReference(item);
        }

        internal override void MergeItem(object content, JsonMergeSettings? settings)
        {
            IEnumerable? a = (IsMultiContent(content) || content is JArray)
                ? (IEnumerable)content
                : null;
            if (a == null)
            {
                return;
            }

            MergeEnumerableContent(this, a, settings);
        }

        #region IList<JToken> Members
        /// <summary>
        /// Determines the index of a specific item in the <see cref="JArray"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="JArray"/>.</param>
        /// <returns>
        /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(JToken item)
        {
            return IndexOfItem(item);
        }

        /// <summary>
        /// Inserts an item to the <see cref="JArray"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The object to insert into the <see cref="JArray"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="JArray"/>.
        /// </exception>
        public void Insert(int index, JToken item)
        {
            InsertItem(index, item, false);
        }

        /// <summary>
        /// Removes the <see cref="JArray"/> item at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the item to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="JArray"/>.
        /// </exception>
        public void RemoveAt(int index)
        {
            RemoveItemAt(index);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> of <see cref="JToken"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<JToken> GetEnumerator()
        {
            return Children().GetEnumerator();
        }
        #endregion

        #region ICollection<JToken> Members
        /// <summary>
        /// Adds an item to the <see cref="JArray"/>.
        /// </summary>
        /// <param name="item">The object to add to the <see cref="JArray"/>.</param>
        public void Add(JToken item)
        {
            Add((object)item);
        }

        /// <summary>
        /// Removes all items from the <see cref="JArray"/>.
        /// </summary>
        public void Clear()
        {
            ClearItems();
        }

        /// <summary>
        /// Determines whether the <see cref="JArray"/> contains a specific value.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="JArray"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> is found in the <see cref="JArray"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(JToken item)
        {
            return ContainsItem(item);
        }

        /// <summary>
        /// Copies the elements of the <see cref="JArray"/> to an array, starting at a particular array index.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="arrayIndex">Index of the array.</param>
        public void CopyTo(JToken[] array, int arrayIndex)
        {
            CopyItemsTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="JArray"/> is read-only.
        /// </summary>
        /// <returns><c>true</c> if the <see cref="JArray"/> is read-only; otherwise, <c>false</c>.</returns>
        public bool IsReadOnly => false;

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="JArray"/>.
        /// </summary>
        /// <param name="item">The object to remove from the <see cref="JArray"/>.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="item"/> was successfully removed from the <see cref="JArray"/>; otherwise, <c>false</c>. This method also returns <c>false</c> if <paramref name="item"/> is not found in the original <see cref="JArray"/>.
        /// </returns>
        public bool Remove(JToken item)
        {
            return RemoveItem(item);
        }
        #endregion

        internal override int GetDeepHashCode()
        {
            return ContentsHashCode();
        }
    }
}