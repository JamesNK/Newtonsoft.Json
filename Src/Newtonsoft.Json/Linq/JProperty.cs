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
using System.Diagnostics;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a JSON property.
    /// </summary>
    public partial class JProperty : JContainer
    {
        #region JPropertyList
        private class JPropertyList : IList<JToken>
        {
            internal JToken? _token;

            public IEnumerator<JToken> GetEnumerator()
            {
                if (_token != null)
                {
                    yield return _token;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Add(JToken item)
            {
                _token = item;
            }

            public void Clear()
            {
                _token = null;
            }

            public bool Contains(JToken item)
            {
                return (_token == item);
            }

            public void CopyTo(JToken[] array, int arrayIndex)
            {
                if (_token != null)
                {
                    array[arrayIndex] = _token;
                }
            }

            public bool Remove(JToken item)
            {
                if (_token == item)
                {
                    _token = null;
                    return true;
                }
                return false;
            }

            public int Count => (_token != null) ? 1 : 0;

            public bool IsReadOnly => false;

            public int IndexOf(JToken item)
            {
                return (_token == item) ? 0 : -1;
            }

            public void Insert(int index, JToken item)
            {
                if (index == 0)
                {
                    _token = item;
                }
            }

            public void RemoveAt(int index)
            {
                if (index == 0)
                {
                    _token = null;
                }
            }

            public JToken this[int index]
            {
                get
                {
                    if (index != 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    MiscellaneousUtils.Assert(_token != null);
                    return _token;
                }
                set
                {
                    if (index != 0)
                    {
                        throw new IndexOutOfRangeException();
                    }

                    _token = value;
                }
            }
        }
        #endregion

        private readonly JPropertyList _content = new JPropertyList();
        private readonly string _name;

        /// <summary>
        /// Gets the container's children tokens.
        /// </summary>
        /// <value>The container's children tokens.</value>
        protected override IList<JToken> ChildrenTokens => _content;

        /// <summary>
        /// Gets the property name.
        /// </summary>
        /// <value>The property name.</value>
        public string Name
        {
            [DebuggerStepThrough]
            get { return _name; }
        }

        /// <summary>
        /// Gets or sets the property value.
        /// </summary>
        /// <value>The property value.</value>
        public JToken Value
        {
            [DebuggerStepThrough]
            get { return _content._token!; }
            set
            {
                CheckReentrancy();

                JToken newValue = value ?? JValue.CreateNull();

                if (_content._token == null)
                {
                    InsertItem(0, newValue, false);
                }
                else
                {
                    SetItem(0, newValue);
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JProperty"/> class from another <see cref="JProperty"/> object.
        /// </summary>
        /// <param name="other">A <see cref="JProperty"/> object to copy from.</param>
        public JProperty(JProperty other)
            : base(other)
        {
            _name = other.Name;
        }

        internal override JToken GetItem(int index)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            return Value;
        }

        internal override void SetItem(int index, JToken? item)
        {
            if (index != 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            if (IsTokenUnchanged(Value, item))
            {
                return;
            }

            ((JObject?)Parent)?.InternalPropertyChanging(this);

            base.SetItem(0, item);

            ((JObject?)Parent)?.InternalPropertyChanged(this);
        }

        internal override bool RemoveItem(JToken? item)
        {
            throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
        }

        internal override void RemoveItemAt(int index)
        {
            throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
        }

        internal override int IndexOfItem(JToken? item)
        {
            if (item == null)
            {
                return -1;
            }

            return _content.IndexOf(item);
        }

        internal override bool InsertItem(int index, JToken? item, bool skipParentCheck)
        {
            // don't add comments to JProperty
            if (item != null && item.Type == JTokenType.Comment)
            {
                return false;
            }

            if (Value != null)
            {
                throw new JsonException("{0} cannot have multiple values.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
            }

            return base.InsertItem(0, item, false);
        }

        internal override bool ContainsItem(JToken? item)
        {
            return (Value == item);
        }

        internal override void MergeItem(object content, JsonMergeSettings? settings)
        {
            JToken? value = (content as JProperty)?.Value;

            if (value != null && value.Type != JTokenType.Null)
            {
                Value = value;
            }
        }

        internal override void ClearItems()
        {
            throw new JsonException("Cannot add or remove items from {0}.".FormatWith(CultureInfo.InvariantCulture, typeof(JProperty)));
        }

        internal override bool DeepEquals(JToken node)
        {
            return (node is JProperty t && _name == t.Name && ContentsEqual(t));
        }

        internal override JToken CloneToken()
        {
            return new JProperty(this);
        }

        /// <summary>
        /// Gets the node type for this <see cref="JToken"/>.
        /// </summary>
        /// <value>The type.</value>
        public override JTokenType Type
        {
            [DebuggerStepThrough]
            get { return JTokenType.Property; }
        }

        internal JProperty(string name)
        {
            // called from JTokenWriter
            ValidationUtils.ArgumentNotNull(name, nameof(name));

            _name = name;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JProperty"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        public JProperty(string name, params object[] content)
            : this(name, (object)content)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JProperty"/> class.
        /// </summary>
        /// <param name="name">The property name.</param>
        /// <param name="content">The property content.</param>
        public JProperty(string name, object? content)
        {
            ValidationUtils.ArgumentNotNull(name, nameof(name));

            _name = name;

            Value = IsMultiContent(content)
                ? new JArray(content)
                : CreateFromContent(content);
        }

        /// <summary>
        /// Writes this token to a <see cref="JsonWriter"/>.
        /// </summary>
        /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
        /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
        public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
        {
            writer.WritePropertyName(_name);

            JToken value = Value;
            if (value != null)
            {
                value.WriteTo(writer, converters);
            }
            else
            {
                writer.WriteNull();
            }
        }

        internal override int GetDeepHashCode()
        {
            return _name.GetHashCode() ^ (Value?.GetDeepHashCode() ?? 0);
        }

        /// <summary>
        /// Loads a <see cref="JProperty"/> from a <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JProperty"/>.</param>
        /// <returns>A <see cref="JProperty"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public new static JProperty Load(JsonReader reader)
        {
            return Load(reader, null);
        }

        /// <summary>
        /// Loads a <see cref="JProperty"/> from a <see cref="JsonReader"/>.
        /// </summary>
        /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JProperty"/>.</param>
        /// <param name="settings">The <see cref="JsonLoadSettings"/> used to load the JSON.
        /// If this is <c>null</c>, default load settings will be used.</param>
        /// <returns>A <see cref="JProperty"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
        public new static JProperty Load(JsonReader reader, JsonLoadSettings? settings)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!reader.Read())
                {
                    throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
                }
            }

            reader.MoveToContent();

            if (reader.TokenType != JsonToken.PropertyName)
            {
                throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JProperty p = new JProperty((string)reader.Value!);
            p.SetLineInfo(reader as IJsonLineInfo, settings);

            p.ReadTokenFrom(reader, settings);

            return p;
        }
    }
}