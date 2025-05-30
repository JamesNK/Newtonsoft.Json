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
    public class Issue3055
    {
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3055
        public void RoundTripWithSpecialCharacters()
        {
            var json = @"{""test:a"":[[""1""],""2""],""test b"":[""3""],""test#c"":[""4""],""test$d"":[""5""],""test%e"":[""6""],""test:f"":[""5""]}";
#if NET20
            var xmldoc = JsonConvert.DeserializeXmlNode(json, "root", writeArrayAttribute: true, encodeSpecialCharacters: true);
            var newJson = JsonConvert.SerializeXmlNode(xmldoc, Formatting.None, true);
#else
            var xdoc = JsonConvert.DeserializeXNode(json, "root", writeArrayAttribute: true, encodeSpecialCharacters: true);
            var newJson = JsonConvert.SerializeXNode(xdoc, Formatting.None, true);
#endif
            StringAssert.AreEqual(json, newJson);
        }
    }
}