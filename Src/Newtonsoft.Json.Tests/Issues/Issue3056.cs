using System.Linq;
using Newtonsoft.Json.Linq;
using System;

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
    public class Issue3056
    {
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3056
        public void RoundTripOfNestedArraysWithOneItem()
        {
            var json = @"{""test"":[[""1""]]}";
            StringAssert.AreEqual(json, JsonToXmlToJson(json));
        }

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3056
        public void RoundTripOfDeeperNestedArrays()
        {
            var json = @"{""test"":[[[[[""1""]]]]]}";
            StringAssert.AreEqual(json, JsonToXmlToJson(json));
        }

        private static string JsonToXmlToJson(string json)
        {
#if NET20
            var xmldoc = JsonConvert.DeserializeXmlNode(json, "root", writeArrayAttribute: true);
            return JsonConvert.SerializeXmlNode(xmldoc, Formatting.None, true);
#else
            var xdoc = JsonConvert.DeserializeXNode(json, "root", writeArrayAttribute: true);
            return JsonConvert.SerializeXNode(xdoc, Formatting.None, true);
#endif
        }
    }
}