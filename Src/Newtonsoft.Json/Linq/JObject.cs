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
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a JSON object.
  /// </summary>
  public class JObject : JContainer, IDictionary<string, JToken>
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class.
    /// </summary>
    public JObject()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class from another <see cref="JObject"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JObject"/> object to copy from.</param>
    public JObject(JObject other)
      : base(other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class with the specified content.
    /// </summary>
    /// <param name="content">The contents of the object.</param>
    public JObject(params object[] content)
      : this((object)content)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JObject"/> class with the specified content.
    /// </summary>
    /// <param name="content">The contents of the object.</param>
    public JObject(object content)
    {
      Add(content);
    }

    internal override bool DeepEquals(JToken node)
    {
      JObject t = node as JObject;
      return (t != null && ContentsEqual(t));
    }

    internal override void ValidateToken(JToken o)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type != JsonTokenType.Property)
        throw new Exception("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));

      JProperty property = (JProperty)o;
      bool matchingProperty = (Properties().Where(p => string.Compare(p.Name, property.Name, StringComparison.Ordinal) == 0).Count() > 0);
      if (matchingProperty)
        throw new Exception("Can not add property {0} to {1}. Property with the same name already exists on object.".FormatWith(CultureInfo.InvariantCulture, property.Name, GetType()));
    }

    internal override JToken CloneNode()
    {
      return new JObject(this);
    }

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    /// <value>The type.</value>
    public override JsonTokenType Type
    {
      get { return JsonTokenType.Object; }
    }

    /// <summary>
    /// Gets an <see cref="IEnumerable{JProperty}"/> of this object's properties.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{JProperty}"/> of this object's properties.</returns>
    public IEnumerable<JProperty> Properties()
    {
      return Children().Cast<JProperty>();
    }

    /// <summary>
    /// Gets a <see cref="JProperty"/> the specified name.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <returns>A <see cref="JProperty"/> with the specified name or null.</returns>
    public JProperty Property(string name)
    {
      return Properties()
        .Where(p => string.Compare(p.Name, name, StringComparison.Ordinal) == 0)
        .SingleOrDefault();
    }

    /// <summary>
    /// Gets an <see cref="JEnumerable{JToken}"/> of this object's property values.
    /// </summary>
    /// <returns>An <see cref="JEnumerable{JToken}"/> of this object's property values.</returns>
    public JEnumerable<JToken> PropertyValues()
    {
      return new JEnumerable<JToken>(Properties().Select(p => p.Value));
    }

    /// <summary>
    /// Gets the <see cref="JToken"/> with the specified key.
    /// </summary>
    /// <value>The <see cref="JToken"/> with the specified key.</value>
    public override JToken this[object key]
    {
      get
      {
        ValidationUtils.ArgumentNotNull(key, "o");

        string propertyName = key as string;
        if (propertyName == null)
          throw new ArgumentException("Accessed JObject values with invalid key value: {0}. Object property name expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));

        return this[propertyName];
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Newtonsoft.Json.Linq.JToken"/> with the specified property name.
    /// </summary>
    /// <value></value>
    public JToken this[string propertyName]
    {
      get
      {
        ValidationUtils.ArgumentNotNull(propertyName, "propertyName");

        JProperty property = Property(propertyName);

        return (property != null) ? property.Value : null;
      }
      set
      {
        JProperty property = Property(propertyName);
        if (property != null)
          property.Value = value;
        else
          Add(new JProperty(propertyName, value));
      }
    }

    /// <summary>
    /// Loads an <see cref="JObject"/> from a <see cref="JsonReader"/>. 
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JObject"/>.</param>
    /// <returns>A <see cref="JObject"/> that contains the JSON that was read from the specified <see cref="JsonReader"/>.</returns>
    public static JObject Load(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      if (reader.TokenType == JsonToken.None)
      {
        if (!reader.Read())
          throw new Exception("Error reading JObject from JsonReader.");
      }
      if (reader.TokenType != JsonToken.StartObject)
        throw new Exception(
          "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(
            CultureInfo.InvariantCulture, reader.TokenType));

      JObject o = new JObject();
      o.SetLineInfo(reader as IJsonLineInfo);
      
      if (!reader.Read())
        throw new Exception("Error reading JObject from JsonReader.");

      o.ReadContentFrom(reader);

      return o;
    }

    /// <summary>
    /// Load a <see cref="JObject"/> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="String"/> that contains JSON.</param>
    /// <returns>A <see cref="JObject"/> populated from the string that contains JSON.</returns>
    public static JObject Parse(string json)
    {
      JsonReader jsonReader = new JsonTextReader(new StringReader(json));

      return Load(jsonReader);
    }

    /// <summary>
    /// Creates a <see cref="JObject"/> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="JObject"/>.</param>
    /// <returns>A <see cref="JObject"/> with the values of the specified object</returns>
    public static new JObject FromObject(object o)
    {
      JToken token = FromObjectInternal(o);

      if (token.Type != JsonTokenType.Object)
        throw new ArgumentException("Object serialized to {0}. JObject instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));

      return (JObject)token;
    }

    internal override void ValidateObject(JToken o, JToken previous)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type != JsonTokenType.Property)
        throw new ArgumentException("An item of type {0} cannot be added to content.".FormatWith(CultureInfo.InvariantCulture, o.Type));
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
    /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WriteStartObject();

      foreach (JProperty property in Properties())
      {
        property.WriteTo(writer, converters);
      }

      writer.WriteEndObject();
    }

    #region IDictionary<string,JToken> Members
    /// <summary>
    /// Adds the specified property name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">The value.</param>
    public void Add(string propertyName, JToken value)
    {
      Add(new JProperty(propertyName, value));
    }

    bool IDictionary<string, JToken>.ContainsKey(string key)
    {
      return (Property(key) != null);
    }

    ICollection<string> IDictionary<string, JToken>.Keys
    {
      get { throw new NotImplementedException(); }
    }

    /// <summary>
    /// Removes the property with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <returns>true if item was successfully removed; otherwise, false.</returns>
    public bool Remove(string propertyName)
    {
      JProperty property = Property(propertyName);
      if (property == null)
        return false;

      property.Remove();
      return true;
    }

    /// <summary>
    /// Tries the get value.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    /// <param name="value">The value.</param>
    /// <returns>true if a value was successfully retrieved; otherwise, false.</returns>
    public bool TryGetValue(string propertyName, out JToken value)
    {
      JProperty property = Property(propertyName);
      if (property == null)
      {
        value = null;
        return false;
      }

      value = property.Value;
      return true;
    }

    ICollection<JToken> IDictionary<string, JToken>.Values
    {
      get { throw new NotImplementedException(); }
    }

    #endregion

    #region ICollection<KeyValuePair<string,JToken>> Members

    void ICollection<KeyValuePair<string,JToken>>.Add(KeyValuePair<string, JToken> item)
    {
      Add(new JProperty(item.Key, item.Value));
    }

    void ICollection<KeyValuePair<string, JToken>>.Clear()
    {
      RemoveAll();
    }

    bool ICollection<KeyValuePair<string,JToken>>.Contains(KeyValuePair<string, JToken> item)
    {
      JProperty property = Property(item.Key);
      if (property == null)
        return false;

      return (property.Value == item.Value);
    }

    void ICollection<KeyValuePair<string,JToken>>.CopyTo(KeyValuePair<string, JToken>[] array, int arrayIndex)
    {
      if (array == null)
        throw new ArgumentNullException("array");
      if (arrayIndex < 0)
        throw new ArgumentOutOfRangeException("arrayIndex", "arrayIndex is less than 0.");
      if (arrayIndex >= array.Length)
        throw new ArgumentException("arrayIndex is equal to or greater than the length of array.");
      if (Count > array.Length - arrayIndex)
        throw new ArgumentException("The number of elements in the source JObject is greater than the available space from arrayIndex to the end of the destination array.");

      int index = 0;
      foreach (JProperty property in Properties())
      {
        array[arrayIndex + index] = new KeyValuePair<string, JToken>(property.Name, property.Value);
        index++;
      }
    }

    /// <summary>
    /// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <value></value>
    /// <returns>The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</returns>
    public int Count
    {
      get { return Children().Count(); }
    }

    bool ICollection<KeyValuePair<string,JToken>>.IsReadOnly
    {
      get { return false; }
    }

    bool ICollection<KeyValuePair<string,JToken>>.Remove(KeyValuePair<string, JToken> item)
    {
      if (!((ICollection<KeyValuePair<string,JToken>>)this).Contains(item))
        return false;

      ((IDictionary<string, JToken>)this).Remove(item.Key);
      return true;
    }

    #endregion

    #region IEnumerable<KeyValuePair<string,JToken>> Members

    IEnumerator<KeyValuePair<string, JToken>> IEnumerable<KeyValuePair<string,JToken>>.GetEnumerator()
    {
      foreach (JProperty property in Properties())
      {
        yield return new KeyValuePair<string, JToken>(property.Name, property.Value);
      }
    }

    #endregion

    internal override int GetDeepHashCode()
    {
      return ContentsHashCode();
    }
  }
}