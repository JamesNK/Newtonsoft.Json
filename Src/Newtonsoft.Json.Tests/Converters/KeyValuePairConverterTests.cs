using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class KeyValuePairConverterTests : TestFixtureBase
    {
        [Test]
        public void SerializeUsingInternalConverter()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver();
            JsonObjectContract contract = (JsonObjectContract)contractResolver.ResolveContract(typeof(KeyValuePair<string, int>));

            Assert.AreEqual(typeof(KeyValuePairConverter), contract.InternalConverter.GetType());

            IList<KeyValuePair<string, int>> values = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("123", 123),
                new KeyValuePair<string, int>("456", 456)
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented);

            StringAssert.AreEqual(@"[
  {
    ""Key"": ""123"",
    ""Value"": 123
  },
  {
    ""Key"": ""456"",
    ""Value"": 456
  }
]", json);

            IList<KeyValuePair<string, int>> v2 = JsonConvert.DeserializeObject<IList<KeyValuePair<string, int>>>(json);

            Assert.AreEqual(2, v2.Count);
            Assert.AreEqual("123", v2[0].Key);
            Assert.AreEqual(123, v2[0].Value);
            Assert.AreEqual("456", v2[1].Key);
            Assert.AreEqual(456, v2[1].Value);
        }

        [Test]
        public void DeserializeUnexpectedEnd()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<KeyValuePair<string, int>>(@"{""Key"": ""123"","), "Unexpected end when reading JSON. Path 'Key', line 1, position 14.");
        }
    }
}