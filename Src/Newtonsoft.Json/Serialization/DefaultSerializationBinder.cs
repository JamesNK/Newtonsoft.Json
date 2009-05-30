using System;
using System.Runtime.Serialization;
using System.Reflection;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// The default serialization binder used when resolving and loading classes from type names.
  /// </summary>
  public class DefaultSerializationBinder : SerializationBinder
  {
    internal static readonly DefaultSerializationBinder Instance = new DefaultSerializationBinder();

    /// <summary>
    /// When overridden in a derived class, controls the binding of a serialized object to a type.
    /// </summary>
    /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object.</param>
    /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object.</param>
    /// <returns>
    /// The type of the object the formatter creates a new instance of.
    /// </returns>
    public override Type BindToType(string assemblyName, string typeName)
    {
      if (assemblyName != null)
      {
        Assembly assembly = Assembly.Load(assemblyName);
        if (assembly == null)
          throw new JsonSerializationException("Could not load assembly '{0}'.".FormatWith(CultureInfo.InvariantCulture, assemblyName));

        Type type = assembly.GetType(typeName);
        if (type == null)
          throw new JsonSerializationException("Could not find type '{0}'.".FormatWith(CultureInfo.InvariantCulture, typeName));

        return type;
      }
      else
      {
        return Type.GetType(typeName);
      }
    }
  }
}