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
using System.Globalization;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
    /// </summary>
    public class JTokenWriter : JsonWriter
    {
        private JContainer _token;
        private JContainer _parent;
        // used when writer is writing single value and the value has no containing parent
        private JValue _value;
        private JToken _current;

        /// <summary>
        /// Gets the <see cref="JToken"/> at the writer's current position.
        /// </summary>
        public JToken CurrentToken
        {
            get { return _current; }
        }

        /// <summary>
        /// Gets the token being writen.
        /// </summary>
        /// <value>The token being writen.</value>
        public JToken Token
        {
            get
            {
                if (_token != null)
                {
                    return _token;
                }

                return _value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JTokenWriter"/> class writing to the given <see cref="JContainer"/>.
        /// </summary>
        /// <param name="container">The container being written to.</param>
        public JTokenWriter(JContainer container)
        {
            ValidationUtils.ArgumentNotNull(container, nameof(container));

            _token = container;
            _parent = container;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JTokenWriter"/> class.
        /// </summary>
        public JTokenWriter()
        {
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
        }

        /// <summary>
        /// Closes this stream and the underlying stream.
        /// </summary>
        public override void Close()
        {
            base.Close();
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        public override void WriteStartObject()
        {
            base.WriteStartObject();

            AddParent(new JObject());
        }

        private void AddParent(JContainer container)
        {
            if (_parent == null)
            {
                _token = container;
            }
            else
            {
                _parent.AddAndSkipParentCheck(container);
            }

            _parent = container;
            _current = container;
        }

        private void RemoveParent()
        {
            _current = _parent;
            _parent = _parent.Parent;

            if (_parent != null && _parent.Type == JTokenType.Property)
            {
                _parent = _parent.Parent;
            }
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        public override void WriteStartArray()
        {
            base.WriteStartArray();

            AddParent(new JArray());
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public override void WriteStartConstructor(string name)
        {
            base.WriteStartConstructor(name);

            AddParent(new JConstructor(name));
        }

        /// <summary>
        /// Writes the end.
        /// </summary>
        /// <param name="token">The token.</param>
        protected override void WriteEnd(JsonToken token)
        {
            RemoveParent();
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            JObject o = _parent as JObject;
            if (o != null)
            {
                // avoid duplicate property name exception
                // last property name wins
                o.Remove(name);
            }

            AddParent(new JProperty(name));

            // don't set state until after in case of an error
            // incorrect state will cause issues if writer is disposed when closing open properties
            base.WritePropertyName(name);
        }

        private void AddValue(object value, JsonToken token)
        {
            AddValue(new JValue(value), token);
        }

        internal void AddValue(JValue value, JsonToken token)
        {
            if (_parent != null)
            {
                _parent.Add(value);
                _current = _parent.Last;

                if (_parent.Type == JTokenType.Property)
                {
                    _parent = _parent.Parent;
                }
            }
            else
            {
                _value = value ?? JValue.CreateNull();
                _current = _value;
            }
        }

        #region WriteValue methods
        /// <summary>
        /// Writes a <see cref="Object"/> value.
        /// An error will raised if the value cannot be written as a single JSON token.
        /// </summary>
        /// <param name="value">The <see cref="Object"/> value to write.</param>
        public override void WriteValue(object value)
        {
#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
            if (value is BigInteger)
            {
                InternalWriteValue(JsonToken.Integer);
                AddValue(value, JsonToken.Integer);
            }
            else
#endif
            {
                base.WriteValue(value);
            }
        }

        /// <summary>
        /// Writes a null value.
        /// </summary>
        public override void WriteNull()
        {
            base.WriteNull();
            AddValue(null, JsonToken.Null);
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public override void WriteUndefined()
        {
            base.WriteUndefined();
            AddValue(null, JsonToken.Undefined);
        }

        /// <summary>
        /// Writes raw JSON.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRaw(string json)
        {
            base.WriteRaw(json);
            AddValue(new JRaw(json), JsonToken.Raw);
        }

        /// <summary>
        /// Writes out a comment <code>/*...*/</code> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            base.WriteComment(text);
            AddValue(JValue.CreateComment(text), JsonToken.Comment);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Boolean);
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Char"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Char"/> value to write.</param>
        public override void WriteValue(char value)
        {
            base.WriteValue(value);
            string s = null;
#if !(DOTNET || PORTABLE40 || PORTABLE)
            s = value.ToString(CultureInfo.InvariantCulture);
#else
            s = value.ToString();
#endif
            AddValue(s, JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Float);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);
            AddValue(value, JsonToken.Date);
        }

#if !NET20
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Date);
        }
#endif

        /// <summary>
        /// Writes a <see cref="Byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/>[] value to write.</param>
        public override void WriteValue(byte[] value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.Bytes);
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);
            AddValue(value, JsonToken.String);
        }
        #endregion
    }
}