using System.Reflection;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class ConstructorHandlingTests : TestFixtureBase
  {
    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.PrivateConstructorTestClass. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute. Line 1, position 6.")]
    public void FailWithPrivateConstructorAndDefault()
    {
      string json = @"{Name:""Name!""}";

      JsonConvert.DeserializeObject<PrivateConstructorTestClass>(json);
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