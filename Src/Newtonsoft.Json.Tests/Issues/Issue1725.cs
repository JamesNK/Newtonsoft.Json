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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
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
    public class Issue1725 : TestFixtureBase
    {
        [Test]
        public void Test_In()
        {
            var p1 = new InPerson("some name");
            string json = JsonConvert.SerializeObject(p1);

            var p2 = JsonConvert.DeserializeObject<InPerson>(json);
            Assert.AreEqual("some name", p2.Name);
        }

        [Test]
        public void Test_Ref()
        {
            string value = "some name";
            var p1 = new RefPerson(ref value);
            string json = JsonConvert.SerializeObject(p1);

            var p2 = JsonConvert.DeserializeObject<RefPerson>(json);
            Assert.AreEqual("some name", p2.Name);
        }

        [Test]
        public void Test_InNullable()
        {
            var p1 = new InNullablePerson(1);
            string json = JsonConvert.SerializeObject(p1);

            var p2 = JsonConvert.DeserializeObject<InNullablePerson>(json);
            Assert.AreEqual(1, p2.Age);
        }

        [Test]
        public void Test_RefNullable()
        {
            int? value = 1;
            var p1 = new RefNullablePerson(ref value);
            string json = JsonConvert.SerializeObject(p1);

            var p2 = JsonConvert.DeserializeObject<RefNullablePerson>(json);
            Assert.AreEqual(1, p2.Age);
        }

        public class InPerson
        {
            public InPerson(in string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public class RefPerson
        {
            public RefPerson(ref string name)
            {
                Name = name;
            }

            public string Name { get; }
        }

        public class InNullablePerson
        {
            public InNullablePerson(in int? age)
            {
                Age = age;
            }

            public int? Age { get; }
        }

        public class RefNullablePerson
        {
            public RefNullablePerson(ref int? age)
            {
                Age = age;
            }

            public int? Age { get; }
        }
    }
}