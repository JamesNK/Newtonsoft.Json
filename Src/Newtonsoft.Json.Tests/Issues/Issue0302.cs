#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

#if !(NET20 || NET35)
using System;
using Newtonsoft.Json.Serialization;
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
    public class Issue0302 : TestFixtureBase
    {
        [Test]
        public void Serialize_Object()
        {
            NeverSerializeTypeNameClass value = new NeverSerializeTypeNameClass { Count = 1 };

            string serializedData = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                SerializationBinder = new CustomSerializationBinder()
            });

            StringAssert.AreEqual(@"{
  ""Count"": 1
}", serializedData);
        }

        [Test]
        public void Serialize_Array()
        {
            NeverSerializeTypeNameClass[] value = new[] { new NeverSerializeTypeNameClass { Count = 1 } };

            string serializedData = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                SerializationBinder = new CustomSerializationBinder()
            });

            StringAssert.AreEqual(@"[
  {
    ""Count"": 1
  }
]", serializedData);
        }

        [Test]
        public void Serialize_ArrayWithReferences()
        {
            NeverSerializeTypeNameClass[] value = new[] { new NeverSerializeTypeNameClass { Count = 1 } };

            string serializedData = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                PreserveReferencesHandling = PreserveReferencesHandling.Arrays,
                Formatting = Formatting.Indented,
                SerializationBinder = new CustomSerializationBinder()
            });

            StringAssert.AreEqual(@"{
  ""$id"": ""1"",
  ""$values"": [
    {
      ""Count"": 1
    }
  ]
}", serializedData);
        }

        [Test]
        public void Serialize_ByteArray()
        {
            NeverSerializeTypeNameClass value = new NeverSerializeTypeNameClass { Bytes = new byte[] { 0xFF, 0x77, 0x11 } };

            string serializedData = JsonConvert.SerializeObject(value, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                DefaultValueHandling = DefaultValueHandling.Ignore,
                Formatting = Formatting.Indented,
                SerializationBinder = new CustomSerializationBinder()
            });

            StringAssert.AreEqual(@"{
  ""Bytes"": ""/3cR""
}", serializedData);
        }

        class NeverSerializeTypeNameClass
        {
            public int Count { get; set; }

            public byte[] Bytes { get; set; }
        }
        
        class CustomSerializationBinder : DefaultSerializationBinder
        {
            public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                if (serializedType == typeof(NeverSerializeTypeNameClass) || serializedType == typeof(NeverSerializeTypeNameClass[]) || serializedType == typeof(byte[]))
                {
                    assemblyName = null;
                    typeName = null;
                }
                else
                {
                    base.BindToName(serializedType, out assemblyName, out typeName);
                }
            }
        }
    }
}
#endif