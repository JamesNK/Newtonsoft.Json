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

#if !(NET20 || NET35 || NET40)
using Moq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
#if PORTABLE && !NETSTANDARD2_0
using BindingFlags = Newtonsoft.Json.Utilities.BindingFlags;
#else
using BindingFlags = System.Reflection.BindingFlags;
#endif
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
    public class Issue1620 : TestFixtureBase
    {
        [Test]
        public void Test_SerializeMock()
        {
            Mock<IFoo> mock = new Mock<IFoo>();
            IFoo foo = mock.Object;

            string json = JsonConvert.SerializeObject(foo, new JsonSerializerSettings() { Converters = { new FooConverter() } });
            Assert.AreEqual(@"""foo""", json);
        }

        [Test]
        public void Test_GetFieldsAndProperties()
        {
            Mock<IFoo> mock = new Mock<IFoo>();
            IFoo foo = mock.Object;

            List<MemberInfo> properties = ReflectionUtils.GetFieldsAndProperties(foo.GetType(), BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).ToList();

            Assert.AreEqual(1, properties.Count(p => p.Name == "Mock"));
        }

        public interface IFoo
        {
        }

        public class Foo : IFoo
        {
        }

        public class FooConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue("foo");
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new Foo();
            }

            public override bool CanConvert(Type objectType)
            {
                return typeof(IFoo).GetTypeInfo().IsAssignableFrom(objectType);
            }
        }
    }
}
#endif