using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class NewGenericPropertyOnADerivedClassTests: TestFixtureBase
    {
        [Test]
        public void CanSerializeWithBuiltInTypeAsGenericArgument()
        {
            var input = new ResponseWithNewGenericProperty<int>()
            {
                Message = "Trying out integer as type parameter",
                Data = 25,
                Result = "This should be fine"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<ResponseWithNewGenericProperty<int>>(json);

            Assert.AreEqual(input.Data, deserialized.Data);
            Assert.AreEqual(input.Message, deserialized.Message);
            Assert.AreEqual(input.Result, deserialized.Result);
        }

        [Test]
        public void CanSerializedWithGenericClosedTypeAsArgument()
        {
            var input = new ResponseWithNewGenericProperty<List<int>>()
            {
                Message = "More complex case - generic list of int",
                Data = Enumerable.Range(50, 70).ToList(),
                Result = "This should be fine too"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<ResponseWithNewGenericProperty<List<int>>>(json);

            Assert.AreEqual(input.Data, deserialized.Data);
            Assert.AreEqual(input.Message, deserialized.Message);
            Assert.AreEqual(input.Result, deserialized.Result);
        }
    }
}
