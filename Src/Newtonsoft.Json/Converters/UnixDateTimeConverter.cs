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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> to and from Unix epoch time
    /// </summary>
    public class UnixDateTimeConverter : DateTimeConverterBase
    {
        internal static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            long ticks;

            if (value is DateTime dateTime)
            {
                ticks = (long)(dateTime.ToUniversalTime() - UnixEpoch).TotalSeconds;
            }
#if HAVE_DATE_TIME_OFFSET
            else if (value is DateTimeOffset dateTimeOffset)
            {
                ticks = (long)(dateTimeOffset.ToUniversalTime() - UnixEpoch).TotalSeconds;
            }
#endif
            else
            {
                throw new JsonSerializationException("Expected date object value.");
            }

            if (ticks < 0)
            {
                throw new JsonSerializationException("Cannot convert date value that is before Unix epoch of 00:00:00 UTC on 1 January 1970.");
            }

            writer.WriteValue(ticks);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing property value of the JSON that is being converted.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            bool nullable = ReflectionUtils.IsNullable(objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (!nullable)
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }

                return null;
            }

            long ticks;

            if (reader.TokenType == JsonToken.Integer)
            {
                ticks = (long)reader.Value;
            }
            else if (reader.TokenType == JsonToken.String)
            {
                if (!long.TryParse((string)reader.Value, out ticks))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert invalid value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
                }
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Unexpected token parsing date. Expected Integer or String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            if (ticks >= 0)
            {
                DateTime d = UnixEpoch.AddSeconds(ticks);

#if HAVE_DATE_TIME_OFFSET
                Type t = (nullable)
                    ? Nullable.GetUnderlyingType(objectType)
                    : objectType;
                if (t == typeof(DateTimeOffset))
                {
                    return new DateTimeOffset(d, TimeSpan.Zero);
                }
#endif
                return d;
            }
            else
            {
                throw JsonSerializationException.Create(reader, "Cannot convert value that is before Unix epoch of 00:00:00 UTC on 1 January 1970 to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));
            }
        }
    }
}