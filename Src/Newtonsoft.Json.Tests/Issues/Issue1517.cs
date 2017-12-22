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

#if (NETSTANDARD2_0)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json.Serialization;
#if !NET20
using System.Xml.Linq;
#endif
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1517 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            RSAParameters rsaParameters = new RSAParameters();
            rsaParameters.D = new byte[] { 1, 2 };
            rsaParameters.DP = new byte[] { 2, 4 };
            rsaParameters.DQ = new byte[] { 5, 6 };
            rsaParameters.Exponent = new byte[] { 7, 8 };
            rsaParameters.InverseQ = new byte[] { 9, 10 };
            rsaParameters.Modulus = new byte[] { 11, 12 };
            rsaParameters.P = new byte[] { 13, 14 };
            rsaParameters.Q = new byte[] { 15, 16 };

            string json = JsonConvert.SerializeObject(rsaParameters, Formatting.Indented);

            // a subset of values is serialized because of the NotSerializedAttribute
            // https://msdn.microsoft.com/en-us/library/system.security.cryptography.rsaparameters.d(v=vs.110).aspx
            StringAssert.AreEqual(@"{
  ""Exponent"": ""Bwg="",
  ""Modulus"": ""Cww=""
}", json);
        }
    }
}
#endif