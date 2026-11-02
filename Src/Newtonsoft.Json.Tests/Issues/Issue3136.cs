#pragma warning disable 618
using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    // https://github.com/JamesNK/Newtonsoft.Json/issues/3136
    // .NET 11 changed Convert.ToDecimal(double) to return the exact binary value of the double.
    // A decimal read from a double-backed JValue must keep the value written in the JSON, as on earlier runtimes.
    [TestFixture]
    public class Issue3136
    {
        [Test]
        public void ToObject_DoubleValue_ReturnsDecimalWrittenInJson()
        {
            JToken token = JObject.Parse(@"{""rate"":0.1}")["rate"];

            Assert.AreEqual(0.1m, token.ToObject<decimal>());
            Assert.AreEqual(0.1m, token.ToObject<decimal?>());
        }

        [Test]
        public void ToObject_ObjectWithDecimalProperty_ReturnsDecimalWrittenInJson()
        {
            JObject o = JObject.Parse(@"{""Rate"":2.675}");

            Assert.AreEqual(2.675m, o.ToObject<Rated>().Rate);
        }

        [Test]
        public void ExplicitConversion_DoubleValue_ReturnsDecimalWrittenInJson()
        {
            JToken token = JObject.Parse(@"{""rate"":1E-7}")["rate"];

            Assert.AreEqual(0.0000001m, (decimal)token);
            Assert.AreEqual(0.0000001m, (decimal?)token);
        }

        [Test]
        public void Value_DoubleValue_ReturnsDecimalWrittenInJson()
        {
            JObject o = JObject.Parse(@"{""rate"":0.1}");

            Assert.AreEqual(0.1m, o.Value<decimal>("rate"));
            Assert.AreEqual(0.1m, o.Value<decimal?>("rate"));
            Assert.AreEqual(0.1m, o["rate"].Value<decimal>());
        }

#if !(NET35 || NET20)
        [Test]
        public void Dynamic_DoubleValue_ReturnsDecimalWrittenInJson()
        {
            dynamic o = JObject.Parse(@"{""rate"":0.1}");

            decimal rate = o.rate;
            Assert.AreEqual(0.1m, rate);

            JValue sum = o.rate + 0.2m;
            Assert.AreEqual(0.3m, sum.Value);
        }
#endif

        [Test]
        public void DeepEquals_DoubleAndDecimal_AreEqual()
        {
            Assert.IsTrue(JToken.DeepEquals(new JValue(0.1d), new JValue(0.1m)));
            Assert.AreEqual(0, new JValue(0.1d).CompareTo(new JValue(0.1m)));
        }

        [Test]
        public void ExplicitConversion_SingleValue_RoundsToSevenSignificantDigits()
        {
            Assert.AreEqual(0.1m, (decimal)new JValue(0.1f));
        }

        [Test]
        public void ExplicitConversion_ComputedDouble_RoundsToFifteenSignificantDigits()
        {
            Assert.AreEqual(0.3m, (decimal)new JValue(0.1d + 0.2d));
            Assert.AreEqual(1m, (decimal)new JValue(1.0000000000000002d));
        }

        [Test]
        public void ExplicitConversion_TinyDouble_HasNoTrailingZeros()
        {
            Assert.AreEqual("0", ((decimal)new JValue(1E-30d)).ToString(CultureInfo.InvariantCulture));
            Assert.AreEqual("0.0000000000000000000000000001", ((decimal)new JValue(1E-28d)).ToString(CultureInfo.InvariantCulture));
        }

        [Test]
        public void ExplicitConversion_DoubleOutsideDecimalRange_Throws()
        {
            ExceptionAssert.Throws<OverflowException>(() => { decimal _ = (decimal)new JValue(double.MaxValue); });
            ExceptionAssert.Throws<OverflowException>(() => { decimal _ = (decimal)new JValue(Math.Pow(2, 96)); });
            ExceptionAssert.Throws<OverflowException>(() => { decimal _ = (decimal)new JValue(double.NaN); });
        }

        [Test]
        public void BsonReader_FloatParseHandlingDecimal_ReturnsDecimalWritten()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartObject();
            writer.WritePropertyName("rate");
            writer.WriteValue(0.1d);
            writer.WriteEndObject();
            writer.Flush();

            ms.Position = 0;
            BsonReader reader = new BsonReader(ms) { FloatParseHandling = FloatParseHandling.Decimal };
            JObject o = (JObject)JToken.ReadFrom(reader);

            Assert.AreEqual(0.1m, o["rate"].Value<decimal>());
        }

        public class Rated
        {
            public decimal Rate { get; set; }
        }
    }
}
