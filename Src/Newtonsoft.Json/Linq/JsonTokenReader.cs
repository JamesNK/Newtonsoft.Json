using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
  /// </summary>
  public class JsonTokenReader : JsonReader
  {
    private JToken _root;
    private JToken _parent;
    private JToken _current;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonTokenReader"/> class.
    /// </summary>
    /// <param name="token">The token to read from.</param>
    public JsonTokenReader(JToken token)
    {
      ValidationUtils.ArgumentNotNull(token, "token");

      _root = token;
      _current = token;
    }

    /// <summary>
    /// Reads the next Json token from the stream.
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

    private JsonToken? GetEndToken(JContainer c)
    {
      switch (c.Type)
      {
        case JsonTokenType.Object:
          return JsonToken.EndObject;
        case JsonTokenType.Array:
          return JsonToken.EndArray;
        case JsonTokenType.Constructor:
          return JsonToken.EndConstructor;
        case JsonTokenType.Property:
          return null;
        default:
          throw new ArgumentOutOfRangeException("Type", c.Type, "Unexpected JContainer type.");
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
        case JsonTokenType.Object:
          SetToken(JsonToken.StartObject);
          break;
        case JsonTokenType.Array:
          SetToken(JsonToken.StartArray);
          break;
        case JsonTokenType.Constructor:
          SetToken(JsonToken.StartConstructor);
          break;
        case JsonTokenType.Property:
          SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
          break;
        case JsonTokenType.Comment:
          SetToken(JsonToken.Comment, ((JValue)token).Value);
          break;
        case JsonTokenType.Integer:
          SetToken(JsonToken.Integer, ((JValue)token).Value);
          break;
        case JsonTokenType.Float:
          SetToken(JsonToken.Float, ((JValue)token).Value);
          break;
        case JsonTokenType.String:
          SetToken(JsonToken.String, ((JValue)token).Value);
          break;
        case JsonTokenType.Boolean:
          SetToken(JsonToken.Boolean, ((JValue)token).Value);
          break;
        case JsonTokenType.Null:
          SetToken(JsonToken.Null, ((JValue)token).Value);
          break;
        case JsonTokenType.Undefined:
          SetToken(JsonToken.Undefined, ((JValue)token).Value);
          break;
        case JsonTokenType.Date:
          SetToken(JsonToken.Date, ((JValue)token).Value);
          break;
        default:
          throw new ArgumentOutOfRangeException("Type", token.Type, "Unexpected JsonTokenType.");
      }
    }
  }
}
