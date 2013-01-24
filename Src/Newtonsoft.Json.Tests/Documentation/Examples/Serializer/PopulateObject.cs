using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples
{
  public class PopulateObject
  {
    public class Account
    {
      public string Email { get; set; }
      public bool Active { get; set; }
      public DateTime CreatedDate { get; set; }
      public IList<string> Roles { get; set; }
    }

    public void Example()
    {
      Account account = new Account
        {
          Email = "james@example.com",
          Active = true,
          CreatedDate = new DateTime(2013, 1, 20, 0, 0, 0, DateTimeKind.Utc),
          Roles = new List<string>
            {
              "User",
              "Admin"
            }
        };

      string json = @"{
        ""Active"": false,
        ""Roles"": [
          ""Expired""
        ]
      }";

      JsonConvert.PopulateObject(json, account);

      Console.WriteLine(account.Email);
      // james@example.com

      Console.WriteLine(account.Active);
      // false

      Console.WriteLine(string.Join(", ", account.Roles));
      // User, Admin, Expired
    }
  }
}