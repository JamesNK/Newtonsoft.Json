#if SILVERLIGHT || PocketPC
using System;
using System.Reflection;

namespace Newtonsoft.Json
{
  /// <summary>
  /// Allows users to control class loading and mandate what class to load.
  /// </summary>
  public abstract class SerializationBinder
  {
    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="Type"/> name of the serialized object</param>
    /// <returns></returns>
    public abstract Type BindToType(string assemblyName, string typeName);
  }
}
#endif