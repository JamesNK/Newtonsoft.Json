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
    public class Issue2954
    {
        private const string JsonToTest = "{\"arg\": 1, \"field\": null, \"fieldEnum\": null}";

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2954
        public void Test_Ignore_Null_Values()
        {
            var result = JsonConvert.DeserializeObject<A>(JsonToTest, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            Assert.AreEqual(result.Field, 0);
            Assert.AreEqual(result.FieldEnum, B.Option1);
        }

        [Test]
        public void Test_Throw_JsonSerializationException()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<A>(JsonToTest));
        }

        class A
        {
            public A(int arg)
            {
            }

            public int Field { get; set; }
            public B FieldEnum { get; set; }
        }

        enum B
        {
            Option1,
            Option2,
        }
    }
}
