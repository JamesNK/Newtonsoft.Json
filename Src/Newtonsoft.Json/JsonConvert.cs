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
using System.IO;
using System.Globalization;
#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Xml;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Text;
#if !NET20 && (!SILVERLIGHT || WINDOWS_PHONE) && !PORTABLE40
using System.Xml.Linq;
#endif

namespace Newtonsoft.Json
{
  /// <summary>
  /// Provides methods for converting between common language runtime types and JSON types.
  /// </summary>
  /// <example>
  ///   <code lang="cs" source="..\Src\Newtonsoft.Json.Tests\Documentation\SerializationTests.cs" region="SerializeObject" title="Serializing and Deserializing JSON with JsonConvert" />
  /// </example>
  public static class JsonConvert
  {
    /// <summary>
    /// Gets or sets a function that creates default <see cref="JsonSerializerSettings"/>.
    /// Default settings are automatically used by serialization methods on <see cref="JsonConvert"/>,
    /// and <see cref="JToken.ToObject{T}()"/> and <see cref="JToken.FromObject(object)"/> on <see cref="JToken"/>.
    /// To serialize without using any default settings create a <see cref="JsonSerializer"/> with
    /// <see cref="JsonSerializer.Create()"/>.
    /// </summary>
    public static Func<JsonSerializerSettings> DefaultSettings { get; set; }

    /// <summary>
    /// Represents JavaScript's boolean value true as a string. This field is read-only.
    /// </summary>
    public static readonly string True = "true";

    /// <summary>
    /// Represents JavaScript's boolean value false as a string. This field is read-only.
    /// </summary>
    public static readonly string False = "false";

    /// <summary>
    /// Represents JavaScript's null as a string. This field is read-only.
    /// </summary>
    public static readonly string Null = "null";

    /// <summary>
    /// Represents JavaScript's undefined as a string. This field is read-only.
    /// </summary>
    public static readonly string Undefined = "undefined";

    /// <summary>
    /// Represents JavaScript's positive infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string PositiveInfinity = "Infinity";

    /// <summary>
    /// Represents JavaScript's negative infinity as a string. This field is read-only.
    /// </summary>
    public static readonly string NegativeInfinity = "-Infinity";

