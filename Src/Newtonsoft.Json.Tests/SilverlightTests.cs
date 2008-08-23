using System;
using System.Reflection;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
#if SILVERLIGHT
  [TestFixture]
  public class SilverlightTests
  {
    [Test]
    public void SystemVersion()
    {
      Assembly systemAssembly = typeof(Uri).Assembly;
      StringAssert.Contains("=2.0.5.0,", systemAssembly.FullName,
          "Check we're testing a Silverlight 2.0 assembly");
    }
  }
#endif
}