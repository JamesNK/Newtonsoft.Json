using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class PropertyJsonIgnore
  {
    public class Account
    {
      public string FullName { get; set; }
      public string EmailAddress { get; set; }
      [JsonIgnore]
      public string PasswordHash { get; set; }
    }

    public void Example()
    {
      Account account = new Account
        {
          FullName = "Joe User",
          EmailAddress = "joe@example.com",
          PasswordHash = "VHdlZXQgJ1F1aWNrc2lsdmVyJyB0byBASmFtZXNOSw=="
        };

      string json = JsonConvert.SerializeObject(account);

      Console.WriteLine(json);
      // {"FullName":"Joe User","EmailAddress":"joe@example.com"}
    }
  }
}