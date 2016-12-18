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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Converters;
using System.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using System.IO;
using System.Threading;
using Newtonsoft.Json.Linq;
using ErrorEventArgs = Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json.Tests.Serialization
{
    [TestFixture]
    public class SerializationErrorHandlingAsyncTests : TestFixtureBase
    {
        [Test]
        public async Task DeserializingErrorHandlingUsingEventAsync()
        {
            List<string> errors = new List<string>();

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Error = delegate(object sender, ErrorEventArgs args)
                {
                    errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                    args.ErrorContext.Handled = true;
                },
                Converters = { new IsoDateTimeConverter() }
            });
            var c = await serializer.DeserializeAsync<List<DateTime>>(new JsonTextReader(new StringReader(@"[
        ""2009-09-09T00:00:00Z"",
        ""I am not a date and will error!"",
        [
          1
        ],
        ""1977-02-20T00:00:00Z"",
        null,
        ""2000-12-01T00:00:00Z""
      ]")));

            // 2009-09-09T00:00:00Z
            // 1977-02-20T00:00:00Z
            // 2000-12-01T00:00:00Z

            // The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.
            // Unexpected token parsing date. Expected String, got StartArray.
            // Cannot convert null value to System.DateTime.

            Assert.AreEqual(3, c.Count);
            Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
            Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
            Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);

            Assert.AreEqual(3, errors.Count);
            var possibleErrs = new[]
            {
                "[1] - 1 - The string was not recognized as a valid DateTime. There is an unknown word starting at index 0.",
                "[1] - 1 - String was not recognized as a valid DateTime."
            };

            Assert.IsTrue(possibleErrs.Any(m => m == errors[0]),
                "Expected One of: " + string.Join(Environment.NewLine, possibleErrs) + Environment.NewLine + "But was: " + errors[0]);

            Assert.AreEqual("[2] - 2 - Unexpected token parsing date. Expected String, got StartArray. Path '[2]', line 4, position 9.", errors[1]);
            Assert.AreEqual("[4] - 4 - Cannot convert null value to System.DateTime. Path '[4]', line 8, position 12.", errors[2]);
        }

        [Test]
        public async Task DeserializeNestedUnhandledAsync()
        {
            List<string> errors = new List<string>();

            string json = @"[[""kjhkjhkjhkjh""]]";

            Exception e = null;
            try
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Error += delegate(object sender, ErrorEventArgs args)
                {
                    // only log an error once
                    if (args.CurrentObject == args.ErrorContext.OriginalObject)
                    {
                        errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                    }
                };

                await serializer.DeserializeAsync(new StringReader(json), typeof(List<List<DateTime>>));
            }
            catch (Exception ex)
            {
                e = ex;
            }

            Assert.AreEqual(@"Could not convert string to DateTime: kjhkjhkjhkjh. Path '[0][0]', line 1, position 16.", e.Message);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual(@"[0][0] - 0 - Could not convert string to DateTime: kjhkjhkjhkjh. Path '[0][0]', line 1, position 16.", errors[0]);
        }

        [Test]
        public async Task MultipleRequiredPropertyErrorsAsync()
        {
            string json = "{}";
            List<string> errors = new List<string>();
            JsonSerializer serializer = new JsonSerializer();
            serializer.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            serializer.Error += delegate(object sender, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
            await serializer.DeserializeAsync(new JsonTextReader(new StringReader(json)), typeof(MyTypeWithRequiredMembers));

            Assert.AreEqual(2, errors.Count);
            Assert.IsTrue(errors[0].StartsWith(" - Required1 - Required property 'Required1' not found in JSON. Path '', line 1, position 2."));
            Assert.IsTrue(errors[1].StartsWith(" - Required2 - Required property 'Required2' not found in JSON. Path '', line 1, position 2."));
        }

        [Test]
        public async Task HandlingArrayErrorsAsync()
        {
            string json = "[\"a\",\"b\",\"45\",34]";

            List<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += delegate(object sender, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };

            await serializer.DeserializeAsync(new JsonTextReader(new StringReader(json)), typeof(int[]));

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual("[0] - 0 - Could not convert string to integer: a. Path '[0]', line 1, position 4.", errors[0]);
            Assert.AreEqual("[1] - 1 - Could not convert string to integer: b. Path '[1]', line 1, position 8.", errors[1]);
        }

        [Test]
        public async Task HandlingMultidimensionalArrayErrorsAsync()
        {
            string json = "[[\"a\",\"45\"],[\"b\",34]]";

            List<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += delegate(object sender, ErrorEventArgs args)
            {
                errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };

            await serializer.DeserializeAsync(new JsonTextReader(new StringReader(json)), typeof(int[,]));

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual("[0][0] - 0 - Could not convert string to integer: a. Path '[0][0]', line 1, position 5.", errors[0]);
            Assert.AreEqual("[1][0] - 0 - Could not convert string to integer: b. Path '[1][0]', line 1, position 16.", errors[1]);
        }

        [Test]
        public async Task ErrorHandlingAndAvoidingRecursiveDepthErrorAsync()
        {
            string json = "{'A':{'A':{'A':{'A':{'A':{}}}}}}";
            JsonSerializer serializer = new JsonSerializer();
            IList<string> errors = new List<string>();
            serializer.Error += (sender, e) =>
            {
                e.ErrorContext.Handled = true;
                errors.Add(e.ErrorContext.Path);
            };

            await serializer.DeserializeAsync<Nest>(new JsonTextReader(new StringReader(json)) { MaxDepth = 3 });

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("A.A.A", errors[0]);
        }

        public class Nest
        {
            public Nest A { get; set; }
        }

        [Test]
        public async Task InfiniteErrorHandlingLoopFromInputErrorAsync()
        {
            IList<string> errors = new List<string>();

            JsonSerializer serializer = new JsonSerializer();
            serializer.Error += (sender, e) =>
            {
                errors.Add(e.ErrorContext.Error.Message);
                e.ErrorContext.Handled = true;
            };

            ErrorPerson[] result = await serializer.DeserializeAsync<ErrorPerson[]>(new JsonTextReader(new ThrowingReader()));

            Assert.IsNull(result);
            Assert.AreEqual(3, errors.Count);
            Assert.AreEqual("too far", errors[0]);
            Assert.AreEqual("too far", errors[1]);
            Assert.AreEqual("Infinite loop detected from error handling. Path '[1023]', line 1, position 65536.", errors[2]);
        }

        [Test]
        public async Task ArrayHandling_JTokenReaderAsync()
        {
            IList<string> errors = new List<string>();

            JTokenReader reader = new JTokenReader(new JArray(0, true));

            JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                Error = (sender, arg) =>
                {
                    errors.Add(arg.ErrorContext.Error.Message);
                    arg.ErrorContext.Handled = true;
                }
            });
            object o = await serializer.DeserializeAsync(reader, typeof(int[]));

            Assert.IsNotNull(o);

            Assert.AreEqual(1, errors.Count);
            Assert.AreEqual("Error reading integer. Unexpected token: Boolean. Path '[1]'.", errors[0]);

            Assert.AreEqual(1, ((int[])o).Length);
            Assert.AreEqual(0, ((int[])o)[0]);
        }

        [Test]
        public async Task ErrorHandlingEndOfContentAsync()
        {
            IList<string> errors = new List<string>();

            const string input = "{\"events\":[{\"code\":64411},{\"code\":64411,\"prio";

            const int maxDepth = 256;
            using (var jsonTextReader = new JsonTextReader(new StringReader(input)) { MaxDepth = maxDepth })
            {
                JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings
                {
                    MaxDepth = maxDepth,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });
                jsonSerializer.Error += (sender, e) =>
                {
                    errors.Add(e.ErrorContext.Error.Message);
                    e.ErrorContext.Handled = true;
                };

                LogMessage logMessage = await jsonSerializer.DeserializeAsync<LogMessage>(jsonTextReader);

                Assert.IsNotNull(logMessage.Events);
                Assert.AreEqual(1, logMessage.Events.Count);
                Assert.AreEqual("64411", logMessage.Events[0].Code);
            }

            Assert.AreEqual(3, errors.Count);
            Assert.AreEqual(@"Unterminated string. Expected delimiter: "". Path 'events[1].code', line 1, position 45.", errors[0]);
            Assert.AreEqual(@"Unexpected end when deserializing array. Path 'events[1].code', line 1, position 45.", errors[1]);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events[1].code', line 1, position 45.", errors[2]);
        }

        [Test]
        public async Task ErrorHandlingEndOfContentDictionaryAsync()
        {
            IList<string> errors = new List<string>();

            const string input = "{\"events\":{\"code\":64411},\"events2\":{\"code\":64412,";

            const int maxDepth = 256;
            using (var jsonTextReader = new JsonTextReader(new StringReader(input)) { MaxDepth = maxDepth })
            {
                JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings { MaxDepth = maxDepth, MetadataPropertyHandling = MetadataPropertyHandling.Default });
                jsonSerializer.Error += (sender, e) =>
                {
                    errors.Add(e.ErrorContext.Error.Message);
                    e.ErrorContext.Handled = true;
                };

                IDictionary<string, LogEvent> logEvents = await jsonSerializer.DeserializeAsync<IDictionary<string, LogEvent>>(jsonTextReader);

                Assert.IsNotNull(logEvents);
                Assert.AreEqual(2, logEvents.Count);
                Assert.AreEqual("64411", logEvents["events"].Code);
                Assert.AreEqual("64412", logEvents["events2"].Code);
            }

            Assert.AreEqual(2, errors.Count);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[0]);
            Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[1]);
        }

        [Test]
        public async Task NoObjectWithEventAsync()
        {
            string json = "{\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            JsonTextReader jReader = new JsonTextReader(new StreamReader(stream));
            JsonSerializer s = new JsonSerializer();
            s.Error += (sender, args) => { args.ErrorContext.Handled = true; };
            ErrorPerson2 obj = await s.DeserializeAsync<ErrorPerson2>(jReader);

            Assert.IsNull(obj);
        }

        [Test]
        public async Task NoObjectWithAttributeAsync()
        {
            string json = "{\"}";
            byte[] byteArray = Encoding.UTF8.GetBytes(json);
            MemoryStream stream = new MemoryStream(byteArray);
            JsonTextReader jReader = new JsonTextReader(new StreamReader(stream));
            JsonSerializer s = new JsonSerializer();

            await ExceptionAssert.ThrowsAsync<JsonReaderException>(async () => { await s.DeserializeAsync<ErrorTestObject>(jReader); }, @"Unterminated string. Expected delimiter: "". Path '', line 1, position 3.");
        }

        public class RootThing
        {
            public Something Something { get; set; }
        }

        public class RootSomethingElse
        {
            public SomethingElse SomethingElse { get; set; }
        }

        /// <summary>
        /// This could be an object we are passing up in an interface.
        /// </summary>
        [JsonConverter(typeof(SomethingConverter))]
        public class Something
        {
            public class SomethingConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return true;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    Assert.Fail("Only async form should be called.");
                    return null;
                }

                public override async Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
                {
                    try
                    {
                        // Do own stuff.
                        // Then call deserialise for inner object.
                        await serializer.DeserializeAsync(reader, typeof(SomethingElse));

                        return null;
                    }
                    catch (Exception ex)
                    {
                        // If we get an error wrap it in something less scary.
                        throw new Exception("An error occurred.", ex);
                    }
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    Assert.Fail("Only async form should be called.");
                }

                public override async Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
                {
                    try
                    {
                        Something s = (Something)value;

                        // Do own stuff.
                        // Then call serialise for inner object.
                        await serializer.SerializeAsync(writer, s.RootSomethingElse);
                    }
                    catch (Exception ex)
                    {
                        // If we get an error wrap it in something less scary.
                        throw new Exception("An error occurred.", ex);
                    }
                }
            }

            public RootSomethingElse RootSomethingElse { get; set; }

            public Something()
            {
                RootSomethingElse = new RootSomethingElse();
            }
        }

        /// <summary>
        /// This is an object that is contained in the interface object.
        /// </summary>
        [JsonConverter(typeof(SomethingElseConverter))]
        public class SomethingElse
        {
            public class SomethingElseConverter : JsonConverter
            {
                public override bool CanConvert(Type objectType)
                {
                    return true;
                }

                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    throw new NotImplementedException();
                }
            }
        }

        [Test]
        public async Task DeserializeWrappingErrorsAndErrorHandlingAsync()
        {
            var serialiser = JsonSerializer.Create(new JsonSerializerSettings());

            string foo = "{ something: { rootSomethingElse { somethingElse: 0 } } }";
            var reader = new StringReader(foo);

            await ExceptionAssert.ThrowsAsync<Exception>(async () => { await serialiser.DeserializeAsync(reader, typeof(Something)); }, "An error occurred.");
        }

        [Test]
        public async Task SerializeWrappingErrorsAndErrorHandlingAsync()
        {
            var serialiser = JsonSerializer.Create(new JsonSerializerSettings());

            Something s = new Something
            {
                RootSomethingElse = new RootSomethingElse
                {
                    SomethingElse = new SomethingElse()
                }
            };
            RootThing r = new RootThing
            {
                Something = s
            };

            var writer = new StringWriter();

            await ExceptionAssert.ThrowsAsync<Exception>(async () => { await serialiser.SerializeAsync(writer, r); }, "An error occurred.");
        }

        [Test]
        public async Task IntegerToLarge_ReadNextValueAsync()
        {
            IList<string> errorMessages = new List<string>();

            JsonReader reader = new JsonTextReader(new StringReader(@"{
  ""string1"": ""blah"",
  ""int1"": 2147483648,
  ""string2"": ""also blah"",
  ""int2"": 2147483648,
  ""string3"": ""more blah"",
  ""dateTime1"": ""200NOTDATE"",
  ""string4"": ""even more blah""
}"));
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Error = (sender, args) =>
            {
                errorMessages.Add(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
            };
            JsonSerializer serializer = JsonSerializer.Create(settings);

            DataModel data = new DataModel();
            await serializer.PopulateAsync(reader, data);

            Assert.AreEqual("blah", data.String1);
            Assert.AreEqual(0, data.Int1);
            Assert.AreEqual("also blah", data.String2);
            Assert.AreEqual(0, data.Int2);
            Assert.AreEqual("more blah", data.String3);
            Assert.AreEqual(default(DateTime), data.DateTime1);
            Assert.AreEqual("even more blah", data.String4);

            //Assert.AreEqual(2, errorMessages.Count);
            Assert.AreEqual("JSON integer 2147483648 is too large or small for an Int32. Path 'int1', line 3, position 20.", errorMessages[0]);
            Assert.AreEqual("JSON integer 2147483648 is too large or small for an Int32. Path 'int2', line 5, position 20.", errorMessages[1]);
            Assert.AreEqual("Could not convert string to DateTime: 200NOTDATE. Path 'dateTime1', line 7, position 27.", errorMessages[2]);
        }

        private class DataModel
        {
            public string String1 { get; set; }
            public int Int1 { get; set; }
            public string String2 { get; set; }
            public int Int2 { get; set; }
            public string String3 { get; set; }
            public DateTime DateTime1 { get; set; }
            public string String4 { get; set; }
        }
    }
}

#endif