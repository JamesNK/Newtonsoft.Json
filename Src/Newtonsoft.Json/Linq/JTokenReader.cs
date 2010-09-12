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

      // attempt to convert possible base 64 string to bytes
      if (TokenType == JsonToken.String)
      {
        string s = (string) Value;
        byte[] data = (s.Length == 0) ? new byte[0] : Convert.FromBase64String(s);
        SetToken(JsonToken.Bytes, data);
      }

      if (TokenType == JsonToken.Bytes)
        return (byte[])Value;
      if (TokenType == JsonToken.Null)
        return null;

      throw new JsonReaderException("Error reading bytes. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
    }

#if !NET20
    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="DateTimeOffset"/>.
    /// </summary>
    /// <returns>A <see cref="DateTimeOffset"/>.</returns>
    public override DateTimeOffset ReadAsDateTimeOffset()
    {
      Read();

      if (TokenType == JsonToken.Date)
        return new DateTimeOffset((DateTime)Value);

      throw new JsonReaderException("Error reading date. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));
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
        default:
          throw MiscellaneousUtils.CreateArgumentOutOfRangeException("Type", token.Type, "Unexpected JTokenType.");
      }
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