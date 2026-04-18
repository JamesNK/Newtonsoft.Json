#if HAVE_INT128
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
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
        [Test]
        public void SerializeInt128()
        {
            Int128 value = Int128.MaxValue;
            string json = JsonConvert.SerializeObject(new { value });
            Assert.AreEqual("{\"value\":170141183460469231731687303715884105727}", json);
        }

        [Test]
        public void SerializeUInt128()
        {
            UInt128 value = UInt128.MaxValue;
            string json = JsonConvert.SerializeObject(new { value });
            Assert.AreEqual("{\"value\":340282366920938463463374607431768211455}", json);
        }

        [Test]
        public void SerializeNullableInt128()
        {
            Int128? value = 123;
            string json = JsonConvert.SerializeObject(new { value });
            Assert.AreEqual("{\"value\":123}", json);

            value = null;
            json = JsonConvert.SerializeObject(new { value });
            Assert.AreEqual("{\"value\":null}", json);
        }

        [Test]
        public async Task SerializeInt128Async()
        {
            Int128 value = 456;
            StringWriter sw = new StringWriter();
            using (JsonTextWriter writer = new JsonTextWriter(sw))
            {
                await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("value");
                await writer.WriteValueAsync((object)value);
                await writer.WriteEndObjectAsync();
            }

            Assert.AreEqual("{\"value\":456}", sw.ToString());
        }

        [Test]
        public void ComparisonTests()
        {
            // Simple check that it's still treated as a number in JValue
            var jv1 = new JValue((object)(Int128)1);
            var jv2 = new JValue((object)(Int128)1);
            var jv3 = new JValue((object)(Int128)2);

            Assert.AreEqual(0, jv1.CompareTo(jv2));
            Assert.IsTrue(jv1.CompareTo(jv3) < 0);
        }
    }
}
#endif
