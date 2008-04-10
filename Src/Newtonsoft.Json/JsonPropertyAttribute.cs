using System;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Instructs the <see cref="JsonSerializer"/> to always serialize the member with the specified name.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
  public sealed class JsonPropertyAttribute : Attribute
  {
    private string _propertyName;

    /// <summary>
    /// Gets or sets the name of the property.
    /// </summary>
    /// <value>The name of the property.</value>
    public string PropertyName
    {
      get { return _propertyName; }
      set { _propertyName = value; }
    }

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
      _propertyName = propertyName;
    }
  }
}