using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Serialization
{
  public class ConstructorHandlingTests : TestFixtureBase
  {
    [Test]
    [ExpectedException(typeof(JsonSerializationException), ExpectedMessage = "Unable to find a constructor to use for type Newtonsoft.Json.Tests.TestObjects.PrivateConstructorTestClass. A class should either have a default constructor, one constructor with arguments or a constructor marked with the JsonConstructor attribute.")]
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
  }
}
