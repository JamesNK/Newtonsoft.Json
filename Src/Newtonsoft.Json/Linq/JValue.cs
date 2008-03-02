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

namespace Newtonsoft.Json.Linq
{
  public class JValue : JToken
  {
    private JsonTokenType _valueType;
    private object _value;

    public static readonly JValue Null = new JValue(null, JsonTokenType.Null);
    public static readonly JValue Undefined = new JValue(null, JsonTokenType.Undefined);

    private JValue(object value, JsonTokenType type)
    {
      _value = value;
      _valueType = type;
    }

    public JValue(long value)
      : this(value, JsonTokenType.Integer)
    {
    }

    public JValue(double value)
      : this(value, JsonTokenType.Float)
    {
    }

    public JValue(DateTime value)
      : this(value, JsonTokenType.Date)
    {
    }

    public JValue(bool value)
      : this(value, JsonTokenType.Boolean)
    {
    }

    public JValue(string value)
      : this(value, JsonTokenType.String)
    {
    }

    public JValue(object value) : this(value, GetValueType(value))
    {
    }

    public static JValue CreateComment(string value)
    {
      return new JValue(value, JsonTokenType.Comment);
    }

    private static JsonTokenType GetValueType(object value)
    {
      if (value == null)
        return JsonTokenType.Null;
      else if (value is string)
        return JsonTokenType.String;
      else if (value is long || value is int || value is short || value is sbyte
        || value is ulong || value is uint || value is ushort || value is byte)
        return JsonTokenType.Integer;
      else if (value is double || value is float || value is decimal)
        return JsonTokenType.Float;
      else if (value is DateTime || value is DateTimeOffset)
        return JsonTokenType.Date;
      else if (value is bool)
        return JsonTokenType.Boolean;

      throw new ArgumentException(string.Format("Could not determin JSON object type for type {0}.", value.GetType()));
    }

    public override JsonTokenType Type
    {
      get { return _valueType; }
    }

    public object Value
    {
      get { return _value; }
      set { _value = value; }
    }

    public static JToken CreateString(string value)
    {
      return new JValue(value, JsonTokenType.String);
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

    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      switch (_valueType)
      {
        case JsonTokenType.Comment:
          writer.WriteComment(_value.ToString());
          break;
        case JsonTokenType.Integer:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToInt64(v)), _value);
          break;
        case JsonTokenType.Float:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToDouble(v)), _value);
          break;
        case JsonTokenType.String:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(v.ToString()), _value);
          break;
        case JsonTokenType.Boolean:
          WriteConvertableValue(writer, converters, v => writer.WriteValue(Convert.ToBoolean(v)), _value);
          break;
        case JsonTokenType.Date:
          WriteConvertableValue(writer, converters,  v =>
          {
            if (v is DateTimeOffset)
              writer.WriteValue((DateTimeOffset)v);
            else
              writer.WriteValue(Convert.ToDateTime(v));
          }, _value);
          break;
        case JsonTokenType.Null:
          writer.WriteNull();
          break;
        case JsonTokenType.Undefined:
          writer.WriteUndefined();
          break;
        default:
          throw new ArgumentOutOfRangeException("TokenType", _valueType, "Unexpected token type.");
      }
    }
  }
}
