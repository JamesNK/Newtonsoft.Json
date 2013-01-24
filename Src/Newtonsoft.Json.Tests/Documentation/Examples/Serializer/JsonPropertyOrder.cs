using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class JsonPropertyOrder
  {
    public class Account
    {
      public string EmailAddress { get; set; }

      // appear last
      [JsonProperty(Order = 1)]
      public bool Deleted { get; set; }
      [JsonProperty(Order = 2)]
      public DateTime DeletedDate { get; set; }

      public DateTime CreatedDate { get; set; }
      public DateTime UpdatedDate { get; set; }

      // appear first
      [JsonProperty(Order = -2)]
      public string FullName { get; set; }
    }

    public void Example()
    {
      Account account = new Account
        {
          FullName = "Aaron Account",
          EmailAddress = "aaron@example.com",
          Deleted = true,
          DeletedDate = new DateTime(2013, 1, 25),
          UpdatedDate = new DateTime(2013, 1, 25),
          CreatedDate = new DateTime(2010, 10, 1)
        };

      string json = JsonConvert.SerializeObject(account, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "FullName": "Aaron Account",
      //   "EmailAddress": "aaron@example.com",
      //   "CreatedDate": "2010-10-01T00:00:00",
      //   "UpdatedDate": "2013-01-25T00:00:00",
      //   "Deleted": true,
      //   "DeletedDate": "2013-01-25T00:00:00"
      // }
    }
  }
}
