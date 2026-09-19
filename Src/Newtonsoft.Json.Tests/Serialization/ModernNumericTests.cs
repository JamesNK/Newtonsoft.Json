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