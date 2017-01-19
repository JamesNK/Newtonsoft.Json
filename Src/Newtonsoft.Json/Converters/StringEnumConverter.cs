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
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Utilities;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts an <see cref="Enum"/> to and from its name string value.
    /// </summary>
    public class StringEnumConverter : JsonConverter
    {
        /// <summary>
        /// Gets or sets a value indicating whether the written enum text should be camel case.
        /// </summary>
        /// <value><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</value>
        public bool CamelCaseText { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether integer values are allowed when deserializing.
        /// </summary>
        /// <value><c>true</c> if integers are allowed when deserializing; otherwise, <c>false</c>.</value>
        public bool AllowIntegerValues { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnumConverter"/> class.
        /// </summary>
        public StringEnumConverter()
        {
            AllowIntegerValues = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringEnumConverter"/> class.
        /// </summary>
        /// <param name="camelCaseText"><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</param>
        public StringEnumConverter(bool camelCaseText)
            : this()
        {
            CamelCaseText = camelCaseText;
        }

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
            string result;
            if (TryConvertToString(value, false, out result))
            {
                writer.WriteValue(result);
            }
            else
            {
                writer.WriteValue(value);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can convert values to strings. Used when serializing dictionary keys.
        /// </summary>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can convert values to strings; otherwise, <c>false</c>.</value>
        public override bool CanConvertToString
        {
            get { return true; }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="JsonConverter"/> can convert values from strings. Used when deserializing dictionary keys.
        /// </summary>
        /// <value></value>
        /// <value><c>true</c> if this <see cref="JsonConverter"/> can convert values from strings; otherwise, <c>false</c>.</value>
        public override bool CanConvertFromString
        {
            get { return true; }
        }

        /// <summary>
        /// Converts the object to its string representation. Used when serializing dictionary keys.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The string representation of the value.</returns>
        public override string ConvertToString(object value)
        {
            string result;
            TryConvertToString(value, true, out result);
            return result;
        }

        private bool TryConvertToString(object value, bool force, out string result)
        {
            if (value == null)
            {
                result = null;
                return true;
            }

            Enum e = (Enum)value;

            string enumName = e.ToString("G");

            if (char.IsNumber(enumName[0]) || enumName[0] == '-')
            {
                // enum value has no name so write number
                if (force)
                {
                    result = enumName;
                    return true;
                }
                result = null;
                return false;
            }
            Type enumType = e.GetType();

            result = EnumUtils.ToEnumName(enumType, enumName, CamelCaseText);
            return true;
        }

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
            bool isNullable = ReflectionUtils.IsNullableType(objectType);

            if (reader.TokenType == JsonToken.Null)
            {
                if (!isNullable)
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            try
            {
                if (reader.TokenType == JsonToken.String)
                {
                    string enumText = reader.Value.ToString();

                    return EnumUtils.ParseEnumName(enumText, isNullable, !AllowIntegerValues, t);
                }

                if (reader.TokenType == JsonToken.Integer)
                {
                    if (!AllowIntegerValues)
                    {
                        throw JsonSerializationException.Create(reader, "Integer value {0} is not allowed.".FormatWith(CultureInfo.InvariantCulture, reader.Value));
                    }

                    return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, t);
                }
            }
            catch (Exception ex)
            {
                throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.FormatValueForPrint(reader.Value), objectType), ex);
            }

            // we don't actually expect to get here.
            throw JsonSerializationException.Create(reader, "Unexpected token {0} when parsing enum.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
        }

        /// <summary>
        /// Converts the string representation of an object to that object. Used when deserializing dictionary keys.
        /// </summary>
        /// <param name="value">The object's string representation.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>The object represented by its string representation.</returns>
        public override object ConvertFromString(string value, Type objectType)
        {
            bool isNullable = ReflectionUtils.IsNullableType(objectType);
            Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

            if (value == null)
            {
                return null;
            }

            return EnumUtils.ParseEnumName(value, isNullable, !AllowIntegerValues, t);
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// <c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            return t.IsEnum();
        }
    }
}