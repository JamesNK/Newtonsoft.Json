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

#if HAVE_BENCHMARKS

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
#if (!DNXCORE50)
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Script.Serialization;
#endif
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Benchmarks
{
    public class SerializeComparisonBenchmarks
    {
        private static readonly TestClass TestClass = CreateSerializationObject();

        private static TestClass CreateSerializationObject()
        {
            TestClass test = new TestClass();

            test.dictionary = new Dictionary<string, int> { { "Val & asd1", 1 }, { "Val2 & asd1", 3 }, { "Val3 & asd1", 4 } };

            test.Address1.Street = "fff Street";
            test.Address1.Entered = DateTime.Now.AddDays(20);

            test.BigNumber = 34123123123.121M;
            test.Now = DateTime.Now.AddHours(1);
            test.strings = new List<string>() { null, "Markus egger ]><[, (2nd)", null };

            Address address = new Address();
            address.Entered = DateTime.Now.AddDays(-1);
            address.Street = "\u001farray\u003caddress";

            test.Addresses.Add(address);

            address = new Address();
            address.Entered = DateTime.Now.AddDays(-2);
            address.Street = "array 2 address";
            test.Addresses.Add(address);
            return test;
        }

        [Benchmark]
        public string DataContractSerializer()
        {
            return SerializeDataContract(TestClass);
        }

        private string SerializeDataContract(object value)
        {
            DataContractSerializer dataContractSerializer = new DataContractSerializer(value.GetType());

            MemoryStream ms = new MemoryStream();
            dataContractSerializer.WriteObject(ms, value);

            ms.Seek(0, SeekOrigin.Begin);

            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }

#if (!DNXCORE50)
        [Benchmark]
        public byte[] BinaryFormatter()
        {
            return SerializeBinaryFormatter(TestClass);
        }

        private byte[] SerializeBinaryFormatter(object value)
        {
            MemoryStream ms = new MemoryStream(Buffer);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(ms, value);

            return ms.ToArray();
        }

        [Benchmark]
        public string JavaScriptSerializer()
        {
            return SerializeWebExtensions(TestClass);
        }

        private string SerializeWebExtensions(object value)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer();

            return ser.Serialize(value);
        }
#endif

        [Benchmark]
        public string DataContractJsonSerializer()
        {
            return SerializeDataContractJson(TestClass);
        }

        public string SerializeDataContractJson(object value)
        {
            DataContractJsonSerializer dataContractSerializer = new DataContractJsonSerializer(value.GetType());

            MemoryStream ms = new MemoryStream();
            dataContractSerializer.WriteObject(ms, value);

            ms.Seek(0, SeekOrigin.Begin);

            using (StreamReader sr = new StreamReader(ms))
            {
                return sr.ReadToEnd();
            }
        }

        [Benchmark]
        public string JsonNet()
        {
            return JsonConvert.SerializeObject(TestClass);
        }

        [Benchmark]
        public string JsonNetLinq()
        {
            return SerializeJsonNetLinq(TestClass);
        }

#region SerializeJsonNetManual
        private string SerializeJsonNetLinq(TestClass c)
        {
            JObject o = new JObject(
                new JProperty("strings", new JArray(c.strings)),
                new JProperty("dictionary", new JObject(c.dictionary.Select(d => new JProperty(d.Key, d.Value)))),
                new JProperty("Name", c.Name),
                new JProperty("Now", c.Now),
                new JProperty("BigNumber", c.BigNumber),
                new JProperty("Address1", new JObject(
                    new JProperty("Street", c.Address1.Street),
                    new JProperty("Phone", c.Address1.Phone),
                    new JProperty("Entered", c.Address1.Entered))),
                new JProperty("Addresses", new JArray(c.Addresses.Select(a =>
                    new JObject(
                        new JProperty("Street", a.Street),
                        new JProperty("Phone", a.Phone),
                        new JProperty("Entered", a.Entered)))))
                );

            return o.ToString(Formatting.None);
        }
#endregion

        [Benchmark]
        public string JsonNetManual()
        {
            return SerializeJsonNetManual(TestClass);
        }

#region SerializeJsonNetManual
        private string SerializeJsonNetManual(TestClass c)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("strings");
            writer.WriteStartArray();
            foreach (string s in c.strings)
            {
                writer.WriteValue(s);
            }
            writer.WriteEndArray();
            writer.WritePropertyName("dictionary");
            writer.WriteStartObject();
            foreach (KeyValuePair<string, int> keyValuePair in c.dictionary)
            {
                writer.WritePropertyName(keyValuePair.Key);
                writer.WriteValue(keyValuePair.Value);
            }
            writer.WriteEndObject();
            writer.WritePropertyName("Name");
            writer.WriteValue(c.Name);
            writer.WritePropertyName("Now");
            writer.WriteValue(c.Now);
            writer.WritePropertyName("BigNumber");
            writer.WriteValue(c.BigNumber);
            writer.WritePropertyName("Address1");
            writer.WriteStartObject();
            writer.WritePropertyName("Street");
            writer.WriteValue(c.BigNumber);
            writer.WritePropertyName("Street");
            writer.WriteValue(c.BigNumber);
            writer.WritePropertyName("Street");
            writer.WriteValue(c.BigNumber);
            writer.WriteEndObject();
            writer.WritePropertyName("Addresses");
            writer.WriteStartArray();
            foreach (Address address in c.Addresses)
            {
                writer.WriteStartObject();
                writer.WritePropertyName("Street");
                writer.WriteValue(address.Street);
                writer.WritePropertyName("Phone");
                writer.WriteValue(address.Phone);
                writer.WritePropertyName("Entered");
                writer.WriteValue(address.Entered);
                writer.WriteEndObject();
            }
            writer.WriteEndArray();
            writer.WriteEndObject();

            writer.Flush();
            return sw.ToString();
        }
