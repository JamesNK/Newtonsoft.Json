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

#if !NET20
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;
#if !(NET20 || NET35 || NET40 || PORTABLE40)
using System.Threading.Tasks;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
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
    public class Issue2632 : TestFixtureBase
    {
#if !DNXCORE50
        [Test]
        public void Test()
        {
            var clientsJson = @"[
                        {
                            ""IsLinkedOn"": 1641019824848,
                            ""IsImportedOn"": -62115578800000,
                            ""LogUpdatedOn"": ""06/30"",
                            ""UpdatedOn"": ""12/30/2021 6:22:04 PM""
                        }
                    ]";
            var serializer = new JsonSerializer
            {
                ContractResolver = new DefaultContractResolver { IgnoreSerializableAttribute = false },
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
                DateParseHandling = DateParseHandling.DateTime,
                DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK",
            };
            serializer.Error += SerializerErrorNotifications;
            var reader = new JsonTextReader(new StringReader(clientsJson));
            var clients = serializer.Deserialize<List<ClientAccount>>(reader);
            CollectionAssert.IsNotEmpty(clients);
            Assert.That(clients[0].IsLinkedOn, Is.EqualTo(default(DateTime)));
            Assert.That(clients[0].IsImportedOn, Is.EqualTo(default(DateTime)));
            Assert.That(clients[0].LogUpdatedOn, Is.EqualTo(new DateTime(DateTime.Now.Year, 6, 30)));
            Assert.That(clients[0].Updatedon, Is.EqualTo(new DateTime(2021, 12, 30, 18, 22, 04)));
        }
#endif

        private void SerializerErrorNotifications(object sender, Json.Serialization.ErrorEventArgs e)
        {
            // This emulates the same logic that .net webapi uses for the json media type formatter
            if (!(e is null))
                e.ErrorContext.Handled = true;
        }

        [DataContract]
        internal class ClientAccount
        {
            internal ClientAccount() { }

            [DataMember]
            internal DateTime IsImportedOn { get; set; }
            [DataMember]
            internal DateTime IsLinkedOn { get; set; }
            [DataMember]
            internal DateTime LogUpdatedOn { get; set; }
            [DataMember]
            internal DateTime Updatedon { get; set; }
        }
    }
}
#endif