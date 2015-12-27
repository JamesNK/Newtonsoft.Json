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
using System.IO;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace Newtonsoft.Json.Bson
{
    /// <summary>
    /// Represents a writer that provides a fast, non-cached, forward-only way of generating JSON data.
    /// </summary>
    public class BsonWriter : JsonWriter
    {
        private readonly BsonBinaryWriter _writer;

        private BsonToken _root;
        private BsonToken _parent;
        private string _propertyName;

        /// <summary>
        /// Gets or sets the <see cref="DateTimeKind" /> used when writing <see cref="DateTime"/> values to BSON.
        /// When set to <see cref="DateTimeKind.Unspecified" /> no conversion will occur.
        /// </summary>
        /// <value>The <see cref="DateTimeKind" /> used when writing <see cref="DateTime"/> values to BSON.</value>
        public DateTimeKind DateTimeKindHandling
        {
            get { return _writer.DateTimeKindHandling; }
            set { _writer.DateTimeKindHandling = value; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonWriter"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        public BsonWriter(Stream stream)
        {
            ValidationUtils.ArgumentNotNull(stream, nameof(stream));
            _writer = new BsonBinaryWriter(new BinaryWriter(stream));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonWriter"/> class.
        /// </summary>
        /// <param name="writer">The writer.</param>
        public BsonWriter(BinaryWriter writer)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            _writer = new BsonBinaryWriter(writer);
        }

        /// <summary>
        /// Flushes whatever is in the buffer to the underlying streams and also flushes the underlying stream.
        /// </summary>
        public override void Flush()
        {
            _writer.Flush();
        }

        /// <summary>
        /// Writes the end.
        /// </summary>
        /// <param name="token">The token.</param>
        protected override void WriteEnd(JsonToken token)
        {
            base.WriteEnd(token);
            RemoveParent();

            if (Top == 0)
            {
                _writer.WriteToken(_root);
            }
        }

        /// <summary>
        /// Writes out a comment <code>/*...*/</code> containing the specified text.
        /// </summary>
        /// <param name="text">Text to place inside the comment.</param>
        public override void WriteComment(string text)
        {
            throw JsonWriterException.Create(this, "Cannot write JSON comment as BSON.", null);
        }

        /// <summary>
        /// Writes the start of a constructor with the given name.
        /// </summary>
        /// <param name="name">The name of the constructor.</param>
        public override void WriteStartConstructor(string name)
        {
            throw JsonWriterException.Create(this, "Cannot write JSON constructor as BSON.", null);
        }

        /// <summary>
        /// Writes raw JSON.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRaw(string json)
        {
            throw JsonWriterException.Create(this, "Cannot write raw JSON as BSON.", null);
        }

        /// <summary>
        /// Writes raw JSON where a value is expected and updates the writer's state.
        /// </summary>
        /// <param name="json">The raw JSON to write.</param>
        public override void WriteRawValue(string json)
        {
            throw JsonWriterException.Create(this, "Cannot write raw JSON as BSON.", null);
        }

        /// <summary>
        /// Writes the beginning of a JSON array.
        /// </summary>
        public override void WriteStartArray()
        {
            base.WriteStartArray();

            AddParent(new BsonArray());
        }

        /// <summary>
        /// Writes the beginning of a JSON object.
        /// </summary>
        public override void WriteStartObject()
        {
            base.WriteStartObject();

            AddParent(new BsonObject());
        }

        /// <summary>
        /// Writes the property name of a name/value pair on a JSON object.
        /// </summary>
        /// <param name="name">The name of the property.</param>
        public override void WritePropertyName(string name)
        {
            base.WritePropertyName(name);

            _propertyName = name;
        }

        /// <summary>
        /// Closes this stream and the underlying stream.
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (CloseOutput && _writer != null)
            {
                _writer.Close();
            }
        }

        private void AddParent(BsonToken container)
        {
            AddToken(container);
            _parent = container;
        }

        private void RemoveParent()
        {
            _parent = _parent.Parent;
        }

        private void AddValue(object value, BsonType type)
        {
            AddToken(new BsonValue(value, type));
        }

        internal void AddToken(BsonToken token)
        {
            if (_parent != null)
            {
                if (_parent is BsonObject)
                {
                    ((BsonObject)_parent).Add(_propertyName, token);
                    _propertyName = null;
                }
                else
                {
                    ((BsonArray)_parent).Add(token);
                }
            }
            else
            {
                if (token.Type != BsonType.Object && token.Type != BsonType.Array)
                {
                    throw JsonWriterException.Create(this, "Error writing {0} value. BSON must start with an Object or Array.".FormatWith(CultureInfo.InvariantCulture, token.Type), null);
                }

                _parent = token;
                _root = token;
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
                AddToken(new BsonBinary(((BigInteger)value).ToByteArray(), BsonBinaryType.Binary));
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
            AddValue(null, BsonType.Null);
        }

        /// <summary>
        /// Writes an undefined value.
        /// </summary>
        public override void WriteUndefined()
        {
            base.WriteUndefined();
            AddValue(null, BsonType.Undefined);
        }

        /// <summary>
        /// Writes a <see cref="String"/> value.
        /// </summary>
        /// <param name="value">The <see cref="String"/> value to write.</param>
        public override void WriteValue(string value)
        {
            base.WriteValue(value);
            if (value == null)
            {
                AddValue(null, BsonType.Null);
            }
            else
            {
                AddToken(new BsonString(value, true));
            }
        }

        /// <summary>
        /// Writes a <see cref="Int32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int32"/> value to write.</param>
        public override void WriteValue(int value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt32"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt32"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(uint value)
        {
            if (value > int.MaxValue)
            {
                throw JsonWriterException.Create(this, "Value is too large to fit in a signed 32 bit integer. BSON does not support unsigned values.", null);
            }

            base.WriteValue(value);
            AddValue(value, BsonType.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Int64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int64"/> value to write.</param>
        public override void WriteValue(long value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Long);
        }

        /// <summary>
        /// Writes a <see cref="UInt64"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt64"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value)
        {
            if (value > long.MaxValue)
            {
                throw JsonWriterException.Create(this, "Value is too large to fit in a signed 64 bit integer. BSON does not support unsigned values.", null);
            }

            base.WriteValue(value);
            AddValue(value, BsonType.Long);
        }

        /// <summary>
        /// Writes a <see cref="Single"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Single"/> value to write.</param>
        public override void WriteValue(float value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Number);
        }

        /// <summary>
        /// Writes a <see cref="Double"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Double"/> value to write.</param>
        public override void WriteValue(double value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Number);
        }

        /// <summary>
        /// Writes a <see cref="Boolean"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Boolean"/> value to write.</param>
        public override void WriteValue(bool value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Boolean);
        }

        /// <summary>
        /// Writes a <see cref="Int16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Int16"/> value to write.</param>
        public override void WriteValue(short value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Integer);
        }

        /// <summary>
        /// Writes a <see cref="UInt16"/> value.
        /// </summary>
        /// <param name="value">The <see cref="UInt16"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Integer);
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
            AddToken(new BsonString(s, true));
        }

        /// <summary>
        /// Writes a <see cref="Byte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/> value to write.</param>
        public override void WriteValue(byte value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Integer);
        }

        /// <summary>
        /// Writes a <see cref="SByte"/> value.
        /// </summary>
        /// <param name="value">The <see cref="SByte"/> value to write.</param>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Integer);
        }

        /// <summary>
        /// Writes a <see cref="Decimal"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Decimal"/> value to write.</param>
        public override void WriteValue(decimal value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Number);
        }

        /// <summary>
        /// Writes a <see cref="DateTime"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTime"/> value to write.</param>
        public override void WriteValue(DateTime value)
        {
            base.WriteValue(value);
            value = DateTimeUtils.EnsureDateTime(value, DateTimeZoneHandling);
            AddValue(value, BsonType.Date);
        }

