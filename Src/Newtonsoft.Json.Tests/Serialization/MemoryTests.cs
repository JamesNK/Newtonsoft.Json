#if HAVE_MEMORY
using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
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
            Assert.Equal(new byte[] { 1, 2 }, JsonConvert.DeserializeObject<ReadOnlyMemory<byte>>("[1,2]").ToArray());
            Assert.Empty(JsonConvert.DeserializeObject<Memory<byte>?>("\"\"").Value.ToArray());
            Assert.Empty(JsonConvert.DeserializeObject<ReadOnlyMemory<byte>?>("\"\"").Value.ToArray());
            Assert.Equal(new[] { 1, 2 }, JsonConvert.DeserializeObject<Memory<int>?>("[1,2]").Value.ToArray());
            Assert.Equal(new[] { 1, 2 }, JsonConvert.DeserializeObject<ReadOnlyMemory<int>?>("[1,2]").Value.ToArray());
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Memory<byte>>("[1,"));
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Memory<byte>>("[true]"));
        }

        [Fact]
        public void RequiredNullableByteMemoryRoundTrips()
        {
            RequiredByteBuffers buffers = new RequiredByteBuffers
            {
                AlwaysMutable = Memory<byte>.Empty,
                AlwaysReadOnly = ReadOnlyMemory<byte>.Empty,
                DisallowNullMutable = Memory<byte>.Empty,
                DisallowNullReadOnly = ReadOnlyMemory<byte>.Empty
            };
            string json = JsonConvert.SerializeObject(buffers);
            Assert.Equal("{\"AlwaysMutable\":\"\",\"AlwaysReadOnly\":\"\",\"DisallowNullMutable\":\"\",\"DisallowNullReadOnly\":\"\"}", json);

            RequiredByteBuffers result = JsonConvert.DeserializeObject<RequiredByteBuffers>(json);
            Assert.Empty(result.AlwaysMutable.Value.ToArray());
            Assert.Empty(result.AlwaysReadOnly.Value.ToArray());
            Assert.Empty(result.DisallowNullMutable.Value.ToArray());
            Assert.Empty(result.DisallowNullReadOnly.Value.ToArray());

            foreach (string propertyName in new[] { nameof(RequiredByteBuffers.AlwaysMutable), nameof(RequiredByteBuffers.AlwaysReadOnly), nameof(RequiredByteBuffers.DisallowNullMutable), nameof(RequiredByteBuffers.DisallowNullReadOnly) })
            {
                JObject invalid = JObject.Parse(json);
                invalid[propertyName] = JValue.CreateNull();
                JsonSerializationException exception = Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<RequiredByteBuffers>(invalid.ToString(Formatting.None)));
                Assert.Contains("Required property '" + propertyName + "'", exception.Message);
            }
        }

        private sealed class RequiredByteBuffers
        {
            [JsonProperty(Required = Required.Always)]
            public Memory<byte>? AlwaysMutable { get; set; }

            [JsonProperty(Required = Required.Always)]
            public ReadOnlyMemory<byte>? AlwaysReadOnly { get; set; }

            [JsonProperty(Required = Required.DisallowNull)]
            public Memory<byte>? DisallowNullMutable { get; set; }

            [JsonProperty(Required = Required.DisallowNull)]
            public ReadOnlyMemory<byte>? DisallowNullReadOnly { get; set; }
        }

#if HAVE_INT128
        [Fact]
        public async Task ModernIntegerByteBuffers()
        {
            JArray array = new JArray(new JValue((object)Int128.Zero), new JValue((object)(UInt128)1), new JValue((object)(Int128)255));
            byte[] expected = { 0, 1, 255 };
            Assert.Equal(expected, array.ToObject<byte[]>());
            Assert.Equal(expected, array.ToObject<Memory<byte>>().ToArray());
            Assert.Equal(expected, array.ToObject<ReadOnlyMemory<byte>>().ToArray());
            using (JsonReader reader = array.CreateReader())
            {
                Assert.Equal(expected, reader.ReadAsBytes());
            }
            using (JsonReader reader = array.CreateReader())
            {
                Assert.Equal(expected, await reader.ReadAsBytesAsync());
            }
            using (JsonReader reader = array.CreateReader())
            {
                Assert.True(await reader.ReadAsync());
                Assert.Equal(expected, await reader.ReadArrayIntoByteArrayAsync(default));
            }
            foreach (object value in new object[] { (Int128)(-1), (Int128)256, (UInt128)256, Int128.MinValue, UInt128.MaxValue })
            {
                JArray invalid = new JArray(new JValue(value));
                Assert.Throws<InvalidOperationException>(() => invalid.ToObject<byte[]>());
                Assert.Throws<InvalidOperationException>(() => invalid.ToObject<Memory<byte>>());
                Assert.Throws<InvalidOperationException>(() => invalid.ToObject<ReadOnlyMemory<byte>>());
            }
        }
