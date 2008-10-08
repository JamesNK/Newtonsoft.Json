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
  /// <summary>
  /// Represents a JSON array.
  /// </summary>
  public class JArray : JContainer, IList<JToken>
  {
    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    /// <value>The type.</value>
    public override JsonTokenType Type
    {
      get { return JsonTokenType.Array; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JArray"/> class.
    /// </summary>
    public JArray()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JArray"/> class from another <see cref="JArray"/> object.
    /// </summary>
    /// <param name="other">A <see cref="JArray"/> object to copy from.</param>
    public JArray(JArray other)
      : base(other)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JArray"/> class with the specified content.
    /// </summary>
    /// <param name="content">The contents of the array.</param>
    public JArray(params object[] content)
      : this((object)content)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JArray"/> class with the specified content.
    /// </summary>
    /// <param name="content">The contents of the array.</param>
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

    /// <summary>
    /// Loads an <see cref="JArray"/> from a <see cref="JsonReader"/>. 
    /// </summary>
    /// <param name="reader">A <see cref="JsonReader"/> that will be read for the content of the <see cref="JArray"/>.</param>
    /// <returns>A <see cref="JArray"/> that contains the XML that was read from the specified <see cref="JsonReader"/>.</returns>
    public static JArray Load(JsonReader reader)
    {
      if (reader.TokenType == JsonToken.None)
      {
        if (!reader.Read())
          throw new Exception("Error reading JArray from JsonReader.");
      }
      if (reader.TokenType != JsonToken.StartArray)
      {
        throw new Exception("Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
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

    /// <summary>
    /// Load a <see cref="JArray"/> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="String"/> that contains JSON.</param>
    /// <returns>A <see cref="JArray"/> populated from the string that contains JSON.</returns>
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

    /// <summary>
    /// Creates a <see cref="JArray"/> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="JArray"/>.</param>
    /// <returns>A <see cref="JArray"/> with the values of the specified object</returns>
    public static JArray FromObject(object o)
    {
      JToken token = FromObjectInternal(o);

      if (token.Type != JsonTokenType.Array)
        throw new ArgumentException("Object serialized to {0}. JArray instance expected.".FormatWith(CultureInfo.InvariantCulture, token.Type));

      return (JArray)token;
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
    /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WriteStartArray();

      foreach (JToken token in Children())
      {
        token.WriteTo(writer, converters);
      }

      writer.WriteEndArray();
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

        if (!(key is int))
          throw new ArgumentException("Accessed JArray values with invalid key value: {0}. Array position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));

        return this[(int)key];
      }
    }

    /// <summary>
    /// Gets or sets the <see cref="Newtonsoft.Json.Linq.JToken"/> at the specified index.
    /// </summary>
    /// <value></value>
    public JToken this[int index]
    {
      get
      {
        return GetIndex(this, index);
      }
      set
      {
        JToken token = this[(object)index];
        token.Replace(value);
      }
    }

    #region IList<JToken> Members

    /// <summary>
    /// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
    /// <returns>
    /// The index of <paramref name="item"/> if found in the list; otherwise, -1.
    /// </returns>
    public int IndexOf(JToken item)
    {
      int index = 0;
      foreach (JToken token in Children())
      {
        if (token == item)
          return index;

        index++;
      }

      return -1;
    }

    /// <summary>
    /// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
    /// <param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
    public void Insert(int index, JToken item)
    {
      if (index == 0)
      {
        AddFirst(item);
      }
      else
      {
        JToken token = GetIndex(this, index);
        AddInternal(false, token.Previous, item);
      }
    }

    /// <summary>
    /// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the item to remove.</param>
    /// <exception cref="T:System.ArgumentOutOfRangeException">
    /// 	<paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
    public void RemoveAt(int index)
    {
      if (index < 0)
        throw new ArgumentOutOfRangeException("index", "index is less than 0.");

      int currentIndex = 0;
      foreach (JToken token in Children())
      {
        if (index == currentIndex)
        {
          token.Remove();
          return;
        }

        currentIndex++;
      }

      throw new ArgumentOutOfRangeException("index", "index is equal to or greater than Count.");
    }

    #endregion

    #region ICollection<JToken> Members

    /// <summary>
    /// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
    public void Add(JToken item)
    {
      Add((object)item);
    }

    /// <summary>
    /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
    public void Clear()
    {
      RemoveAll();
    }

    /// <summary>
    /// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
    /// </summary>
    /// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    /// <returns>
    /// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
    /// </returns>
    public bool Contains(JToken item)
    {
      return Children().Contains(item);
    }

    void ICollection<JToken>.CopyTo(JToken[] array, int arrayIndex)
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
      foreach (JToken token in Children())
      {
        array[arrayIndex + index] = token;
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

    bool ICollection<JToken>.IsReadOnly
    {
      get { return false; }
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </summary>
    /// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
    /// <returns>
    /// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
    /// </returns>
    /// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
    public new bool Remove(JToken item)
    {
      if (!((ICollection<JToken>)this).Contains(item))
        return false;

      item.Remove();
      return true;
    }

    #endregion

    internal override int GetDeepHashCode()
    {
      return ContentsHashCode();
    }
  }
}