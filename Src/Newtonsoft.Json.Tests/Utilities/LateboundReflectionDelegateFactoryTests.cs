using System;
using System.Reflection;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Utilities
{
    public class OutAndRefTestClass
    {
        public string Input { get; set; }
        public bool B1 { get; set; }
        public bool B2 { get; set; }

        public OutAndRefTestClass(ref string value)
        {
            Input = value;
            value = "Output";
        }

        public OutAndRefTestClass(ref string value, out bool b1)
            : this(ref value)
        {
            b1 = true;
            B1 = true;
        }

        public OutAndRefTestClass(ref string value, ref bool b1, ref bool b2)
            : this(ref value)
        {
            B1 = b1;
            B2 = b2;
        }
    }

    [TestFixture]
    public class LateboundReflectionDelegateFactoryTests : TestFixtureBase
    {
        [Test]
        public void ConstructorWithRefString()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 1);

            var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input" };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Test]
        public void ConstructorWithRefStringAndOutBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 2);

            var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input", null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
        }

        [Test]
        public void ConstructorWithRefStringAndRefBoolAndRefBool()
        {
            ConstructorInfo constructor = TestReflectionUtils.GetConstructors(typeof(OutAndRefTestClass)).Single(c => c.GetParameters().Count() == 3);

            var creator = LateBoundReflectionDelegateFactory.Instance.CreateParameterizedConstructor(constructor);

            object[] args = new object[] { "Input", true, null };
            OutAndRefTestClass o = (OutAndRefTestClass)creator(args);
            Assert.IsNotNull(o);
            Assert.AreEqual("Input", o.Input);
            Assert.AreEqual(true, o.B1);
            Assert.AreEqual(false, o.B2);
        }
    }

    public struct MyStruct
    {
        private int _intProperty;

        public int IntProperty
        {
            get { return _intProperty; }
            set { _intProperty = value; }
        }
    }
}