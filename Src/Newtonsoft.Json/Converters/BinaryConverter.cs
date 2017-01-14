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

#if HAVE_BINARY_SERIALIZATION
using System;
using System.Data.SqlTypes;
using System.Globalization;
using Newtonsoft.Json.Utilities;
using System.Collections.Generic;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a binary value to and from a base 64 string value.
    /// </summary>
    public class BinaryConverter : JsonConverter
    {
#if HAVE_LINQ
        private const string BinaryTypeName = "System.Data.Linq.Binary";
        private const string BinaryToArrayName = "ToArray";
        private static ReflectionObject _reflectionObject;
#endif

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            byte[] data = GetByteArray(value);

            writer.WriteValue(data);
        }

        private byte[] GetByteArray(object value)
        {
#if HAVE_LINQ
            if (value.GetType().FullName == BinaryTypeName)
            {
                EnsureReflectionObject(value.GetType());
                return (byte[])_reflectionObject.GetValue(value, BinaryToArrayName);
            }
#endif
            if (value is SqlBinary)
            {
                return ((SqlBinary)value).Value;
            }

            throw new JsonSerializationException("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
        }

#if HAVE_LINQ
        private static void EnsureReflectionObject(Type t)
        {
            if (_reflectionObject == null)
            {
                _reflectionObject = ReflectionObject.Create(t, t.GetConstructor(new[] { typeof(byte[]) }), BinaryToArrayName);
            }
        }
#endif

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionUtils.IsNullable(objectType))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            byte[] data;

            if (reader.TokenType == JsonToken.StartArray)
            {
                data = ReadByteArray(reader);
            }
            else if (reader.TokenType == JsonToken.String)
            {
                // current token is already at base64 string
                // unable to call ReadAsBytes so do it the old fashion way
                string encodedData = reader.Value.ToString();
                data = Convert.FromBase64String(encodedData);
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unexpected token parsing binary. Expected String or StartArray, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

#if HAVE_LINQ
            if (t.FullName == BinaryTypeName)
            {
                EnsureReflectionObject(t);

                return _reflectionObject.Creator(data);
            }
#endif

            if (t == typeof(SqlBinary))
            {
                return new SqlBinary(data);
            }

            throw JsonSerializationException.Create(reader, "Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));
        }

        private byte[] ReadByteArray(JsonReader reader)
        {
            List<byte> byteList = new List<byte>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Integer:
                        byteList.Add(Convert.ToByte(reader.Value, CultureInfo.InvariantCulture));
                        break;
                    case JsonToken.EndArray:
                        return byteList.ToArray();
                    case JsonToken.Comment:
                        // skip
                        break;
                    default:
                        throw JsonSerializationException.Create(reader, "Unexpected token when reading bytes: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading bytes.");
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
#if HAVE_LINQ
            if (objectType.FullName == BinaryTypeName)
            {
                return true;
            }
#endif

            if (objectType == typeof(SqlBinary) || objectType == typeof(SqlBinary?))
            {
                return true;
            }

            return false;
        }
    }
}

#endif