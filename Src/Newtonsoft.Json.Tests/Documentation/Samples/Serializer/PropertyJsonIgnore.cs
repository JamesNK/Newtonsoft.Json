using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class PropertyJsonIgnore
  {
    #region Types
    public class Account
    {
      public string FullName { get; set; }
      public string EmailAddress { get; set; }
      [JsonIgnore]
      public string PasswordHash { get; set; }
    }
    #endregion

    public void Example()
    {
      #region Usage
      Account account = new Account
        {
          FullName = "Joe User",
          EmailAddress = "joe@example.com",
          PasswordHash = "VHdlZXQgJ1F1aWNrc2lsdmVyJyB0byBASmFtZXNOSw=="
        };

      string json = JsonConvert.SerializeObject(account);

      Console.WriteLine(json);
      // {"FullName":"Joe User","EmailAddress":"joe@example.com"}
      #endregion
    }
  }
}