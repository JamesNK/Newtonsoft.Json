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

#if !(SILVERLIGHT || PORTABLE || NETFX_CORE || PORTABLE40)
using System;
using System.Diagnostics;
using System.Reflection;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.Serialization;

namespace Newtonsoft.Json.Tests.Utilities
{
  [TestFixture]
  public class DynamicReflectionDelegateFactoryTests : TestFixtureBase
  {
    [Test]
    public void CreateGetWithBadObjectTarget()
    {
      ExceptionAssert.Throws<InvalidCastException>("Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.",
      () =>
      {
        Person p = new Person();
        p.Name = "Hi";

        Func<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(typeof(Movie).GetProperty("Name"));

        setter(p);
      });
    }

    [Test]
    public void CreateSetWithBadObjectTarget()
    {
      ExceptionAssert.Throws<InvalidCastException>("Unable to cast object of type 'Newtonsoft.Json.Tests.TestObjects.Person' to type 'Newtonsoft.Json.Tests.TestObjects.Movie'.",
      () =>
      {
        Person p = new Person();
        Movie m = new Movie();

        Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

        setter(m, "Hi");

        Assert.AreEqual(m.Name, "Hi");

        setter(p, "Hi");

        Assert.AreEqual(p.Name, "Hi");
      });
    }

    [Test]
    public void CreateSetWithBadTarget()
    {
      ExceptionAssert.Throws<InvalidCastException>("Specified cast is not valid.",
      () =>
      {
        object structTest = new StructTest();

        Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(StructTest).GetProperty("StringProperty"));

        setter(structTest, "Hi");

        Assert.AreEqual("Hi", ((StructTest)structTest).StringProperty);

        setter(new TimeSpan(), "Hi");
      });
    }

    [Test]
    public void CreateSetWithBadObjectValue()
    {
      ExceptionAssert.Throws<InvalidCastException>("Unable to cast object of type 'System.Version' to type 'System.String'.",
      () =>
      {
        Movie m = new Movie();

        Action<object, object> setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(typeof(Movie).GetProperty("Name"));

        setter(m, new Version("1.1.1.1"));
      });
    }

    [Test]
    public void CreateStaticMethodCall()
    {
      MethodInfo castMethodInfo = typeof(JsonSerializerTest.DictionaryKey).GetMethod("op_Implicit", new[] { typeof(string) });

      Assert.IsNotNull(castMethodInfo);

      MethodCall<object, object> call = DynamicReflectionDelegateFactory.Instance.CreateMethodCall<object>(castMethodInfo);

      object result = call(null, "First!");
      Assert.IsNotNull(result);

      JsonSerializerTest.DictionaryKey key = (JsonSerializerTest.DictionaryKey) result;
      Assert.AreEqual("First!", key.Value);
    }
  }
}
#endif