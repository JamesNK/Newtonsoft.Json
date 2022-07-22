﻿#region License
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

#if !(NET40 || NET35 || NET20 || PORTABLE || DNXCORE50) || NETSTANDARD1_3 || NETSTANDARD2_0 || NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Tests.Documentation
{
    #region JsonConverterAttribute
    [JsonConverter(typeof(PersonConverter))]
    public class Person
    {
        public Person()
        {
            Likes = new List<string>();
        }

        public string Name { get; set; }
        public IList<string> Likes { get; private set; }
    }
    #endregion

    #region JsonConverterContractResolver
    public class ConverterContractResolver : DefaultContractResolver
    {
        public new static readonly ConverterContractResolver Instance = new ConverterContractResolver();

        protected override JsonContract CreateContract(Type objectType)
        {
            JsonContract contract = base.CreateContract(objectType);

            // this will only be called once and then cached
            if (objectType == typeof(DateTime) || objectType == typeof(DateTimeOffset))
            {
                contract.Converter = new JavaScriptDateTimeConverter();
            }

            return contract;
        }
    }
    #endregion

    public class PersonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject o = (JObject)JToken.ReadFrom(reader);

            Person p = new Person
            {
                Name = (string)o["Name"]
            };

            return p;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(Person));
        }
    }

    [TestFixture]
    public class PerformanceTests : TestFixtureBase
    {
        private static class AppSettings
        {
            public static readonly IContractResolver SnakeCaseContractResolver = new DefaultContractResolver();
        }

        [Test]
        public void ReuseContractResolverTest()
        {
            Person person = new Person();

            #region ReuseContractResolver
            // BAD - a new contract resolver is created each time, forcing slow reflection to be used
            string json1 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });

            // GOOD - reuse the contract resolver from a shared location
            string json2 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = AppSettings.SnakeCaseContractResolver
            });

            // GOOD - an internal contract resolver is used
            string json3 = JsonConvert.SerializeObject(person, new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            });
            #endregion
        }

        [Test]
        public void ConverterContractResolverTest()
        {
            string json = JsonConvert.SerializeObject(new DateTime(2000, 10, 10, 10, 10, 10, DateTimeKind.Utc), new JsonSerializerSettings
            {
                ContractResolver = ConverterContractResolver.Instance
            });

            Console.WriteLine(json);
        }

        public class HttpClient
        {
            public Task<Stream> GetStreamAsync(string url)
            {
                return Task.FromResult<Stream>(new MemoryStream());
            }

            public Task<string> GetStringAsync(string url)
            {
                return Task.FromResult("{}");
            }
        }

        [Test]
        public void DeserializeString()
        {
            #region DeserializeString
            HttpClient client = new HttpClient();

            // read the json into a string
            // string could potentially be very large and cause memory problems
            string json = client.GetStringAsync("http://www.test.com/large.json").Result;

            Person p = JsonConvert.DeserializeObject<Person>(json);
            #endregion
        }

        [Test]
        public void DeserializeStream()
        {
            #region DeserializeStream
            HttpClient client = new HttpClient();

            using (Stream s = client.GetStreamAsync("http://www.test.com/large.json").Result)
            using (StreamReader sr = new StreamReader(s))
            using (JsonReader reader = new JsonTextReader(sr))
            {
                JsonSerializer serializer = new JsonSerializer();

                // read the json from a stream
                // json size doesn't matter because only a small piece is read at a time from the HTTP request
                Person p = serializer.Deserialize<Person>(reader);
            }
            #endregion
        }
    }

    public static class PersonWriter
    {
        #region ReaderWriter
        public static string ToJson(this Person p)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            // {
            writer.WriteStartObject();

            // "name" : "Jerry"
            writer.WritePropertyName("name");
            writer.WriteValue(p.Name);

            // "likes": ["Comedy", "Superman"]
            writer.WritePropertyName("likes");
            writer.WriteStartArray();
            foreach (string like in p.Likes)
            {
                writer.WriteValue(like);
            }
            writer.WriteEndArray();

            // }
            writer.WriteEndObject();

            return sw.ToString();
        }
        #endregion

        public static Person ToPerson(this string s)
        {
            StringReader sr = new StringReader(s);
            JsonTextReader reader = new JsonTextReader(sr);

            Person p = new Person();

            // {
            reader.Read();
            // "name"
            reader.Read();
            // "Jerry"
            p.Name = reader.ReadAsString();
            // "likes"
            reader.Read();
            // [
            reader.Read();
            // "Comedy", "Superman", ]
            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                p.Likes.Add((string)reader.Value);
            }
            // }
            reader.Read();

            return p;
        }
    }
}

#endif