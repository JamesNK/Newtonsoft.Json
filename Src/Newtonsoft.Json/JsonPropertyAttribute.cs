using System;

namespace Newtonsoft.Json
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
  public sealed class JsonPropertyAttribute : Attribute
  {
    private string _propertyName;

    public string PropertyName
    {
      get { return _propertyName; }
      set { _propertyName = value; }
    }

    public JsonPropertyAttribute(string propertyName)
    {
      _propertyName = propertyName;
    }
  }
}