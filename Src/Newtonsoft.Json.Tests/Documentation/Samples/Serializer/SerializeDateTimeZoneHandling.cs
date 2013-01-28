using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class SerializeDateTimeZoneHandling
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