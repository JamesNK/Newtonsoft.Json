using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
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
            JsonObjectContract contract = (JsonObjectContract) contractResolver.ResolveContract(typeof (KeyValuePair<string, int>));

            Assert.AreEqual(typeof(KeyValuePairConverter), contract.InternalConverter.GetType());

            IList<KeyValuePair<string, int>> values = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("123", 123),
                new KeyValuePair<string, int>("456", 456)
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented);

            Console.WriteLine(json);
        }
    }
}