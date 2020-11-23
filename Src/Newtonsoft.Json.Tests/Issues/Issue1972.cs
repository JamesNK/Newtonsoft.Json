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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
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
    public class Issue1972 : TestFixtureBase
    {
        [Test]
        public void Test_ObjectsCrossPlatform()
        {
            JsonSerializerSettings jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects
            };

            TestClass<int> a = new TestClass<int>() {
                Prop1 = 123
            };

            string serializedData = JsonConvert.SerializeObject(a, jsonSettings);

            // Swap "mscorlib" and "System.Private.CoreLib" to simulate cross-platform serialization
            string serializedDataChangedAssembly;
            if (serializedData.Contains("mscorlib"))
                serializedDataChangedAssembly = serializedData.Replace("mscorlib", "System.Private.CoreLib");
            else
                serializedDataChangedAssembly = serializedData.Replace("System.Private.CoreLib", "mscorlib");

            TestClass<int> deserialized = JsonConvert.DeserializeObject<TestClass<int>>(serializedDataChangedAssembly, jsonSettings);

            Assert.AreEqual(a.Prop1, deserialized.Prop1);
        }

        class TestClass<T>
        {
            public T Prop1 { get; set; }
        }
    }
}
#endif
