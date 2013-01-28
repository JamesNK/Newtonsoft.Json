using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class SerializeDateFormatHandling
  {
    public void Example()
    {
      #region Usage
      DateTime mayanEndOfTheWorld = new DateTime(2012, 12, 21);

      string jsonIsoDate = JsonConvert.SerializeObject(mayanEndOfTheWorld);

      Console.WriteLine(jsonIsoDate);
      // "2012-12-21T00:00:00"

      string jsonMsDate = JsonConvert.SerializeObject(mayanEndOfTheWorld, new JsonSerializerSettings
        {
          DateFormatHandling = DateFormatHandling.MicrosoftDateFormat
        });

      Console.WriteLine(jsonMsDate);
      // "\/Date(1356044400000+0100)\/"
      #endregion
    }
  }
}