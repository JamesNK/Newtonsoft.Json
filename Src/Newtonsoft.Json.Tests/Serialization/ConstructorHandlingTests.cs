using System.Reflection;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
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
    [ExpectedException(typeof(TargetInvocationException))]
    public void FailWithPrivateConstructorPlusParametizedAndDefault()
    {
      string json = @"{Name:""Name!""}";

      PrivateConstructorWithPublicParametizedConstructorTestClass c = JsonConvert.DeserializeObject<PrivateConstructorWithPublicParametizedConstructorTestClass>(json);
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