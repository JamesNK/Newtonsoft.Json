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
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a value in JSON (string, integer, date, etc).
  /// </summary>
  public class JValue : JToken
  {
    private JsonTokenType _valueType;
    private object _value;

    internal JValue(object value, JsonTokenType type)
    {
      _value = value;
      _valueType = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class from another <see cref="JValue"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JValue"/> object to copy from.</param>
    public JValue(JValue other)
      : this(other.Value, other.Type)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(long value)
      : this(value, JsonTokenType.Integer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(ulong value)
      : this(value, JsonTokenType.Integer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(double value)
      : this(value, JsonTokenType.Float)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(DateTime value)
      : this(value, JsonTokenType.Date)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(bool value)
      : this(value, JsonTokenType.Boolean)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(string value)
      : this(value, JsonTokenType.String)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(object value)
      : this(value, GetValueType(null, value))
    {
    }

    internal override bool DeepEquals(JToken node)
    {
      JValue other = node as JValue;
      if (other == null)
        return false;

      return (this == other || (_valueType == other.Type && Compare(_value, other.Value)));
    }

    /// <summary>
    /// Gets a value indicating whether this token has childen tokens.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
    /// </value>
    public override bool HasValues
    {
      get { return false; }
    }

    private bool Compare(object objA, object objB)
    {
      if (objA == null && objB == null)
        return true;

      switch (_valueType)
      {
        case JsonTokenType.Integer:
          if (objA is ulong || objB is ulong)
            return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).Equals(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
          else
            return Convert.ToInt64(objA, CultureInfo.InvariantCulture).Equals(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
        case JsonTokenType.Float:
          return Convert.ToDouble(objA, CultureInfo.InvariantCulture).Equals(Convert.ToDouble(objB, CultureInfo.InvariantCulture));
        case JsonTokenType.Comment:
        case JsonTokenType.String:
        case JsonTokenType.Boolean:
          return objA.Equals(objB);
        case JsonTokenType.Date:
          return objA.Equals(objB);
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("valueType", _valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, _valueType));
      }
    }

    internal override JToken CloneNode()
    {
      return new JValue(this);
    }

    /// <summary>
    /// Creates a <see cref="JValue"/> comment with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="JValue"/> comment with the given value.</returns>
    public static JValue CreateComment(string value)
    {
      return new JValue(value, JsonTokenType.Comment);
    }

    /// <summary>
    /// Creates a <see cref="JValue"/> string with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="JValue"/> string with the given value.</returns>
    public static JValue CreateString(string value)
    {
      return new JValue(value, JsonTokenType.String);
    }

    private static JsonTokenType GetValueType(JsonTokenType? current, object value)
    {
      if (value == null)
        return JsonTokenType.Null;
      else if (value is string)
        return (current == JsonTokenType.Comment) ? JsonTokenType.Comment : JsonTokenType.String;
      else if (value is long || value is int || value is short || value is sbyte
        || value is ulong || value is uint || value is ushort || value is byte)
        return JsonTokenType.Integer;
      else if (value is double || value is float || value is decimal)
        return JsonTokenType.Float;
      else if (value is DateTime)
        return JsonTokenType.Date;
      else if (value is DateTimeOffset)
        return JsonTokenType.Date;
      else if (value is bool)
        return JsonTokenType.Boolean;

      throw new ArgumentException("Could not determin JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    /// <value>The type.</value>
    public override JsonTokenType Type
    {
      get { return _valueType; }
    }

    /// <summary>
    /// Gets or sets the underlying token value.
    /// </summary>
    /// <value>The underlying token value.</value>
    public object Value
    {
      get { return _value; }
      set
      {
        Type currentType = (_value != null) ? _value.GetType() : null;
        Type newType = (value != null) ? value.GetType() : null;

        if (currentType != newType)
          _valueType = GetValueType(_valueType, value);

        _value = value;
      }
    }

    private static void WriteConvertableValue(JsonWriter writer, IList<JsonConverter> converters, Action<object> _defaultWrite, object value)
    {
      JsonConverter matchingConverter;

      JsonSerializer.HasMatchingConverter(converters, value.GetType(), out matchingConverter);
      if (matchingConverter != null)
        matchingConverter.WriteJson(writer, value);
      else
        _defaultWrite(value);
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
    /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      switch (_valueType)
      {
        case JsonTokenType.Comment:
          writer.WriteComment(_value.ToString());
          break;
        case JsonTokenType.Integer:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToInt64(v, CultureInfo.InvariantCulture)), _value);
          break;
        case JsonTokenType.Float:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToDouble(v, CultureInfo.InvariantCulture)), _value);
          break;
        case JsonTokenType.String:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(v.ToString()), _value);
          break;
        case JsonTokenType.Boolean:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToBoolean(v, CultureInfo.InvariantCulture)), _value);
          break;
        case JsonTokenType.Date:
          WriteConvertableValue(writer, converters, v =>
          {
            if (v is DateTimeOffset)
              writer.WriteValue((DateTimeOffset)v);
            else
              writer.WriteValue(Convert.ToDateTime(v, CultureInfo.InvariantCulture));
          }, _value);
          break;
        case JsonTokenType.Null:
          writer.WriteNull();
          break;
        case JsonTokenType.Undefined:
          writer.WriteUndefined();
          break;
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("TokenType", _valueType, "Unexpected token type.");
      }
    }

    internal override int GetDeepHashCode()
    {
      int valueHashCode = (_value != null) ? _value.GetHashCode() : 0;
      
      return _valueType.GetHashCode() ^ valueHashCode;
    }
  }
}