#if HAVE_HALF
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Numerics;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Xunit;

namespace Newtonsoft.Json.Tests.Serialization
{
    public class ModernNumericTests
    {
        [Fact]
        public void AssignableModernNumbersPreserveValue()
        {
            object[] values = { (Half)42, Half.NaN, BitConverter.Int16BitsToHalf(short.MinValue),
#if HAVE_INT128
                (Int128)42, Int128.MinValue, Int128.MaxValue, (UInt128)42, UInt128.MaxValue,
#endif
            };
            foreach (object value in values)
            {
                JValue token = new JValue(value);
                Assert.Same(value, token.ToObject<object>());
                Assert.Same(value, token.ToObject<ValueType>());
                Assert.Same(value, token.ToObject<IComparable>());
                Assert.Same(value, token.ToObject<IFormattable>());
                Assert.Throws<JsonSerializationException>(() => token.ToObject<IDisposable>());
                foreach (Type targetType in new[] { typeof(object), typeof(ValueType), typeof(IComparable), typeof(IFormattable) })
                {
                    Assert.Same(value, ConvertUtils.Convert(value, CultureInfo.InvariantCulture, targetType));
                    Assert.Same(value, ConvertUtils.ConvertOrCast(value, CultureInfo.InvariantCulture, targetType));
                }

                JObject container = new JObject
                {
                    ["Object"] = new JValue(value),
                    ["ValueType"] = new JValue(value),
                    ["Comparable"] = new JValue(value),
                    ["Formattable"] = new JValue(value)
                };
                AssignableNumbers result = container.ToObject<AssignableNumbers>();
                Assert.Same(value, result.Object);
                Assert.Same(value, result.ValueType);
                Assert.Same(value, result.Comparable);
                Assert.Same(value, result.Formattable);
                Dictionary<string, object> dictionary = container.ToObject<Dictionary<string, object>>();
                Assert.All(dictionary.Values, item => Assert.Same(value, item));
                Assert.Same(value, new JArray(token).ToObject<object[]>()[0]);
                Assert.Same(value, token.Value);
            }
        }

        private sealed class AssignableNumbers
        {
            public object Object { get; set; }
            public ValueType ValueType { get; set; }
            public IComparable Comparable { get; set; }
            public IFormattable Formattable { get; set; }
        }

        [Fact]
        public void BinaryNumberToHalfMidpoint()
        {
            object[] values = { 1.00048828125f, 1.00048828125d };
            foreach (object value in values)
            {
                JValue token = new JValue(value);
                Assert.Equal((Half)1, token.Value<Half>());
                Assert.Equal((Half)1, token.ToObject<Half>());
                Assert.Equal((Half)1, token.ToObject<Half?>().Value);
                NullableHalf result = new JObject { ["Value"] = token }.ToObject<NullableHalf>();
                Assert.Equal((Half)1, result.Value.Value);
                Assert.Same(value, token.Value);
            }
        }

        [Fact]
        public void BinaryNumberToHalfEdges()
        {
            const float singleMidpoint = 1.00048828125f;
            foreach (float value in new[]
            {
                singleMidpoint, MathF.BitDecrement(singleMidpoint), MathF.BitIncrement(singleMidpoint),
                -singleMidpoint, -MathF.BitDecrement(singleMidpoint), -MathF.BitIncrement(singleMidpoint),
                1.00146484375f, 0f, BitConverter.Int32BitsToSingle(int.MinValue), float.Epsilon,
                (float)Half.Epsilon, (float)Half.Epsilon / 2, 65519f, 65520f,
                float.MinValue, float.MaxValue, float.NaN, float.PositiveInfinity, float.NegativeInfinity
            })
            {
                AssertBinaryNumberToHalf(value, (Half)value);
            }

            const double doubleMidpoint = 1.00048828125d;
            foreach (double value in new[]
            {
                doubleMidpoint, Math.BitDecrement(doubleMidpoint), Math.BitIncrement(doubleMidpoint),
                -doubleMidpoint, -Math.BitDecrement(doubleMidpoint), -Math.BitIncrement(doubleMidpoint),
                1.00146484375d, 0d, BitConverter.Int64BitsToDouble(long.MinValue), double.Epsilon,
                (double)Half.Epsilon, (double)Half.Epsilon / 2, 65519d, 65520d,
                double.MinValue, double.MaxValue, double.NaN, double.PositiveInfinity, double.NegativeInfinity
            })
            {
                AssertBinaryNumberToHalf(value, (Half)value);
            }
        }

