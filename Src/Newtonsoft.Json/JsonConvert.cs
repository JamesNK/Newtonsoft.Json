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
using Newtonsoft.Json.Utilities;
using System.Xml;
using Newtonsoft.Json.Converters;
using System.Text;
#if !NET20 && !SILVERLIGHT
using System.Xml.Linq;
#endif

namespace Newtonsoft.Json
{
  /// <summary>
  /// Provides methods for converting between common language runtime types and JSON types.
  /// </summary>
  public static class JsonConvert
  {
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

    internal static readonly long InitialJavaScriptDateTicks = 621355968000000000;

    /// <summary>
    /// Converts the <see cref="DateTime"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="DateTime"/>.</returns>
    public static string ToString(DateTime value)
    {
      using (StringWriter writer = StringUtils.CreateStringWriter(64))
      {
        WriteDateTimeString(writer, value, GetUtcOffset(value), value.Kind);
        return writer.ToString();
      }
    }

#if !PocketPC && !NET20
    /// <summary>
    /// Converts the <see cref="DateTimeOffset"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="DateTimeOffset"/>.</returns>
    public static string ToString(DateTimeOffset value)
    {
      using (StringWriter writer = StringUtils.CreateStringWriter(64))
      {
        WriteDateTimeString(writer, value.UtcDateTime, value.Offset, DateTimeKind.Local);
        return writer.ToString();
      }
    }
#endif

    private static TimeSpan GetUtcOffset(DateTime dateTime)
    {
#if SILVERLIGHT
      return TimeZoneInfo.Local.GetUtcOffset(dateTime);
#else
      return TimeZone.CurrentTimeZone.GetUtcOffset(dateTime);
#endif
    }

    internal static void WriteDateTimeString(TextWriter writer, DateTime value)
    {
      WriteDateTimeString(writer, value, GetUtcOffset(value), value.Kind);
    }

    internal static void WriteDateTimeString(TextWriter writer, DateTime value, TimeSpan offset, DateTimeKind kind)
    {
      long javaScriptTicks = ConvertDateTimeToJavaScriptTicks(value, offset);

      writer.Write(@"""\/Date(");
      writer.Write(javaScriptTicks);
      
      switch (kind)
      {
        case DateTimeKind.Local:
        case DateTimeKind.Unspecified:
          writer.Write((offset.Ticks >= 0L) ? "+" : "-");

          int absHours = Math.Abs(offset.Hours);
          if (absHours < 10)
            writer.Write(0);
          writer.Write(absHours);
          int absMinutes = Math.Abs(offset.Minutes);
          if (absMinutes < 10)
            writer.Write(0);
          writer.Write(absMinutes);
          break;
      }

      writer.Write(@")\/""");
    }

    private static long ToUniversalTicks(DateTime dateTime)
    {
      if (dateTime.Kind == DateTimeKind.Utc)
        return dateTime.Ticks;

      return ToUniversalTicks(dateTime, GetUtcOffset(dateTime));
    }

