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

using System.Linq;
using Newtonsoft.Json.Linq;
#if !(PORTABLE || PORTABLE40)
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    public class Issue2887
    {
// GAC doesn't exist in NetCore apps. Issue does not repro.
#if !(PORTABLE || PORTABLE40)
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2887
        public void LoadNewtonsoftWithGacConflict()
        {
            // TODO: verify test fails without build changes.
            // TODO: Fix test to work locally
            // TODO: Fix test + setup to ensure 

            // TestSetup: Run Gacutil -i v13.0.1 Newtonsoft.Json
            // Call Newtonsoft.Json.Test.dll RuntimeLoadTest
            // TestCleanup: Run Gacutil -u v13.0.1 Newtonsoft.Json
        }

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/2887
        public void NewtonsoftJsonHasBuildVersion()
        {
            var assembly = typeof(JsonConvert).GetTypeInfo().Assembly;
            var assemblyVersion = assembly.GetName().Version;
            Assert.AreNotEqual(0, assemblyVersion.Build, "Newtonsoft needs a non-zero build version in the assemblyversion to avoid issue 2887.");
        }
#endif

        public void RuntimeLoadTest()
        {
            var jObject = new JObject { { "NullProperty", false ? "0" : null } };

            var jToken = JToken.FromObject(jObject);
            
            Assert.AreEqual(JTokenType.Null, jToken.Children().Children().Single().Type);

            jObject = new JObject { { "NullProperty", (string)null } };

            jToken = JToken.FromObject(jObject);
            Assert.AreEqual(JTokenType.Null, jToken.Children().Children().Single().Type);
        }
    }
}
