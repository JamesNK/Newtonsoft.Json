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

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class JsonPropertyOrder : TestFixtureBase
    {
        #region Types
        public class Account
        {
            public string EmailAddress { get; set; }

            // appear last
            [JsonProperty(Order = 1)]
            public bool Deleted { get; set; }

            [JsonProperty(Order = 2)]
            public DateTime DeletedDate { get; set; }

            public DateTime CreatedDate { get; set; }
            public DateTime UpdatedDate { get; set; }

            // appear first
            [JsonProperty(Order = -2)]
            public string FullName { get; set; }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            Account account = new Account
            {
                FullName = "Aaron Account",
                EmailAddress = "aaron@example.com",
                Deleted = true,
                DeletedDate = new DateTime(2013, 1, 25),
                UpdatedDate = new DateTime(2013, 1, 25),
                CreatedDate = new DateTime(2010, 10, 1)
            };

            string json = JsonConvert.SerializeObject(account, Formatting.Indented);

            Console.WriteLine(json);
            // {
            //   "FullName": "Aaron Account",
            //   "EmailAddress": "aaron@example.com",
            //   "CreatedDate": "2010-10-01T00:00:00",
            //   "UpdatedDate": "2013-01-25T00:00:00",
            //   "Deleted": true,
            //   "DeletedDate": "2013-01-25T00:00:00"
            // }
            #endregion

            StringAssert.AreEqual(@"{
  ""FullName"": ""Aaron Account"",
  ""EmailAddress"": ""aaron@example.com"",
  ""CreatedDate"": ""2010-10-01T00:00:00"",
  ""UpdatedDate"": ""2013-01-25T00:00:00"",
  ""Deleted"": true,
  ""DeletedDate"": ""2013-01-25T00:00:00""
}", json);
        }
    }
}