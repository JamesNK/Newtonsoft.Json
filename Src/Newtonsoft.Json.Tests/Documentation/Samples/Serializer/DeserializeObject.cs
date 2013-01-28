using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class DeserializeObject
  {
    #region Types
    public class Account
    {
      public string Email { get; set; }
      public bool Active { get; set; }
      public DateTime CreatedDate { get; set; }
      public IList<string> Roles { get; set; }
    }
    #endregion

    public void Example()
    {
      #region Usage
      string json = @"{
        'Email': 'james@example.com',
        'Active': true,
        'CreatedDate': '2013-01-20T00:00:00Z',
        'Roles': [
          'User',
          'Admin'
        ]
      }";

      Account account = JsonConvert.DeserializeObject<Account>(json);

      Console.WriteLine(account.Email);
      // james@example.com
      #endregion
    }
  }
}