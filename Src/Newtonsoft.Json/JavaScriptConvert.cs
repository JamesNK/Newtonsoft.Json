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
using System.Drawing;
using System.Web.UI.WebControls;
using System.Collections;
using System.IO;
using System.Globalization;
using System.Runtime.Serialization;
using System.Reflection;
using System.Data.SqlTypes;
using Newtonsoft.Json.Utilities;
using System.Xml;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Provides methods for converting between common language runtime types and JavaScript types.
  /// </summary>
  public static class JavaScriptConvert
  {
    /// <summary>
    /// Represents JavaScript's boolean value true as a string. This field is read-only.
    /// </summary>
    public static readonly string True;

    /// <summary>
    /// Represents JavaScript's boolean value false as a string. This field is read-only.
    /// </summary>
    public static readonly string False;

    /// <summary>
    /// Represents JavaScript's null as a string. This field is read-only.
    /// </summary>
    public static readonly string Null;

    /// <summary>
    /// Represents JavaScript's undefined as a string. This field is read-only.
    /// </summary>
    public static readonly string Undefined;

    internal static long InitialJavaScriptDateTicks;
    internal static DateTime MinimumJavaScriptDate;

    static JavaScriptConvert()
    {
      True = "true";
      False = "false";
      Null = "null";
      Undefined = "undefined";

      InitialJavaScriptDateTicks = (new DateTime(1970, 1, 1)).Ticks;
      MinimumJavaScriptDate = new DateTime(100, 1, 1);
    }

    /// <summary>
    /// Converts the <see cref="DateTime"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="DateTime"/>.</returns>
    public static string ToString(DateTime value)
    {
      long javaScriptTicks = ConvertDateTimeToJavaScriptTicks(value);

      return "new Date(" + javaScriptTicks + ")";
    }

    internal static long ConvertDateTimeToJavaScriptTicks(DateTime dateTime)
    {
      if (dateTime < MinimumJavaScriptDate)
        dateTime = MinimumJavaScriptDate;

      long javaScriptTicks = (dateTime.Ticks - InitialJavaScriptDateTicks) / (long)10000;

      return javaScriptTicks;
    }

    internal static DateTime ConvertJavaScriptTicksToDateTime(long javaScriptTicks)
    {
      DateTime dateTime = new DateTime((javaScriptTicks * 10000) + InitialJavaScriptDateTicks);

      return dateTime;
    }

    /// <summary>
    /// Converts the <see cref="Boolean"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Boolean"/>.</returns>
    public static string ToString(bool value)
    {
      return (value) ? True : False;
    }

    /// <summary>
    /// Converts the <see cref="Char"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Char"/>.</returns>
    public static string ToString(char value)
    {
      return ToString(char.ToString(value));
    }

    /// <summary>
    /// Converts the <see cref="Enum"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Enum"/>.</returns>
    public static string ToString(Enum value)
    {
      return value.ToString();
    }

    /// <summary>
    /// Converts the <see cref="Int32"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Int32"/>.</returns>
    public static string ToString(int value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int16"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Int16"/>.</returns>
    public static string ToString(short value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt16"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="UInt16"/>.</returns>
    public static string ToString(ushort value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt32"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="UInt32"/>.</returns>
    public static string ToString(uint value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Int64"/>  to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Int64"/>.</returns>
    public static string ToString(long value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="UInt64"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="UInt64"/>.</returns>
    public static string ToString(ulong value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Single"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Single"/>.</returns>
    public static string ToString(float value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Double"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Double"/>.</returns>
    public static string ToString(double value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Byte"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Byte"/>.</returns>
    public static string ToString(byte value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="SByte"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="SByte"/>.</returns>
    public static string ToString(sbyte value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Decimal"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="SByte"/>.</returns>
    public static string ToString(decimal value)
    {
      return value.ToString(null, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Converts the <see cref="Guid"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Guid"/>.</returns>
    public static string ToString(Guid value)
    {
      return '"' + value.ToString("D", CultureInfo.InvariantCulture) + '"';
    }

    /// <summary>
    /// Converts the <see cref="String"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value)
    {
      return ToString(value, '"');
    }

    /// <summary>
    /// Converts the <see cref="String"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="delimter">The string delimiter character.</param>
    /// <returns>A Json string representation of the <see cref="String"/>.</returns>
    public static string ToString(string value, char delimter)
    {
      return JavaScriptUtils.ToEscapedJavaScriptString(value, delimter, true);
    }

    /// <summary>
    /// Converts the <see cref="Object"/> to it's JavaScript string representation.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <returns>A Json string representation of the <see cref="Object"/>.</returns>
    public static string ToString(object value)
    {
      if (value == null)
      {
        return Null;
      }
      else if (value is IConvertible)
      {
        IConvertible convertible = value as IConvertible;

        switch (convertible.GetTypeCode())
        {
          case TypeCode.String:
            return ToString((string)convertible);
          case TypeCode.Char:
            return ToString((char)convertible);
          case TypeCode.Boolean:
            return ToString((bool)convertible);
          case TypeCode.SByte:
            return ToString((sbyte)convertible);
          case TypeCode.Int16:
            return ToString((short)convertible);
          case TypeCode.UInt16:
            return ToString((ushort)convertible);
          case TypeCode.Int32:
            return ToString((int)convertible);
          case TypeCode.Byte:
            return ToString((byte)convertible);
          case TypeCode.UInt32:
            return ToString((uint)convertible);
          case TypeCode.Int64:
            return ToString((long)convertible);
          case TypeCode.UInt64:
            return ToString((ulong)convertible);
          case TypeCode.Single:
            return ToString((float)convertible);
          case TypeCode.Double:
            return ToString((double)convertible);
          case TypeCode.DateTime:
            return ToString((DateTime)convertible);
          case TypeCode.Decimal:
            return ToString((decimal)convertible);
        }
      }

      throw new ArgumentException(string.Format("Unsupported type: {0}. Use the JsonSerializer class to get the object's JSON representation.", value.GetType()));
    }

    /// <summary>
    /// Serializes the specified object to a Json object.
    /// </summary>
    /// <param name="value">The object to serialize.</param>
    /// <returns>A Json string representation of the object.</returns>
    public static string SerializeObject(object value)
    {
      return SerializeObject(value, null);
    }

    public static string SerializeObject(object value, params JsonConverter[] converters)
    {
      StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
      JsonSerializer jsonSerializer = new JsonSerializer();

      if (!CollectionUtils.IsNullOrEmpty<JsonConverter>(converters))
      {
        for (int i = 0; i < converters.Length; i++)
        {
          jsonSerializer.Converters.Add(converters[i]);
        }
      }

      using (JsonWriter jsonWriter = new JsonWriter(sw))
      {
        //jsonWriter.Formatting = Formatting.Indented;
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
      return DeserializeObject(value, null, null);
    }

    /// <summary>
    /// Deserializes the specified object to a Json object.
    /// </summary>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="type">The <see cref="Type"/> of object being deserialized.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static object DeserializeObject(string value, Type type)
    {
      return DeserializeObject(value, type, null);
    }

    /// <summary>
    /// Deserializes the specified object to a Json object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static T DeserializeObject<T>(string value)
    {
      return DeserializeObject<T>(value, null);
    }

    /// <summary>
    /// Deserializes the specified object to a Json object.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="value">The object to deserialize.</param>
    /// <param name="converters">Converters to use while deserializing.</param>
    /// <returns>The deserialized object from the Json string.</returns>
    public static T DeserializeObject<T>(string value, params JsonConverter[] converters)
    {
      return (T)DeserializeObject(value, typeof(T), converters);
    }

    public static object DeserializeObject(string value, Type type, params JsonConverter[] converters)
    {
      StringReader sr = new StringReader(value);
      JsonSerializer jsonSerializer = new JsonSerializer();

      if (!CollectionUtils.IsNullOrEmpty<JsonConverter>(converters))
      {
        for (int i = 0; i < converters.Length; i++)
        {
          jsonSerializer.Converters.Add(converters[i]);
        }
      }

      object deserializedValue;

      using (JsonReader jsonReader = new JsonReader(sr))
      {
        deserializedValue = jsonSerializer.Deserialize(jsonReader, type);
      }

      return deserializedValue;
    }

    public static string SerializeXmlNode(XmlNode node)
    {
      XmlNodeConverter converter = new XmlNodeConverter();

      return SerializeObject(node, converter);
    }

    public static XmlNode DeerializeXmlNode(string value)
    {
      XmlNodeConverter converter = new XmlNodeConverter();

      return (XmlDocument)DeserializeObject(value, typeof(XmlDocument), converter);
    }
  }
}