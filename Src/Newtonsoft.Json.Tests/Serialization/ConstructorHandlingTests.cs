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
  [TestFixture]
  public class ConstructorHandlingTests : TestFixtureBase
  {
    [Test]
    public void UsePrivateConstructorIfThereAreMultipleConstructorsWithParametersAndNothingToFallbackTo()
    {
      string json = @"{Name:""Name!""}";

      var c = JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json);

      Assert.AreEqual("Name!", c.Name);
    }

    [Test]
    public void SuccessWithPrivateConstructorAndAllowNonPublic()
    {
      string json = @"{Name:""Name!""}";

      PrivateConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json,
        new JsonSerializerSettings
          {
            ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
          });
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name);
    }

    [Test]
    public void FailWithPrivateConstructorPlusParametizedAndDefault()
    {
      ExceptionAssert.Throws<TargetInvocationException>(
        null,
        () =>
        {
          string json = @"{Name:""Name!""}";

          PrivateConstructorWithPublicParametizedConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParametizedConstructorTestClass>(json);
        });
    }

    [Test]
    public void SuccessWithPrivateConstructorPlusParametizedAndAllowNonPublic()
    {
      string json = @"{Name:""Name!""}";

      PrivateConstructorWithPublicParametizedConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParametizedConstructorTestClass>(json,
        new JsonSerializerSettings
        {
          ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
        });
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name);
      Assert.AreEqual(1, c.Age);
    }

    [Test]
    public void SuccessWithPublicParametizedConstructor()
    {
      string json = @"{Name:""Name!""}";

      var c = JsonConvert.DeserializeObject<PublicParametizedConstructorTestClass>(json);
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name);
    }

    [Test]
    public void SuccessWithPublicParametizedConstructorWhenParamaterIsNotAProperty()
    {
      string json = @"{nameParameter:""Name!""}";

      PublicParametizedConstructorWithNonPropertyParameterTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithNonPropertyParameterTestClass>(json);
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name);
    }

    [Test]
    public void SuccessWithPublicParametizedConstructorWhenParamaterRequiresAConverter()
    {
      string json = @"{nameParameter:""Name!""}";

      PublicParametizedConstructorRequiringConverterTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorRequiringConverterTestClass>(json, new NameContainerConverter());
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name.Value);
    }

    [Test]
    public void SuccessWithPublicParametizedConstructorWhenParamaterRequiresAConverterWithParameterAttribute()
    {
      string json = @"{nameParameter:""Name!""}";

      PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass>(json);
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name.Value);
    }

    [Test]
    public void SuccessWithPublicParametizedConstructorWhenParamaterRequiresAConverterWithPropertyAttribute()
    {
      string json = @"{name:""Name!""}";

      PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass c = JsonConvert.DeserializeObject<PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass>(json);
      Assert.IsNotNull(c);
      Assert.AreEqual("Name!", c.Name.Value);
    }

    [Test]
    public void SuccessWithPublicParametizedConstructorWhenParamaterNameConflictsWithPropertyName()
    {
      string json = @"{name:""1""}";

      PublicParametizedConstructorWithPropertyNameConflict c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithPropertyNameConflict>(json);
      Assert.IsNotNull(c);
      Assert.AreEqual(1, c.Name);
    }

    [Test]
    public void PublicParametizedConstructorWithPropertyNameConflictWithAttribute()
    {
      string json = @"{name:""1""}";

      PublicParametizedConstructorWithPropertyNameConflictWithAttribute c = JsonConvert.DeserializeObject<PublicParametizedConstructorWithPropertyNameConflictWithAttribute>(json);
      Assert.IsNotNull(c);
      Assert.AreEqual(1, c.Name);
    }
  }
}