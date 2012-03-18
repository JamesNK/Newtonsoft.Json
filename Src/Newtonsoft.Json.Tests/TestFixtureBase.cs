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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
#if NET20
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities.LinqBridge;
#endif
using System.Text;
using System.Threading;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestMethod = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Newtonsoft.Json.Utilities;
using System.Collections;

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public abstract class TestFixtureBase
  {
    protected string GetOffset(DateTime d, DateFormatHandling dateFormatHandling)
    {
      StringWriter sw = new StringWriter();
      JsonConvert.WriteDateTimeOffset(sw, DateTime.SpecifyKind(d, DateTimeKind.Local).GetUtcOffset(), dateFormatHandling);
      sw.Flush();

      return sw.ToString();
    }

#if !NETFX_CORE
    [SetUp]
    protected void TestSetup()
    {
      //CultureInfo turkey = CultureInfo.CreateSpecificCulture("tr");
      //Thread.CurrentThread.CurrentCulture = turkey;
      //Thread.CurrentThread.CurrentUICulture = turkey;
    }

    protected void WriteEscapedJson(string json)
    {
      Console.WriteLine(EscapeJson(json));
    }

    protected string EscapeJson(string json)
    {
      return @"@""" + json.Replace(@"""", @"""""") + @"""";
    }
#endif
  }

#if NETFX_CORE
  public static class Console
  {
    public static void WriteLine(params object[] args)
    {
    }
  }
#endif

  public static class CustomAssert
  {
    public static void IsInstanceOfType(Type t, object instance)
    {
#if !NETFX_CORE
      Assert.IsInstanceOfType(t, instance);
#else
      if (!instance.GetType().IsAssignableFrom(t))
        throw new Exception("Blah");
#endif
    }

    public static void Contains(IList collection, object value)
    {
#if !NETFX_CORE
      Assert.Contains(value, collection);
#else
      if (!collection.Cast<object>().Any(i => i.Equals(value)))
        throw new Exception("Value not found in collection.");
#endif
    }
  }

  public static class ExceptionAssert
  {
    public static void Throws<TException>(string message, Action action)
        where TException : Exception
    {
      try
      {
        action();

        Assert.Fail("Exception of type {0} expected; got none exception", typeof(TException).Name);
      }
      catch (TException ex)
      {
        if (message != null)
          Assert.AreEqual(message, ex.Message);
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Exception of type {0} expected; got exception of type {1}.", typeof(TException).Name, ex.GetType().Name), ex);
      }
    }
  }
}
