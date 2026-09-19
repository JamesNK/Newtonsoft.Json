#if HAVE_MEMORY
using System;
using Newtonsoft.Json.Linq;
using Xunit;

namespace Newtonsoft.Json.Tests.Serialization
{
    public class MemoryTests
    {
        [Fact]
        public void SlicesAndRepresentations()
        {
            RoundTrip(new byte[] { 0, 1, 2, 3 }, "\"AQI=\"");
            RoundTrip(new[] { 0, 1, 2, 3 }, "[1,2]");
            RoundTrip(new[] { "outside", "one", "two", "outside" }, "[\"one\",\"two\"]");
            RoundTrip(new[] { 'x', 'a', 'b', 'x' }, "[\"a\",\"b\"]");
            RoundTrip(new[] { (Half)0, (Half)1.5f, (Half)2.5f, (Half)0 }, "[1.5,2.5]");
#if HAVE_INT128
            RoundTrip(new[] { Int128.Zero, Int128.MinValue, Int128.MaxValue, Int128.Zero }, "[" + Int128.MinValue + "," + Int128.MaxValue + "]");
            RoundTrip(new[] { UInt128.Zero, UInt128.MinValue, UInt128.MaxValue, UInt128.Zero }, "[0," + UInt128.MaxValue + "]");
#endif
            Assert.Equal("[\"a\",\"b\"]", JsonConvert.SerializeObject("xabx".AsMemory(1, 2)));
            Assert.Equal("[]", JsonConvert.SerializeObject(default(Memory<int>)));
            Assert.Equal("\"\"", JsonConvert.SerializeObject(default(ReadOnlyMemory<byte>)));
            Assert.Equal("[[1,2],[3]]", JsonConvert.SerializeObject(new Memory<int>[] { new[] { 1, 2 }, new[] { 3 } }.AsMemory()));
        }

        private static void RoundTrip<T>(T[] source, string expected)
        {
            Memory<T> memory = source.AsMemory(1, 2);
            ReadOnlyMemory<T> readOnly = memory;
            Assert.Equal(expected, JsonConvert.SerializeObject(memory));
            Assert.Equal(expected, JsonConvert.SerializeObject(readOnly));
            Assert.Equal(memory.ToArray(), JsonConvert.DeserializeObject<Memory<T>>(expected).ToArray());
            Assert.Equal(memory.ToArray(), JsonConvert.DeserializeObject<ReadOnlyMemory<T>>(expected).ToArray());
            Assert.Equal(memory.ToArray(), JToken.FromObject(memory).ToObject<Memory<T>>().ToArray());
#if HAVE_INT128
            Assert.Equal(memory.ToArray(), System.Text.Json.JsonSerializer.Deserialize<Memory<T>>(expected).ToArray());
            Assert.True(JToken.DeepEquals(JToken.Parse(expected), JToken.Parse(System.Text.Json.JsonSerializer.Serialize(memory))));
#endif
        }

        [Fact]
        public void NullAndInvalidValues()
        {
            Assert.Null(JsonConvert.DeserializeObject<Memory<int>?>("null"));
            Assert.Null(JsonConvert.DeserializeObject<ReadOnlyMemory<byte>?>("null"));
            Assert.Equal("null", JsonConvert.SerializeObject((Memory<int>?)null));
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Memory<int>>("null"));
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<ReadOnlyMemory<byte>>("null"));
            Assert.Throws<FormatException>(() => JsonConvert.DeserializeObject<Memory<byte>>("\"!\""));
            Assert.Equal(new byte[] { 1, 2 }, JsonConvert.DeserializeObject<Memory<byte>>("[1,2]").ToArray());
        }

        [Fact]
        public void ConverterPrecedence()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new ElementConverter());
            Memory<Element> memory = new[] { new Element { Value = 3 } };
            ReadOnlyMemory<Element> readOnly = memory;
            Assert.Equal("[3]", JsonConvert.SerializeObject(memory, settings));
            Assert.Equal(3, JsonConvert.DeserializeObject<Memory<Element>>("[3]", settings).Span[0].Value);
            Assert.Equal("[3]", JsonConvert.SerializeObject(readOnly, settings));
            Assert.Equal(3, JsonConvert.DeserializeObject<ReadOnlyMemory<Element>>("[3]", settings).Span[0].Value);
            settings.Converters.Insert(0, new BufferConverter());
            settings.Converters.Insert(0, new ReadOnlyBufferConverter());
            Assert.Equal("\"buffer\"", JsonConvert.SerializeObject(memory, settings));
            Assert.Equal(7, JsonConvert.DeserializeObject<Memory<Element>>("\"buffer\"", settings).Span[0].Value);
            Assert.Equal("\"read-only-buffer\"", JsonConvert.SerializeObject(readOnly, settings));
            Assert.Equal(11, JsonConvert.DeserializeObject<ReadOnlyMemory<Element>>("\"read-only-buffer\"", settings).Span[0].Value);
        }

        [Fact]
        public void PropertyConverterPrecedence()
        {
            AttributedBuffers buffers = new AttributedBuffers
            {
                Mutable = new[] { new Element { Value = 3 } },
                ReadOnly = new[] { new Element { Value = 5 } }
            };
            string json = "{\"Mutable\":\"buffer\",\"ReadOnly\":\"read-only-buffer\"}";

            Assert.Equal(json, JsonConvert.SerializeObject(buffers));
            AttributedBuffers result = JsonConvert.DeserializeObject<AttributedBuffers>(json);
            Assert.Equal(7, result.Mutable.Span[0].Value);
            Assert.Equal(11, result.ReadOnly.Span[0].Value);
        }

        private sealed class AttributedBuffers
        {
            [JsonConverter(typeof(BufferConverter))]
            public Memory<Element> Mutable { get; set; }

            [JsonConverter(typeof(ReadOnlyBufferConverter))]
            public ReadOnlyMemory<Element> ReadOnly { get; set; }
        }

        private sealed class Element
        {
            public int Value { get; set; }
        }

        private sealed class ElementConverter : JsonConverter<Element>
        {
            public override void WriteJson(JsonWriter writer, Element value, JsonSerializer serializer) => writer.WriteValue(value.Value);
            public override Element ReadJson(JsonReader reader, Type objectType, Element existingValue, bool hasExistingValue, JsonSerializer serializer) => new Element { Value = Convert.ToInt32(reader.Value) };
        }

        private sealed class BufferConverter : JsonConverter<Memory<Element>>
        {
            public override void WriteJson(JsonWriter writer, Memory<Element> value, JsonSerializer serializer) => writer.WriteValue("buffer");
            public override Memory<Element> ReadJson(JsonReader reader, Type objectType, Memory<Element> existingValue, bool hasExistingValue, JsonSerializer serializer) => new[] { new Element { Value = 7 } };
        }

        private sealed class ReadOnlyBufferConverter : JsonConverter<ReadOnlyMemory<Element>>
        {
            public override void WriteJson(JsonWriter writer, ReadOnlyMemory<Element> value, JsonSerializer serializer) => writer.WriteValue("read-only-buffer");
            public override ReadOnlyMemory<Element> ReadJson(JsonReader reader, Type objectType, ReadOnlyMemory<Element> existingValue, bool hasExistingValue, JsonSerializer serializer) => new[] { new Element { Value = 11 } };
        }
    }
}
#endif