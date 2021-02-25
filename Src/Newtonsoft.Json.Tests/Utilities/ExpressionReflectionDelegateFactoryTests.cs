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

using System.Collections.Generic;
#if !(NET20 || NET35)
using System.Linq;
using System;
using System.Diagnostics;
using System.Reflection;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using Newtonsoft.Json.Tests.Serialization;

namespace Newtonsoft.Json.Tests.Utilities
{
    [TestFixture]
    public class ExpressionReflectionDelegateFactoryTests : TestFixtureBase
    {
        [Test]
        public void ConstructorWithInString()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(InTestClass)).Single(c => c.GetParameters().Count() == 1);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Value" };
            InTestClass o = (InTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Value", o.Value);
        }

        [Test]
        public void ConstructorWithInStringAndBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(InTestClass)).Single(c => c.GetParameters().Count() == 2);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Value", true };
            InTestClass o = (InTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Value", o.Value);
            Assert.AreEqual(true, o.B1);
        }

        [Test]
        public void ConstructorWithRefString()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 1);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input" };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Test]
        public void ConstructorWithRefStringAndOutBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 2);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input", null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Test]
        public void ConstructorWithRefStringAndRefBoolAndRefBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 3);

            var creator = ExpressionReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input", true, null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
            Assert.AreEqual(true, o.B1);
            Assert.AreEqual(false, o.B2);
        }

        [Test]
        public void DefaultConstructor()
        {
            Func<object> create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Movie));

            Movie m = (Movie)create();
            Assert.IsNotNull(m);
        }

        [Test]
        public void DefaultConstructor_Struct()
        {
            Func<object> create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(StructTest));

            StructTest m = (StructTest)create();
            Assert.IsNotNull(m);
        }

        [Test]
        public void DefaultConstructor_Abstract()
        {
            ExceptionAssert.Throws<Exception>(
                () =>
                {
                    Func<object> create = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(Type));

                    create();
                }, new[]
                {
                    "Cannot create an abstract class.",
                    "Cannot create an abstract class 'System.Type'." // mono
                });
        }

        [Test]
        public void CreatePropertySetter()
        {
            Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

            Movie m = new Movie();

            setter(m, "OH HAI!");

            Assert.AreEqual("OH HAI!", m.Name);
        }

        [Test]
        public void CreatePropertyGetter()
        {
            Func<object, object> getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

            Movie m = new Movie();
            m.Name = "OH HAI!";

            object value = getter(m);

            Assert.AreEqual("OH HAI!", value);
        }

        [Test]
        public void CreateMethodCall()
        {
            MethodCall<object, object> method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(TestReflectionUtils.GetMethod(typeof(Movie), "ToString"));

            Movie m = new Movie();
            object result = method(m);
            Assert.AreEqual("Newtonsoft.Json.Tests.TestObjects.Movie", result);

            method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(TestReflectionUtils.GetMethod(typeof(Movie), "Equals"));

            result = method(m, m);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void CreateMethodCall_Constructor()
        {
            MethodCall<object, object> method = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(typeof(Movie).GetConstructor(new Type[0]));

            object result = method(null);

            Assert.IsTrue(result is Movie);
        }

        public static class StaticTestClass
        {
            public static string StringField;
            public static string StringProperty { get; set; }
        }

        [Test]
        public void GetStatic()
        {
            StaticTestClass.StringField = "Field!";
            StaticTestClass.StringProperty = "Property!";

            Func<object, object> getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetProperty(typeof(StaticTestClass), "StringProperty"));

            object v = getter(null);
            Assert.AreEqual(StaticTestClass.StringProperty, v);

            getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetField(typeof(StaticTestClass), "StringField"));

            v = getter(null);
            Assert.AreEqual(StaticTestClass.StringField, v);
        }

        [Test]
        public void SetStatic()
        {
            Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(StaticTestClass), "StringProperty"));

            setter(null, "New property!");
            Assert.AreEqual("New property!", StaticTestClass.StringProperty);

            setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(StaticTestClass), "StringField"));

            setter(null, "New field!");
            Assert.AreEqual("New field!", StaticTestClass.StringField);
        }

        public class FieldsTestClass
        {
            public string StringField;
            public bool BoolField;

            public readonly int IntReadOnlyField = int.MaxValue;
        }

        [Test]
        public void CreateGetField()
        {
            FieldsTestClass c = new FieldsTestClass
            {
                BoolField = true,
                StringField = "String!"
            };

            Func<object, object> getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "StringField"));

            object value = getter(c);
            Assert.AreEqual("String!", value);

            getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "BoolField"));

            value = getter(c);
            Assert.AreEqual(true, value);
        }

        [Test]
        public void CreateSetField_ReadOnly()
        {
            FieldsTestClass c = new FieldsTestClass();

            Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "IntReadOnlyField"));

            setter(c, int.MinValue);
            Assert.AreEqual(int.MinValue, c.IntReadOnlyField);
        }

        [Test]
        public void CreateSetField()
        {
            FieldsTestClass c = new FieldsTestClass();

            Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "StringField"));

            setter(c, "String!");
            Assert.AreEqual("String!", c.StringField);

            setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(FieldsTestClass), "BoolField"));

            setter(c, true);
            Assert.AreEqual(true, c.BoolField);
        }

        [Test]
        public void SetOnStruct()
        {
            object structTest = new StructTest();

            Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(StructTest), "StringProperty"));

            setter(structTest, "Hi1");
            Assert.AreEqual("Hi1", ((StructTest)structTest).StringProperty);

            setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetField(typeof(StructTest), "StringField"));

            setter(structTest, "Hi2");
            Assert.AreEqual("Hi2", ((StructTest)structTest).StringField);
        }

        [Test]
        public void CreateGetWithBadObjectTarget()
        {
            ExceptionAssert.Throws<InvalidCastException>(
                () =>
                {
                    Person p = new Person();
                    p.Name = "Hi";

                    Func<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

                    setter(p);
                },
                new[]
                {
                    "Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.",
                    "Cannot cast from source type to destination type." // mono
                });
        }

        [Test]
        public void CreateSetWithBadObjectTarget()
        {
            ExceptionAssert.Throws<InvalidCastException>(
                () =>
                {
                    Person p = new Person();
                    Movie m = new Movie();

                    Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

                    setter(m, "Hi");

                    Assert.AreEqual(m.Name, "Hi");

                    setter(p, "Hi");

                    Assert.AreEqual(p.Name, "Hi");
                },
                new[]
                {
                    "Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Organization.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.",
                    "Cannot cast from source type to destination type." // mono
                });
        }

        [Test]
        public void CreateSetWithBadObjectValue()
        {
            ExceptionAssert.Throws<InvalidCastException>(
                () =>
                {
                    Movie m = new Movie();

                    Action<object, object> setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(TestReflectionUtils.GetProperty(typeof(Movie), "Name"));

                    setter(m, new Version("1.1.1.1"));
                }, new[]
                {
                    "Unable to cast object of type 'System.Version' to type 'System.String'.",
                    "Cannot cast from source type to destination type." //mono
                });
        }

        [Test]
        public void CreateStaticMethodCall()
        {
            MethodInfo castMethodInfo = typeof(DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

            Assert.IsNotNull(castMethodInfo);

            MethodCall<object, object> call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

            object result = call(null, "First!");
            Assert.IsNotNull(result);

            DictionaryKey key = (DictionaryKey)result;
            Assert.AreEqual("First!", key.Value);
        }

        [Test]
        public void ConstructorStruct()
        {
            Func<object> creator1 = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<object>(typeof(MyStruct));
            MyStruct myStruct1 = (MyStruct)creator1.Invoke();
            Assert.AreEqual(0, myStruct1.IntProperty);

            Func<MyStruct> creator2 = ExpressionReflectionDelegateFactory.Instance.CreateDefaultConstructor<MyStruct>(typeof(MyStruct));
            MyStruct myStruct2 = creator2.Invoke();
            Assert.AreEqual(0, myStruct2.IntProperty);
        }

        public struct TestStruct
        {
            public TestStruct(int i)
            {
                Value = i;
            }

            public int Value { get; }
        }

        public static TestStruct StructMethod(TestStruct s)
        {
            return new TestStruct(s.Value + s.Value);
        }

        [Test]
        public void CreateStructMethodCall()
        {
            MethodInfo methodInfo = typeof(ExpressionReflectionDelegateFactoryTests).GetMethod(nameof(StructMethod), new[] { typeof(TestStruct) });

            Assert.IsNotNull(methodInfo);

            MethodCall<object, object> call = ExpressionReflectionDelegateFactory.Instance.CreateMethodCall<object>(methodInfo);

            object result = call(null, new TestStruct(123));
            Assert.IsNotNull(result);

            TestStruct s = (TestStruct)result;
            Assert.AreEqual(246, s.Value);
        }
    }
}

#endif