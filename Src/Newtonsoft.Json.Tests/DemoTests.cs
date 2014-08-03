#region License
// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class DemoTests : TestFixtureBase
    {
        public class HtmlColor
        {
            public int Red { get; set; }
            public int Green { get; set; }
            public int Blue { get; set; }
        }

        [Test]
        public void JsonConverter()
        {
            HtmlColor red = new HtmlColor
            {
                Red = 255,
                Green = 0,
                Blue = 0
            };

            string json = JsonConvert.SerializeObject(red, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            // {
            //   "Red": 255,
            //   "Green": 0,
            //   "Blue": 0
            // }

            json = JsonConvert.SerializeObject(red, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new HtmlColorConverter() }
            });
            // "#FF0000"

            HtmlColor r2 = JsonConvert.DeserializeObject<HtmlColor>(json, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new HtmlColorConverter() }
            });
            Assert.AreEqual(255, r2.Red);
            Assert.AreEqual(0, r2.Green);
            Assert.AreEqual(0, r2.Blue);

            Console.WriteLine(json);
        }

        public class PersonDemo
        {
            public string Name { get; set; }
            public int Age { get; set; }
            public string Job { get; set; }
        }

        public class Session
        {
            public string Name { get; set; }
            public DateTime Date { get; set; }
        }

        [Test]
        public void GenerateSchema()
        {
            JsonSchemaGenerator generator = new JsonSchemaGenerator();

            JsonSchema schema = generator.Generate(typeof(Session));

            // {
            //   "type": "object",
            //   "properties": {
            //     "Name": {
            //       "required": true,
            //       "type": [
            //         "string",
            //         "null"
            //       ]
            //     },
            //     "Date": {
            //       "required": true,
            //       "type": "string"
            //     }
            //   }
            // }
        }

        public class HtmlColorConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                // create hex string from value
                HtmlColor color = (HtmlColor)value;
                string hexString = color.Red.ToString("X2")
                    + color.Green.ToString("X2")
                    + color.Blue.ToString("X2");

                // write value to json
                writer.WriteValue("#" + hexString);
            }

            //public override object ReadJson(JsonReader reader, Type objectType,
            //    object existingValue, JsonSerializer serializer)
            //{
            //    throw new NotImplementedException();
            //}

            public override object ReadJson(JsonReader reader, Type objectType,
                object existingValue, JsonSerializer serializer)
            {
                // get hex string
                string hexString = (string)reader.Value;
                hexString = hexString.TrimStart('#');

                // build html color from hex
                return new HtmlColor
                {
                    Red = Convert.ToInt32(hexString.Substring(0, 2), 16),
                    Green = Convert.ToInt32(hexString.Substring(2, 2), 16),
                    Blue = Convert.ToInt32(hexString.Substring(4, 2), 16)
                };
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(HtmlColor);
            }
        }

        [Test]
        public void SerializationGuide()
        {
            IList<string> roles = new List<string>
            {
                "User",
                "Admin"
            };

            string roleJson = JsonConvert.SerializeObject(roles, Formatting.Indented);
            // [
            //   "User",
            //   "Admin"
            // ]

            IDictionary<DateTime, int> dailyRegistrations = new Dictionary<DateTime, int>
            {
                { new DateTime(2014, 6, 1), 23 },
                { new DateTime(2014, 6, 2), 50 }
            };

            string regJson = JsonConvert.SerializeObject(dailyRegistrations, Formatting.Indented);
            // {
            //   "2014-06-01T00:00:00": 23,
            //   "2014-06-02T00:00:00": 50
            // }

            City c = new City { Name = "Oslo", Population = 650000 };

            string cityJson = JsonConvert.SerializeObject(c, Formatting.Indented);
            // {
            //   "Name": "Oslo",
            //   "Population": 650000
            // }
        }

        [Test]
        public void SerializationBasics()
        {
            IList<string> roles = new List<string>
            {
                "User",
                "Admin"
            };

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();

            string j = JsonConvert.SerializeObject(roles, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TraceWriter = traceWriter
            });

            string trace = traceWriter.ToString();
            // Started serializing System.Collections.Generic.List`1[System.String].
            // Finished serializing System.Collections.Generic.List`1[System.String].
            // 2014-05-13T13:41:53.706 Verbose Serialized JSON: 
            // [
            //   "User",
            //   "Admin"
            // ]

            Console.WriteLine(trace);
        }

        [Test]
        public void SerializationBasics2()
        {
            var s = new Session
            {
                Name = "Serialize All The Things",
                Date = new DateTime(2014, 6, 4)
            };

            string j = JsonConvert.SerializeObject(s, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                Converters = { new JavaScriptDateTimeConverter() }
            });
            // {
            //   "Name": "Serialize All The Things",
            //   "Date": new Date(1401796800000)
            // }

            Console.WriteLine(j);
        }

        [Test]
        public void DeserializationBasics1()
        {
            string j = @"{
              'Name': 'Serialize All The Things',
              'Date': new Date(1401796800000)
            }";

            var s = JsonConvert.DeserializeObject<Session>(j, new JsonSerializerSettings
            {
                Converters = { new JavaScriptDateTimeConverter() }
            });
            // Name = Serialize All The Things
            // Date = Tuesday, 3 June 2014
        }

        [Test]
        public void DeserializationBasics2()
        {
            Session s = new Session();
            s.Date = new DateTime(2014, 6, 4);

            string j = @"{
              'Name': 'Serialize All The Things'
            }";

            JsonConvert.PopulateObject(j, s);
            // Name = Serialize All The Things
            // Date = Tuesday, 3 June 2014
        }

        public class City
        {
            public string Name { get; set; }
            public int Population { get; set; }
        }

        public class Employee
        {
            public string Name { get; set; }
        }

        public class Manager : Employee
        {
            public IList<Employee> Reportees { get; set; }
        }

        [Test]
        public void SerializeReferencesByValue()
        {
            Employee arnie = new Employee { Name = "Arnie Admin" };
            Manager mike = new Manager { Name = "Mike Manager" };
            Manager susan = new Manager { Name = "Susan Supervisor" };

            mike.Reportees = new[] { arnie, susan };
            susan.Reportees = new[] { arnie };

            string json = JsonConvert.SerializeObject(mike, Formatting.Indented);
            // {
            //   "Reportees": [
            //     { 
            //       "Name": "Arnie Admin"
            //     },
            //     {
            //       "Reportees": [
            //         {
            //           "Name": "Arnie Admin"
            //         }
            //       ],
            //       "Name": "Susan Supervisor"
            //     }
            //   ],
            //   "Name": "Mike Manager"
            // }
            Console.WriteLine(json);
        }

        [Test]
        public void SerializeReferencesWithMetadata()
        {
            Employee arnie = new Employee { Name = "Arnie Admin" };
            Manager mike = new Manager { Name = "Mike Manager" };
            Manager susan = new Manager { Name = "Susan Supervisor" };

            mike.Reportees = new[] { arnie, susan };
            susan.Reportees = new[] { arnie };

            string json = JsonConvert.SerializeObject(mike, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            // {
            //   "$id": "1",
            //   "$type": "YourNamespace.Manager, YourAssembly",
            //   "Name": "Mike Manager",
            //   "Reportees": [
            //     {
            //       "$id": "2",
            //       "$type": "YourNamespace.Employee, YourAssembly",
            //       "Name": "Arnie Admin"
            //     },
            //     {
            //       "$id": "3",
            //       "$type": "YourNamespace.Manager, YourAssembly",
            //       "Name": "Susan Supervisor",
            //       "Reportees": [
            //         {
            //           "$ref": "2"
            //         }
            //       ]
            //     }
            //   ]
            // }
            Console.WriteLine(json);
        }

        [Test]
        public void RoundtripTypesAndReferences()
        {
            string json = @"{
  '$id': '1',
  '$type': 'Newtonsoft.Json.Tests.DemoTests+Manager, Newtonsoft.Json.Tests',
  'Reportees': [
    {
      '$id': '2',
      '$type': 'Newtonsoft.Json.Tests.DemoTests+Employee, Newtonsoft.Json.Tests',
      'Name': 'Arnie Admin'
    },
    {
      '$id': '3',
      '$type': 'Newtonsoft.Json.Tests.DemoTests+Manager, Newtonsoft.Json.Tests',
      'Reportees': [
        {
          '$ref': '2'
        }
      ],
      'Name': 'Susan Supervisor'
    }
  ],
  'Name': 'Mike Manager'
}";

            var e = JsonConvert.DeserializeObject<Employee>(json, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Objects,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            // Name = Mike Manager
            // Reportees = Arnie Admin, Susan Supervisor

            Manager mike = (Manager)e;
            Manager susan = (Manager)mike.Reportees[1];

            Object.ReferenceEquals(mike.Reportees[0], susan.Reportees[0]);
            // true

            Assert.IsTrue(ReferenceEquals(mike.Reportees[0], susan.Reportees[0]));
        }

        public class House
        {
            public string StreetAddress { get; set; }
            public DateTime BuildDate { get; set; }
            public int Bedrooms { get; set; }
            public decimal FloorArea { get; set; }
        }

        public class House1
        {
            public string StreetAddress { get; set; }
            [JsonIgnore]
            public int Bedrooms { get; set; }
            [JsonIgnore]
            public decimal FloorArea { get; set; }
            [JsonIgnore]
            public DateTime BuildDate { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class House3
        {
            [JsonProperty]
            public string StreetAddress { get; set; }
            public int Bedrooms { get; set; }
            public decimal FloorArea { get; set; }
            public DateTime BuildDate { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class House2
        {
            [JsonProperty("address")]
            public string StreetAddress { get; set; }
            public int Bedrooms { get; set; }
            public decimal FloorArea { get; set; }
            public DateTime BuildDate { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class House4
        {
            [JsonProperty("address", Order = 2)]
            public string StreetAddress { get; set; }
            public int Bedrooms { get; set; }
            public decimal FloorArea { get; set; }
            [JsonProperty("buildDate", Order = 1)]
            public DateTime BuildDate { get; set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class House5
        {
            [JsonProperty("address", Order = 2)]
            public string StreetAddress { get; set; }
            public int Bedrooms { get; set; }
            public decimal FloorArea { get; set; }
            [JsonProperty("buildDate", Order = 1)]
            [JsonConverter(typeof(JavaScriptDateTimeConverter))]
            public DateTime BuildDate { get; set; }
        }

        [Test]
        public void SerializeAttributes()
        {
            var house = new House3();
            house.StreetAddress = "221B Baker Street";
            house.Bedrooms = 2;
            house.FloorArea = 100m;
            house.BuildDate = new DateTime(1890, 1, 1);

            string json = JsonConvert.SerializeObject(house, Formatting.Indented);
            // {
            //   "StreetAddress": "221B Baker Street",
            //   "Bedrooms": 2,
            //   "FloorArea": 100.0,
            //   "BuildDate": "1890-01-01T00:00:00"
            // }

            // {
            //   "StreetAddress": "221B Baker Street"
            // }

            // {
            //   "address": "221B Baker Street"
            // }

            // {
            //   "buildDate": "1890-01-01T00:00:00",
            //   "address": "221B Baker Street"
            // }

            // {
            //   "buildDate": new Date(-2524568400000),
            //   "address": "221B Baker Street"
            // }
        }

        [Test]
        public void MergeJson()
        {
            JObject o1 = JObject.Parse(@"{
              'FirstName': 'John',
              'LastName': 'Smith',
              'Enabled': false,
              'Roles': [ 'User' ]
            }");
            JObject o2 = JObject.Parse(@"{
              'Enabled': true,
              'Roles': [ 'User', 'Admin' ]
            }");

            o1.Merge(o2, new JsonMergeSettings
            {
                // union arrays together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            string json = o1.ToString();
            // {
            //   "FirstName": "John",
            //   "LastName": "Smith",
            //   "Enabled": true,
            //   "Roles": [
            //     "User",
            //     "Admin"
            //   ]
            // }

            Console.WriteLine(json);
        }
    }
}