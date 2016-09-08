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
using Newtonsoft.Json.Bson;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="BsonObjectId"/> to and from JSON and BSON.
    /// </summary>
    public class BsonObjectIdConverter : JsonConverter<BsonObjectId>
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, BsonObjectId value, JsonSerializer serializer)
        {
            BsonWriter bsonWriter = writer as BsonWriter;
            if (bsonWriter != null)
            {
                bsonWriter.WriteObjectId(value.Value);
            }
            else
            {
                writer.WriteValue(value.Value);
            }
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingHasValue">The existing value has a value.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override BsonObjectId ReadJson(JsonReader reader, Type objectType, bool existingHasValue, BsonObjectId existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType != JsonToken.Bytes)
            {
                throw new JsonSerializationException("Expected Bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            byte[] value = (byte[])reader.Value;

            return new BsonObjectId(value);
        }
    }
}