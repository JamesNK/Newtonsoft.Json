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
    public class SerializeTypeNameHandling : TestFixtureBase
    {
        #region Types
        public abstract class Business
        {
            public string Name { get; set; }
        }

        public class Hotel : Business
        {
            public int Stars { get; set; }
        }

        public class Stockholder
        {
            public string FullName { get; set; }
            public IList<Business> Businesses { get; set; }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            Stockholder stockholder = new Stockholder
            {
                FullName = "Steve Stockholder",
                Businesses = new List<Business>
                {
                    new Hotel
                    {
                        Name = "Hudson Hotel",
                        Stars = 4
                    }
                }
            };

            string jsonTypeNameAll = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            });

            Console.WriteLine(jsonTypeNameAll);
            // {
            //   "$type": "Newtonsoft.Json.Samples.Stockholder, Newtonsoft.Json.Tests",
            //   "FullName": "Steve Stockholder",
            //   "Businesses": {
            //     "$type": "System.Collections.Generic.List`1[[Newtonsoft.Json.Samples.Business, Newtonsoft.Json.Tests]], mscorlib",
            //     "$values": [
            //       {
            //         "$type": "Newtonsoft.Json.Samples.Hotel, Newtonsoft.Json.Tests",
            //         "Stars": 4,
            //         "Name": "Hudson Hotel"
            //       }
            //     ]
            //   }
            // }

            string jsonTypeNameAuto = JsonConvert.SerializeObject(stockholder, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            Console.WriteLine(jsonTypeNameAuto);
            // {
            //   "FullName": "Steve Stockholder",
            //   "Businesses": [
            //     {
            //       "$type": "Newtonsoft.Json.Samples.Hotel, Newtonsoft.Json.Tests",
            //       "Stars": 4,
            //       "Name": "Hudson Hotel"
            //     }
            //   ]
            // }

            // for security TypeNameHandling is required when deserializing
            Stockholder newStockholder = JsonConvert.DeserializeObject<Stockholder>(jsonTypeNameAuto, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto
            });

            Console.WriteLine(newStockholder.Businesses[0].GetType().Name);
            // Hotel
            #endregion

            Assert.AreEqual("Hotel", newStockholder.Businesses[0].GetType().Name);
        }
    }
}