#if !NET20
        /// <summary>
        /// Writes a <see cref="DateTimeOffset"/> value.
        /// </summary>
        /// <param name="value">The <see cref="DateTimeOffset"/> value to write.</param>
        public override void WriteValue(DateTimeOffset value)
        {
            base.WriteValue(value);
            AddValue(value, BsonType.Date);
        }
#endif

        /// <summary>
        /// Writes a <see cref="Byte"/>[] value.
        /// </summary>
        /// <param name="value">The <see cref="Byte"/>[] value to write.</param>
        public override void WriteValue(byte[] value)
        {
            base.WriteValue(value);
            AddToken(new BsonBinary(value, BsonBinaryType.Binary));
        }

        /// <summary>
        /// Writes a <see cref="Guid"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Guid"/> value to write.</param>
        public override void WriteValue(Guid value)
        {
            base.WriteValue(value);
            AddToken(new BsonBinary(value.ToByteArray(), BsonBinaryType.Uuid));
        }

        /// <summary>
        /// Writes a <see cref="TimeSpan"/> value.
        /// </summary>
        /// <param name="value">The <see cref="TimeSpan"/> value to write.</param>
        public override void WriteValue(TimeSpan value)
        {
            base.WriteValue(value);
            AddToken(new BsonString(value.ToString(), true));
        }

        /// <summary>
        /// Writes a <see cref="Uri"/> value.
        /// </summary>
        /// <param name="value">The <see cref="Uri"/> value to write.</param>
        public override void WriteValue(Uri value)
        {
            base.WriteValue(value);
            AddToken(new BsonString(value.ToString(), true));
        }
        #endregion

        /// <summary>
        /// Writes a <see cref="Byte"/>[] value that represents a BSON object id.
        /// </summary>
        /// <param name="value">The Object ID value to write.</param>
        public void WriteObjectId(byte[] value)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            if (value.Length != 12)
            {
                throw JsonWriterException.Create(this, "An object id must be 12 bytes", null);
            }

            // hack to update the writer state
            UpdateScopeWithFinishedValue();
            AutoComplete(JsonToken.Undefined);
            AddValue(value, BsonType.Oid);
        }

        /// <summary>
        /// Writes a BSON regex.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <param name="options">The regex options.</param>
        public void WriteRegex(string pattern, string options)
        {
            ValidationUtils.ArgumentNotNull(pattern, nameof(pattern));

            // hack to update the writer state
            UpdateScopeWithFinishedValue();
            AutoComplete(JsonToken.Undefined);
            AddToken(new BsonRegex(pattern, options));
        }
    }
}