    private static long ToUniversalTicks(DateTime dateTime, TimeSpan offset)
    {
      if (dateTime.Kind == DateTimeKind.Utc)
        return dateTime.Ticks;

      long ticks = dateTime.Ticks - offset.Ticks;
      if (ticks > 3155378975999999999L)
        return 3155378975999999999L;

      if (ticks < 0L)
        return 0L;

      return ticks;
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime, TimeSpan offset)
    {
      long universialTicks = ToUniversalTicks(dateTime, offset);

      return UniversialTicksToJavaScriptTicks(universialTicks);
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
    {
      long universialTicks = ToUniversalTicks(dateTime);

      return UniversialTicksToJavaScriptTicks(universialTicks);
    }

    private static long UniversialTicksToJavaScriptTicks(long universialTicks)
    {
      long javaScriptTicks = (universialTicks - InitialJavaScriptDateTicks) / 10000;

      return javaScriptTicks;
    }

    internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
    {
      DateTime dateTime = new DateTime((javaScriptTicks * 10000) + InitialJavaScriptDateTicks, DateTimeKind.Utc);

      return dateTime;
    }

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
    public static string ToString(ushort value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt32"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt32"/>.</returns>
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

    /// <summary>
    /// Converts the <see cref="UInt64"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="UInt64"/>.</returns>
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

    /// <summary>
    /// Converts the <see cref="Double"/> to its JSON string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A JSON string representation of the <see cref="Double"/>.</returns>
    public static string ToString(double value)
    {
      return EnsureDecimalPlace(value, value.ToString("R", CultureInfo.InvariantCulture));
    }

    private static string EnsureDecimalPlace(double value, string text)
    {
      if (double.IsNaN(value) || double.IsInfinity(value) || text.IndexOf('.') != -1 || text.IndexOf('E') != -1)
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
      return '"' + value.ToString("D", CultureInfo.InvariantCulture) + '"';
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
    /// <param name="delimter">The string delimiter character.</param>
    /// <returns>A JSON string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value, char delimter)
    {
      return JavaScriptUtils.ToEscapedJavaScriptString(value, delimter, true);
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

      IConvertible convertible = value as IConvertible;

      if (convertible != null)
      {
        switch (convertible.GetTypeCode())
        {
          case TypeCode.String:
            return ToString(convertible.ToString(CultureInfo.InvariantCulture));
          case TypeCode.Char:
            return ToString(convertible.ToChar(CultureInfo.InvariantCulture));
          case TypeCode.Boolean:
            return ToString(convertible.ToBoolean(CultureInfo.InvariantCulture));
          case TypeCode.SByte:
            return ToString(convertible.ToSByte(CultureInfo.InvariantCulture));
          case TypeCode.Int16:
            return ToString(convertible.ToInt16(CultureInfo.InvariantCulture));
          case TypeCode.UInt16:
            return ToString(convertible.ToUInt16(CultureInfo.InvariantCulture));
          case TypeCode.Int32:
            return ToString(convertible.ToInt32(CultureInfo.InvariantCulture));
          case TypeCode.Byte:
            return ToString(convertible.ToByte(CultureInfo.InvariantCulture));
          case TypeCode.UInt32:
            return ToString(convertible.ToUInt32(CultureInfo.InvariantCulture));
          case TypeCode.Int64:
            return ToString(convertible.ToInt64(CultureInfo.InvariantCulture));
          case TypeCode.UInt64:
            return ToString(convertible.ToUInt64(CultureInfo.InvariantCulture));
          case TypeCode.Single:
            return ToString(convertible.ToSingle(CultureInfo.InvariantCulture));
          case TypeCode.Double:
            return ToString(convertible.ToDouble(CultureInfo.InvariantCulture));
          case TypeCode.DateTime:
            return ToString(convertible.ToDateTime(CultureInfo.InvariantCulture));
          case TypeCode.Decimal:
            return ToString(convertible.ToDecimal(CultureInfo.InvariantCulture));
          case TypeCode.DBNull:
            return Null;
        }
      }
#if !PocketPC && !NET20
      else if (value is DateTimeOffset)
      {
        return ToString((DateTimeOffset)value);
      }
#endif

      throw new ArgumentException("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    private static bool IsJsonPrimitiveTypeCode(TypeCode typeCode)
    {
      switch (typeCode)
      {
        case TypeCode.String:
        case TypeCode.Char:
        case TypeCode.Boolean:
        case TypeCode.SByte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.Byte:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.DateTime:
        case TypeCode.Decimal:
        case TypeCode.DBNull:
          return true;
        default:
          return false;
      }
    }

    internal static bool IsJsonPrimitiveType(Type type)
    {
      if (ReflectionUtils.IsNullableType(type))
        type = Nullable.GetUnderlyingType(type);

#if !PocketPC && !NET20
     if (type == typeof(DateTimeOffset))
        return true;
#endif
      if (type == typeof(byte[]))
        return true;

      return IsJsonPrimitiveTypeCode(Type.GetTypeCode(type));
    }

    internal static bool IsJsonPrimitive(object value)
    {
      if (value == null)
        return true;

      IConvertible convertible = value as IConvertible;

      if (convertible != null)
        return IsJsonPrimitiveTypeCode(convertible.GetTypeCode());

#if !PocketPC && !NET20
      if (value is DateTimeOffset)
        return true;
#endif

      if (value is byte[])
        return true;

      return false;
    }

    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value)
    {
      return SerializeObject(value, Formatting.None, (JsonSerializerSettings)null);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <returns>
    /// A JSON string representation of the object.
    /// </returns>
    public static string SerializeObject(object value, Formatting formatting)
    {
      return SerializeObject(value, formatting, (JsonSerializerSettings)null);
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
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="converters">A collection converters used while serializing.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string SerializeObject(object value, Formatting formatting, params JsonConverter[] converters)
    {
      JsonSerializerSettings settings = (converters != null && converters.Length > 0)
        ? new JsonSerializerSettings { Converters = converters }
        : null;

      return SerializeObject(value, formatting, settings);
    }

    /// <summary>
    /// Serializes the specified object to a JSON string using a collection of <see cref="JsonConverter"/>.
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
      JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

      StringBuilder sb = new StringBuilder(128);
      StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
      using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
      {
        jsonWriter.Formatting = formatting;

        jsonSerializer.Serialize(jsonWriter, value);
      }

      return sw.ToString();
    }

    /// <summary>
    /// Deserializes the specified object to a Json object.
    /// </summary>
    /// <param name="value">The object to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value)
    {
      return DeserializeObject(value, null, (JsonSerializerSettings)null);
    }

    /// <summary>
    /// Deserializes the specified object to a Json object.
    /// </summary>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value, Type type)
    {
      return DeserializeObject(value, type, (JsonSerializerSettings)null);
    }

    /// <summary>
    /// Deserializes the specified object to a Json object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static T DeserializeObject<T>(string value)
    {
      return DeserializeObject<T>(value, (JsonSerializerSettings)null);
    }

    /// <summary>
    /// Deserializes the specified JSON to the given anonymous type.
    /// </summary>
    /// <typeparam name="T">
    /// The anonymous type to deserialize to. This can't be specified
    /// traditionally and must be infered from the anonymous type passed
    /// as a parameter.
    /// </typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="anonymousTypeObject">The anonymous type object.</param>
    /// <returns>The deserialized anonymous type from the JSON string.</returns>
    public static T DeserializeAnonymousType<T>(string value, T anonymousTypeObject)
    {
      return DeserializeObject<T>(value);
    }

    /// <summary>
    /// Deserializes the JSON string to the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
    {
      return (T)DeserializeObject(value, typeof(T), converters);
    }

    /// <summary>
    /// Deserializes the JSON string to the specified type.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static T DeserializeObject<T>(string value, JsonSerializerSettings settings)
    {
      return (T)DeserializeObject(value, typeof(T), settings);
    }

    /// <summary>
    /// Deserializes the JSON string to the specified type.
    /// </summary>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
    {
      JsonSerializerSettings settings = (converters != null && converters.Length > 0)
        ? new JsonSerializerSettings { Converters = converters }
        : null;

      return DeserializeObject(value, type, settings);
    }

    /// <summary>
    /// Deserializes the JSON string to the specified type.
    /// </summary>
    /// <param name="value">The JSON to deserialize.</param>
    /// <param name="type">The type of the object to deserialize.</param>
    /// <param name="settings">
    /// The <see cref="JsonSerializerSettings"/> used to deserialize the object.
    /// If this is null, default serialization settings will be is used.
    /// </param>
    /// <returns>The deserialized object from the JSON string.</returns>
    public static object DeserializeObject(string value, Type type, JsonSerializerSettings settings)
    {
      StringReader sr = new StringReader(value);
      JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

      object deserializedValue;

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        deserializedValue = jsonSerializer.Deserialize(jsonReader, type);

        if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
          throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
      }

      return deserializedValue;
    }

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
    /// Populates the object with values from the JSON string.
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
      JsonSerializer jsonSerializer = JsonSerializer.Create(settings);

      using (JsonReader jsonReader = new JsonTextReader(sr))
      {
        jsonSerializer.Populate(jsonReader, target);

        if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
          throw new JsonSerializationException("Additional text found in JSON string after finishing deserializing object.");
      }
    }

#if !NET20 && !SILVERLIGHT
    public static string SerializeXNode(XObject node)
    {
      return SerializeXNode(node, Formatting.None);
    }

    public static string SerializeXNode(XObject node, Formatting formatting)
    {
      XmlNodeConverter converter = new XmlNodeConverter();

      return SerializeObject(node, formatting, converter);
    }
#endif

#if !SILVERLIGHT
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
    /// Serializes the XML node to a JSON string.
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
    /// Deserializes the XmlNode from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value)
    {
      return DeserializeXmlNode(value, null);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root elment.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName)
    {
      XmlNodeConverter converter = new XmlNodeConverter();
      converter.DeserializeRootElementName = deserializeRootElementName;

      return (XmlDocument)DeserializeObject(value, typeof(XmlDocument), converter);
    }

#if !NET20
    /// <summary>
    /// Deserializes the XNode from a JSON string.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XDocument DeserializeXNode(string value)
    {
      return DeserializeXNode(value, null);
    }

    /// <summary>
    /// Deserializes the XmlNode from a JSON string nested in a root elment.
    /// </summary>
    /// <param name="value">The JSON string.</param>
    /// <param name="deserializeRootElementName">The name of the root element to append when deserializing.</param>
    /// <returns>The deserialized XmlNode</returns>
    public static XDocument DeserializeXNode(string value, string deserializeRootElementName)
    {
      XmlNodeConverter converter = new XmlNodeConverter();
      converter.DeserializeRootElementName = deserializeRootElementName;

      return (XDocument)DeserializeObject(value, typeof(XDocument), converter);
    }
#endif
#endif
  }
}