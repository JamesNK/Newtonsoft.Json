using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class DeserializeMissingMemberHandling
  {
    public class Account
    {
      public string FullName { get; set; }
      public bool Deleted { get; set; }
    }

    public void Example()
    {
      string json = @"{
        ""FullName"": ""Dan Deleted"",
        ""Deleted"": true,
        ""DeletedDate"": ""2013-01-20T00:00:00""
      }";

      try
      {
        JsonConvert.DeserializeObject<Account>(json, new JsonSerializerSettings
          {
            MissingMemberHandling = MissingMemberHandling.Error
          });
      }
      catch (JsonSerializationException)
      {
        // Could not find member 'DeletedDate' on object of type 'Account'. Path 'DeletedDate', line 4, position 23.
      }
    }
  }
}