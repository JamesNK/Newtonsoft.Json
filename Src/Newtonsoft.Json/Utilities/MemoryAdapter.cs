#if HAVE_MEMORY
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Newtonsoft.Json.Utilities
{
    internal abstract class MemoryAdapter
    {
        protected bool IsReadOnly { get; private set; }

        internal static bool IsMemoryType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Memory<>) || type.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<>));
        }

        [RequiresUnreferencedCode(MiscellaneousUtils.TrimWarning)]
        [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
        internal static MemoryAdapter Create(Type type)
        {
            MemoryAdapter adapter = (MemoryAdapter)Activator.CreateInstance(typeof(TypedMemoryAdapter<>).MakeGenericType(type.GetGenericArguments()))!;
            adapter.IsReadOnly = type.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<>);
            return adapter;
        }

        internal abstract Array ToArray(object value);
        internal abstract object FromList(IList values);

        private sealed class TypedMemoryAdapter<T> : MemoryAdapter
        {
            internal override Array ToArray(object value)
            {
                return value is Memory<T> memory ? memory.ToArray() : ((ReadOnlyMemory<T>)value).ToArray();
            }

            internal override object FromList(IList values)
            {
                T[] array = new T[values.Count];
                values.CopyTo(array, 0);
                return IsReadOnly ? (object)new ReadOnlyMemory<T>(array) : new Memory<T>(array);
            }
        }
    }
}
#endif