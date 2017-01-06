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
using System.Text;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Schema;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Converts an object to and from JSON.
    /// </summary>
    public abstract class JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public abstract void WriteJson(JsonWriter writer, object value, JsonSerializer serializer);

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public abstract object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer);

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public abstract bool CanConvert(Type objectType);

        /// <summary>
        /// <para>
        /// Gets the <see cref="JsonSchema"/> of the JSON produced by the <see cref="JsonConverter"/>.
        /// </para>
        /// <note type="caution">
        /// JSON Schema validation has been moved to its own package. See <see href="http://www.newtonsoft.com/jsonschema">http://www.newtonsoft.com/jsonschema</see> for more details.
        /// </note>
        /// </summary>
        /// <returns>The <see cref="JsonSchema"/> of the JSON produced by the <see cref="JsonConverter"/>.</returns>
        [Obsolete("JSON Schema validation has been moved to its own package. It is strongly recommended that you do not override GetSchema() in your own converter. It is not used by Json.NET and will be removed at some point in the future. Converter's that override GetSchema() will stop working when it is removed.")]
        public virtual JsonSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can read JSON.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can read JSON; otherwise, <c>false</c>.</value>
        public virtual bool CanRead
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can write JSON.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can write JSON; otherwise, <c>false</c>.</value>
        public virtual bool CanWrite
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can convert values to strings. Used when serializing dictionary keys.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can convert values to strings; otherwise, <c>false</c>.</value>
        public virtual bool CanConvertToString
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can convert values from strings. Used when deserializing dictionary keys.
        /// </summary>
        /// <value></value>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can convert values from strings; otherwise, <c>false</c>.</value>
        public virtual bool CanConvertFromString
        {
            get { return false; }
        }

        /// <summary>
        /// Converts the object to its string representation. Used when serializing dictionary keys.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string representation of the value.</returns>
        public virtual string ConvertToString(object value)
        {
            throw new NotSupportedException("This method must be overriden to use");
        }

        /// <summary>
        /// Converts the string representation of an object to that object. Used when deserializing dictionary keys.
        /// </summary>
        /// <param name="value">The object's string representation.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object represented by its string representation.</returns>
        public virtual object ConvertFromString(string value, Type objectType)
        {
            throw new NotSupportedException("This method must be overriden to use");
        }
    }
}