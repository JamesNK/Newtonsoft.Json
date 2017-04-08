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
    public class DeserializeComparisonBenchmarks
    {
        private static readonly byte[] BinaryFormatterData = TestFixtureBase.HexToBytes(BenchmarkConstants.BinaryFormatterHex);
        private static readonly byte[] BsonData = TestFixtureBase.HexToBytes(BenchmarkConstants.BsonHex);

        [Benchmark]
        public TestClass DataContractSerializer()
        {
            return DeserializeDataContract<TestClass>(BenchmarkConstants.XmlText);
        }

        private T DeserializeDataContract<T>(string xml)
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));

            return (T)serializer.ReadObject(ms);
        }

        private T DeserializeDataContractJson<T>(string json)
        {
            DataContractJsonSerializer dataContractSerializer = new DataContractJsonSerializer(typeof(T));
            MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(json));

            return (T)dataContractSerializer.ReadObject(ms);
        }

#if (!DNXCORE50)
        [Benchmark]
        public TestClass BinaryFormatter()
        {
            return DeserializeBinaryFormatter<TestClass>(BinaryFormatterData);
        }

        private T DeserializeBinaryFormatter<T>(byte[] bytes)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(new MemoryStream(bytes));
        }

        [Benchmark]
        public TestClass JavaScriptSerializer()
        {
            return DeserializeWebExtensions<TestClass>(BenchmarkConstants.JsonText);
        }

        public T DeserializeWebExtensions<T>(string json)
        {
            JavaScriptSerializer ser = new JavaScriptSerializer { MaxJsonLength = int.MaxValue };

            return ser.Deserialize<T>(json);
        }
#endif

        [Benchmark]
        public TestClass DataContractJsonSerializer()
        {
            return DeserializeDataContractJson<TestClass>(BenchmarkConstants.JsonText);
        }

        [Benchmark]
        public TestClass JsonNet()
        {
            return JsonConvert.DeserializeObject<TestClass>(BenchmarkConstants.JsonText);
        }

        [Benchmark]
        public TestClass JsonNetIso()
        {
            return JsonConvert.DeserializeObject<TestClass>(BenchmarkConstants.JsonIsoText);
        }

        [Benchmark]
        public TestClass JsonNetManual()
        {
            return DeserializeJsonNetManual(BenchmarkConstants.JsonText);
        }

#region DeserializeJsonNetManual
        private TestClass DeserializeJsonNetManual(string json)
        {
            TestClass c = new TestClass();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.Read();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;
                    switch (propertyName)
                    {
                        case "strings":
                            reader.Read();
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                c.strings.Add((string)reader.Value);
                            }
                            break;
                        case "dictionary":
                            reader.Read();
                            while (reader.Read() && reader.TokenType != JsonToken.EndObject)
                            {
                                string key = (string)reader.Value;
                                c.dictionary.Add(key, reader.ReadAsInt32().GetValueOrDefault());
                            }
                            break;
                        case "Name":
                            c.Name = reader.ReadAsString();
                            break;
                        case "Now":
                            c.Now = reader.ReadAsDateTime().GetValueOrDefault();
                            break;
                        case "BigNumber":
                            c.BigNumber = reader.ReadAsDecimal().GetValueOrDefault();
                            break;
                        case "Address1":
                            reader.Read();
                            c.Address1 = CreateAddress(reader);
                            break;
                        case "Addresses":
                            reader.Read();
                            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                            {
                                var address = CreateAddress(reader);
                                c.Addresses.Add(address);
                            }
                            break;
                    }
                }
                else
                {
                    break;
                }
            }

            return c;
        }

        private static Address CreateAddress(JsonTextReader reader)
        {
            Address a = new Address();
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch ((string)reader.Value)
                    {
                        case "Street":
                            a.Street = reader.ReadAsString();
                            break;
                        case "Phone":
                            a.Phone = reader.ReadAsString();
                            break;
                        case "Entered":
                            a.Entered = reader.ReadAsDateTime().GetValueOrDefault();
                            break;
                    }
                }
                else
                {
                    break;
                }
            }
            return a;
        }
#endregion

        [Benchmark]
        public Task<TestClass> JsonNetManualAsync()
        {
            return DeserializeJsonNetManualAsync(BenchmarkConstants.JsonText);
        }

        [Benchmark]
        public Task<TestClass> JsonNetManualIndentedAsync()
        {
            return DeserializeJsonNetManualAsync(BenchmarkConstants.JsonIndentedText);
        }

        private async Task<TestClass> DeserializeJsonNetManualAsync(string json)
        {
            TestClass c = new TestClass();

            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            await reader.ReadAsync();
            while (await reader.ReadAsync())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propertyName = (string)reader.Value;
                    switch (propertyName)
                    {
                        case "strings":
                            await reader.ReadAsync();
                            while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndArray)
                            {
                                c.strings.Add((string)reader.Value);
                            }
                            break;
                        case "dictionary":
                            await reader.ReadAsync();
                            while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndObject)
                            {
                                string key = (string)reader.Value;
                                c.dictionary.Add(key, (await reader.ReadAsInt32Async()).GetValueOrDefault());
                            }
                            break;
                        case "Name":
                            c.Name = await reader.ReadAsStringAsync();
                            break;
                        case "Now":
                            c.Now = (await reader.ReadAsDateTimeAsync()).GetValueOrDefault();
                            break;
                        case "BigNumber":
                            c.BigNumber = (await reader.ReadAsDecimalAsync()).GetValueOrDefault();
                            break;
                        case "Address1":
                            await reader.ReadAsync();
                            c.Address1 = await CreateAddressAsync(reader);
                            break;
                        case "Addresses":
                            await reader.ReadAsync();
                            while (await reader.ReadAsync() && reader.TokenType != JsonToken.EndArray)
                            {
                                var address = await CreateAddressAsync(reader);
                                c.Addresses.Add(address);
                            }
                            break;
                    }
                }
                else
                {
                    break;
                }
            }

            return c;
        }

        private static async Task<Address> CreateAddressAsync(JsonTextReader reader)
        {
            Address a = new Address();
            while (await reader.ReadAsync())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    switch ((string)reader.Value)
                    {
                        case "Street":
                            a.Street = await reader.ReadAsStringAsync();
                            break;
                        case "Phone":
                            a.Phone = await reader.ReadAsStringAsync();
                            break;
                        case "Entered":
                            a.Entered = (await reader.ReadAsDateTimeAsync()).GetValueOrDefault();
                            break;
                    }
                }
                else
                {
                    break;
                }
            }
            return a;
        }

#pragma warning disable 618
        [Benchmark]
        public TestClass JsonNetBson()
        {
            return DeserializeJsonNetBson<TestClass>(BsonData);
        }

        private T DeserializeJsonNetBson<T>(byte[] bson)
        {
            JsonSerializer serializer = new JsonSerializer();
            return (T)serializer.Deserialize(new BsonReader(new MemoryStream(bson)), typeof(T));
        }
#pragma warning restore 618
    }
}

#endif