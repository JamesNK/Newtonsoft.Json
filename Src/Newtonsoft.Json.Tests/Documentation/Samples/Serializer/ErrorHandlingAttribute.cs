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

using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
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
    public class ErrorHandlingAttribute : TestFixtureBase
    {
        #region Types
        public class Employee
        {
            private List<string> _roles;

            public string Name { get; set; }
            public int Age { get; set; }

            public List<string> Roles
            {
                get
                {
                    if (_roles == null)
                    {
                        throw new Exception("Roles not loaded!");
                    }

                    return _roles;
                }
                set { _roles = value; }
            }

            public string Title { get; set; }

            [OnError]
            internal void OnError(StreamingContext context, ErrorContext errorContext)
            {
                errorContext.Handled = true;
            }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            Employee person = new Employee
            {
                Name = "George Michael Bluth",
                Age = 16,
                Roles = null,
                Title = "Mister Manager"
            };

            string json = JsonConvert.SerializeObject(person, Formatting.Indented);

            Console.WriteLine(json);
            // {
            //   "Name": "George Michael Bluth",
            //   "Age": 16,
            //   "Title": "Mister Manager"
            // }
            #endregion

            Assert.AreEqual(@"{
  ""Name"": ""George Michael Bluth"",
  ""Age"": 16,
  ""Title"": ""Mister Manager""
}", json);
        }
    }
}