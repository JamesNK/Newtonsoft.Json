using System;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Instructs the <see cref="JsonSerializer"/> to populate properties with no matching class member onto the specified collection.
  /// </summary>
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
  public class JsonExtensionDataAttribute : Attribute
  {
    public bool WriteData { get; set; }
    public bool ReadData { get; set; }

    public JsonExtensionDataAttribute()
    {
      WriteData = true;
      ReadData = true;
    }
  }
}