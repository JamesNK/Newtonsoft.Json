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
      else if (value is long || value is int || value is short
        || value is ulong || value is uint || value is ushort)
        return JsonTokenType.Integer;
      else if (value is double || value is float)
        return JsonTokenType.Float;
      else if (value is DateTime)
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

    public override void WriteTo(JsonWriter writer)
    {
      switch (_valueType)
      {
        case JsonTokenType.Comment:
          writer.WriteComment(_value.ToString());
          break;
        case JsonTokenType.Integer:
          writer.WriteValue(Convert.ToInt64(_value));
          break;
        case JsonTokenType.Float:
          writer.WriteValue(Convert.ToDouble(_value));
          break;
        case JsonTokenType.String:
          writer.WriteValue(_value.ToString());
          break;
        case JsonTokenType.Boolean:
          writer.WriteValue(Convert.ToBoolean(_value));
          break;
        case JsonTokenType.Null:
          writer.WriteNull();
          break;
        case JsonTokenType.Undefined:
          writer.WriteUndefined();
          break;
        case JsonTokenType.Date:
          writer.WriteValue(Convert.ToDateTime(_value));
          break;
        default:
          throw new ArgumentOutOfRangeException("TokenType", _valueType, "Unexpected token type.");
      }
    }
  }
}
