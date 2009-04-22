using System;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Instructs the <see cref="JsonSerializer"/> to always serialize the member with the specified name.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
  public sealed class JsonPropertyAttribute : Attribute
  {
    // yuck. can't set nullable properties on an attribute in C#
    // have to use this approach to get an unset default state
    internal NullValueHandling? _nullValueHandling;
    internal DefaultValueHandling? _defaultValueHandling;
    internal ReferenceLoopHandling? _referenceLoopHandling;

    public NullValueHandling NullValueHandling
    {
      get { return _nullValueHandling ?? default(NullValueHandling); }
      set { _nullValueHandling = value; }
    }

    public DefaultValueHandling DefaultValueHandling
    {
      get { return _defaultValueHandling ?? default(DefaultValueHandling); }
      set { _defaultValueHandling = value; }
    }

    public ReferenceLoopHandling ReferenceLoopHandling
    {
      get { return _referenceLoopHandling ?? default(ReferenceLoopHandling); }
      set { _referenceLoopHandling = value; }
    }

    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    /// <value>The name of the property.</value>
    public string PropertyName { get; set; }

    public bool IsRequired { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class.
    /// </summary>
    public JsonPropertyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonPropertyAttribute"/> class with the specified name.
    /// </summary>
    /// <param name="propertyName">Name of the property.</param>
    public JsonPropertyAttribute(string propertyName)
    {
      PropertyName = propertyName;
    }
  }
}