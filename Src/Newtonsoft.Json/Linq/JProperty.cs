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
using System.Diagnostics;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a JSON property.
  /// </summary>
  public class JProperty : JContainer
  {
    private readonly string _name;

    /// <summary>
    /// Gets the property name.
    /// </summary>
    /// <value>The property name.</value>
    public string Name
    {
      [DebuggerStepThrough]
      get { return _name; }
    }

    /// <summary>
    /// Gets or sets the property value.
    /// </summary>
    /// <value>The property value.</value>
    public JToken Value
    {
      [DebuggerStepThrough]
      get { return Last; }
      set { ReplaceAll(value); }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JProperty"/> class from another <see cref="JProperty"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JProperty"/> object to copy from.</param>
    public JProperty(JProperty other)
      : base(other)
    {
      _name = other.Name;
    }

    internal override bool DeepEquals(JToken node)
    {
      JProperty t = node as JProperty;
      return (t != null && _name == t.Name && ContentsEqual(t));
    }

    internal override JToken CloneNode()
    {
      return new JProperty(this);
    }

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    /// <value>The type.</value>
    public override JTokenType Type
    {
      [DebuggerStepThrough]
      get { return JTokenType.Property; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JProperty"/> class.
    /// </summary>
    /// <param name="name">The property name.</param>
    public JProperty(string name)
    {
      ValidationUtils.ArgumentNotNull(name, "name");

      _name = name;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JProperty"/> class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="content">The property content.</param>
    public JProperty(string name, params object[] content)
      : this(name, (object)content)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JProperty"/> class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="content">The property content.</param>
    public JProperty(string name, object content)
    {
      ValidationUtils.ArgumentNotNull(name, "name");

      _name = name;

      Value = IsMultiContent(content)
        ? new JArray(content)
        : CreateFromContent(content);
    }

    internal override void ValidateObject(JToken o, JToken previous)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type == JTokenType.Property)
        throw new ArgumentException("An item of type {0} cannot be added to content.".FormatWith(CultureInfo.InvariantCulture, o.Type));
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
    /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WritePropertyName(_name);
      Value.WriteTo(writer, converters);
    }

    //public static explicit operator JValue(JProperty property)
    //{
    //  if (property == null)
    //    return null;

    //  JToken value = property.Value;
    //  if (value == null)
    //    return null;

    //  if (!(value is JValue))
    //    throw new Exception("Could not cast {0} to JValue".FormatWith(CultureInfo.InvariantCulture, value.GetType()));

    //  return (JValue)value;
    //}

    internal override int GetDeepHashCode()
    {
      return _name.GetHashCode() ^ ((Value != null) ? Value.GetDeepHashCode() : 0);
    }

    /// <summary>
    /// Loads an <see cref="JProperty"/> from a <see cref="JsonReader"/>. 
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JProperty"/>.</param>
    /// <returns>A <see cref="JProperty"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public static JProperty Load(JsonReader reader)
    {
      if (reader.TokenType == JsonToken.None)
      {
        if (!reader.Read())
          throw new Exception("Error reading JProperty from JsonReader.");
      }
      if (reader.TokenType != JsonToken.PropertyName)
        throw new Exception(
          "Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(
            CultureInfo.InvariantCulture, reader.TokenType));

      JProperty p = new JProperty((string)reader.Value);
      p.SetLineInfo(reader as IJsonLineInfo);

      if (!reader.Read())
        throw new Exception("Error reading JProperty from JsonReader.");

      p.ReadContentFrom(reader);

      return p;
    }
  }
}