using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
    public class DerivedWithNewProperty<T>: SimpleBaseClass
    {
        public new T Data { get; set; }
    }

    public abstract class SimpleBaseClass
    {
        public string Result { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        //just in case - changing constructors to public does not make tests pass
        protected SimpleBaseClass()
        {
            
        }

        protected SimpleBaseClass(string message)
        {
            Message = message;
        }
    }

    [TestFixture]
    public class NewGenericPropertyOnADerivedClassTests: TestFixtureBase
    {
        //possible problems
        //0. constructors may need to be public - not the case
        //1. base class should be concrete - not the case
        //2. serialization can't handle new keyword properly - got rid of new and Data property in parent class, still tests failed
        //3. serialization can't handle classes with generic properties properly
        [Test]
        public void CanSerializeWithBuiltInTypeAsGenericArgument()
        {
            var input = new DerivedWithNewProperty<int>()
            {
                Message = "Something",
                Data = 25,
                Result = "This should be fine"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<DerivedWithNewProperty<int>>(json);

            Assert.AreEqual(input, deserialized);
        }

        [Test]
        public void CanSerializedWithGenericClosedTypeAsArgument()
        {
            var input = new DerivedWithNewProperty<List<int>>()
            {
                Message = "Something",
                Data = Enumerable.Range(50, 70).ToList(),
                Result = "This should be fine"
            };

            var json = JsonConvert.SerializeObject(input);
            var deserialized = JsonConvert.DeserializeObject<DerivedWithNewProperty<List<int>>>(json);

            Assert.AreEqual(input, deserialized);
        }
    }
}
