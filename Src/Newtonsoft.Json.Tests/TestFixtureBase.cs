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
using System.Reflection;
using System.Text.RegularExpressions;
#if NET20
using Newtonsoft.Json.Serialization;
#else
using System.Runtime.Serialization.Json;
#endif
using System.Text;
using System.Threading;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using TestMethod = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
using SetUp = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestInitializeAttribute;
#elif ASPNETCORE50
using Xunit;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using XAssert = Xunit.Assert;
#else
using NUnit.Framework;
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
    public class TestReflectionUtils
    {
        public static IEnumerable<ConstructorInfo> GetConstructors(Type type)
        {
#if !(NETFX_CORE || ASPNETCORE50)
            return type.GetConstructors();
#else
            return type.GetTypeInfo().DeclaredConstructors;
#endif
        }

        public static PropertyInfo GetProperty(Type type, string name)
        {
#if !(NETFX_CORE || ASPNETCORE50)
            return type.GetProperty(name);
#else
            return type.GetTypeInfo().GetDeclaredProperty(name);
#endif
        }

        public static FieldInfo GetField(Type type, string name)
        {
#if !(NETFX_CORE || ASPNETCORE50)
            return type.GetField(name);
#else
            return type.GetTypeInfo().GetDeclaredField(name);
#endif
        }

        public static MethodInfo GetMethod(Type type, string name)
        {
#if !(NETFX_CORE || ASPNETCORE50)
            return type.GetMethod(name);
#else
            return type.GetTypeInfo().GetDeclaredMethod(name);
#endif
        }
    }

#if ASPNETCORE50
    public class TestFixtureAttribute : Attribute
    {
        // xunit doesn't need a test fixture attribute
        // this exists so the project compiles
    }

    public class XUnitAssert
    {
        public static void IsInstanceOf(Type expectedType, object o)
        {
            XAssert.IsType(expectedType, o);
        }

        public static void AreEqual(double expected, double actual, double r)
        {
            XAssert.Equal(expected, actual, 5); // hack
        }

        public static void AreEqual(object expected, object actual, string message = null)
        {
            XAssert.Equal(expected, actual);
        }

        public static void AreEqual<T>(T expected, T actual, string message = null)
        {
            XAssert.Equal(expected, actual);
        }

        public static void AreNotEqual(object expected, object actual, string message = null)
        {
            XAssert.NotEqual(expected, actual);
        }

        public static void AreNotEqual<T>(T expected, T actual, string message = null)
        {
            XAssert.NotEqual(expected, actual);
        }

        public static void Fail(string message = null, params object[] args)
        {
            if (message != null)
                message = message.FormatWith(CultureInfo.InvariantCulture, args);

            XAssert.True(false, message);
        }

        public static void IsTrue(bool condition, string message = null)
        {
            XAssert.True(condition);
        }

        public static void IsFalse(bool condition)
        {
            XAssert.False(condition);
        }

        public static void IsNull(object o)
        {
            XAssert.Null(o);
        }

        public static void IsNotNull(object o)
        {
            XAssert.NotNull(o);
        }

        public static void AreNotSame(object expected, object actual)
        {
            XAssert.NotSame(expected, actual);
        }

        public static void AreSame(object expected, object actual)
        {
            XAssert.Same(expected, actual);
        }
    }

    public class CollectionAssert
    {
        public static void AreEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            XAssert.Equal(expected, actual);
        }

        public static void AreEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            XAssert.Equal(expected, actual);
        }
    }
#endif

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

#if ASPNETCORE50
        protected TestFixtureBase()
#else
        [SetUp]
        protected void TestSetup()
#endif
        {
//#if !NETFX_CORE
//            CultureInfo turkey = CultureInfo.CreateSpecificCulture("tr");
//            Thread.CurrentThread.CurrentCulture = turkey;
//            Thread.CurrentThread.CurrentUICulture = turkey;
//#endif

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
#if NETFX_CORE
            if (!instance.GetType().IsAssignableFrom(t))
                throw new Exception("Not instance of type");
#else
            Assert.IsInstanceOf(t, instance);
#endif
        }

        public static void Contains(IList collection, object value)
        {
            Contains(collection, value, null);
        }

        public static void Contains(IList collection, object value, string message)
        {
#if !(NETFX_CORE || ASPNETCORE50)
            Assert.Contains(value, collection, message);
#else
            if (!collection.Cast<object>().Any(i => i.Equals(value)))
                throw new Exception(message ?? "Value not found in collection.");
#endif
        }
    }

    public static class StringAssert
    {
        private readonly static Regex Regex = new Regex(@"\r\n|\n\r|\n|\r", RegexOptions.CultureInvariant);

        public static void AreEqual(string expected, string actual)
        {
            expected = Normalize(expected);
            actual = Normalize(actual);

            Assert.AreEqual(expected, actual);
        }

        public static bool Equals(string s1, string s2)
        {
            s1 = Normalize(s1);
            s2 = Normalize(s2);

            return string.Equals(s1, s2);
        }

        public static string Normalize(string s)
        {
            if (s != null)
                s = Regex.Replace(s, "\r\n");

            return s;
        }
    }

    public static class ExceptionAssert
    {
        public static void Throws<TException>(Action action, params string[] possibleMessages)
            where TException : Exception
        {
            try
            {
                action();

                Assert.Fail("Exception of type {0} expected. No exception thrown.", typeof(TException).Name);
            }
            catch (TException ex)
            {
                if (possibleMessages != null && possibleMessages.Length > 0)
                {
                    bool match = false;
                    foreach (string possibleMessage in possibleMessages)
                    {
                        if (StringAssert.Equals(possibleMessage, ex.Message))
                        {
                            match = true;
                            break;
                        }
                    }

                    if (!match)
                        throw new Exception("Unexpected exception message." + Environment.NewLine + "Expected one of: " + string.Join(Environment.NewLine, possibleMessages) + Environment.NewLine + "Got: " + ex.Message + Environment.NewLine + Environment.NewLine + ex);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Exception of type {0} expected; got exception of type {1}.", typeof(TException).Name, ex.GetType().Name), ex);
            }
        }
    }
}