        private static void AssertBinaryNumberToHalf(object value, Half expected)
        {
            short expectedBits = BitConverter.HalfToInt16Bits(expected);
            JValue token = new JValue(value);
            Assert.Equal(expectedBits, BitConverter.HalfToInt16Bits(token.Value<Half>()));
            Assert.Equal(expectedBits, BitConverter.HalfToInt16Bits(token.ToObject<Half>()));
            Assert.Equal(expectedBits, BitConverter.HalfToInt16Bits(token.ToObject<Half?>().Value));
            foreach (CultureInfo culture in new[] { CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("fr-FR") })
            {
                Assert.Equal(expectedBits, BitConverter.HalfToInt16Bits((Half)ConvertUtils.Convert(value, culture, typeof(Half))));
                Assert.Equal(expectedBits, BitConverter.HalfToInt16Bits((Half)ConvertUtils.ConvertOrCast(value, culture, typeof(Half?))));
                JsonSerializer serializer = new JsonSerializer { Culture = culture };
                NullableHalf result = new JObject { ["Value"] = token }.ToObject<NullableHalf>(serializer);
                Assert.Equal(expectedBits, BitConverter.HalfToInt16Bits(result.Value.Value));
            }
            Assert.Same(value, token.Value);
        }

        [Fact]
        public void TypedReaderModernNumbers()
        {
            object[] values = { (Half)42,
#if HAVE_INT128
                (Int128)42, (UInt128)42,
#endif
            };
            foreach (object value in values)
            {
                AssertTypedRead(value, reader => reader.ReadAsInt32(), 42, JsonToken.Integer);
                AssertTypedRead(value, reader => reader.ReadAsDouble(), 42d, JsonToken.Float);
                AssertTypedRead(value, reader => reader.ReadAsDecimal(), 42m, JsonToken.Float);
                AssertTypedRead(value, reader => reader.ReadAsBoolean(), true, JsonToken.Boolean);

                JObject token = JObject.FromObject(new { Integer = value, Double = value, Decimal = value, Boolean = value });
                object originalValue = ((JValue)token["Integer"]).Value;
                Assert.Equal(value, originalValue);
                LegacyNumbers result = token.ToObject<LegacyNumbers>();
                Assert.Equal(42, result.Integer);
                Assert.Equal(42d, result.Double);
                Assert.Equal(42m, result.Decimal);
                Assert.True(result.Boolean);
                Assert.Same(originalValue, ((JValue)token["Integer"]).Value);

                JsonSerializer serializer = new JsonSerializer();
                serializer.Converters.Add(new OriginalNumberConverter(value));
                Assert.Equal(42d, new JValue(value).ToObject<double>(serializer));
            }
        }

