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
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Tests.Documentation.Samples.Serializer
{
    public class CustomContractResolver
    {
        #region Types
        public class DynamicContractResolver : DefaultContractResolver
        {
            private readonly char _startingWithChar;

            public DynamicContractResolver(char startingWithChar)
            {
                _startingWithChar = startingWithChar;
            }

            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> properties = base.CreateProperties(type, memberSerialization);

                // only serializer properties that start with the specified character
                properties =
                    properties.Where(p => p.PropertyName.StartsWith(_startingWithChar.ToString())).ToList();

                return properties;
            }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }

            public string FullName
            {
                get { return FirstName + " " + LastName; }
            }
        }
        #endregion

        public void Example()
        {
            #region Usage
            Person person = new Person
            {
                FirstName = "Dennis",
                LastName = "Deepwater-Diver"
            };

            string startingWithF = JsonConvert.SerializeObject(person, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('F') });

            Console.WriteLine(startingWithF);
            // {
            //   "FirstName": "Dennis",
            //   "FullName": "Dennis Deepwater-Diver"
            // }

            string startingWithL = JsonConvert.SerializeObject(person, Formatting.Indented,
                new JsonSerializerSettings { ContractResolver = new DynamicContractResolver('L') });

            Console.WriteLine(startingWithL);
            // {
            //   "LastName": "Deepwater-Diver"
            // }
            #endregion
        }
    }
}