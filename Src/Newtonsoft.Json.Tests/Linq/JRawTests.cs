using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Linq
{
  public class JRawTests : TestFixtureBase
  {
    [Test]
    public void RawEquals()
    {
      JRaw r1 = new JRaw("raw1");
      JRaw r2 = new JRaw("raw1");
      JRaw r3 = new JRaw("raw2");

      Assert.IsTrue(JToken.DeepEquals(r1, r2));
      Assert.IsFalse(JToken.DeepEquals(r1, r3));
    }

    [Test]
    public void RawClone()
    {
      JRaw r1 = new JRaw("raw1");
      JToken r2 = r1.CloneToken();

      Assert.IsInstanceOfType(typeof(JRaw), r2);
    }
  }
}