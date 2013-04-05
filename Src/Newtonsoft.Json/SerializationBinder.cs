#if SILVERLIGHT || PocketPC || NETFX_CORE || PORTABLE40 || PORTABLE
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
    /// <returns>The type of the object the formatter creates a new instance of.</returns>
    public abstract Type BindToType(string assemblyName, string typeName);

    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
    /// <param name="assemblyName">Specifies the <see cref="Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="Type"/> name of the serialized object.</param>
    public virtual void BindToName(Type serializedType, out string assemblyName, out string typeName)
    {
      assemblyName = null;
      typeName = null;
    }
  }
}
#endif