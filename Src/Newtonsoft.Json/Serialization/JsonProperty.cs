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
using Newtonsoft.Json.Utilities;

#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#endif

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Maps a JSON property to a .NET member or constructor parameter.
  /// </summary>
  public class JsonProperty
  {
    internal Required? _required;
    internal bool _hasExplicitDefaultValue;
    internal object _defaultValue;

    private string _propertyName;
    private bool _skipPropertyNameEscape;

    // use to cache contract during deserialization
    internal JsonContract PropertyContract { get; set; }
    
    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    /// <value>The name of the property.</value>
    public string PropertyName
    {
      get { return _propertyName; }
      set
      {
        _propertyName = value;
        CalculateSkipPropertyNameEscape();
      }
    }

    private void CalculateSkipPropertyNameEscape()
    {
      if (_propertyName == null)
      {
        _skipPropertyNameEscape = false;
      }
      else
      {
        _skipPropertyNameEscape = true;
        foreach (char c in _propertyName)
        {
          if (!char.IsLetterOrDigit(c) && c != '_' && c != '@')
          {
            _skipPropertyNameEscape = false;
            break;
          }
        }
      }
    }

    /// <summary>
    /// Gets or sets the type that declared this property.
    /// </summary>
    /// <value>The type that declared this property.</value>
    public Type DeclaringType { get; set; }

    /// <summary>
    /// Gets or sets the order of serialization and deserialization of a member.
    /// </summary>
    /// <value>The numeric order of serialization or deserialization.</value>
    public int? Order { get; set; }

    /// <summary>
    /// Gets or sets the name of the underlying member or parameter.
    /// </summary>
    /// <value>The name of the underlying member or parameter.</value>
    public string UnderlyingName { get; set; }

    /// <summary>
    /// Gets the <see cref="IValueProvider"/> that will get and set the <see cref="JsonProperty"/> during serialization.
    /// </summary>
    /// <value>The <see cref="IValueProvider"/> that will get and set the <see cref="JsonProperty"/> during serialization.</value>
    public IValueProvider ValueProvider { get; set; }

    /// <summary>
    /// Gets or sets the type of the property.
    /// </summary>
    /// <value>The type of the property.</value>
    public Type PropertyType { get; set; }

    /// <summary>
    /// Gets or sets the <see cref="JsonConverter" /> for the property.
    /// If set this converter takes presidence over the contract converter for the property type.
    /// </summary>
    /// <value>The converter.</value>
    public JsonConverter Converter { get; set; }

    /// <summary>
    /// Gets the member converter.
    /// </summary>
    /// <value>The member converter.</value>
    public JsonConverter MemberConverter { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonProperty"/> is ignored.
    /// </summary>
    /// <value><c>true</c> if ignored; otherwise, <c>false</c>.</value>
    public bool Ignored { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonProperty"/> is readable.
    /// </summary>
    /// <value><c>true</c> if readable; otherwise, <c>false</c>.</value>
    public bool Readable { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonProperty"/> is writable.
    /// </summary>
    /// <value><c>true</c> if writable; otherwise, <c>false</c>.</value>
    public bool Writable { get; set; }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonProperty"/> has a member attribute.
    /// </summary>
    /// <value><c>true</c> if has a member attribute; otherwise, <c>false</c>.</value>
    public bool HasMemberAttribute { get; set; }

    /// <summary>
    /// Gets the default value.
    /// </summary>
    /// <value>The default value.</value>
    public object DefaultValue
    {
      get
      {
        return _defaultValue;
      }
      set
      {
        _hasExplicitDefaultValue = true;
        _defaultValue = value;
      }
    }

    internal object GetResolvedDefaultValue()
    {
      if (!_hasExplicitDefaultValue && PropertyType != null)
        return ReflectionUtils.GetDefaultValue(PropertyType);

      return _defaultValue;
    }

    /// <summary>
    /// Gets a value indicating whether this <see cref="JsonProperty"/> is required.
    /// </summary>
    /// <value>A value indicating whether this <see cref="JsonProperty"/> is required.</value>
    public Required Required
    {
      get { return _required ?? Required.Default; }
      set { _required = value; }
    }

    /// <summary>
    /// Gets a value indicating whether this property preserves object references.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this instance is reference; otherwise, <c>false</c>.
    /// </value>
    public bool? IsReference { get; set; }

    /// <summary>
    /// Gets the property null value handling.
    /// </summary>
    /// <value>The null value handling.</value>
    public NullValueHandling? NullValueHandling { get; set; }

    /// <summary>
    /// Gets the property default value handling.
    /// </summary>
    /// <value>The default value handling.</value>
    public DefaultValueHandling? DefaultValueHandling { get; set; }

    /// <summary>
    /// Gets the property reference loop handling.
    /// </summary>
    /// <value>The reference loop handling.</value>
    public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }

    /// <summary>
    /// Gets the property object creation handling.
    /// </summary>
    /// <value>The object creation handling.</value>
    public ObjectCreationHandling? ObjectCreationHandling { get; set; }

    /// <summary>
    /// Gets or sets the type name handling.
    /// </summary>
    /// <value>The type name handling.</value>
    public TypeNameHandling? TypeNameHandling { get; set; }

    /// <summary>
    /// Gets or sets a predicate used to determine whether the property should be serialize.
    /// </summary>
    /// <value>A predicate used to determine whether the property should be serialize.</value>
    public Predicate<object> ShouldSerialize { get; set; }

    /// <summary>
    /// Gets or sets a predicate used to determine whether the property should be serialized.
    /// </summary>
    /// <value>A predicate used to determine whether the property should be serialized.</value>
    public Predicate<object> GetIsSpecified { get; set; }

    /// <summary>
    /// Gets or sets an action used to set whether the property has been deserialized.
    /// </summary>
    /// <value>An action used to set whether the property has been deserialized.</value>
    public Action<object, object> SetIsSpecified { get; set; }

    /// <summary>
    /// Returns a <see cref="String"/> that represents this instance.
    /// </summary>
    /// <returns>
    /// A <see cref="String"/> that represents this instance.
    /// </returns>
    public override string ToString()
    {
      return PropertyName;
    }

    /// <summary>
    /// Gets or sets the converter used when serializing the property's collection items.
    /// </summary>
    /// <value>The collection's items converter.</value>
    public JsonConverter ItemConverter { get; set; }

    /// <summary>
    /// Gets or sets whether this property's collection items are serialized as a reference.
    /// </summary>
    /// <value>Whether this property's collection items are serialized as a reference.</value>
    public bool? ItemIsReference { get; set; }

    /// <summary>
    /// Gets or sets the the type name handling used when serializing the property's collection items.
    /// </summary>
    /// <value>The collection's items type name handling.</value>
    public TypeNameHandling? ItemTypeNameHandling { get; set; }

    /// <summary>
    /// Gets or sets the the reference loop handling used when serializing the property's collection items.
    /// </summary>
    /// <value>The collection's items reference loop handling.</value>
    public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

    internal void WritePropertyName(JsonWriter writer)
    {
      if (_skipPropertyNameEscape)
        writer.WritePropertyName(PropertyName, false);
      else
        writer.WritePropertyName(PropertyName);
    }
  }
}