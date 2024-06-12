using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if DNXCORE50
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
        private static List<KeyValuePair<string, int>> testValues = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("First", 123),
                new KeyValuePair<string, int>("Second", 456)
            };

        private const string serialized = @"[
  {
    ""Key"": ""First"",
    ""Value"": 123
  },
  {
    ""Key"": ""Second"",
    ""Value"": 456
  }
]";

        [Test]
        public void SerializeUsingInternalConverter()
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver();
            JsonObjectContract contract = (JsonObjectContract)contractResolver.ResolveContract(typeof(KeyValuePair<string, int>));

            Assert.AreEqual(typeof(KeyValuePairConverter), contract.InternalConverter.GetType());

            string json = JsonConvert.SerializeObject(testValues, Formatting.Indented);

            StringAssert.AreEqual(serialized, json);

            IList<KeyValuePair<string, int>> v2 = JsonConvert.DeserializeObject<IList<KeyValuePair<string, int>>>(serialized);
            AssertValues(v2);
        }

#if HAVE_ASYNC
        [Test]
        public async System.Threading.Tasks.Task SerializeAndDeserializeAsync()
        {
            var helper = new AsyncTestHelper();
            string json = await helper.SerializeAsync(testValues);
            StringAssert.AreEqual(serialized, json);

            helper.ResetStream();
            IList<KeyValuePair<string, int>> v2 = await helper.DeserializeAsync<IList<KeyValuePair<string, int>>>(serialized);
            AssertValues(v2);
        }
#endif
        private void AssertValues(IList<KeyValuePair<string, int>> v2)
        {
            Assert.AreEqual(testValues.Count, v2.Count);
            for(int i = 0; i < testValues.Count; i++)
            {
                Assert.AreEqual(testValues[i].Key, v2[i].Key);
                Assert.AreEqual(testValues[i].Value, v2[i].Value);
            }
        }

        [Test]
        public void DeserializeUnexpectedEnd()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<KeyValuePair<string, int>>(@"{""Key"": ""123"","), "Unexpected end when reading JSON. Path 'Key', line 1, position 14.");
        }

    }
}