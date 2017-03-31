using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif ASPNETCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else

#endif

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class ArrayAndDictionaryConverterTests
    {
        private Dictionary<string, object> DeserializeToDictionary(string json) 
        {
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json, new ArrayAndDictionaryConverter());
        }
        private object[] DeserializeToArray(string json)
        {
            return JsonConvert.DeserializeObject<object[]>(json, new ArrayAndDictionaryConverter());
        }
        private Dictionary<string, object> GetADictioaryWithArrayAndDictionary()
        {
            var d = new Dictionary<string, object>();
            d["prop"] = "msg";
            d["dic"] = new Dictionary<string, object>
                           {
                               {"key1", "val1"},
                               {"key2", "val2"},
                               {"key3", new[] {"val3"}},
                               {"key4", new[] {1, 12345}},
                               {"key5", new[] {new Dictionary<string, object> {{"key6", "val6"}}}}
                           };
            d["array"] = new[] { "val4" };
            d["int"] = 1;
            d["double"] = 1.2;
            d["null"] = null;
            d["bool"] = true;
            return d;
        }

        [Test]
        public void CanDeserializeADictionaryWithArrayAndDictionary()
        {
            var dictionary = GetADictioaryWithArrayAndDictionary();
            var deserialized = DeserializeToDictionary(JsonConvert.SerializeObject(dictionary));
            Assert.That(deserialized, Is.EqualTo(dictionary));
        }

        private object AnAnonymousObjectWithADictionary()
        {
            return new
                {
                    level = "ALERT",
                    message = "Message",
                    properties =
                    new Dictionary<string, object> { { "prop", "msg" } }
                };
        }

        [Test]
        public void SerializeAnonymousObject()
        {
            const string json =
    @"{
    ""level"":""ALERT"",
    ""message"":""Message"",
    ""properties"":{""prop"":""msg""}
}";
            var result = DeserializeToDictionary(json);
            Assert.That(result, Js.IsEqualTo(AnAnonymousObjectWithADictionary()));
        }

        [Test]
        public void ArrayOfArrayOf()
        {
            const string json =
@"[//an initial comment
    ['1',['1','2',[1,3,null,{'1':1,'2':[10,20,30]}]]],//some comment
    [['3'],['4'],[5]], /* some arrays followed by comment */
]";
            var result = DeserializeToArray(json);
            var expected = new object[] { 
                new object[]{"1",new object[]{"1","2",new object[]{ 1,3,null,new Dictionary<string,object>{{"1",1},{"2",new []{10,20,30}}}}}},
                new object[]{new object[]{"3"},new object[]{"4"},new object[]{5}},
            };
            Assert.That(result, Js.IsEqualTo(expected));
        }

        [Test]
        public void CanHandleDates()
        {
            const string json =
@"[
    [""/Date(1335205592410)/"",""2012-04-23T18:25:43.511Z""]
]";
            DeserializeToArray(json);
        }
    }
}
