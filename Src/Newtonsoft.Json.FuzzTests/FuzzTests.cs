using System;
using System.IO;

namespace Newtonsoft.Json.FuzzTests
{
    public static class Fuzzers
    {
        static readonly JsonSerializer jsonSerializer = new();

        public static void FuzzDeserialization(ReadOnlySpan<byte> buffer)
        {
            try
            {
                using var ms = new MemoryStream(buffer.ToArray());
                using var sr = new StreamReader(ms);
                using var reader = new JsonTextReader(sr);
                jsonSerializer.Deserialize(reader);
            }
            catch (JsonException)
            {
                // this can be JsonReaderException, JsonWriterException, or JsonSerializationException;
                // the latter two can be thrown by deserializing into a json object
                return;
                // ignore - known/expected exceptions are okay
            }
        }

        public static void FuzzSerialization(ReadOnlySpan<byte> buffer)
        {
            object? deserialized;
            try
            {
                deserialized = Deserialize(buffer);
            }
            catch
            {
                return;
            }

            // see if this throws
            Serialize(deserialized);
        }

        public static void FuzzIdempotent(ReadOnlySpan<byte> buffer)
        {
            string serialized1;
            try
            {
                serialized1 = Serialize(Deserialize(buffer));
            }
            catch
            {
                return;
            }

            var serialized2 = Serialize(Deserialize(serialized1));
            if (serialized1 != serialized2)
            {
                throw new Exception($"not idempotent: {serialized1} {serialized2}");
            }
        }

        private static string Serialize(object? o)
        {
            using var sw1 = new StringWriter();
            jsonSerializer.Serialize(sw1, o);
            return sw1.ToString();
        }

        private static object? Deserialize(string input)
        {
            using var sr = new StringReader(input);
            using var tr = new JsonTextReader(sr);
            return jsonSerializer.Deserialize(tr);
        }

        private static object? Deserialize(ReadOnlySpan<byte> bytes)
        {
            using var ms = new MemoryStream(bytes.ToArray());
            using var sr = new StreamReader(ms);
            using var reader = new JsonTextReader(sr);
            return jsonSerializer.Deserialize(reader);
        }
    }
}
