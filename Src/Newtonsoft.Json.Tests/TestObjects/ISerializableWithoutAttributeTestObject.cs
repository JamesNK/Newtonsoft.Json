using System;
using System.Runtime.Serialization;

namespace Newtonsoft.Json.Tests.TestObjects
{
    public class ISerializableWithoutAttributeTestObject : ISerializable
    {
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }
    }
}