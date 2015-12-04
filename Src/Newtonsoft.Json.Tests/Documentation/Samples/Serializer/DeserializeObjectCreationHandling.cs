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
    public class DeserializeObjectCreationHandling : TestFixtureBase
    {
        #region Types
        public class UserViewModel
        {
            public string Name { get; set; }
            public IList<string> Offices { get; private set; }

            public UserViewModel()
            {
                Offices = new List<string>
                {
                    "Auckland",
                    "Wellington",
                    "Christchurch"
                };
            }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            string json = @"{
              'Name': 'James',
              'Offices': [
                'Auckland',
                'Wellington',
                'Christchurch'
              ]
            }";

            UserViewModel model1 = JsonConvert.DeserializeObject<UserViewModel>(json);

            foreach (string office in model1.Offices)
            {
                Console.WriteLine(office);
            }
            // Auckland
            // Wellington
            // Christchurch
            // Auckland
            // Wellington
            // Christchurch

            UserViewModel model2 = JsonConvert.DeserializeObject<UserViewModel>(json, new JsonSerializerSettings
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace
            });

            foreach (string office in model2.Offices)
            {
                Console.WriteLine(office);
            }
            // Auckland
            // Wellington
            // Christchurch
            #endregion

            Assert.AreEqual(3, model2.Offices.Count);
        }
    }
}