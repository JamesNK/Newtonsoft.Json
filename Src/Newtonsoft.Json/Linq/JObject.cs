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
using System.IO;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
  public class JObject : JContainer
  {
    public JObject()
    {
    }

    public JObject(JObject other)
      : base(other)
    {
    }

    public JObject(params object[] content)
      : this((object)content)
    {
    }

    public JObject(object content)
    {
      Add(content);
    }

    internal override bool DeepEquals(JToken node)
    {
      JObject t = node as JObject;
      return (t != null && ContentsEqual(t));
    }

    internal override JToken CloneNode()
    {
      return new JObject(this);
    }

    public override JsonTokenType Type
    {
      get { return JsonTokenType.Object; }
    }

    public IEnumerable<JProperty> Properties()
    {
      return Children().Cast<JProperty>();
    }

    public JProperty Property(string name)
    {
      return Properties()
        .Where(p => string.Compare(p.Name, name, StringComparison.Ordinal) == 0)
        .SingleOrDefault();
    }

    public JEnumerable<JToken> PropertyValues()
    {
      return new JEnumerable<JToken>(Properties().Select(p => p.Value));
    }

    public override void Add(object content)
    {
      ValidationUtils.ArgumentNotNull(content, "content");

      if (!(content is JProperty) && !IsMultiContent(content))
        throw new ArgumentException("Error adding {0} to JObject. JObject only supports JProperty content.".FormatWith(content.GetType().Name));

      base.Add(content);
    }

    public override JToken this[object key]
    {
      get
      {
        ValidationUtils.ArgumentNotNull(key, "o");

        string propertyName = key as string;
        if (propertyName == null)
          throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(MiscellaneousUtils.ToString(key)));

        JProperty property = Property(propertyName);

        return (property != null) ? property.Value : null;
      }
    }

    public static JObject Load(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      if (reader.TokenType == JsonToken.None)
      {
        if (!reader.Read())
          throw new Exception("Error reading JObject from JsonReader.");
      }
      if (reader.TokenType != JsonToken.StartObject)
      {
        throw new Exception("Current JsonReader item is not an object.");
      }
      else
      {
        if (!reader.Read())
          throw new Exception("Error reading JObject from JsonReader.");
      }

      JObject o = new JObject();
      o.ReadContentFrom(reader);

      return o;
    }

    public static JObject Parse(string json)
    {
      JsonReader jsonReader = new JsonTextReader(new StringReader(json));

      return Load(jsonReader);
    }

    public static JObject FromObject(object o)
    {
      JToken token = FromObjectInternal(o);

      if (token.Type != JsonTokenType.Object)
        throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(token.Type));

      return (JObject)token;
    }

    internal override void ValidateObject(JToken o, JToken previous)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type != JsonTokenType.Property)
        throw new ArgumentException("An item of type {0} cannot be added to content.".FormatWith(o.Type));
    }

    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WriteStartObject();

      foreach (JProperty property in Properties())
      {
        property.WriteTo(writer, converters);
      }

      writer.WriteEndObject();
    }
  }
}