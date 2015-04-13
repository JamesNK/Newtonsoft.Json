using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Serialization
{
    // http://stackoverflow.com/a/662657/505457
    public static class TypeHelpers
    {
        /// <summary>
        /// Gets the System.Type with the specified name, performing a case-sensitive search.
        /// </summary>
        /// <param name="typeName">The assembly-qualified name of the type to get. See System.Type.AssemblyQualifiedName.</param>
        /// <param name="throwOnError">Whether or not to throw an exception or return null if the type was not found.</param>
        /// <param name="ignoreCase">Whether or not to perform a case-insensitive search.</param>
        /// <returns>The System.Type with the specified name.</returns>
        /// <remarks>
        /// This method can load types from dynamically loaded assemblies as long as the referenced assembly 
        /// has already been loaded into the current AppDomain.
        /// </remarks>
        public static Type GetType(string typeName, bool throwOnError, bool ignoreCase)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            // handle the trivial case
            Type type;
            if ((type = Type.GetType(typeName, false, ignoreCase)) != null)
                return type;

            // otherwise, perform the recursive search
            try
            {
                return GetTypeFromRecursive(typeName, ignoreCase);
            }
            catch (Exception)
            {
                if (throwOnError)
                    throw;
            }

            return null;
        }

        #region Private Static Helper Methods

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.Int32.Parse(System.String)")]
        private static Type GetTypeFromRecursive(string typeName, bool ignoreCase)
        {
            int startIndex = typeName.IndexOf('[');
            int endIndex = typeName.LastIndexOf(']');

            if (startIndex == -1)
            {
                // try to load the non-generic type (e.g. System.Int32)
                return TypeHelpers.GetNonGenericType(typeName, ignoreCase);
            }
            else
            {
                // determine the cardinality of the generic type
                int cardinalityIndex = typeName.IndexOf('`', 0, startIndex);
                string cardinalityString = typeName.Substring(cardinalityIndex + 1, startIndex - cardinalityIndex - 1);
                int cardinality = int.Parse(cardinalityString);

                // get the FullName of the non-generic type (e.g. System.Collections.Generic.List`1)
                string fullName = typeName.Substring(0, startIndex);
                if (typeName.Length - endIndex - 1 > 0)
                    fullName += typeName.Substring(endIndex + 1, typeName.Length - endIndex - 1);

                // parse the child type arguments for this generic type (recursive)
                List<Type> list = new List<Type>();
                string typeArguments = typeName.Substring(startIndex + 1, endIndex - startIndex - 1);
                foreach (string item in EachAssemblyQualifiedName(typeArguments, cardinality))
                {
                    Type typeArgument = GetTypeFromRecursive(item, ignoreCase);
                    list.Add(typeArgument);
                }

                // construct the generic type definition
                return TypeHelpers.GetNonGenericType(fullName, ignoreCase).MakeGenericType(list.ToArray());
            }
        }

        private static IEnumerable<string> EachAssemblyQualifiedName(string s, int count)
        {
            Debug.Assert(count != 0);
            Debug.Assert(string.IsNullOrEmpty(s) == false);
            Debug.Assert(s.Length > 2);

            // e.g. "[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"
            // e.g. "[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"
            // e.g. "[System.Collections.Generic.KeyValuePair`2[[System.Int32, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.DateTime, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]"

            int startIndex = 0;
            int bracketCount = 0;

            while (count > 0)
            {
                bracketCount = 0;

                for (int i = startIndex; i < s.Length; i++)
                {
                    switch (s[i])
                    {
                        case '[':
                            bracketCount++;
                            continue;

                        case ']':
                            if (--bracketCount == 0)
                            {
                                string item = s.Substring(startIndex + 1, i - startIndex - 1);
                                yield return item;
                                startIndex = i + 2;
                            }
                            break;

                        default:
                            continue;
                    }
                }

                if (bracketCount != 0)
                {
                    const string SR_Malformed = "The brackets are unbalanced in the string, '{0}'.";
                    throw new FormatException(string.Format(SR_Malformed, s));
                }

                count--;
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        private static Type GetNonGenericType(string typeName, bool ignoreCase)
        {
            // assume the type information is not a dynamically loaded assembly
            Type type = Type.GetType(typeName, false, ignoreCase);
            if (type != null)
                return type;

            // otherwise, search the assemblies in the current AppDomain for the type
            int assemblyFullNameIndex = typeName.IndexOf(',');
            if (assemblyFullNameIndex != -1)
            {
                string assemblyPartialOrFullName = typeName.Substring(assemblyFullNameIndex + 2, typeName.Length - assemblyFullNameIndex - 2);
                string typeNameWithoutAssembly = typeName.Substring(0, assemblyFullNameIndex);

                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = assembly.GetName();
                    if (name.FullName == assemblyPartialOrFullName  || name.Name == assemblyPartialOrFullName)
                    {
                        type = assembly.GetType(typeNameWithoutAssembly, false, ignoreCase);
                        if (type != null)
                            return type;
                    }
                }
            }

            // no luck? blow up
            const string SR_TypeNotFound = "The type, '{0}', was not found.";
            throw new ArgumentException(string.Format(SR_TypeNotFound, typeName), "typeName");
        }

        #endregion
    }
}
