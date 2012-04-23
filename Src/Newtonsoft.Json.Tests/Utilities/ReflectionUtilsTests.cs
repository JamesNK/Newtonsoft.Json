#if !PORTABLE
using global::System;
using global::System.Collections.Generic;
using global::System.Runtime.Serialization.Formatters;
#if !NETFX_CORE
using global::NUnit.Framework;
#else
using global::Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = global::Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = global::Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using global::Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Tests.Utilities
{
  [TestFixture]
  public class ReflectionUtilsTests : TestFixtureBase
  {
    [Test]
    public void GetTypeNameSimpleForGenericTypes()
    {
      string typeName;

      typeName = ReflectionUtils.GetTypeName(typeof(IList<Type>), FormatterAssemblyStyle.Simple);
      Assert.AreEqual("System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib", typeName);

      typeName = ReflectionUtils.GetTypeName(typeof(IDictionary<IList<Type>, IList<Type>>), FormatterAssemblyStyle.Simple);
      Assert.AreEqual("System.Collections.Generic.IDictionary`2[[System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib],[System.Collections.Generic.IList`1[[System.Type, mscorlib]], mscorlib]], mscorlib", typeName);

      typeName = ReflectionUtils.GetTypeName(typeof(IList<>), FormatterAssemblyStyle.Simple);
      Assert.AreEqual("System.Collections.Generic.IList`1, mscorlib", typeName);

      typeName = ReflectionUtils.GetTypeName(typeof(IDictionary<,>), FormatterAssemblyStyle.Simple);
      Assert.AreEqual("System.Collections.Generic.IDictionary`2, mscorlib", typeName);
    }
  }
}
#endif