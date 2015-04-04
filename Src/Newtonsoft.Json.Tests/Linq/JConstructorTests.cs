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
using Newtonsoft.Json.Linq;
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
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Linq
{
    [TestFixture]
    public class JConstructorTests : TestFixtureBase
    {
        [Test]
        public void Load()
        {
            JsonReader reader = new JsonTextReader(new StringReader("new Date(123)"));
            reader.Read();

            JConstructor constructor = JConstructor.Load(reader);
            Assert.AreEqual("Date", constructor.Name);
            Assert.IsTrue(JToken.DeepEquals(new JValue(123), constructor.Values().ElementAt(0)));
        }

        [Test]
        public void CreateWithMultiValue()
        {
            JConstructor constructor = new JConstructor("Test", new List<int> { 1, 2, 3 });
            Assert.AreEqual("Test", constructor.Name);
            Assert.AreEqual(3, constructor.Children().Count());
            Assert.AreEqual(1, (int)constructor.Children().ElementAt(0));
            Assert.AreEqual(2, (int)constructor.Children().ElementAt(1));
            Assert.AreEqual(3, (int)constructor.Children().ElementAt(2));
        }

        [Test]
        public void Iterate()
        {
            JConstructor c = new JConstructor("MrConstructor", 1, 2, 3, 4, 5);

            int i = 1;
            foreach (JToken token in c)
            {
                Assert.AreEqual(i, (int)token);
                i++;
            }
        }

        [Test]
        public void SetValueWithInvalidIndex()
        {
            ExceptionAssert.Throws<ArgumentException>(() =>
            {
                JConstructor c = new JConstructor();
                c["badvalue"] = new JValue(3);
            }, @"Set JConstructor values with invalid key value: ""badvalue"". Argument position index expected.");
        }

        [Test]
        public void SetValue()
        {
            object key = 0;

            JConstructor c = new JConstructor();
            c.Name = "con";
            c.Add(null);
            c[key] = new JValue(3);

            Assert.AreEqual(3, (int)c[key]);
        }
    }
}