#endregion

        [Benchmark]
        public Task<string> JsonNetManualAsync()
        {
            return SerializeJsonNetManualAsync(TestClass, Formatting.None);
        }

        [Benchmark]
        public Task<string> JsonNetManualIndentedAsync()
        {
            return SerializeJsonNetManualAsync(TestClass, Formatting.Indented);
        }

        private async Task<string> SerializeJsonNetManualAsync(TestClass c, Formatting formatting)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.Formatting = formatting;

            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("strings");
            await writer.WriteStartArrayAsync();
            foreach (string s in c.strings)
            {
                await writer.WriteValueAsync(s);
            }
            await writer.WriteEndArrayAsync();
            await writer.WritePropertyNameAsync("dictionary");
            await writer.WriteStartObjectAsync();
            foreach (KeyValuePair<string, int> keyValuePair in c.dictionary)
            {
                await writer.WritePropertyNameAsync(keyValuePair.Key);
                await writer.WriteValueAsync(keyValuePair.Value);
            }
            await writer.WriteEndObjectAsync();
            await writer.WritePropertyNameAsync("Name");
            await writer.WriteValueAsync(c.Name);
            await writer.WritePropertyNameAsync("Now");
            await writer.WriteValueAsync(c.Now);
            await writer.WritePropertyNameAsync("BigNumber");
            await writer.WriteValueAsync(c.BigNumber);
            await writer.WritePropertyNameAsync("Address1");
            await writer.WriteStartObjectAsync();
            await writer.WritePropertyNameAsync("Street");
            await writer.WriteValueAsync(c.BigNumber);
            await writer.WritePropertyNameAsync("Street");
            await writer.WriteValueAsync(c.BigNumber);
            await writer.WritePropertyNameAsync("Street");
            await writer.WriteValueAsync(c.BigNumber);
            await writer.WriteEndObjectAsync();
            await writer.WritePropertyNameAsync("Addresses");
            await writer.WriteStartArrayAsync();
            foreach (Address address in c.Addresses)
            {
                await writer.WriteStartObjectAsync();
                await writer.WritePropertyNameAsync("Street");
                await writer.WriteValueAsync(address.Street);
                await writer.WritePropertyNameAsync("Phone");
                await writer.WriteValueAsync(address.Phone);
                await writer.WritePropertyNameAsync("Entered");
                await writer.WriteValueAsync(address.Entered);
                await writer.WriteEndObjectAsync();
            }
            await writer.WriteEndArrayAsync();
            await writer.WriteEndObjectAsync();

            await writer.FlushAsync();
            return sw.ToString();
        }

#pragma warning disable 618
        [Benchmark]
        public byte[] JsonNetBson()
        {
            return SerializeJsonNetBson(TestClass);
        }

        private static readonly byte[] Buffer = new byte[4096];

        private byte[] SerializeJsonNetBson(TestClass c)
        {
            MemoryStream ms = new MemoryStream(Buffer);
            JsonSerializer serializer = new JsonSerializer();
            BsonWriter writer = new BsonWriter(ms);
            serializer.Serialize(writer, c);

            return ms.ToArray();
        }
#pragma warning restore 618
    }
}

#endif