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
using System.IO;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  public class JArray : JContainer
  {
    public override JsonTokenType Type
    {
      get { return JsonTokenType.Array; }
    }

    public JArray()
    {
    }

    public JArray(JArray other)
      : base(other)
    {
    }

    public JArray(params object[] content)
      : this((object)content)
    {
    }

    public JArray(object content)
    {
      Add(content);
    }

    internal override bool DeepEquals(JToken node)
    {
      JArray t = node as JArray;
      return (t != null && ContentsEqual(t));
    }

    internal override JToken CloneNode()
    {
      return new JArray(this);
    }

    public int Count()
    {
      return Children().Count();
    }

    public static JArray Load(JsonReader reader)
    {
      if (reader.TokenType == JsonToken.None)
      {
        if (!reader.Read())
          throw new Exception("Error reading JArray from JsonReader.");
      }
      if (reader.TokenType != JsonToken.StartArray)
      {
        throw new Exception("Current JsonReader item is not an object.");
      }
      else
      {
        if (!reader.Read())
          throw new Exception("Error reading JArray from JsonReader.");
      }

      JArray a = new JArray();
      a.ReadContentFrom(reader);

      return a;
    }

    public static JArray Parse(string json)
    {
      JsonReader jsonReader = new JsonTextReader(new StringReader(json));

      return Load(jsonReader);
    }

    internal override void ValidateObject(JToken o, JToken previous)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type == JsonTokenType.Property)
        throw new ArgumentException("An item of type {0} cannot be added to content.".FormatWith(CultureInfo.InvariantCulture, o.Type));
    }

    public static JArray FromObject(object o)
    {
      JToken token = FromObjectInternal(o);

      if (token.Type != JsonTokenType.Array)
        throw new ArgumentException("Object serialized to {0}. JArray instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));

      return (JArray)token;
    }

    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WriteStartArray();

      foreach (JToken token in Children())
      {
        token.WriteTo(writer, converters);
      }

      writer.WriteEndArray();
    }

    public override JToken this[object key]
    {
      get
      {
        ValidationUtils.ArgumentNotNull(key, "o");

        if (!(key is int))
          throw new ArgumentException("Accessed JArray values with invalid key value: {0}. Array position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));

        return GetIndex(this, (int)key);
      }
    }
  }
}