    /// <summary>
    /// Represents JavaScript's NaN as a string. This field is read-only.
    /// </summary>
    public static readonly string NaN = "NaN";

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="DateTime"/>.</returns>
    public static string ToString(DateTime value)
    {
      return ToString(value, DateFormatHandling.IsoDateFormat, DateTimeZoneHandling.RoundtripKind);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation using the <see cref="DateFormatHandling"/> specified.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">The format the date will be converted to.</param>
    /// <param name="timeZoneHandling">The time zone handling when the date is converted to a string.</param>
    /// <returns>A JSON string representation of the <see cref="DateTime"/>.</returns>
    public static string ToString(DateTime value, DateFormatHandling format, DateTimeZoneHandling timeZoneHandling)
    {
      DateTime updatedDateTime = DateTimeUtils.EnsureDateTime(value, timeZoneHandling);

      using (StringWriter writer = StringUtils.CreateStringWriter(64))
      {
        writer.Write('"');
        DateTimeUtils.WriteDateTimeString(writer, updatedDateTime, format, null, CultureInfo.InvariantCulture);
        writer.Write('"');
        return writer.ToString();
      }
    }

#if !NET20
    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="DateTimeOffset"/>.</returns>
    public static string ToString(DateTimeOffset value)
    {
      return ToString(value, DateFormatHandling.IsoDateFormat);
    }

    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to its JSON string representation using the <see cref="DateFormatHandling"/> specified.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="format">The format the date will be converted to.</param>
    /// <returns>A JSON string representation of the <see cref="DateTimeOffset"/>.</returns>
    public static string ToString(DateTimeOffset value, DateFormatHandling format)
    {
      using (StringWriter writer = StringUtils.CreateStringWriter(64))
      {
        writer.Write('"');
        DateTimeUtils.WriteDateTimeOffsetString(writer, value, format, null, CultureInfo.InvariantCulture);
        writer.Write('"');
        return writer.ToString();
      }
    }
#endif

    /// <summary>
    /// Converts the <see cref="Boolean"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Boolean"/>.</returns>
    public static string ToString(bool value)
    {
      return (value) ? True : False;
    }

    /// <summary>
    /// Converts the <see cref="Char"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Char"/>.</returns>
    public static string ToString(char value)
    {
      return ToString(char.ToString(value));
    }

    /// <summary>
    /// Converts the <see cref="Enum"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Enum"/>.</returns>
    public static string ToString(Enum value)
    {
      return value.ToString("D");
    }

    /// <summary>
    /// Converts the <see cref="Int32"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Int32"/>.</returns>
    public static string ToString(int value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int16"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Int16"/>.</returns>
    public static string ToString(short value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt16"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt16"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(ushort value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt32"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt32"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(uint value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int64"/>  to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Int64"/>.</returns>
    public static string ToString(long value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40 || PORTABLE)
    private static string ToStringInternal(BigInteger value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }
#endif

    /// <summary>
    /// Converts the <see cref="UInt64"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt64"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(ulong value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Single"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Single"/>.</returns>
    public static string ToString(float value)
    {
      return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    internal static string ToString(float value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
    {
      return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
    }

    private static string EnsureFloatFormat(double value, string text, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
    {
      if (floatFormatHandling == FloatFormatHandling.Symbol || !(double.IsInfinity(value) || double.IsNaN(value)))
        return text;

      if (floatFormatHandling == FloatFormatHandling.DefaultValue)
        return (!nullable) ? "0.0" : Null;
      
      return quoteChar + text + quoteChar;
    }

    /// <summary>
    /// Converts the <see cref="Double"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Double"/>.</returns>
    public static string ToString(double value)
    {
      return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    internal static string ToString(double value, FloatFormatHandling floatFormatHandling, char quoteChar, bool nullable)
    {
      return EnsureFloatFormat(value, EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture)), floatFormatHandling, quoteChar, nullable);
    }

    private static string EnsureDecimalPlace(double value, string text)
    {
      if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1 || text.IndexOf('e') != -1)
        return text;

      return text + ".0";
    }

    private static string EnsureDecimalPlace(string text)
    {
      if (text.IndexOf('.') != -1)
        return text;

      return text + ".0";
    }

    /// <summary>
    /// Converts the <see cref="Byte"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Byte"/>.</returns>
    public static string ToString(byte value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="SByte"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="SByte"/>.</returns>
    [CLSCompliant(false)]
    public static string ToString(sbyte value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Decimal"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="SByte"/>.</returns>
    public static string ToString(decimal value)
    {
      return EnsureDecimalPlace(value.ToString(null, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts the <see cref="Guid"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Guid"/>.</returns>
    public static string ToString(Guid value)
    {
      return ToString(value, '"');
    }

    internal static string ToString(Guid value, char quoteChar)
    {
      string text = null;

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
      text = value.ToString("D", CultureInfo.InvariantCulture);
#else
      text = value.ToString("D");
#endif

      return quoteChar + text + quoteChar;
    }

    /// <summary>
    /// Converts the <see cref="TimeSpan"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="TimeSpan"/>.</returns>
    public static string ToString(TimeSpan value)
    {
      return ToString(value, '"');
    }

    internal static string ToString(TimeSpan value, char quoteChar)
    {
      return ToString(value.ToString(), quoteChar);
    }

    /// <summary>
    /// Converts the <see cref="Uri"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Uri"/>.</returns>
    public static string ToString(Uri value)
    {
      if (value == null)
        return Null;

      return ToString(value, '"');
    }

    internal static string ToString(Uri value, char quoteChar)
    {
      return ToString(value.ToString(), quoteChar);
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value)
    {
      return ToString(value, '"');
    }

    /// <summary>
    /// Converts the <see cref="String"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="delimiter">The string delimiter character.</param>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value, char delimiter)
    {
      if (delimiter != '"' && delimiter != '\'')
        throw new ArgumentException("Delimiter must be a single or double quote.", "delimiter");

      return JavaScriptUtils.ToEscapedJavaScriptString(value, delimiter, true);
    }

    /// <summary>
    /// Converts the <see cref="Object"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Object"/>.</returns>
    public static string ToString(object value)
    {
      if (value == null)
        return Null;

      PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(value);

      switch (typeCode)
      {
        case PrimitiveTypeCode.String:
          return ToString((string) value);
        case PrimitiveTypeCode.Char:
          return ToString((char) value);
        case PrimitiveTypeCode.Boolean:
          return ToString((bool) value);
        case PrimitiveTypeCode.SByte:
          return ToString((sbyte) value);
        case PrimitiveTypeCode.Int16:
          return ToString((short) value);
        case PrimitiveTypeCode.UInt16:
          return ToString((ushort) value);
        case PrimitiveTypeCode.Int32:
          return ToString((int) value);
        case PrimitiveTypeCode.Byte:
          return ToString((byte) value);
        case PrimitiveTypeCode.UInt32:
          return ToString((uint) value);
        case PrimitiveTypeCode.Int64:
          return ToString((long) value);
        case PrimitiveTypeCode.UInt64:
          return ToString((ulong) value);
        case PrimitiveTypeCode.Single:
          return ToString((float) value);
        case PrimitiveTypeCode.Double:
          return ToString((double) value);
        case PrimitiveTypeCode.DateTime:
          return ToString((DateTime) value);
        case PrimitiveTypeCode.Decimal:
          return ToString((decimal) value);
#if !(NETFX_CORE || PORTABLE)
        case PrimitiveTypeCode.DBNull:
          return Null;
#endif
#if !NET20
        case PrimitiveTypeCode.DateTimeOffset:
          return ToString((DateTimeOffset) value);
#endif
        case PrimitiveTypeCode.Guid:
          return ToString((Guid) value);
        case PrimitiveTypeCode.Uri:
          return ToString((Uri) value);
        case PrimitiveTypeCode.TimeSpan:
          return ToString((TimeSpan) value);
#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40 || PORTABLE)
        case PrimitiveTypeCode.BigInteger:
          return ToStringInternal((BigInteger)value);
#endif
      }

      throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    internal static bool IsJsonPrimitiveType(Type t)
    {
      PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(t);

      if (typeCode != PrimitiveTypeCode.Empty && typeCode != PrimitiveTypeCode.Object)
        return true;

#if !(PORTABLE || NETFX_CORE)
      if (typeof(IConvertible).IsAssignableFrom(t)
        || (ReflectionUtils.IsNullableType(t) && typeof(IConvertible).IsAssignableFrom(Nullable.GetUnderlyingType(t))))
      {
        return !typeof(JToken).IsAssignableFrom(t);
      }
#endif

      return false;
    }

    #region Serialize
    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value)
    {
      return SerializeObject(value, Formatting.None, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, Formatting formatting)
    {
      return SerializeObject(value, formatting, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="converters">A collection converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value, params JsonConverter[] converters)
    {
      return SerializeObject(value, Formatting.None, converters);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting and a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="converters">A collection converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value, Formatting formatting, params JsonConverter[] converters)
    {
      JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                                          ? new JsonSerializerSettings {Converters = converters}
                                          : null;

      return SerializeObject(value, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, JsonSerializerSettings settings)
    {
      return SerializeObject(value, Formatting.None, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using formatting and <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, Formatting formatting, JsonSerializerSettings settings)
    {
      return SerializeObject(value, null, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a type, formatting and <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <param name="type">
    /// The type of the value being serialized.
    /// This parameter is used when <see cref="TypeNameHandling"/> is Auto to write out the type name if the type of the value does not match.
    /// Specifing the type is optional.
    /// </param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, Type type, Formatting formatting, JsonSerializerSettings settings)
    {
      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

      StringBuilder sb = new StringBuilder(256);
      StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = formatting;

        jsonSerializer.Serialize(jsonWriter, value, type);
      }

      return sw.ToString();
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40)
    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string.
    /// Serialization will happen on a new thread.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>
    /// A task that represents the asynchronous serialize operation. The value of the <c>TResult</c> parameter contains a JSON string representation of the object.
    /// </returns>
    public static Task<string> SerializeObjectAsync(object value)
    {
      return SerializeObjectAsync(value, Formatting.None, null);
    }

    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string using formatting.
    /// Serialization will happen on a new thread.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>
    /// A task that represents the asynchronous serialize operation. The value of the <c>TResult</c> parameter contains a JSON string representation of the object.
    /// </returns>
    public static Task<string> SerializeObjectAsync(object value, Formatting formatting)
    {
      return SerializeObjectAsync(value, formatting, null);
    }

    /// <summary>
    /// Asynchronously serializes the specified object to a JSON string using formatting and a collection of <see cref="JsonConverter"/>.
    /// Serialization will happen on a new thread.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="settings">The <see cref="JsonSerializerSettings"/> used to serialize the object.
    /// If this is null, default serialization settings will be is used.</param>
    /// <returns>
    /// A task that represents the asynchronous serialize operation. The value of the <c>TResult</c> parameter contains a JSON string representation of the object.
    /// </returns>
    public static Task<string> SerializeObjectAsync(object value, Formatting formatting, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => SerializeObject(value, formatting, settings));
    }
#endif
    #endregion

    #region Deserialize
    /// <summary>
    /// Deserializes the JSON to a .NET object.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value)
    {
      return DeserializeObject(value, null, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to a .NET object using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, JsonSerializerSettings settings)
    {
      return DeserializeObject(value, null, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value, Type type)
    {
      return DeserializeObject(value, type, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static T DeserializeObject<T>(string value)
    {
      return DeserializeObject<T>(value, (JsonSerializerSettings) null);
    }

    /// <summary>
    /// Deserializes the JSON to the given anonymous type.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be infered from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="anonymousTypeObject">The anonymous type object.</param>
    /// <returns>The deserialized anonymous type from the JSON string.</returns>
    public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
    {
      return DeserializeObject<T>(value);
    }

    /// <summary>
    /// Deserializes the JSON to the given anonymous type using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be infered from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="anonymousTypeObject">The anonymous type object.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized anonymous type from the JSON string.</returns>
    public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject, JsonSerializerSettings settings)
    {
      return DeserializeObject<T>(value, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
    {
      return (T) DeserializeObject(value, typeof (T), converters);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static T DeserializeObject<T>(string value, JsonSerializerSettings settings)
    {
      return (T) DeserializeObject(value, typeof (T), settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
    {
      JsonSerializerSettings settings = (converters != null && converters.Length > 0)
                                          ? new JsonSerializerSettings {Converters = converters}
                                          : null;

      return DeserializeObject(value, type, settings);
    }

    /// <summary>
    /// Deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, Type type, JsonSerializerSettings settings)
    {
      ValidationUtils.ArgumentNotNull(value, "value");

      StringReader sr = new StringReader(value);
      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

      // by default DeserializeObject should check for additional content
      if (!jsonSerializer.IsCheckAdditionalContentSet())
        jsonSerializer.CheckAdditionalContent = true;

      return jsonSerializer.Deserialize(new JsonTextReader(sr), type);
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40)
    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type.
    /// Deserialization will happen on a new thread.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<T> DeserializeObjectAsync<T>(string value)
    {
      return DeserializeObjectAsync<T>(value, null);
    }

    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
    /// Deserialization will happen on a new thread.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize to.</typeparam>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<T> DeserializeObjectAsync<T>(string value, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => DeserializeObject<T>(value, settings));
    }

    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type.
    /// Deserialization will happen on a new thread.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<object> DeserializeObjectAsync(string value)
    {
      return DeserializeObjectAsync(value, null, null);
    }

    /// <summary>
    /// Asynchronously deserializes the JSON to the specified .NET type using <see cref="JsonSerializerSettings"/>.
    /// Deserialization will happen on a new thread.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize to.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous deserialize operation. The value of the <c>TResult</c> parameter contains the deserialized object from the JSON string.
    /// </returns>
    public static Task<object> DeserializeObjectAsync(string value, Type type, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => DeserializeObject(value, type, settings));
    }
#endif
    #endregion

    /// <summary>
    /// Populates the object with values from the JSON string.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    public static void PopulateObject(string value, object target)
    {
      PopulateObject(value, target, null);
    }

    /// <summary>
    /// Populates the object with values from the JSON string using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    public static void PopulateObject(string value, object target, JsonSerializerSettings settings)
    {
      StringReader sr = new StringReader(value);
      JsonSerializer jsonSerializer = JsonSerializer.CreateDefault(settings);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        jsonSerializer.Populate(jsonReader, target);

        if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
          throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
      }
    }

#if !(NET20 || NET35 || SILVERLIGHT || PORTABLE40)
    /// <summary>
    /// Asynchronously populates the object with values from the JSON string using <see cref="JsonSerializerSettings"/>.
    /// </summary>
    /// <param name="value">The JSON to populate values from.</param>
    /// <param name="target">The target object to populate values onto.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous populate operation.
    /// </returns>
    public static Task PopulateObjectAsync(string value, object target, JsonSerializerSettings settings)
    {
      return Task.Factory.StartNew(() => PopulateObject(value, target, settings));
    }
#endif

#if !(SILVERLIGHT || PORTABLE40 || PORTABLE || NETFX_CORE)
    /// <summary>
    /// Serializes the XML node to a JSON string.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <returns>A JSON string of the XmlNode.</returns>
    public static string SerializeXmlNode(XmlNode node)
    {
      return SerializeXmlNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the XML node to a JSON string using formatting.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>A JSON string of the XmlNode.</returns>
    public static string SerializeXmlNode(XmlNode node, Formatting formatting)
    {
      XmlNodeConverter converter = new XmlNodeConverter();

      return SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Serializes the XML node to a JSON string using formatting and omits the root object if <see cref="omitRootObject"/> is <c>true</c>.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the XmlNode.</returns>
    public static string SerializeXmlNode(XmlNode node, Formatting formatting, bool omitRootObject)
    {
      XmlNodeConverter converter = new XmlNodeConverter {OmitRootObject = omitRootObject};

      return SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value)
    {
      return DeserializeXmlNode(value, null);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root elment specified by <see cref="deserializeRootElementName"/>.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName)
    {
      return DeserializeXmlNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root elment specified by <see cref="deserializeRootElementName"/>
    /// and writes a .NET array attribute for collections.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A flag to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
    {
      XmlNodeConverter converter = new XmlNodeConverter();
      converter.DeserializeRootElementName = deserializeRootElementName;
      converter.WriteArrayAttribute = writeArrayAttribute;

      return (XmlDocument) DeserializeObject(value, typeof (XmlDocument), converter);
    }
#endif

#if !NET20 && (!(SILVERLIGHT) || WINDOWS_PHONE) && !PORTABLE40
    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <returns>A JSON string of the XNode.</returns>
    public static string SerializeXNode(XObject node)
    {
      return SerializeXNode(node, Formatting.None);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string using formatting.
    /// </summary>
    /// <param name="node">The node to convert to JSON.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>A JSON string of the XNode.</returns>
    public static string SerializeXNode(XObject node, Formatting formatting)
    {
      return SerializeXNode(node, formatting, false);
    }

    /// <summary>
    /// Serializes the <see cref="XNode"/> to a JSON string using formatting and omits the root object if <see cref="omitRootObject"/> is <c>true</c>.
    /// </summary>
    /// <param name="node">The node to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="omitRootObject">Omits writing the root object.</param>
    /// <returns>A JSON string of the XNode.</returns>
    public static string SerializeXNode(XObject node, Formatting formatting, bool omitRootObject)
    {
      XmlNodeConverter converter = new XmlNodeConverter {OmitRootObject = omitRootObject};

      return SerializeObject(node, formatting, converter);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized XNode</returns>
    public static XDocument DeserializeXNode(string value)
    {
      return DeserializeXNode(value, null);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root elment specified by <see cref="deserializeRootElementName"/>.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XNode</returns>
    public static XDocument DeserializeXNode(string value, string deserializeRootElementName)
    {
      return DeserializeXNode(value, deserializeRootElementName, false);
    }

    /// <summary>
    /// Deserializes the <see cref="XNode"/> from a JSON string nested in a root elment specified by <see cref="deserializeRootElementName"/>
    /// and writes a .NET array attribute for collections.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <param name="writeArrayAttribute">
    /// A flag to indicate whether to write the Json.NET array attribute.
    /// This attribute helps preserve arrays when converting the written XML back to JSON.
    /// </param>
    /// <returns>The deserialized XNode</returns>
    public static XDocument DeserializeXNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
    {
      XmlNodeConverter converter = new XmlNodeConverter();
      converter.DeserializeRootElementName = deserializeRootElementName;
      converter.WriteArrayAttribute = writeArrayAttribute;

      return (XDocument) DeserializeObject(value, typeof (XDocument), converter);
    }
#endif
  }
}