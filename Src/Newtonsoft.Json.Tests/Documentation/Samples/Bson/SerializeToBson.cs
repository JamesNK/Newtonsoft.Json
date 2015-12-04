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
using System.IO;
using System.Text;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Bson
{
    [TestFixture]
    public class SerializeToBson : TestFixtureBase
    {
        #region Types
        public class Event
        {
            public string Name { get; set; }
            public DateTime StartDate { get; set; }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            Event e = new Event
            {
                Name = "Movie Premiere",
                StartDate = new DateTime(2013, 1, 22, 20, 30, 0, DateTimeKind.Utc)
            };

            MemoryStream ms = new MemoryStream();
            using (BsonWriter writer = new BsonWriter(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(writer, e);
            }

            string data = Convert.ToBase64String(ms.ToArray());

            Console.WriteLine(data);
            // MQAAAAJOYW1lAA8AAABNb3ZpZSBQcmVtaWVyZQAJU3RhcnREYXRlAED982M8AQAAAA==
            #endregion

            Assert.AreEqual("MQAAAAJOYW1lAA8AAABNb3ZpZSBQcmVtaWVyZQAJU3RhcnREYXRlAED982M8AQAAAA==", data);
        }
    }
}