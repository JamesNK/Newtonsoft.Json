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

#if !(PORTABLE || DNXCORE50 || NETFX_CORE || PORTABLE40)
using System;
using System.Diagnostics;
using System.Reflection;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.Serialization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Utilities
{
    [TestFixture]
    public class DynamicReflectionDelegateFactoryTests : TestFixtureBase
    {
        [Test]
        public void ConstructorWithRefString()
        {
            ConstructorInfo constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 1);

            var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input" };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Test]
        public void ConstructorWithRefStringAndOutBool()
        {
            ConstructorInfo constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 2);

            var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input", false };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
            Assert.AreEqual(true, o.B1);
        }

        [Test]
        public void ConstructorWithRefStringAndRefBoolAndRefBool()
        {
            ConstructorInfo constructor = typeof(OutAndRefTestClass).GetConstructors().Single(c => c.GetParameters().Count() == 3);

            var creator = DynamicReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input", true, null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
            Assert.AreEqual(true, o.B1);
            Assert.AreEqual(false, o.B2);
        }

        [Test]
        public void CreateGetWithBadObjectTarget()
        {
            ExceptionAssert.Throws<InvalidCastException>(() =>
            {
                Person p = new Person();
                p.Name = "Hi";

                Func<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

                setter(p);
            }, "Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.");
        }

        [Test]
        public void CreateSetWithBadObjectTarget()
        {
            ExceptionAssert.Throws<InvalidCastException>(() =>
            {
                Person p = new Person();
                Movie m = new Movie();

                Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

                setter(m, "Hi");

                Assert.AreEqual(m.Name, "Hi");

                setter(p, "Hi");

                Assert.AreEqual(p.Name, "Hi");
            }, "Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.");
        }

        [Test]
        public void CreateSetWithBadTarget()
        {
            ExceptionAssert.Throws<InvalidCastException>(() =>
            {
                object structTest = new StructTest();

                Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetProperty("StringProperty"));

                setter(structTest, "Hi");

                Assert.AreEqual("Hi", ((StructTest)structTest).StringProperty);

                setter(new TimeSpan(), "Hi");
            }, "Specified cast is not valid.");
        }

        [Test]
        public void CreateSetWithBadObjectValue()
        {
            ExceptionAssert.Throws<InvalidCastException>(() =>
            {
                Movie m = new Movie();

                Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

                setter(m, new Version("1.1.1.1"));
            }, "Unable to cast object of type 'System.Version' to type 'System.String'.");
        }

        [Test]
        public void CreateStaticMethodCall()
        {
            MethodInfo castMethodInfo = typeof(JsonSerializerTest.DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

            Assert.IsNotNull(castMethodInfo);

            MethodCall<object, object> call = DynamicReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

            object result = call(null, "First!");
            Assert.IsNotNull(result);

            JsonSerializerTest.DictionaryKey key = (JsonSerializerTest.DictionaryKey)result;
            Assert.AreEqual("First!", key.Value);
        }
    }
}

#endif