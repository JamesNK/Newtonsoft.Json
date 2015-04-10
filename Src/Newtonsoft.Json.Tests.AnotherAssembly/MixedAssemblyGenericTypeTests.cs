using System;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.AnotherAssembly
{
    class MixedAssemblyGenericTypeTests
    {
        [Test]
        public void BaseAssemblyGenericTypeWithSecondAssemblyGenericParameter()
        {
            // NOTE: This test will pass when ran by itself, but will fail when all tests in the solution ran.

            var json = @"{
  ""$type"":""Newtonsoft.Json.Tests.TestObjects.DocumentChanged`2[[System.Guid, mscorlib],[Newtonsoft.Json.Tests.AnotherAssembly.Foo, Newtonsoft.Json.Tests.AnotherAssembly]], Newtonsoft.Json.Tests"",
  ""Id"":""53b64a3e-4e55-4a6d-a3b4-d5edd55c059d"",
  ""Document"":{
    ""$type"":""Newtonsoft.Json.Tests.AnotherAssembly.Foo, Newtonsoft.Json.Tests.AnotherAssembly"",
    ""Bar"":""Test1""
  }
}";
            object e = JsonConvert.DeserializeObject(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
            });


            Assert.That(e, Is.Not.Null);
            Assert.That(e.GetType(), Is.EqualTo(typeof(DocumentChanged<Guid, Foo>)));
        }
    }
}
