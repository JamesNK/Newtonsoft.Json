using System;
using System.Reflection;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests
{
  // todo: need to fix this to get WP unit tests running off right dlls
#if SILVERLIGHT
  [TestFixture]
  public class SilverlightTests
  {
    [Test]
    public void SystemVersion()
    {
      Assembly systemAssembly = typeof(Uri).Assembly;
      StringAssert.Contains("=4.0.0.0,", systemAssembly.FullName,
          "Check we're testing a Silverlight 4.0 assembly");
    }
  }
#endif
}