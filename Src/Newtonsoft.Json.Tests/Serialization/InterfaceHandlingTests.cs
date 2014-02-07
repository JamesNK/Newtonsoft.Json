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

using System.Reflection;
using Newtonsoft.Json.Tests.TestObjects;

#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
    using System;

    [TestFixture]
    public class InterfaceHandlingTests : TestFixtureBase
    {
        [Test]
        public void UseDynamicConcreteIfTargetObjectTypeIsAnInterfaceWithNoBackingClass()
        {
            string json = @"{Name:""Name!""}";

            var c = JsonConvert.DeserializeObject<IInterfaceWithNoConcrete>(json);

            Assert.AreEqual("Name!", c.Name);
        }

        [Test]
        public void UseDynamicConcreteIfTargetObjectTypeIsAnAbstractClassWithNoConcrete() 
        {
            string json = @"{Name:""Name!"", Game:""Same""}";

            var c = JsonConvert.DeserializeObject<AbstractWithNoConcrete>(json);

            Assert.AreEqual("Name!", c.Name);
            Assert.AreEqual("Same", c.Game);
        }

        [Test]
        public void AnyMethodsExposedByDynamicConcreteAreHarmless()
        {
            string json = @"{Name:""Name!""}";

            var c = JsonConvert.DeserializeObject<IInterfaceWithNoConcrete>(json);

            try
            {
                c.FuncWithRefType(10, null);
                c.FuncWithValType_1();
                c.FuncWithValType_2();
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
        }
    }

    public abstract class AbstractWithNoConcrete
    {
        public string Name { get; set; }
        public abstract string Game { get; set; }
    }

    public interface IInterfaceWithNoConcrete
    {
        string Name { get; set; }
        object FuncWithRefType(int a, object b);
        int FuncWithValType_1();
        bool FuncWithValType_2();
    }
}