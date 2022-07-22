﻿#region License
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

#if !(PORTABLE || PORTABLE40 || DNXCORE50) || NETSTANDARD2_0 || NET6_0_OR_GREATER
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq.ComponentModel
{
    [TestFixture]
    public class JPropertyDescriptorTests : TestFixtureBase
    {
        [Test]
        public void GetValue()
        {
            JObject o = JObject.Parse("{prop1:'12345!',prop2:[1,'two','III']}");

            JPropertyDescriptor prop1 = new JPropertyDescriptor("prop1");
            JPropertyDescriptor prop2 = new JPropertyDescriptor("prop2");

            Assert.AreEqual("12345!", ((JValue)prop1.GetValue(o)).Value);
            Assert.AreEqual(o["prop2"], prop2.GetValue(o));
        }

        [Test]
        public void GetValue_NullOwner_ReturnsNull()
        {
            JPropertyDescriptor prop1 = new JPropertyDescriptor("prop1");

            Assert.AreEqual(null, prop1.GetValue(null));
        }

        [Test]
        public void SetValue()
        {
            JObject o = JObject.Parse("{prop1:'12345!'}");

            JPropertyDescriptor propertyDescriptor1 = new JPropertyDescriptor("prop1");

            propertyDescriptor1.SetValue(o, "54321!");

            Assert.AreEqual("54321!", (string)o["prop1"]);
        }

        [Test]
        public void SetValue_NullOwner_NoError()
        {
            JPropertyDescriptor prop1 = new JPropertyDescriptor("prop1");

            prop1.SetValue(null, "value!");
        }

        [Test]
        public void ResetValue()
        {
            JObject o = JObject.Parse("{prop1:'12345!'}");

            JPropertyDescriptor propertyDescriptor1 = new JPropertyDescriptor("prop1");
            propertyDescriptor1.ResetValue(o);

            Assert.AreEqual("12345!", (string)o["prop1"]);
        }

        [Test]
        public void IsReadOnly()
        {
            JPropertyDescriptor propertyDescriptor1 = new JPropertyDescriptor("prop1");

            Assert.AreEqual(false, propertyDescriptor1.IsReadOnly);
        }

        [Test]
        public void PropertyType()
        {
            JPropertyDescriptor propertyDescriptor1 = new JPropertyDescriptor("prop1");

            Assert.AreEqual(typeof(object), propertyDescriptor1.PropertyType);
        }
    }
}

#endif