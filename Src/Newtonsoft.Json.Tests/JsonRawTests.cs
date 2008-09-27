using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  public class JsonRawTests : TestFixtureBase
  {
    [Test]
    public void Create()
    {
      JsonRaw raw = new JsonRaw("Test");
      Assert.AreEqual("Test", raw.Content);
      Assert.AreEqual("Test", raw.ToString());
    }

    [Test]
    public void EqualsShouldEqualOnSameContent()
    {
      JsonRaw r1 = new JsonRaw("Test");
      JsonRaw r2 = new JsonRaw("Test");

      Assert.AreEqual(r1, r2);
      Assert.AreNotEqual(r1, null);
      Assert.IsFalse(r1.Equals(null));
    }

    [Test]
    public void GetHashCodeShouldEqualOnSameContent()
    {
      JsonRaw r1 = new JsonRaw("Test");
      JsonRaw r2 = new JsonRaw("Test");
      JsonRaw r3 = new JsonRaw("Test1");

      Assert.AreEqual(r1.GetHashCode(), r2.GetHashCode());
      Assert.AreNotEqual(r1.GetHashCode(), r3.GetHashCode());
    }
  }
}