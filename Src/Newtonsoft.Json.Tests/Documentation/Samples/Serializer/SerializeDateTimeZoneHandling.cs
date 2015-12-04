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
    public class SerializeDateTimeZoneHandling : TestFixtureBase
    {
        #region Types
        public class Flight
        {
            public string Destination { get; set; }
            public DateTime DepartureDate { get; set; }
            public DateTime DepartureDateUtc { get; set; }
            public DateTime DepartureDateLocal { get; set; }
            public TimeSpan Duration { get; set; }
        }
        #endregion

        [Test]
        public void Example()
        {
            #region Usage
            Flight flight = new Flight
            {
                Destination = "Dubai",
                DepartureDate = new DateTime(2013, 1, 21, 0, 0, 0, DateTimeKind.Unspecified),
                DepartureDateUtc = new DateTime(2013, 1, 21, 0, 0, 0, DateTimeKind.Utc),
                DepartureDateLocal = new DateTime(2013, 1, 21, 0, 0, 0, DateTimeKind.Local),
                Duration = TimeSpan.FromHours(5.5)
            };

            string jsonWithRoundtripTimeZone = JsonConvert.SerializeObject(flight, Formatting.Indented, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind
            });

            Console.WriteLine(jsonWithRoundtripTimeZone);
            // {
            //   "Destination": "Dubai",
            //   "DepartureDate": "2013-01-21T00:00:00",
            //   "DepartureDateUtc": "2013-01-21T00:00:00Z",
            //   "DepartureDateLocal": "2013-01-21T00:00:00+01:00",
            //   "Duration": "05:30:00"
            // }

            string jsonWithLocalTimeZone = JsonConvert.SerializeObject(flight, Formatting.Indented, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Local
            });

            Console.WriteLine(jsonWithLocalTimeZone);
            // {
            //   "Destination": "Dubai",
            //   "DepartureDate": "2013-01-21T00:00:00+01:00",
            //   "DepartureDateUtc": "2013-01-21T01:00:00+01:00",
            //   "DepartureDateLocal": "2013-01-21T00:00:00+01:00",
            //   "Duration": "05:30:00"
            // }

            string jsonWithUtcTimeZone = JsonConvert.SerializeObject(flight, Formatting.Indented, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Utc
            });

            Console.WriteLine(jsonWithUtcTimeZone);
            // {
            //   "Destination": "Dubai",
            //   "DepartureDate": "2013-01-21T00:00:00Z",
            //   "DepartureDateUtc": "2013-01-21T00:00:00Z",
            //   "DepartureDateLocal": "2013-01-20T23:00:00Z",
            //   "Duration": "05:30:00"
            // }

            string jsonWithUnspecifiedTimeZone = JsonConvert.SerializeObject(flight, Formatting.Indented, new JsonSerializerSettings
            {
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
            });

            Console.WriteLine(jsonWithUnspecifiedTimeZone);
            // {
            //   "Destination": "Dubai",
            //   "DepartureDate": "2013-01-21T00:00:00",
            //   "DepartureDateUtc": "2013-01-21T00:00:00",
            //   "DepartureDateLocal": "2013-01-21T00:00:00",
            //   "Duration": "05:30:00"
            // }
            #endregion
        }
    }
}