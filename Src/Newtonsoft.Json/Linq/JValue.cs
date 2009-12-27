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
  public class JValue : JToken, IEquatable<JValue>
  {
    private JTokenType _valueType;
    private object _value;

    internal JValue(object value, JTokenType type)
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
      : this(value, JTokenType.Integer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(ulong value)
      : this(value, JTokenType.Integer)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(double value)
      : this(value, JTokenType.Float)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(DateTime value)
      : this(value, JTokenType.Date)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(bool value)
      : this(value, JTokenType.Boolean)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JValue"/> class with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    public JValue(string value)
      : this(value, JTokenType.String)
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

      return ValuesEquals(this, other);
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

    private static bool Compare(JTokenType valueType, object objA, object objB)
    {
      if (objA == null && objB == null)
        return true;
      if (objA == null || objB == null)
        return false;

      switch (valueType)
      {
        case JTokenType.Integer:
          if (objA is ulong || objB is ulong)
            return Convert.ToDecimal(objA, CultureInfo.InvariantCulture).Equals(Convert.ToDecimal(objB, CultureInfo.InvariantCulture));
          else
            return Convert.ToInt64(objA, CultureInfo.InvariantCulture).Equals(Convert.ToInt64(objB, CultureInfo.InvariantCulture));
        case JTokenType.Float:
          return Convert.ToDouble(objA, CultureInfo.InvariantCulture).Equals(Convert.ToDouble(objB, CultureInfo.InvariantCulture));
        case JTokenType.Comment:
        case JTokenType.String:
        case JTokenType.Boolean:
        case JTokenType.Raw:
          return objA.Equals(objB);
        case JTokenType.Date:
          return objA.Equals(objB);
        case JTokenType.Bytes:
          byte[] b1 = objA as byte[];
          byte[] b2 = objB as byte[];
          if (b1 == null || b2 == null)
            return false;

          return MiscellaneousUtils.ByteArrayCompare(b1, b2);
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("valueType", valueType, "Unexpected value type: {0}".FormatWith(CultureInfo.InvariantCulture, valueType));
      }
    }

    internal override JToken CloneToken()
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
      return new JValue(value, JTokenType.Comment);
    }

    /// <summary>
    /// Creates a <see cref="JValue"/> string with the given value.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A <see cref="JValue"/> string with the given value.</returns>
    public static JValue CreateString(string value)
    {
      return new JValue(value, JTokenType.String);
    }

    private static JTokenType GetValueType(JTokenType? current, object value)
    {
      if (value == null)
        return JTokenType.Null;
      else if (value == DBNull.Value)
        return JTokenType.Null;
      else if (value is string)
        return GetStringValueType(current);
      else if (value is long || value is int || value is short || value is sbyte
        || value is ulong || value is uint || value is ushort || value is byte)
        return JTokenType.Integer;
      else if (value is double || value is float || value is decimal)
        return JTokenType.Float;
      else if (value is DateTime)
        return JTokenType.Date;
#if !PocketPC && !NET20
      else if (value is DateTimeOffset)
        return JTokenType.Date;
#endif
      else if (value is byte[])
        return JTokenType.Bytes;
      else if (value is bool)
        return JTokenType.Boolean;

      throw new ArgumentException("Could not determine JSON object type for type {0}.".FormatWith(CultureInfo.InvariantCulture, value.GetType()));
    }

    private static JTokenType GetStringValueType(JTokenType? current)
    {
      if (current == null)
        return JTokenType.String;

      switch (current.Value)
      {
        case JTokenType.Comment:
        case JTokenType.String:
        case JTokenType.Raw:
          return current.Value;
        default:
          return JTokenType.String;
      }
    }

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    /// <value>The type.</value>
    public override JTokenType Type
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

    private static void WriteConvertableValue(JsonWriter writer, IList<JsonConverter> converters, Action<object> defaultWrite, object value)
    {
      JsonConverter matchingConverter;

      if (value != null && JsonSerializer.HasMatchingConverter(converters, value.GetType(), out matchingConverter))
        matchingConverter.WriteJson(writer, value, new JsonSerializer());
      else
        defaultWrite(value);
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
        case JTokenType.Comment:
          writer.WriteComment(_value.ToString());
          break;
        case JTokenType.Integer:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToInt64(v, CultureInfo.InvariantCulture)), _value);
          break;
        case JTokenType.Float:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToDouble(v, CultureInfo.InvariantCulture)), _value);
          break;
        case JTokenType.String:
          WriteConvertableValue(writer, converters, v => writer.WriteValue((v != null) ? v.ToString() : null), _value);
          break;
        case JTokenType.Boolean:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToBoolean(v, CultureInfo.InvariantCulture)), _value);
          break;
        case JTokenType.Date:
          WriteConvertableValue(writer, converters, v =>
          {
#if !PocketPC && !NET20
            if (v is DateTimeOffset)
              writer.WriteValue((DateTimeOffset)v);
            else
#endif
            writer.WriteValue(Convert.ToDateTime(v, CultureInfo.InvariantCulture));
          }, _value);
          break;
        case JTokenType.Raw:
          writer.WriteRawValue((string)_value);
          break;
        case JTokenType.Bytes:
          WriteConvertableValue(writer, converters, v => writer.WriteValue((byte[])v), _value);
          break;
        case JTokenType.Null:
          writer.WriteNull();
          break;
        case JTokenType.Undefined:
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

    private static bool ValuesEquals(JValue v1, JValue v2)
    {
      return (v1 == v2|| (v1._valueType == v2._valueType && Compare(v1._valueType, v1._value, v2._value)));
    }

    /// <summary>
    /// Indicates whether the current object is equal to another object of the same type.
    /// </summary>
    /// <returns>
    /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
    /// </returns>
    /// <param name="other">An object to compare with this object.</param>
    public bool Equals(JValue other)
    {
      if (other == null)
        return false;

      return ValuesEquals(this, other);
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
    /// </returns>
    /// <exception cref="T:System.NullReferenceException">
    /// The <paramref name="obj"/> parameter is null.
    /// </exception>
    public override bool Equals(object obj)
    {
      if (obj == null)
        return false;

      JValue otherValue = obj as JValue;
      if (otherValue != null)
        return Equals(otherValue);

      return base.Equals(obj);
    }

    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"/>.
    /// </returns>
    public override int GetHashCode()
    {
      if (_value == null)
        return 0;

      return _value.GetHashCode();
    }
  }
}