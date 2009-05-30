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
#if !SILVERLIGHT && !PocketPC
using System.Data.Linq;
#endif
#if !SILVERLIGHT
using System.Data.SqlTypes;
#endif
using System.Linq;
using System.Text;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
  /// <summary>
  /// Converts a binary value to and from a base 64 string value.
  /// </summary>
  public class BinaryConverter : JsonConverter
  {
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

      byte[] data = value as byte[];

      if (data == null)
        data = GetByteArray(value);

      writer.WriteValue(Convert.ToBase64String(data));
    }

    private byte[] GetByteArray(object value)
    {
#if !SILVERLIGHT && !PocketPC
      if (value is Binary)
        return ((Binary)value).ToArray();
#endif
#if !SILVERLIGHT
      if (value is SqlBinary)
        return ((SqlBinary) value).Value;
#endif
      throw new Exception("Unexpected value type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    /// <summary>
    /// Reads the JSON representation of the object.
    /// </summary>
    /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, JsonSerializer serializer)
    {
      Type t = (ReflectionUtils.IsNullableType(objectType))
        ? Nullable.GetUnderlyingType(objectType)
        : objectType;

      if (reader.TokenType == JsonToken.Null)
      {
        if (!ReflectionUtils.IsNullable(objectType))
          throw new Exception("Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));

        return null;
      }

      if (reader.TokenType != JsonToken.String)
        throw new Exception("Unexpected token parsing binary. Expected String, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));

      string encodedData = reader.Value.ToString();

      byte[] data = Convert.FromBase64String(encodedData);

      if (t == typeof(byte[]))
        return data;

#if !SILVERLIGHT && !PocketPC
      if (typeof(Binary).IsAssignableFrom(t))
        return new Binary(data);
#endif
#if !SILVERLIGHT 
      if (typeof(SqlBinary).IsAssignableFrom(t))
        return new SqlBinary(data);
#endif
      throw new Exception("Unexpected object type when writing binary: {0}".FormatWith(CultureInfo.InvariantCulture, objectType));
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
      Type t = (ReflectionUtils.IsNullableType(objectType))
        ? Nullable.GetUnderlyingType(objectType)
        : objectType;

      if (t == typeof(byte[]))
        return true;

#if !SILVERLIGHT && !PocketPC
      if (typeof(Binary).IsAssignableFrom(t))
        return true;
#endif
#if !SILVERLIGHT
      if (typeof(SqlBinary).IsAssignableFrom(t))
        return true;
#endif
      return false;
    }
  }
}