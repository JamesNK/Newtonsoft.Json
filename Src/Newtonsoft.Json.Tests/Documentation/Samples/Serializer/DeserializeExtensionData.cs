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
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class DeserializeExtensionData : TestFixtureBase
    {
        #region Types
        public class DirectoryAccount
        {
            // normal deserialization
            public string DisplayName { get; set; }

            // these properties are set in OnDeserialized
            public string UserName { get; set; }
            public string Domain { get; set; }

            [JsonExtensionData]
            private IDictionary<string, JToken> _additionalData;

            [OnDeserialized]
            private void OnDeserialized(StreamingContext context)
            {
                // SAMAccountName is not deserialized to any property
                // and so it is added to the extension data dictionary
                string samAccountName = (string)_additionalData["SAMAccountName"];

                Domain = samAccountName.Split('\\')[0];
                UserName = samAccountName.Split('\\')[1];
            }

            public DirectoryAccount()
            {
                _additionalData = new Dictionary<string, JToken>();
            }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            string json = @"{
              'DisplayName': 'John Smith',
              'SAMAccountName': 'contoso\\johns'
            }";

            DirectoryAccount account = JsonConvert.DeserializeObject<DirectoryAccount>(json);

            Console.WriteLine(account.DisplayName);
            // John Smith

            Console.WriteLine(account.Domain);
            // contoso

            Console.WriteLine(account.UserName);
            // johns
            #endregion

            Assert.AreEqual("John Smith", account.DisplayName);
            Assert.AreEqual("contoso", account.Domain);
            Assert.AreEqual("johns", account.UserName);
        }
    }
}