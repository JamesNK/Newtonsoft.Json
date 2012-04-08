using System;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Instructs the <see cref="JsonSerializer"/> to use the specified constructor when deserializing that object.
  /// </summary>
  [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
  public sealed class JsonConstructorAttribute : Attribute
  {
  }
}