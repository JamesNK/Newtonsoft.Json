using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Instructs the <see cref="JsonSerializer"/> not to serialize the public field or public read/write property value.
  /// </summary>
  [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Property, AllowMultiple = false)]
  public sealed class JsonConstructorAttribute : Attribute
  {
  }
}