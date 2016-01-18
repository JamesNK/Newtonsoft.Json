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

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    [TestFixture]
    public class CustomJsonConverter : TestFixtureBase
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

        [Test]
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

            Assert.AreEqual("James", newEmployee.FirstName);
        }
    }
}