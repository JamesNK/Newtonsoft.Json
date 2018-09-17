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
    public class NullValueHandlingIgnore : TestFixtureBase
    {
        #region Types
        public class Person
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public Person Partner { get; set; }
            public decimal? Salary { get; set; }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            Person person = new Person
            {
                Name = "Nigal Newborn",
                Age = 1
            };

            string jsonIncludeNullValues = JsonConvert.SerializeObject(person, Formatting.Indented);

            Console.WriteLine(jsonIncludeNullValues);
            // {
            //   "Name": "Nigal Newborn",
            //   "Age": 1,
            //   "Partner": null,
            //   "Salary": null
            // }

            string jsonIgnoreNullValues = JsonConvert.SerializeObject(person, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Console.WriteLine(jsonIgnoreNullValues);
            // {
            //   "Name": "Nigal Newborn",
            //   "Age": 1
            // }
            #endregion

            StringAssert.AreEqual(@"{
  ""Name"": ""Nigal Newborn"",
  ""Age"": 1
}", jsonIgnoreNullValues);
        }
    }
}