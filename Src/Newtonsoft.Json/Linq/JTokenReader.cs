using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
  /// </summary>
  public class JTokenReader : JsonReader, IJsonLineInfo
  {
    private readonly JToken _root;
    private JToken _parent;
    private JToken _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="JTokenReader"/> class.
    /// </summary>
    /// <param name="token">The token to read from.</param>
    public JTokenReader(JToken token)
    {
      ValidationUtils.ArgumentNotNull(token, "token");

      _root = token;
      _current = token;
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null.
    /// </returns>
    public override byte[] ReadAsBytes()
    {
      Read();

      if (IsWrappedInTypeObject())
      {
        byte[] data = ReadAsBytes();
        Read();
        SetToken(JsonToken.Bytes, data);
        return data;
      }

      // attempt to convert possible base 64 string to bytes
      if (TokenType == JsonToken.String)
      {
        string s = (string) Value;
        byte[] data = (s.Length == 0) ? new byte[0] : Convert.FromBase64String(s);
        SetToken(JsonToken.Bytes, data);
      }

      if (TokenType == JsonToken.Null)
        return null;
      if (TokenType == JsonToken.Bytes)
        return (byte[])Value;

      if (ReaderIsSerializerInArray())
        return null;

      throw CreateReaderException(this, "Error reading bytes. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
    }

    private bool IsWrappedInTypeObject()
    {
      if (TokenType == JsonToken.StartObject)
      {
        Read();
        if (Value.ToString() == "$type")
        {
          Read();
          if (Value != null && Value.ToString().StartsWith("System.Byte[]"))
          {
            Read();
            if (Value.ToString() == "$value")
            {
              return true;
            }
          }
        }

        throw CreateReaderException(this, "Unexpected token when reading bytes: {0}.".FormatWith(CultureInfo.InvariantCulture, JsonToken.StartObject));
      }

      return false;
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Decimal}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Decimal}"/>.</returns>
    public override decimal? ReadAsDecimal()
    {
      Read();

      if (TokenType == JsonToken.Integer || TokenType == JsonToken.Float)
      {
        SetToken(JsonToken.Float, Convert.ToDecimal(Value, CultureInfo.InvariantCulture));
        return (decimal) Value;
      }

      if (TokenType == JsonToken.Null)
        return null;

      decimal d;
      if (TokenType == JsonToken.String)
      {
        if (decimal.TryParse((string)Value, NumberStyles.Number, Culture, out d))
        {
          SetToken(JsonToken.Float, d);
          return d;
        }
        else
        {
          throw CreateReaderException(this, "Could not convert string to decimal: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
        }
      }

      if (ReaderIsSerializerInArray())
        return null;

      throw CreateReaderException(this, "Error reading decimal. Expected a number but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
    }

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{Int32}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{Int32}"/>.</returns>
    public override int? ReadAsInt32()
    {
      Read();

      if (TokenType == JsonToken.Integer || TokenType == JsonToken.Float)
      {
        SetToken(JsonToken.Integer, Convert.ToInt32(Value, CultureInfo.InvariantCulture));
        return (int)Value;
      }

      if (TokenType == JsonToken.Null)
        return null;

      int i;
      if (TokenType == JsonToken.String)
      {
        if (int.TryParse((string)Value, NumberStyles.Integer, Culture, out i))
        {
          SetToken(JsonToken.Integer, i);
          return i;
        }
        else
        {
          throw CreateReaderException(this, "Could not convert string to integer: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
        }
      }

      if (ReaderIsSerializerInArray())
        return null;

      throw CreateReaderException(this, "Error reading integer. Expected a number but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
    }

#if !NET20
    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="Nullable{DateTimeOffset}"/>.
    /// </summary>
    /// <returns>A <see cref="Nullable{DateTimeOffset}"/>.</returns>
    public override DateTimeOffset? ReadAsDateTimeOffset()
    {
      Read();

      if (TokenType == JsonToken.Date)
      {
        SetToken(JsonToken.Date, new DateTimeOffset((DateTime)Value));
        return (DateTimeOffset)Value;
      }

      if (TokenType == JsonToken.Null)
        return null;

      DateTimeOffset dt;
      if (TokenType == JsonToken.String)
      {
        if (DateTimeOffset.TryParse((string)Value, Culture, DateTimeStyles.None, out dt))
        {
          SetToken(JsonToken.Date, dt);
          return dt;
        }
        else
        {
          throw CreateReaderException(this, "Could not convert string to DateTimeOffset: {0}.".FormatWith(CultureInfo.InvariantCulture, Value));
        }
      }
      
      if (ReaderIsSerializerInArray())
        return null;

      throw CreateReaderException(this, "Error reading date. Expected date but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
    }
#endif

    /// <summary>
    /// Reads the next JSON token from the stream.
    /// </summary>
    /// <returns>
    /// true if the next token was read successfully; false if there are no more tokens to read.
    /// </returns>
    public override bool Read()
    {
      if (CurrentState != State.Start)
      {
        JContainer container = _current as JContainer;
        if (container != null && _parent != container)
          return ReadInto(container);
        else
          return ReadOver(_current);
      }

      SetToken(_current);
      return true;
    }

    private bool ReadOver(JToken t)
    {
      if (t == _root)
        return ReadToEnd();

      JToken next = t.Next;
      if ((next == null || next == t) || t == t.Parent.Last)
      {
        if (t.Parent == null)
          return ReadToEnd();

        return SetEnd(t.Parent);
      }
      else
      {
        _current = next;
        SetToken(_current);
        return true;
      }
    }

    private bool ReadToEnd()
    {
      //CurrentState = State.Finished;
      return false;
    }

    private bool IsEndElement
    {
      get { return (_current == _parent); }
    }

    private JsonToken? GetEndToken(JContainer c)
    {
      switch (c.Type)
      {
        case JTokenType.Object:
          return JsonToken.EndObject;
        case JTokenType.Array:
          return JsonToken.EndArray;
        case JTokenType.Constructor:
          return JsonToken.EndConstructor;
        case JTokenType.Property:
          return null;
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", c.Type, "Unexpected JContainer type.");
      }
    }

    private bool ReadInto(JContainer c)
    {
      JToken firstChild = c.First;
      if (firstChild == null)
      {
        return SetEnd(c);
      }
      else
      {
        SetToken(firstChild);
        _current = firstChild;
        _parent = c;
        return true;
      }
    }

    private bool SetEnd(JContainer c)
    {
      JsonToken? endToken = GetEndToken(c);
      if (endToken != null)
      {
        SetToken(endToken.Value);
        _current = c;
        _parent = c;
        return true;
      }
      else
      {
        return ReadOver(c);
      }
    }

    private void SetToken(JToken token)
    {
      switch (token.Type)
      {
        case JTokenType.Object:
          SetToken(JsonToken.StartObject);
          break;
        case JTokenType.Array:
          SetToken(JsonToken.StartArray);
          break;
        case JTokenType.Constructor:
          SetToken(JsonToken.StartConstructor);
          break;
        case JTokenType.Property:
          SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
          break;
        case JTokenType.Comment:
          SetToken(JsonToken.Comment, ((JValue)token).Value);
          break;
        case JTokenType.Integer:
          SetToken(JsonToken.Integer, ((JValue)token).Value);
          break;
        case JTokenType.Float:
          SetToken(JsonToken.Float, ((JValue)token).Value);
          break;
        case JTokenType.String:
          SetToken(JsonToken.String, ((JValue)token).Value);
          break;
        case JTokenType.Boolean:
          SetToken(JsonToken.Boolean, ((JValue)token).Value);
          break;
        case JTokenType.Null:
          SetToken(JsonToken.Null, ((JValue)token).Value);
          break;
        case JTokenType.Undefined:
          SetToken(JsonToken.Undefined, ((JValue)token).Value);
          break;
        case JTokenType.Date:
          SetToken(JsonToken.Date, ((JValue)token).Value);
          break;
        case JTokenType.Raw:
          SetToken(JsonToken.Raw, ((JValue)token).Value);
          break;
        case JTokenType.Bytes:
          SetToken(JsonToken.Bytes, ((JValue)token).Value);
          break;
        case JTokenType.Guid:
          SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
          break;
        case JTokenType.Uri:
          SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
          break;
        case JTokenType.TimeSpan:
          SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
          break;
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", token.Type, "Unexpected JTokenType.");
      }
    }

    private string SafeToString(object value)
    {
      return (value != null) ? value.ToString() : null;
    }

    bool IJsonLineInfo.HasLineInfo()
    {
      if (CurrentState == State.Start)
        return false;

      IJsonLineInfo info = IsEndElement ? null : _current;
      return (info != null && info.HasLineInfo());
    }

    int IJsonLineInfo.LineNumber
    {
      get
      {
        if (CurrentState == State.Start)
          return 0;

        IJsonLineInfo info = IsEndElement ? null : _current;
        if (info != null)
          return info.LineNumber;
        
        return 0;
      }
    }

    int IJsonLineInfo.LinePosition
    {
      get
      {
        if (CurrentState == State.Start)
          return 0;

        IJsonLineInfo info = IsEndElement ? null : _current;
        if (info != null)
          return info.LinePosition;

        return 0;
      }
    }
  }
}