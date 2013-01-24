using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests.Documentation.Examples.Serializer
{
  public class JsonConverterAttributeProperty
  {
    public enum UserStatus
    {
      NotConfirmed,
      Active,
      Deleted
    }

    public class User
    {
      public string UserName { get; set; }

      [JsonConverter(typeof(StringEnumConverter))]
      public UserStatus Status { get; set; }
    }

    public void Example()
    {
      User user = new User
        {
          UserName = @"domain\username",
          Status = UserStatus.Deleted
        };

      string json = JsonConvert.SerializeObject(user, Formatting.Indented);

      Console.WriteLine(json);
      // {
      //   "UserName": "domain\\username",
      //   "Status": "Deleted"
      // }
    }
  }
}