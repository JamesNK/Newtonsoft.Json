using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class CustomJsonConverter
    {
        #region Types
        public class KeysJsonConverter : JsonConverter
        {
            private readonly Type[] _types;

            public KeysJsonConverter(params Type[] types)
            {
                _types = types;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                JToken t = JToken.FromObject(value);

                if (t.Type != JTokenType.Object)
                {
                    t.WriteTo(writer);
                }
                else
                {
                    JObject o = (JObject)t;
                    IList<string> propertyNames = o.Properties().Select(p => p.Name).ToList();

                    o.AddFirst(new JProperty("Keys", new JArray(propertyNames)));

                    o.WriteTo(writer);
                }
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException("Unnecessary because CanRead is false. The type will skip the converter.");
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return _types.Any(t => t == objectType);
            }
        }

        public class Employee
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public IList<string> Roles { get; set; }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Employee employee = new Employee
            {
                FirstName = "James",
                LastName = "Newton-King",
                Roles = new List<string>
                {
                    "Admin"
                }
            };

            string json = JsonConvert.SerializeObject(employee, Formatting.Indented, new KeysJsonConverter(typeof(Employee)));

            Console.WriteLine(json);
            // {
            //   "Keys": [
            //     "FirstName",
            //     "LastName",
            //     "Roles"
            //   ],
            //   "FirstName": "James",
            //   "LastName": "Newton-King",
            //   "Roles": [
            //     "Admin"
            //   ]
            // }

            Employee newEmployee = JsonConvert.DeserializeObject<Employee>(json, new KeysJsonConverter(typeof(Employee)));

            Console.WriteLine(newEmployee.FirstName);
            // James
            #endregion
        }
    }
}