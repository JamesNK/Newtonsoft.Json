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
#else
using System.Runtime.Serialization.Json;
#endif
using System.Text;
using System.Threading;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestMethod = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
#endif
using Newtonsoft.Json.Utilities;
using System.Collections;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public abstract class TestFixtureBase
  {
#if !NET20
    protected string GetDataContractJsonSerializeResult(object o)
    {
      MemoryStream ms = new MemoryStream();
      DataContractJsonSerializer s = new DataContractJsonSerializer(o.GetType());
      s.WriteObject(ms, o);

      var data = ms.ToArray();
      return Encoding.UTF8.GetString(data, 0, data.Length);
    }
#endif

    protected string GetOffset(DateTime d, DateFormatHandling dateFormatHandling)
    {
      char[] chars = new char[8];
      int pos = DateTimeUtils.WriteDateTimeOffset(chars, 0, DateTime.SpecifyKind(d, DateTimeKind.Local).GetUtcOffset(), dateFormatHandling);

      return new string(chars, 0, pos);
    }

    protected string BytesToHex(byte[] bytes)
    {
      return BytesToHex(bytes, false);
    }

    protected string BytesToHex(byte[] bytes, bool removeDashes)
    {
      string hex = BitConverter.ToString(bytes);
      if (removeDashes)
        hex = hex.Replace("-", "");

      return hex;
    }

    protected byte[] HexToBytes(string hex)
    {
      string fixedHex = hex.Replace("-", string.Empty);

      // array to put the result in
      byte[] bytes = new byte[fixedHex.Length / 2];
      // variable to determine shift of high/low nibble
      int shift = 4;
      // offset of the current byte in the array
      int offset = 0;
      // loop the characters in the string
      foreach (char c in fixedHex)
      {
        // get character code in range 0-9, 17-22
        // the % 32 handles lower case characters
        int b = (c - '0') % 32;
        // correction for a-f
        if (b > 9) b -= 7;
        // store nibble (4 bits) in byte array
        bytes[offset] |= (byte)(b << shift);
        // toggle the shift variable between 0 and 4
        shift ^= 4;
        // move to next byte
        if (shift != 0) offset++;
      }
      return bytes;
    }

    [SetUp]
    protected void TestSetup()
    {
      //CultureInfo turkey = CultureInfo.CreateSpecificCulture("tr");
      //Thread.CurrentThread.CurrentCulture = turkey;
      //Thread.CurrentThread.CurrentUICulture = turkey;

      JsonConvert.DefaultSettings = null;
    }

    protected void WriteEscapedJson(string json)
    {
      Console.WriteLine(EscapeJson(json));
    }

    protected string EscapeJson(string json)
    {
      return @"@""" + json.Replace(@"""", @"""""") + @"""";
    }
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
#if (WINDOWS_PHONE || SILVERLIGHT)
      Assert.IsInstanceOfType(t, instance);
#elif NETFX_CORE
      if (!instance.GetType().IsAssignableFrom(t))
        throw new Exception("Not instance of type");
#else
      Assert.IsInstanceOf(t, instance);
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
          Assert.AreEqual(message, ex.Message, "Unexpected exception message." + Environment.NewLine + "Expected: " + message + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
      }
      catch (Exception ex)
      {
        throw new Exception(string.Format("Exception of type {0} expected; got exception of type {1}.", typeof(TException).Name, ex.GetType().Name), ex);
      }
    }
  }
}