#endif

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

        [Fact]
        public void OneWayConverterPrecedence()
        {
            VerifyOneWayConverter<Memory<int>>(new[] { 1, 2 }, "[1,2]", result => Assert.Equal(new[] { 1, 2 }, result.ToArray()));
            VerifyOneWayConverter<ReadOnlyMemory<int>>(new[] { 1, 2 }, "[1,2]", result => Assert.Equal(new[] { 1, 2 }, result.ToArray()));
        }

        [Fact]
        public void ByteMemoryOneWayConverterPrecedence()
        {
            VerifyOneWayConverter<Memory<byte>>(new byte[] { 1, 2 }, "\"AQI=\"", result => Assert.Equal(new byte[] { 1, 2 }, result.ToArray()));
            VerifyOneWayConverter<ReadOnlyMemory<byte>>(new byte[] { 1, 2 }, "\"AQI=\"", result => Assert.Equal(new byte[] { 1, 2 }, result.ToArray()));
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new OneWayConverter<Memory<byte>>(default, false));
            settings.Converters.Add(new OneWayConverter<ReadOnlyMemory<byte>>(default, false));
            Assert.Equal(new byte[] { 1, 2 }, JsonConvert.DeserializeObject<Memory<byte>>("[1,2]", settings).ToArray());
            Assert.Equal(new byte[] { 1, 2 }, JsonConvert.DeserializeObject<ReadOnlyMemory<byte>>("[1,2]", settings).ToArray());
        }

        private static void VerifyOneWayConverter<T>(T value, string json, Action<T> verify)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new OneWayConverter<T>(value, true));
            Assert.Equal(json, JsonConvert.SerializeObject(value, settings));
            verify(JsonConvert.DeserializeObject<T>("\"custom\"", settings));

            settings.Converters.Clear();
            settings.Converters.Add(new OneWayConverter<T>(value, false));
            Assert.Equal("\"custom\"", JsonConvert.SerializeObject(value, settings));
            verify(JsonConvert.DeserializeObject<T>(json, settings));
        }

        [Fact]
        public void PropertyItemConverterPrecedence()
        {
            ItemConvertedBuffers buffers = new ItemConvertedBuffers
            {
                Mutable = new[] { new Element { Value = 3 } },
                ReadOnly = new[] { new Element { Value = 5 } }
            };
            string json = "{\"Mutable\":[3],\"ReadOnly\":[5]}";
            Assert.Equal(json, JsonConvert.SerializeObject(buffers));
            ItemConvertedBuffers result = JsonConvert.DeserializeObject<ItemConvertedBuffers>(json);
            Assert.Equal(3, result.Mutable.Span[0].Value);
            Assert.Equal(5, result.ReadOnly.Span[0].Value);
        }

        [Fact]
        public void NativeContracts()
        {
            DefaultContractResolver resolver = new DefaultContractResolver();
            foreach (Type type in new[] { typeof(Memory<int>), typeof(ReadOnlyMemory<int>), typeof(Memory<int>?), typeof(ReadOnlyMemory<int>?) })
            {
                JsonArrayContract contract = Assert.IsType<JsonArrayContract>(resolver.ResolveContract(type));
                Assert.Equal(typeof(int), contract.CollectionItemType);
                Assert.Null(contract.InternalConverter);
            }
            foreach (Type type in new[] { typeof(Memory<byte>), typeof(ReadOnlyMemory<byte>) })
            {
                JsonPrimitiveContract contract = Assert.IsType<JsonPrimitiveContract>(resolver.ResolveContract(type));
                Assert.Null(contract.InternalConverter);
            }
        }

        [Fact]
        public void TypeMetadataRoundTrips()
        {
            object[] values = { new[] { 1, 2 }.AsMemory(), (ReadOnlyMemory<int>)new[] { 1, 2 }, new byte[] { 1, 2 }.AsMemory(), (ReadOnlyMemory<byte>)new byte[] { 1, 2 } };
            foreach (object value in values)
            {
                foreach (MetadataPropertyHandling metadataHandling in new[] { MetadataPropertyHandling.Default, MetadataPropertyHandling.ReadAhead })
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = metadataHandling };
                    string json = JsonConvert.SerializeObject(new ObjectBuffer { Value = value }, settings);
                    ObjectBuffer result = JsonConvert.DeserializeObject<ObjectBuffer>(json, settings);
                    Assert.Equal(value.GetType(), result.Value.GetType());
                    Assert.Equal(JsonConvert.SerializeObject(value), JsonConvert.SerializeObject(result.Value));

                    settings.TypeNameHandling = TypeNameHandling.All;
                    json = JsonConvert.SerializeObject(value, settings);
                    object root = JsonConvert.DeserializeObject(json, value.GetType(), settings);
                    Assert.Equal(value.GetType(), root.GetType());
                    Assert.Equal(JsonConvert.SerializeObject(value), JsonConvert.SerializeObject(root));
                }
            }
        }

        [Fact]
        public void PropertyItemMetadataAndReferences()
        {
            Element element = new Element { Value = 3 };
            ItemMetadataBuffers buffers = new ItemMetadataBuffers
            {
                Mutable = new object[] { element, element },
                ReadOnly = new object[] { element }
            };
            string json = JsonConvert.SerializeObject(buffers);
            ItemMetadataBuffers result = JsonConvert.DeserializeObject<ItemMetadataBuffers>(json);
            Assert.Equal(3, Assert.IsType<Element>(result.Mutable.Span[0]).Value);
            Assert.Same(result.Mutable.Span[0], result.Mutable.Span[1]);
            Assert.Same(result.Mutable.Span[0], result.ReadOnly.Span[0]);
        }

        [Fact]
        public void ItemErrorsAndPropertyPopulation()
        {
            int errors = 0;
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Error = (sender, arguments) =>
                {
                    errors++;
                    Assert.Equal("[1]", arguments.ErrorContext.Path);
                    arguments.ErrorContext.Handled = true;
                }
            };
            Assert.Equal(new[] { 1, 2 }, JsonConvert.DeserializeObject<Memory<int>>("[1,\"invalid\",2]", settings).ToArray());
            Assert.Equal(new[] { 1, 2 }, JsonConvert.DeserializeObject<ReadOnlyMemory<int>>("[1,\"invalid\",2]", settings).ToArray());
            Assert.Equal(2, errors);

            ItemConvertedBuffers buffers = new ItemConvertedBuffers { Mutable = new[] { new Element { Value = 0 } } };
            JsonConvert.PopulateObject("{\"Mutable\":[3],\"ReadOnly\":[5]}", buffers);
            Assert.Equal(3, buffers.Mutable.Span[0].Value);
            Assert.Equal(5, buffers.ReadOnly.Span[0].Value);

            Memory<int> memory = new[] { 1, 2 };
            ReadOnlyMemory<int> readOnly = memory;
            JsonConvert.PopulateObject("[3,4]", memory);
            JsonConvert.PopulateObject("[3,4]", readOnly);
            Assert.Equal(new[] { 1, 2 }, memory.ToArray());
        }

        [Fact]
        public void PropertyOneWayConverters()
        {
            OneWayBuffers buffers = new OneWayBuffers { Mutable = new[] { 1, 2 }, ReadOnly = new[] { 3, 4 } };
            Assert.Equal("{\"Mutable\":\"custom\",\"ReadOnly\":[3,4]}", JsonConvert.SerializeObject(buffers));
            OneWayBuffers result = JsonConvert.DeserializeObject<OneWayBuffers>("{\"Mutable\":[1,2],\"ReadOnly\":\"custom\"}");
            Assert.Equal(new[] { 1, 2 }, result.Mutable.ToArray());
            Assert.Empty(result.ReadOnly.ToArray());
        }

        [Fact]
        public void ArrayContractCustomization()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new ItemConverterResolver() };
            Assert.Equal("[3]", JsonConvert.SerializeObject(new[] { new Element { Value = 3 } }.AsMemory(), settings));
            Assert.Equal(5, JsonConvert.DeserializeObject<ReadOnlyMemory<Element>>("[5]", settings).Span[0].Value);
        }

        [Fact]
        public void ArrayContractCallbacksUseCurrentMemory()
        {
            foreach (bool readOnly in new[] { false, true })
            {
                int[] values = { 0, 1, 2, 3 };
                Memory<int> slice = values.AsMemory(1, 2);
                object memory = readOnly ? (object)(ReadOnlyMemory<int>)slice : slice;
                CallbackResolver resolver = new CallbackResolver(memory, values);
                JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = resolver };
                Assert.Equal("[4,5]", JsonConvert.SerializeObject(memory, settings));
                Assert.True(resolver.Serialized);
                Assert.Equal(new[] { 0, 4, 5, 3 }, values);
            }
        }

        private sealed class CallbackResolver : DefaultContractResolver
        {
            private readonly object _memory;
            private readonly int[] _values;
            public bool Serialized { get; private set; }

            public CallbackResolver(object memory, int[] values)
            {
                _memory = memory;
                _values = values;
            }

            protected override JsonArrayContract CreateArrayContract(Type objectType)
            {
                JsonArrayContract contract = base.CreateArrayContract(objectType);
                contract.OnSerializingCallbacks.Add((value, context) =>
                {
                    Assert.Same(_memory, value);
                    _values[1] = 4;
                });
                contract.ItemConverter = new UpdatingItemConverter(_values);
                contract.OnSerializedCallbacks.Add((value, context) =>
                {
                    Assert.Same(_memory, value);
                    Serialized = true;
                });
                return contract;
            }
        }

        private sealed class UpdatingItemConverter : JsonConverter<int>
        {
            private readonly int[] _values;

            public UpdatingItemConverter(int[] values)
            {
                _values = values;
            }

            public override void WriteJson(JsonWriter writer, int value, JsonSerializer serializer)
            {
                _values[2] = 5;
                writer.WriteValue(value);
            }

            public override int ReadJson(JsonReader reader, Type objectType, int existingValue, bool hasExistingValue, JsonSerializer serializer)
                => throw new NotSupportedException();
        }

        private sealed class ItemConverterResolver : DefaultContractResolver
        {
            protected override JsonArrayContract CreateArrayContract(Type objectType)
            {
                JsonArrayContract contract = base.CreateArrayContract(objectType);
                if (contract.CollectionItemType == typeof(Element))
                {
                    contract.ItemConverter = new ElementConverter();
                }
                return contract;
            }
        }

        private sealed class OneWayBuffers
        {
            [JsonConverter(typeof(OneWayConverter<Memory<int>>), false)]
            public Memory<int> Mutable { get; set; }

            [JsonConverter(typeof(OneWayConverter<ReadOnlyMemory<int>>), true)]
            public ReadOnlyMemory<int> ReadOnly { get; set; }
        }

        [Fact]
        public void ArrayConvertersDoNotApplyToMemory()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new RejectArrayConverter());
            Memory<byte> memory = new byte[] { 1, 2 };
            Assert.Equal("\"AQI=\"", JsonConvert.SerializeObject(memory, settings));
            Assert.Equal("\"AQI=\"", JsonConvert.SerializeObject((ReadOnlyMemory<byte>)memory, settings));
            Assert.Equal(memory.ToArray(), JsonConvert.DeserializeObject<Memory<byte>>("\"AQI=\"", settings).ToArray());
            Assert.Equal(memory.ToArray(), JsonConvert.DeserializeObject<ReadOnlyMemory<byte>>("[1,2]", settings).ToArray());
            Assert.Equal("[1,2]", JsonConvert.SerializeObject(new[] { 1, 2 }.AsMemory(), settings));
            Assert.Equal(new[] { 1, 2 }, JsonConvert.DeserializeObject<ReadOnlyMemory<int>>("[1,2]", settings).ToArray());
            Assert.Throws<InvalidOperationException>(() => JsonConvert.SerializeObject(new byte[] { 1, 2 }, settings));
        }

        private sealed class ObjectBuffer
        {
            public object Value { get; set; }
        }

        private sealed class ItemMetadataBuffers
        {
            [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto, ItemIsReference = true)]
            public Memory<object> Mutable { get; set; }

            [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto, ItemIsReference = true)]
            public ReadOnlyMemory<object> ReadOnly { get; set; }
        }

        private sealed class RejectArrayConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType.IsArray;
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) => throw new InvalidOperationException("Array converter invoked.");
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer) => throw new InvalidOperationException("Array converter invoked.");
        }

        private sealed class ItemConvertedBuffers
        {
            [JsonProperty(ItemConverterType = typeof(ElementConverter))]
            public Memory<Element> Mutable { get; set; }

            [JsonProperty(ItemConverterType = typeof(ElementConverter))]
            public ReadOnlyMemory<Element> ReadOnly { get; set; }
        }

        private sealed class OneWayConverter<T> : JsonConverter<T>
        {
            private readonly T _value;
            private readonly bool _canRead;

            public OneWayConverter(bool canRead)
                : this(default(T), canRead)
            {
            }

            public OneWayConverter(T value, bool canRead)
            {
                _value = value;
                _canRead = canRead;
            }

            public override bool CanRead => _canRead;
            public override bool CanWrite => !_canRead;

            public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer) => writer.WriteValue("custom");
            public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer) => _value;
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