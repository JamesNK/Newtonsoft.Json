using Newtonsoft.Json.Tests.TestObjects;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1295 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            var fsharpStructRecord = new FSharpStructRecordWithDataMember("Hi there");
            var asJson = JsonConvert.SerializeObject(fsharpStructRecord);
            var deserialized = JsonConvert.DeserializeObject<FSharpStructRecordWithDataMember>(asJson);

            Assert.AreEqual(fsharpStructRecord.Foo, deserialized.Foo);
        }

        [Test]
        public void TestJsonSanity()
        {
            var fsharpStructRecord = new FSharpStructRecordWithDataMember("42");
            var asJson = JsonConvert.SerializeObject(fsharpStructRecord);         

            Assert.AreEqual(@"{""foo_field"":""42""}", asJson);
        }
    }
}
