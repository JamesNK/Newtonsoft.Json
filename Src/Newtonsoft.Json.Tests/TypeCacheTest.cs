#if (NET45)
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class TypeCacheTest
    {
        //[Test]
        public void MemoryTest()
        {
            DeserializeAndSerializeTest();

            GC.Collect();
            GC.WaitForFullGCComplete();
        }

        [Test]
        public void DeserializeAndSerializeTest()
        {
            Type dynamicType = DynamicTypeBuilder.CreateType(new Dictionary<string, Type>() { { "number1", typeof(Int64) }, { "text1", typeof(string) } });

            string expectedJSON = "{\"number1\":1,\"text1\":\"1\"}";

            var settings = new JsonSerializerSettings() {
                ContractResolver = new DefaultContractResolver(),
                SerializationBinder = new DefaultSerializationBinder() };
            object deserializedObject = JsonConvert.DeserializeObject(expectedJSON, dynamicType, settings);

            Assert.AreEqual(dynamicType, deserializedObject.GetType());

            Assert.AreEqual(expectedJSON, JsonConvert.SerializeObject(deserializedObject));
        }
    }
}
#endif
