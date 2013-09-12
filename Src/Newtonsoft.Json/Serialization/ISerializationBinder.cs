using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// 
    /// </summary>
    public interface ISerializationBinder
    {
        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// <param name="serializedType">The type of the object the formatter creates a new instance of.</param>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object. </param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object. </param>
        void BindToName(Type serializedType, out string assemblyName, out string typeName);

        /// <summary>
        /// When overridden in a derived class, controls the binding of a serialized object to a type.
        /// </summary>
        /// 
        /// <returns>
        /// The type of the object the formatter creates a new instance of.
        /// </returns>
        /// <param name="assemblyName">Specifies the <see cref="T:System.Reflection.Assembly"/> name of the serialized object. </param>
        /// <param name="typeName">Specifies the <see cref="T:System.Type"/> name of the serialized object. </param>
        Type BindToType(string assemblyName, string typeName);
    }
}
