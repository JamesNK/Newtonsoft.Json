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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class ReflectionAttributeProviderTests : TestFixtureBase
    {
        public class ReflectionTestObject
        {
            [DefaultValue("1")]
            [JsonProperty]
            public int TestProperty { get; set; }

            [DefaultValue("1")]
            [JsonProperty]
            public int TestField;

            public ReflectionTestObject(
                [DefaultValue("1")] [JsonProperty] int testParameter)
            {
                TestProperty = testParameter;
                TestField = testParameter;
            }
        }

        [Test]
        public void GetAttributes_Property()
        {
            PropertyInfo property;
#if DNXCORE50
            property = Newtonsoft.Json.Utilities.TypeExtensions.GetProperty(typeof(ReflectionTestObject), "TestProperty");
#else
            property = typeof(ReflectionTestObject).GetProperty("TestProperty");
#endif

            ReflectionAttributeProvider provider = new ReflectionAttributeProvider(property);

            IList<Attribute> attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
            Assert.AreEqual(1, attributes.Count);

            attributes = provider.GetAttributes(false);
            Assert.AreEqual(2, attributes.Count);
        }

        [Test]
        public void GetAttributes_Field()
        {
            FieldInfo field;
#if DNXCORE50
            field = (FieldInfo)Newtonsoft.Json.Utilities.TypeExtensions.GetField(typeof(ReflectionTestObject), "TestField");
#else
            field = typeof(ReflectionTestObject).GetField("TestField");
#endif

            ReflectionAttributeProvider provider = new ReflectionAttributeProvider(field);

            IList<Attribute> attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
            Assert.AreEqual(1, attributes.Count);

            attributes = provider.GetAttributes(false);
            Assert.AreEqual(2, attributes.Count);
        }

        [Test]
        public void GetAttributes_Parameter()
        {
            ParameterInfo[] parameters = typeof(ReflectionTestObject).GetConstructor(new[] { typeof(int) }).GetParameters();

            ParameterInfo parameter = parameters[0];

            ReflectionAttributeProvider provider = new ReflectionAttributeProvider(parameter);

            IList<Attribute> attributes = provider.GetAttributes(typeof(DefaultValueAttribute), false);
            Assert.AreEqual(1, attributes.Count);

            attributes = provider.GetAttributes(false);
            Assert.AreEqual(2, attributes.Count);
        }
    }
}