        [Fact]
        public void TypedReaderHalfEdges()
        {
            foreach (Half value in new[] { (Half)1.5f, (Half)2.5f, (Half)(-1.5f), (Half)(-2.5f), Half.MaxValue, Half.Epsilon, (Half)0, BitConverter.Int16BitsToHalf(short.MinValue) })
            {
                AssertTypedRead(value, reader => reader.ReadAsInt32(), Convert.ToInt32((float)value), JsonToken.Integer);
                AssertTypedRead(value, reader => reader.ReadAsDouble(), (double)value, JsonToken.Float);
                AssertTypedRead(value, reader => reader.ReadAsDecimal(), Convert.ToDecimal((float)value), JsonToken.Float);
                AssertTypedRead(value, reader => reader.ReadAsBoolean(), value != (Half)0, JsonToken.Boolean);
            }
            foreach (Half value in new[] { Half.NaN, Half.PositiveInfinity, Half.NegativeInfinity })
            {
                AssertTypedRead(value, reader => reader.ReadAsDouble(), (double)value, JsonToken.Float);
                AssertTypedRead(value, reader => reader.ReadAsBoolean(), true, JsonToken.Boolean);
                using (JsonReader reader = new JValue((object)value).CreateReader())
                {
                    Assert.IsType<OverflowException>(Assert.Throws<JsonReaderException>(() => reader.ReadAsInt32()).InnerException);
                    Assert.Equal(value, reader.Value);
                }
                using (JsonReader reader = new JValue((object)value).CreateReader())
                {
                    Assert.IsType<OverflowException>(Assert.Throws<JsonReaderException>(() => reader.ReadAsDecimal()).InnerException);
                    Assert.Equal(value, reader.Value);
                }
            }
        }

#if HAVE_INT128
        [Fact]
        public void TypedReaderIntegerEdges()
        {
            object[] values = { Int128.MinValue, Int128.MaxValue, UInt128.MaxValue, Int128.Zero, UInt128.Zero,
                (Int128)int.MinValue, (UInt128)int.MaxValue, (Int128)int.MaxValue + 1, (Int128)decimal.MaxValue, (UInt128)decimal.MaxValue };
            foreach (object value in values)
            {
                BigInteger integer = value is Int128 signed ? (BigInteger)signed : (BigInteger)(UInt128)value;
                AssertTypedRead(value, reader => reader.ReadAsDouble(), (double)integer, JsonToken.Float);
                AssertTypedRead(value, reader => reader.ReadAsBoolean(), integer != 0, JsonToken.Boolean);
                if (integer >= int.MinValue && integer <= int.MaxValue)
                {
                    AssertTypedRead(value, reader => reader.ReadAsInt32(), (int)integer, JsonToken.Integer);
                }
                else
                {
                    using (JsonReader reader = new JValue(value).CreateReader())
                    {
                        Assert.Throws<OverflowException>(() => reader.ReadAsInt32());
                        Assert.Same(value, reader.Value);
                    }
                }
                if (integer >= (BigInteger)decimal.MinValue && integer <= (BigInteger)decimal.MaxValue)
                {
                    AssertTypedRead(value, reader => reader.ReadAsDecimal(), (decimal)integer, JsonToken.Float);
                }
                else
                {
                    using (JsonReader reader = new JValue(value).CreateReader())
                    {
                        Assert.Throws<OverflowException>(() => reader.ReadAsDecimal());
                        Assert.Same(value, reader.Value);
                    }
                }
            }
        }
#endif

        private static void AssertTypedRead<T>(object value, Func<JsonReader, T?> read, T expected, JsonToken tokenType) where T : struct
        {
            JValue token = new JValue(value);
            using (JsonReader reader = new JArray(token, JValue.CreateNull()).CreateReader())
            {
                Assert.True(reader.Read());
                Assert.Equal(expected, read(reader).Value);
                Assert.Equal(tokenType, reader.TokenType);
                Assert.Equal(expected, Assert.IsType<T>(reader.Value));
                Assert.Same(value, token.Value);
                Assert.Null(read(reader));
                Assert.Equal(JsonToken.Null, reader.TokenType);
                Assert.Null(read(reader));
                Assert.Equal(JsonToken.EndArray, reader.TokenType);
                Assert.Null(read(reader));
                Assert.Equal(JsonToken.None, reader.TokenType);
            }
        }

        private sealed class LegacyNumbers
        {
            public int Integer { get; set; }
            public double Double { get; set; }
            public decimal Decimal { get; set; }
            public bool Boolean { get; set; }
        }

