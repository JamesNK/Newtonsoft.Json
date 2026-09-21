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

        internal abstract IEnumerable GetEnumerable(object value);
        internal abstract object FromList(IList values);

        private sealed class TypedMemoryAdapter<T> : MemoryAdapter
        {
            internal override IEnumerable GetEnumerable(object value)
            {
                return Enumerate(value is Memory<T> memory ? memory : (ReadOnlyMemory<T>)value);
            }

            private static IEnumerable Enumerate(ReadOnlyMemory<T> memory)
            {
                for (int index = 0; index < memory.Length; index++)
                {
                    yield return memory.Span[index];
                }
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