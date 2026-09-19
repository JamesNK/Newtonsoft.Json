#if HAVE_MEMORY
using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    [RequiresUnreferencedCode(MiscellaneousUtils.TrimWarning)]
    [RequiresDynamicCode(MiscellaneousUtils.AotWarning)]
    internal sealed class MemoryConverter : JsonConverter
    {
        internal static readonly MemoryConverter Instance = new MemoryConverter();

        private static readonly ThreadSafeStore<Type, MemoryAdapter> Adapters = new ThreadSafeStore<Type, MemoryAdapter>(CreateAdapter);

        private static MemoryAdapter CreateAdapter(Type type)
        {
            return (MemoryAdapter)Activator.CreateInstance(typeof(MemoryAdapter<>).MakeGenericType(type.GetGenericArguments()))!;
        }

        internal static bool IsMemoryType(Type type)
        {
            return type.IsGenericType && (type.GetGenericTypeDefinition() == typeof(Memory<>) || type.GetGenericTypeDefinition() == typeof(ReadOnlyMemory<>));
        }

        public override bool CanConvert(Type objectType)
        {
            return IsMemoryType(Nullable.GetUnderlyingType(objectType) ?? objectType);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            Adapters.Get(value.GetType()).Write(writer, value, serializer);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            Type? underlyingType = Nullable.GetUnderlyingType(objectType);
            if (reader.TokenType == JsonToken.Null)
            {
                if (underlyingType == null)
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to {0}.".FormatWith(System.Globalization.CultureInfo.InvariantCulture, objectType));
                }
                return null;
            }

            Type memoryType = underlyingType ?? objectType;
            return Adapters.Get(memoryType).Read(reader, memoryType, serializer);
        }

        private abstract class MemoryAdapter
        {
            internal abstract void Write(JsonWriter writer, object value, JsonSerializer serializer);
            internal abstract object Read(JsonReader reader, Type memoryType, JsonSerializer serializer);
        }

        private sealed class MemoryAdapter<T> : MemoryAdapter
        {
            internal override void Write(JsonWriter writer, object value, JsonSerializer serializer)
            {
                T[] array = value is Memory<T> memory ? memory.ToArray() : ((ReadOnlyMemory<T>)value).ToArray();
                serializer.Serialize(writer, array, typeof(T[]));
            }

            internal override object Read(JsonReader reader, Type memoryType, JsonSerializer serializer)
            {
                T[]? array = typeof(T) == typeof(byte) && reader.TokenType == JsonToken.StartArray
                    ? (T[])(object)reader.ReadArrayIntoByteArray()
                    : serializer.Deserialize<T[]>(reader);
                if (array == null)
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null array to memory.");
                }
                return memoryType.GetGenericTypeDefinition() == typeof(Memory<>)
                    ? (object)new Memory<T>(array)
                    : new ReadOnlyMemory<T>(array);
            }
        }
    }
}
#endif