using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
  public class DeserializeMissingMemberHandling
  {
    #region Types
    public class Account
    {
      public string FullName { get; set; }
      public bool Deleted { get; set; }
    }
    #endregion

    public void Example()
    {
      #region Usage
      string json = @"{
        'FullName': 'Dan Deleted',
        'Deleted': true,
        'DeletedDate': '2013-01-20T00:00:00'
      }";

      try
      {
        JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
          {
            MissingMemberHandling = MissingMemberHandling.Error
          });
      }
      catch (JsonSerializationException ex)
      {
        Console.WriteLine(ex.Message);
        // Could not find member 'DeletedDate' on object of type 'Account'. Path 'DeletedDate', line 4, position 23.
      }
      #endregion
    }
  }
}