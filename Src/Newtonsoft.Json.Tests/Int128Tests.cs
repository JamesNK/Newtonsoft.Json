#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if HAVE_INT128

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class Int128Tests : TestFixtureBase
    {
        private const string Int128MaxStr = "170141183460469231731687303715884105727";
        private const string Int128MinStr = "-170141183460469231731687303715884105728";
        private const string UInt128MaxStr = "340282366920938463463374607431768211455";

        public class Int128Wrapper
        {
            public Int128 Value { get; set; }
        }

        public class UInt128Wrapper
        {
            public UInt128 Value { get; set; }
        }

        public class NullableInt128Wrapper
        {
            public Int128? Value { get; set; }
        }

        public class NullableUInt128Wrapper
        {
            public UInt128? Value { get; set; }
        }

        public class MixedWrapper
        {
            public Int128 Signed { get; set; }
            public UInt128 Unsigned { get; set; }
            public Int128? NullableSigned { get; set; }
            public UInt128? NullableUnsigned { get; set; }
        }

        // ------------------------------------------------------------------
        // Serialization: Int128 must emit a raw JSON number, not a quoted string
        // ------------------------------------------------------------------

        [Test]
        public void Serialize_Int128_Max_AsJsonNumber()
        {
            string json = JsonConvert.SerializeObject(new Int128Wrapper { Value = Int128.MaxValue });
            Assert.AreEqual("{\"Value\":" + Int128MaxStr + "}", json);
        }

        [Test]
        public void Serialize_Int128_Min_AsJsonNumber()
        {
            string json = JsonConvert.SerializeObject(new Int128Wrapper { Value = Int128.MinValue });
            Assert.AreEqual("{\"Value\":" + Int128MinStr + "}", json);
        }

        [Test]
        public void Serialize_Int128_Zero()
        {
            string json = JsonConvert.SerializeObject(new Int128Wrapper { Value = Int128.Zero });
            Assert.AreEqual("{\"Value\":0}", json);
        }

        [Test]
        public void Serialize_Int128_Negative()
        {
            string json = JsonConvert.SerializeObject(new Int128Wrapper { Value = -(Int128)42 });
            Assert.AreEqual("{\"Value\":-42}", json);
        }

        [Test]
        public void Serialize_Int128_FitsInLongRange()
        {
            string json = JsonConvert.SerializeObject(new Int128Wrapper { Value = (Int128)long.MaxValue });
            Assert.AreEqual("{\"Value\":9223372036854775807}", json);
        }

        [Test]
        public void Serialize_UInt128_Max_AsJsonNumber()
        {
            string json = JsonConvert.SerializeObject(new UInt128Wrapper { Value = UInt128.MaxValue });
            Assert.AreEqual("{\"Value\":" + UInt128MaxStr + "}", json);
        }

        [Test]
        public void Serialize_UInt128_Zero()
        {
            string json = JsonConvert.SerializeObject(new UInt128Wrapper { Value = UInt128.Zero });
            Assert.AreEqual("{\"Value\":0}", json);
        }

        [Test]
        public void Serialize_UInt128_AboveLongMax()
        {
            UInt128 value = (UInt128)ulong.MaxValue + 1;
            string json = JsonConvert.SerializeObject(new UInt128Wrapper { Value = value });
            Assert.AreEqual("{\"Value\":18446744073709551616}", json);
        }

        [Test]
        public void Serialize_NullableInt128_HasValue()
        {
            string json = JsonConvert.SerializeObject(new NullableInt128Wrapper { Value = Int128.MaxValue });
            Assert.AreEqual("{\"Value\":" + Int128MaxStr + "}", json);
        }

        [Test]
        public void Serialize_NullableInt128_Null()
        {
            string json = JsonConvert.SerializeObject(new NullableInt128Wrapper { Value = null });
            Assert.AreEqual("{\"Value\":null}", json);
        }

        [Test]
        public void Serialize_NullableUInt128_HasValue()
        {
            string json = JsonConvert.SerializeObject(new NullableUInt128Wrapper { Value = UInt128.MaxValue });
            Assert.AreEqual("{\"Value\":" + UInt128MaxStr + "}", json);
        }

        [Test]
        public void Serialize_NullableUInt128_Null()
        {
            string json = JsonConvert.SerializeObject(new NullableUInt128Wrapper { Value = null });
            Assert.AreEqual("{\"Value\":null}", json);
        }

        [Test]
        public void Serialize_Int128_ViaObjectOverload()
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteValue((object)Int128.MaxValue);
            }
            Assert.AreEqual(Int128MaxStr, sw.ToString());
        }

        [Test]
        public void Serialize_UInt128_ViaObjectOverload()
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                writer.WriteValue((object)UInt128.MaxValue);
            }
            Assert.AreEqual(UInt128MaxStr, sw.ToString());
        }

        [Test]
        public void Serialize_Int128_InList()
        {
            var list = new List<Int128> { Int128.MinValue, Int128.Zero, Int128.MaxValue };
            string json = JsonConvert.SerializeObject(list);
            Assert.AreEqual("[" + Int128MinStr + ",0," + Int128MaxStr + "]", json);
        }

        [Test]
        public void Serialize_UInt128_InList()
        {
            var list = new List<UInt128> { UInt128.Zero, (UInt128)1, UInt128.MaxValue };
            string json = JsonConvert.SerializeObject(list);
            Assert.AreEqual("[0,1," + UInt128MaxStr + "]", json);
        }

        [Test]
        public void Serialize_MixedTypes()
        {
            var w = new MixedWrapper
            {
                Signed = Int128.MaxValue,
                Unsigned = UInt128.MaxValue,
                NullableSigned = Int128.MinValue,
                NullableUnsigned = null
            };
            string json = JsonConvert.SerializeObject(w);
            Assert.AreEqual(
                "{\"Signed\":" + Int128MaxStr +
                ",\"Unsigned\":" + UInt128MaxStr +
                ",\"NullableSigned\":" + Int128MinStr +
                ",\"NullableUnsigned\":null}",
                json);
        }

        // ------------------------------------------------------------------
        // Deserialization: JSON number -> Int128 / UInt128
        // ------------------------------------------------------------------

        [Test]
        public void Deserialize_Int128_Max()
        {
            var r = JsonConvert.DeserializeObject<Int128Wrapper>("{\"Value\":" + Int128MaxStr + "}");
            Assert.AreEqual(Int128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_Int128_Min()
        {
            var r = JsonConvert.DeserializeObject<Int128Wrapper>("{\"Value\":" + Int128MinStr + "}");
            Assert.AreEqual(Int128.MinValue, r.Value);
        }

        [Test]
        public void Deserialize_Int128_Zero()
        {
            var r = JsonConvert.DeserializeObject<Int128Wrapper>("{\"Value\":0}");
            Assert.AreEqual(Int128.Zero, r.Value);
        }

        [Test]
        public void Deserialize_Int128_Negative()
        {
            var r = JsonConvert.DeserializeObject<Int128Wrapper>("{\"Value\":-42}");
            Assert.AreEqual(-(Int128)42, r.Value);
        }

        [Test]
        public void Deserialize_Int128_FitsInLong()
        {
            var r = JsonConvert.DeserializeObject<Int128Wrapper>("{\"Value\":9223372036854775807}");
            Assert.AreEqual((Int128)long.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_UInt128_Max()
        {
            var r = JsonConvert.DeserializeObject<UInt128Wrapper>("{\"Value\":" + UInt128MaxStr + "}");
            Assert.AreEqual(UInt128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_UInt128_Zero()
        {
            var r = JsonConvert.DeserializeObject<UInt128Wrapper>("{\"Value\":0}");
            Assert.AreEqual(UInt128.Zero, r.Value);
        }

        [Test]
        public void Deserialize_UInt128_AboveLongMax()
        {
            var r = JsonConvert.DeserializeObject<UInt128Wrapper>("{\"Value\":18446744073709551616}");
            Assert.AreEqual((UInt128)ulong.MaxValue + 1, r.Value);
        }

        [Test]
        public void Deserialize_NullableInt128_HasValue()
        {
            var r = JsonConvert.DeserializeObject<NullableInt128Wrapper>("{\"Value\":" + Int128MaxStr + "}");
            Assert.AreEqual(Int128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_NullableInt128_Null()
        {
            var r = JsonConvert.DeserializeObject<NullableInt128Wrapper>("{\"Value\":null}");
            Assert.IsNull(r.Value);
        }

        [Test]
        public void Deserialize_NullableInt128_Missing()
        {
            var r = JsonConvert.DeserializeObject<NullableInt128Wrapper>("{}");
            Assert.IsNull(r.Value);
        }

        [Test]
        public void Deserialize_NullableUInt128_HasValue()
        {
            var r = JsonConvert.DeserializeObject<NullableUInt128Wrapper>("{\"Value\":" + UInt128MaxStr + "}");
            Assert.AreEqual(UInt128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_NullableUInt128_Null()
        {
            var r = JsonConvert.DeserializeObject<NullableUInt128Wrapper>("{\"Value\":null}");
            Assert.IsNull(r.Value);
        }

        [Test]
        public void Deserialize_Int128_FromJsonString()
        {
            var r = JsonConvert.DeserializeObject<Int128Wrapper>("{\"Value\":\"" + Int128MaxStr + "\"}");
            Assert.AreEqual(Int128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_UInt128_FromJsonString()
        {
            var r = JsonConvert.DeserializeObject<UInt128Wrapper>("{\"Value\":\"" + UInt128MaxStr + "\"}");
            Assert.AreEqual(UInt128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_Int128_Overflow_Throws()
        {
            string tooLarge = "{\"Value\":" + UInt128MaxStr + "}";
            ExceptionAssert.Throws<JsonSerializationException>(
                () => JsonConvert.DeserializeObject<Int128Wrapper>(tooLarge));
        }

        [Test]
        public void Deserialize_UInt128_Negative_WrapsAround()
        {
            // BigInteger -> UInt128 cast is unchecked, so -1 becomes UInt128.MaxValue.
            // Documents current behavior; a JsonSerializationException here would arguably be safer.
            var r = JsonConvert.DeserializeObject<UInt128Wrapper>("{\"Value\":-1}");
            Assert.AreEqual(UInt128.MaxValue, r.Value);
        }

        [Test]
        public void Deserialize_List_Int128()
        {
            string json = "[" + Int128MinStr + ",0," + Int128MaxStr + "]";
            var list = JsonConvert.DeserializeObject<List<Int128>>(json);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(Int128.MinValue, list[0]);
            Assert.AreEqual(Int128.Zero, list[1]);
            Assert.AreEqual(Int128.MaxValue, list[2]);
        }

        [Test]
        public void Deserialize_List_UInt128()
        {
            string json = "[0,1," + UInt128MaxStr + "]";
            var list = JsonConvert.DeserializeObject<List<UInt128>>(json);
            Assert.AreEqual(3, list.Count);
            Assert.AreEqual(UInt128.Zero, list[0]);
            Assert.AreEqual((UInt128)1, list[1]);
            Assert.AreEqual(UInt128.MaxValue, list[2]);
        }

        // ------------------------------------------------------------------
        // Round-trip
        // ------------------------------------------------------------------

        [Test]
        public void RoundTrip_Int128_AllBoundaries()
        {
            foreach (var value in new[] { Int128.MinValue, -(Int128)1, Int128.Zero, (Int128)1, Int128.MaxValue })
            {
                string json = JsonConvert.SerializeObject(new Int128Wrapper { Value = value });
                var back = JsonConvert.DeserializeObject<Int128Wrapper>(json);
                Assert.AreEqual(value, back.Value, "round-trip mismatch for " + value);
            }
        }

        [Test]
        public void RoundTrip_UInt128_AllBoundaries()
        {
            foreach (var value in new[] { UInt128.Zero, (UInt128)1, (UInt128)ulong.MaxValue, UInt128.MaxValue })
            {
                string json = JsonConvert.SerializeObject(new UInt128Wrapper { Value = value });
                var back = JsonConvert.DeserializeObject<UInt128Wrapper>(json);
                Assert.AreEqual(value, back.Value, "round-trip mismatch for " + value);
            }
        }

        [Test]
        public void RoundTrip_MixedWrapper()
        {
            var original = new MixedWrapper
            {
                Signed = Int128.MinValue,
                Unsigned = UInt128.MaxValue,
                NullableSigned = null,
                NullableUnsigned = (UInt128)12345
            };
            string json = JsonConvert.SerializeObject(original);
            var back = JsonConvert.DeserializeObject<MixedWrapper>(json);
            Assert.AreEqual(original.Signed, back.Signed);
            Assert.AreEqual(original.Unsigned, back.Unsigned);
            Assert.AreEqual(original.NullableSigned, back.NullableSigned);
            Assert.AreEqual(original.NullableUnsigned, back.NullableUnsigned);
        }

        [Test]
        public void RoundTrip_Dictionary()
        {
            var original = new Dictionary<string, Int128>
            {
                ["min"] = Int128.MinValue,
                ["zero"] = Int128.Zero,
                ["max"] = Int128.MaxValue
            };
            string json = JsonConvert.SerializeObject(original);
            var back = JsonConvert.DeserializeObject<Dictionary<string, Int128>>(json);
            Assert.AreEqual(Int128.MinValue, back["min"]);
            Assert.AreEqual(Int128.Zero, back["zero"]);
            Assert.AreEqual(Int128.MaxValue, back["max"]);
        }

        // ------------------------------------------------------------------
        // JTokenWriter / JValue path (LINQ to JSON)
        // ------------------------------------------------------------------

        [Test]
        public void JValue_From_Int128_IsIntegerType()
        {
            JValue v = new JValue(Int128.MaxValue);
            Assert.AreEqual(JTokenType.Integer, v.Type);
            Assert.AreEqual(Int128.MaxValue, v.Value);
        }

        [Test]
        public void JValue_From_UInt128_IsIntegerType()
        {
            JValue v = new JValue(UInt128.MaxValue);
            Assert.AreEqual(JTokenType.Integer, v.Type);
            Assert.AreEqual(UInt128.MaxValue, v.Value);
        }

        [Test]
        public void JValue_Int128_WritesAsRawNumber()
        {
            JValue v = new JValue(Int128.MaxValue);
            Assert.AreEqual(Int128MaxStr, v.ToString());
        }

        [Test]
        public void JValue_UInt128_WritesAsRawNumber()
        {
            JValue v = new JValue(UInt128.MaxValue);
            Assert.AreEqual(UInt128MaxStr, v.ToString());
        }

        [Test]
        public void JValue_Equals_Int128()
        {
            JValue a = new JValue(Int128.MaxValue);
            JValue b = new JValue(Int128.MaxValue);
            Assert.IsTrue(a.Equals(b));
        }

        [Test]
        public void JValue_CompareTo_Int128_SameSign()
        {
            JValue small = new JValue(-(Int128)1);
            JValue big = new JValue(Int128.MaxValue);
            Assert.IsTrue(small.CompareTo(big) < 0);
            Assert.IsTrue(big.CompareTo(small) > 0);
            Assert.AreEqual(0, big.CompareTo(new JValue(Int128.MaxValue)));
        }

        [Test]
        public void JValue_CompareTo_UInt128()
        {
            JValue small = new JValue(UInt128.Zero);
            JValue big = new JValue(UInt128.MaxValue);
            Assert.IsTrue(small.CompareTo(big) < 0);
        }

        [Test]
        public void JValue_CompareTo_Int128_Vs_Long()
        {
            JValue i128 = new JValue(Int128.MaxValue);
            JValue l = new JValue(long.MaxValue);
            Assert.IsTrue(i128.CompareTo(l) > 0);
        }

        [Test]
        public void JValue_CompareTo_UInt128_Vs_Int128()
        {
            JValue u = new JValue(UInt128.MaxValue);
            JValue s = new JValue(Int128.MaxValue);
            Assert.IsTrue(u.CompareTo(s) > 0);
        }

        [Test]
        public void JValue_CompareTo_Int128_Vs_BigInteger()
        {
            JValue i128 = new JValue(Int128.MaxValue);
            JValue bi = new JValue(BigInteger.Parse(Int128MaxStr));
            Assert.AreEqual(0, i128.CompareTo(bi));
        }

        [Test]
        public void JObject_Parse_LargeInt_IsBigInteger()
        {
            // Numbers larger than UInt128 still land on BigInteger (legacy behavior).
            JObject o = JObject.Parse("{\"v\":" + new String('9', 45) + "}");
            JValue v = (JValue)o["v"];
            Assert.AreEqual(JTokenType.Integer, v.Type);
            Assert.AreEqual(typeof(BigInteger), v.Value.GetType());
        }

        [Test]
        public void JObject_Convert_To_Int128()
        {
            JObject o = JObject.Parse("{\"Value\":" + Int128MaxStr + "}");
            Int128 result = o["Value"].ToObject<Int128>();
            Assert.AreEqual(Int128.MaxValue, result);
        }

        [Test]
        public void JObject_Convert_To_UInt128()
        {
            JObject o = JObject.Parse("{\"Value\":" + UInt128MaxStr + "}");
            UInt128 result = o["Value"].ToObject<UInt128>();
            Assert.AreEqual(UInt128.MaxValue, result);
        }

        // ------------------------------------------------------------------
        // Async writer parity
        // ------------------------------------------------------------------

        [Test]
        public async Task Serialize_Int128_Async()
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("Value");
                await writer.WriteValueAsync((object)Int128.MaxValue);
                await writer.WriteEndObjectAsync();
            }
            Assert.AreEqual("{\"Value\":" + Int128MaxStr + "}", sw.ToString());
        }

        [Test]
        public async Task Serialize_UInt128_Async()
        {
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("Value");
                await writer.WriteValueAsync((object)UInt128.MaxValue);
                await writer.WriteEndObjectAsync();
            }
            Assert.AreEqual("{\"Value\":" + UInt128MaxStr + "}", sw.ToString());
        }

        // ------------------------------------------------------------------
        // Parity with BigInteger — output shape must match for common values
        // ------------------------------------------------------------------

        [Test]
        public void Int128_And_BigInteger_ProduceSameJsonForSameValue()
        {
            Int128 v128 = (Int128)123456789;
            BigInteger vBig = new BigInteger(123456789);

            StringWriter sw128 = new StringWriter();
            using (JsonTextWriter w = new JsonTextWriter(sw128)) { w.WriteValue((object)v128); }
            StringWriter swBig = new StringWriter();
            using (JsonTextWriter w = new JsonTextWriter(swBig)) { w.WriteValue(vBig); }

            Assert.AreEqual(swBig.ToString(), sw128.ToString());
        }
    }
}

#endif