        private sealed class OriginalNumberConverter : JsonConverter<double>
        {
            private readonly object _expectedValue;

            public OriginalNumberConverter(object expectedValue)
            {
                _expectedValue = expectedValue;
            }

            public override void WriteJson(JsonWriter writer, double value, JsonSerializer serializer) => throw new NotSupportedException();

            public override double ReadJson(JsonReader reader, Type objectType, double existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                Assert.Same(_expectedValue, reader.Value);
                Assert.Equal(_expectedValue is Half ? JsonToken.Float : JsonToken.Integer, reader.TokenType);
                return 42d;
            }
        }

        [Fact]
        public async Task ConverterAndTokenEntryPoints()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new HalfConverter());
            Assert.Equal("\"half\"", JsonConvert.SerializeObject((Half)1.5f, settings));
            Assert.Equal((Half)2.5f, JsonConvert.DeserializeObject<Half>("1.5", settings));

            object[] values = { (Half)1.5f,
#if HAVE_INT128
                Int128.MinValue, UInt128.MaxValue,
#endif
            };
            foreach (object value in values)
            {
                using (JTokenWriter writer = new JTokenWriter())
                {
                    await writer.WriteValueAsync(value);
                    Assert.Equal(value, ((JValue)writer.Token).Value);
                }
                JValue token = new JValue(value);
                StringWriter output = new StringWriter();
                using (JsonTextWriter writer = new JsonTextWriter(output))
                {
                    await writer.WriteTokenAsync(token.CreateReader());
                }
                Assert.Equal(JsonConvert.SerializeObject(value), output.ToString());
                using (JsonTextWriter writer = new JsonTextWriter(new StringWriter()))
                {
                    await Assert.ThrowsAnyAsync<OperationCanceledException>(() => writer.WriteValueAsync(value, new CancellationToken(true)));
                }
            }
        }

        [Fact]
        public void HalfEdges()
        {
            foreach (short bits in new short[] { 0, short.MinValue, 1, -32767, 1023, 1024, 31743, -1025 })
            {
                Half value = BitConverter.Int16BitsToHalf(bits);
                Half result = JsonConvert.DeserializeObject<Half>(JsonConvert.SerializeObject(value));
                Assert.Equal(bits, BitConverter.HalfToInt16Bits(result));
            }
            Assert.Equal(Half.PositiveInfinity, JsonConvert.DeserializeObject<Half>("1e100"));
            Assert.Equal(Half.NegativeInfinity, JsonConvert.DeserializeObject<Half>("-Infinity"));
            Assert.True(Half.IsNaN(JsonConvert.DeserializeObject<Half>("NaN")));
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Half[]>("[1.5,\"invalid\"]"));
            Assert.IsType<double>(((JValue)JToken.Parse("1.5")).Value);
        }

        [Fact]
        public void ReadHalfUnexpectedEndWithCustomReader()
        {
            using (UnexpectedEndReader reader = new UnexpectedEndReader())
            {
                JsonSerializationException exception = Assert.Throws<JsonSerializationException>(() => new JsonSerializer().Deserialize<Half[]>(reader));
                Assert.Equal("Unexpected end when deserializing array. Path '[0]'.", exception.Message);
                Assert.Equal(3, reader.ReadCalls);
                Assert.Equal(JsonToken.None, reader.TokenType);
            }
        }

        private sealed class UnexpectedEndReader : JsonReader
        {
            public int ReadCalls { get; private set; }

            public override bool Read()
            {
                ReadCalls++;
                switch (ReadCalls)
                {
                    case 1:
                        SetToken(JsonToken.StartArray);
                        return true;
                    case 2:
                        SetToken(JsonToken.Float, 1.0);
                        return true;
                    case 3:
                        return false;
                    default:
                        throw new InvalidOperationException("Read called again after reaching the end.");
                }
            }
        }

        private sealed class HalfConverter : JsonConverter<Half>
        {
            public override void WriteJson(JsonWriter writer, Half value, JsonSerializer serializer) => writer.WriteValue("half");

            public override Half ReadJson(JsonReader reader, Type objectType, Half existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                Assert.Equal(JsonToken.Float, reader.TokenType);
                Assert.IsType<double>(reader.Value);
                return (Half)2.5f;
            }
        }

        [Fact]
        public async Task FloatSettingsAndCasts()
        {
            Assert.Equal(1.5, (double)new JValue((object)(Half)1.5f));
            Assert.Equal(2, (int)new JValue((object)(Half)2));
#if HAVE_INT128
            Assert.Equal(42L, (long)new JValue((object)(Int128)42));
            Assert.Throws<OverflowException>(() => (long)new JValue((object)UInt128.MaxValue));
            Assert.Throws<OverflowException>(() => ConvertUtils.ConvertOrCast(-1, CultureInfo.InvariantCulture, typeof(UInt128)));
            Assert.Equal((Int128)12, ConvertUtils.Convert((byte)12, CultureInfo.InvariantCulture, typeof(Int128)));
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Int128>(((BigInteger)Int128.MaxValue + 1).ToString()));
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<UInt128>(((BigInteger)UInt128.MaxValue + 1).ToString()));
#endif
            foreach (FloatFormatHandling handling in new[] { FloatFormatHandling.String, FloatFormatHandling.Symbol, FloatFormatHandling.DefaultValue })
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { FloatFormatHandling = handling, TraceWriter = new MemoryTraceWriter() };
                string expected = handling == FloatFormatHandling.String ? "\"Infinity\"" : handling == FloatFormatHandling.Symbol ? "Infinity" : "0.0";
                Assert.Equal(expected, JsonConvert.SerializeObject(Half.PositiveInfinity, settings));
                Assert.Equal("{\"Value\":" + (handling == FloatFormatHandling.DefaultValue ? "null" : expected) + "}", JsonConvert.SerializeObject(new NullableHalf { Value = Half.PositiveInfinity }, settings));
                StringWriter output = new StringWriter();
                using (JsonTextWriter writer = new JsonTextWriter(output) { FloatFormatHandling = handling, QuoteChar = '\'' })
                {
                    await writer.WriteValueAsync((object)Half.PositiveInfinity);
                }
                Assert.Equal(expected.Replace('"', '\''), output.ToString());
            }
        }

        private sealed class NullableHalf
        {
            public Half? Value { get; set; }
        }

        [Fact]
        public async Task TokensAndKeys()
        {
            object[] values = { (Half)1.1f,
#if HAVE_INT128
                Int128.MinValue, UInt128.MaxValue,
#endif
            };
            foreach (object value in values)
            {
                JValue token = new JValue(value);
                string json = JsonConvert.SerializeObject(value);
                Assert.Equal(json, token.ToString(Formatting.None));
                Assert.Equal(value, ((JValue)JToken.FromObject(value)).Value);
                Assert.Equal(value, token.ToObject(value.GetType()));
                Assert.Equal(json, JsonConvert.SerializeObject(value, new JsonSerializerSettings { TraceWriter = new MemoryTraceWriter() }));
                StringWriter output = new StringWriter();
                using (JsonTextWriter writer = new JsonTextWriter(output))
                {
                    await token.WriteToAsync(writer);
                }
                Assert.Equal(json, output.ToString());
            }
            Assert.Equal((Half)1.5f, new JValue(1.5).Value<Half>());
            Assert.Equal(1.5, new JValue((object)(Half)1.5f).Value<double>());
            Dictionary<Half, Half> halves = new Dictionary<Half, Half> { [(Half)1.1f] = (Half)2.5f };
            Assert.Equal(halves, JsonConvert.DeserializeObject<Dictionary<Half, Half>>(JsonConvert.SerializeObject(halves)));
#if HAVE_INT128
            Assert.Equal(UInt128.MaxValue, JToken.Parse(UInt128.MaxValue.ToString()).Value<UInt128>());
            Assert.True(new JValue((object)UInt128.MaxValue).CompareTo(new JValue((object)Int128.MaxValue)) > 0);
            Assert.Equal(0, new JValue((object)Int128.MaxValue).CompareTo(new JValue((object)(BigInteger)Int128.MaxValue)));
            Dictionary<UInt128, Int128> integers = new Dictionary<UInt128, Int128> { [UInt128.MaxValue] = Int128.MinValue };
            Assert.Equal(integers, JsonConvert.DeserializeObject<Dictionary<UInt128, Int128>>(JsonConvert.SerializeObject(integers)));
#endif
        }

        [Fact]
        public void ReadHalfPrecisely()
        {
            string number = "1.00048828125000000000001";
            Half expected = Half.Parse(number, CultureInfo.InvariantCulture);
            Assert.Equal(expected, JsonConvert.DeserializeObject<Half>(number));
            Assert.Equal(expected, JsonConvert.DeserializeObject<Half[]>("[/*number*/" + number + "]")[0]);
            JsonSerializerSettings settings = new JsonSerializerSettings { TraceWriter = new MemoryTraceWriter(), Culture = CultureInfo.GetCultureInfo("fr-FR") };
            Assert.Equal(expected, JsonConvert.DeserializeObject<Half>(number, settings));
            Assert.Equal((Half)1.5f, JsonConvert.DeserializeObject<Half>("\"1,5\"", settings));
            Assert.Null(JsonConvert.DeserializeObject<Half?>("null"));
            Assert.Throws<JsonReaderException>(() => JsonConvert.DeserializeObject<Half>("true"));
        }

        [Fact]
        public async Task WriteNumbers()
        {
            object[] values = {
                (Half)1.1f, Half.MaxValue, Half.Epsilon,
#if HAVE_INT128
                Int128.MinValue, Int128.MaxValue, UInt128.MaxValue,
#endif
            };
            foreach (object value in values)
            {
                string json = JsonConvert.SerializeObject(value);
                Assert.Equal(value, JsonConvert.DeserializeObject(json, value.GetType()));
                Assert.Equal(json, JsonConvert.ToString(value));
                StringWriter output = new StringWriter(CultureInfo.InvariantCulture);
                using (JsonTextWriter writer = new JsonTextWriter(output))
                {
                    await writer.WriteStartArrayAsync();
                    await writer.WriteValueAsync(value);
                    await writer.WriteEndArrayAsync();
                }
                Assert.Equal("[" + json + "]", output.ToString());
            }
            Assert.Equal("\"NaN\"", JsonConvert.SerializeObject(Half.NaN));
        }

        [Fact]
        public void PrimitiveContracts()
        {
            Assert.IsType<JsonPrimitiveContract>(DefaultContractResolver.Instance.ResolveContract(typeof(Half)));
            Assert.Equal((Half)1.5f, ConvertUtils.Convert("1.5", CultureInfo.InvariantCulture, typeof(Half)));
#if HAVE_INT128
            Assert.IsType<JsonPrimitiveContract>(DefaultContractResolver.Instance.ResolveContract(typeof(Int128)));
            Assert.IsType<JsonPrimitiveContract>(DefaultContractResolver.Instance.ResolveContract(typeof(UInt128?)));
            Assert.Equal(Int128.MinValue, JsonConvert.DeserializeObject<Int128>(Int128.MinValue.ToString(CultureInfo.InvariantCulture)));
            Assert.Equal(UInt128.MaxValue, JsonConvert.DeserializeObject<UInt128>(UInt128.MaxValue.ToString(CultureInfo.InvariantCulture)));
            Assert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<UInt128>("-1"));
#endif
        }
    }
}
#endif