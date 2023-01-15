using System.Linq;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    public class Issue2775
    {
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2775
        public void TokenType()
        {
            var jObject = new JObject { { "NullProperty", false ? "0" : null } };

            var jToken = JToken.FromObject(jObject);
            
            Assert.AreEqual(JTokenType.Null, jToken.Children().Children().Single().Type);

            jObject = new JObject { { "NullProperty", (string)null } };

            jToken = JToken.FromObject(jObject);
            Assert.AreEqual(JTokenType.Null, jToken.Children().Children().Single().Type);
        }
    }
}