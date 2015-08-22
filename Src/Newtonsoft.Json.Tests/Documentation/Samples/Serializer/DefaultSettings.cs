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
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.Serialization;
using Staff = Newtonsoft.Json.Tests.TestObjects.Employee;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class DefaultSettings : TestFixtureBase
    {
        [Test]
        public void Example()
        {
            string json;

            try
            {
                #region Usage
                // settings will automatically be used by JsonConvert.SerializeObject/DeserializeObject
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    ContractResolver = new CamelCasePropertyNamesContractResolver()
                };

                Staff s = new Staff
                {
                    FirstName = "Eric",
                    LastName = "Example",
                    BirthDate = new DateTime(1980, 4, 20, 0, 0, 0, DateTimeKind.Utc),
                    Department = "IT",
                    JobTitle = "Web Dude"
                };

                json = JsonConvert.SerializeObject(s);
                // {
                //   "firstName": "Eric",
                //   "lastName": "Example",
                //   "birthDate": "1980-04-20T00:00:00Z",
                //   "department": "IT",
                //   "jobTitle": "Web Dude"
                // }
                #endregion
            }
            finally
            {
                JsonConvert.DefaultSettings = null;
            }

            StringAssert.AreEqual(@"{
  ""firstName"": ""Eric"",
  ""lastName"": ""Example"",
  ""birthDate"": ""1980-04-20T00:00:00Z"",
  ""department"": ""IT"",
  ""jobTitle"": ""Web Dude""
}", json);
        }
    }
}