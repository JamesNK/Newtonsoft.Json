using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class JsonConverterAttributeProperty
    {
        #region Types
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
        #endregion

        public void Example()
        {
            #region Usage
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
            #endregion
        }
    }
}