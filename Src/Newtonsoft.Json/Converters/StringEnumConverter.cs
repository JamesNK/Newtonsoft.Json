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
#if NET20
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
    private readonly Dictionary<Type, BidirectionalDictionary<string, string>> _enumMemberNamesPerType = new Dictionary<Type, BidirectionalDictionary<string, string>>();

    /// <summary>
    /// Gets or sets a value indicating whether the written enum text should be camel case.
    /// </summary>
    /// <value><c>true</c> if the written enum text will be camel case; otherwise, <c>false</c>.</value>
    public bool CamelCaseText { get; set; }
    
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

      Enum e = (Enum)value;

      string enumName = e.ToString("G");

      if (char.IsNumber(enumName[0]) || enumName[0] == '-')
      {
        // enum value has no name so write number
        writer.WriteValue(value);
      }
      else
      {
        BidirectionalDictionary<string, string> map = GetEnumNameMap(e.GetType());

        string resolvedEnumName;
        map.TryGetByFirst(enumName, out resolvedEnumName);
        resolvedEnumName = resolvedEnumName ?? enumName;

        if (CamelCaseText)
        {
          string[] names = resolvedEnumName.Split(',').Select(item => StringUtils.ToCamelCase(item.Trim())).ToArray();
          resolvedEnumName = string.Join(", ", names);
        }

        writer.WriteValue(resolvedEnumName);
      }
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
      Type t = isNullable ? Nullable.GetUnderlyingType(objectType) : objectType;

      if (reader.TokenType == JsonToken.Null)
      {
        if (!ReflectionUtils.IsNullableType(objectType))
          throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(CultureInfo.InvariantCulture, objectType));

        return null;
      }

      try
      {
        if (reader.TokenType == JsonToken.String)
        {
          string enumText = reader.Value.ToString();
          if (enumText == string.Empty && isNullable)
            return null;

          var map = GetEnumNameMap(t);
          string resolvedEnumName;
          map.TryGetBySecond(enumText, out resolvedEnumName);
          resolvedEnumName = resolvedEnumName ?? enumText;

          return Enum.Parse(t, resolvedEnumName, true);
        }

        if (reader.TokenType == JsonToken.Integer)
          return ConvertUtils.ConvertOrCast(reader.Value, CultureInfo.InvariantCulture, t);
      }
      catch (Exception ex)
      {
        throw JsonSerializationException.Create(reader, "Error converting value {0} to type '{1}'.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.FormatValueForPrint(reader.Value), objectType), ex);
      }


      throw JsonSerializationException.Create(reader, "Unexpected token when parsing enum. Expected String or Integer, got {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
    }

    private BidirectionalDictionary<string, string> GetEnumNameMap(Type t)
    {
      BidirectionalDictionary<string, string> map;

      if (!_enumMemberNamesPerType.TryGetValue(t, out map))
      {
        lock (_enumMemberNamesPerType)
        {
          if (_enumMemberNamesPerType.TryGetValue(t, out map))
            return map;

          map = new BidirectionalDictionary<string, string>(
            StringComparer.OrdinalIgnoreCase,
            StringComparer.OrdinalIgnoreCase);

          foreach (FieldInfo f in t.GetFields())
          {
            string n1 = f.Name;
            string n2;
            
#if !NET20
            n2 = f.GetCustomAttributes(typeof (EnumMemberAttribute), true)
                          .Cast<EnumMemberAttribute>()
                          .Select(a => a.Value)
                          .SingleOrDefault() ?? f.Name;
#else
            n2 = f.Name;
#endif

            string s;
            if (map.TryGetBySecond(n2, out s))
            {
              throw new InvalidOperationException("Enum name '{0}' already exists on enum '{1}'."
                .FormatWith(CultureInfo.InvariantCulture, n2, t.Name));
            }

            map.Set(n1, n2);
          }

          _enumMemberNamesPerType[t] = map;
        }
      }

      return map;
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