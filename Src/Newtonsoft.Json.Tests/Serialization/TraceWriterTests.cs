using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Newtonsoft.Json.Linq;
#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
using System.Numerics;
#endif
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.Organization;
using Newtonsoft.Json.Utilities;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif
#if !(NET20 || NET35 || NET40 || PORTABLE40 || PORTABLE) || DNXCORE50
using System.Threading.Tasks;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{ 
    public class Staff
    {
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public IList<string> Roles { get; set; }
    }

    public class RoleTrace
    {
        public string Name { get; set; }
    }

    [TestFixture]
    public class TraceWriterTests : TestFixtureBase
    {
        [Test]
        public void DeserializedJsonWithAlreadyReadReader()
        {
            string json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";
            IList<RoleTrace> roles = new List<RoleTrace>();
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.SupportMultipleContent = true;
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter();
            while (true)
            {
                if (!reader.Read())
                {
                    break;
                }
                JsonSerializer serializer = new JsonSerializer();
                //the next line raise an exception
                serializer.TraceWriter = traceWriter;
                RoleTrace role = serializer.Deserialize<RoleTrace>(reader);
                roles.Add(role);
            }

            Assert.AreEqual("Admin", roles[0].Name);
            Assert.AreEqual("Publisher", roles[1].Name);

            StringAssert.AreEqual(@"Deserialized JSON: 
{
  ""name"": ""Admin""
}", traceWriter.TraceRecords[2].Message);

            StringAssert.AreEqual(@"Deserialized JSON: 
{
  ""name"": ""Publisher""
}", traceWriter.TraceRecords[5].Message);
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40 || PORTABLE) || DNXCORE50
        [Test]
        public async Task DeserializedJsonWithAlreadyReadReader_Async()
        {
            string json = @"{ 'name': 'Admin' }{ 'name': 'Publisher' }";
            IList<RoleTrace> roles = new List<RoleTrace>();
            JsonTextReader reader = new JsonTextReader(new StringReader(json));
            reader.SupportMultipleContent = true;
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter();
            while (true)
            {
                if (!await reader.ReadAsync())
                {
                    break;
                }
                JsonSerializer serializer = new JsonSerializer();
                //the next line raise an exception
                serializer.TraceWriter = traceWriter;
                RoleTrace role = serializer.Deserialize<RoleTrace>(reader);
                roles.Add(role);
            }

            Assert.AreEqual("Admin", roles[0].Name);
            Assert.AreEqual("Publisher", roles[1].Name);

            StringAssert.AreEqual(@"Deserialized JSON: 
{
  ""name"": ""Admin""
}", traceWriter.TraceRecords[2].Message);

            StringAssert.AreEqual(@"Deserialized JSON: 
{
  ""name"": ""Publisher""
}", traceWriter.TraceRecords[5].Message);
        }
#endif

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD2_0
        [Test]
        public void DiagnosticsTraceWriterTest()
        {
            StringWriter sw = new StringWriter();
            TextWriterTraceListener listener = new TextWriterTraceListener(sw);

            try
            {
                Trace.AutoFlush = true;
                Trace.Listeners.Add(listener);

                DiagnosticsTraceWriter traceWriter = new DiagnosticsTraceWriter();
                traceWriter.Trace(TraceLevel.Verbose, "Verbose!", null);
                traceWriter.Trace(TraceLevel.Info, "Info!", null);
                traceWriter.Trace(TraceLevel.Warning, "Warning!", null);
                traceWriter.Trace(TraceLevel.Error, "Error!", null);
                traceWriter.Trace(TraceLevel.Off, "Off!", null);

                StringAssert.AreEqual(@"Newtonsoft.Json Verbose: 0 : Verbose!
Newtonsoft.Json Information: 0 : Info!
Newtonsoft.Json Warning: 0 : Warning!
Newtonsoft.Json Error: 0 : Error!
", sw.ToString());
            }
            finally
            {
                Trace.Listeners.Remove(listener);
                Trace.AutoFlush = false;
            }
        }
#endif

        [Test]
        public void WriteNullableByte()
        {
            StringWriter sw = new StringWriter();
            TraceJsonWriter traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(sw));
            traceJsonWriter.WriteStartArray();
            traceJsonWriter.WriteValue((byte?)null);
            traceJsonWriter.WriteEndArray();

            StringAssert.AreEqual(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
        }

        [Test]
        public void WriteNullObject()
        {
            StringWriter sw = new StringWriter();
            TraceJsonWriter traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(sw));
            traceJsonWriter.WriteStartArray();
            traceJsonWriter.WriteValue((object)null);
            traceJsonWriter.WriteEndArray();

            StringAssert.AreEqual(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
        }

        [Test]
        public void WriteNullString()
        {
            StringWriter sw = new StringWriter();
            TraceJsonWriter traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(sw));
            traceJsonWriter.WriteStartArray();
            traceJsonWriter.WriteValue((string)null);
            traceJsonWriter.WriteEndArray();

            StringAssert.AreEqual(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
        }

        [Test]
        public void WriteNullUri()
        {
            StringWriter sw = new StringWriter();
            TraceJsonWriter traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(sw));
            traceJsonWriter.WriteStartArray();
            traceJsonWriter.WriteValue((Uri)null);
            traceJsonWriter.WriteEndArray();

            StringAssert.AreEqual(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
        }

        [Test]
        public void WriteNullByteArray()
        {
            StringWriter sw = new StringWriter();
            TraceJsonWriter traceJsonWriter = new TraceJsonWriter(new JsonTextWriter(sw));
            traceJsonWriter.WriteStartArray();
            traceJsonWriter.WriteValue((byte[])null);
            traceJsonWriter.WriteEndArray();

            StringAssert.AreEqual(@"Serialized JSON: 
[
  null
]", traceJsonWriter.GetSerializedJsonMessage());
        }

        [Test]
        public void WriteJRaw()
        {
            ITraceWriter traceWriter = new MemoryTraceWriter();

            JRaw settings = new JRaw("$('#element')");
            string json = JsonConvert.SerializeObject(settings, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

            Assert.AreEqual("$('#element')", json);

            Assert.IsTrue(traceWriter.ToString().EndsWith("Verbose Serialized JSON: " + Environment.NewLine + "$('#element')", StringComparison.Ordinal));
        }

        [Test]
        public void WriteJRawInArray()
        {
            ITraceWriter traceWriter = new MemoryTraceWriter();

            List<JRaw> raws = new List<JRaw>
            {
                new JRaw("$('#element')"),
                new JRaw("$('#element')"),
                new JRaw("$('#element')")
            };

            string json = JsonConvert.SerializeObject(raws, new JsonSerializerSettings
            {
                TraceWriter = traceWriter,
                Formatting = Formatting.Indented
            });

            StringAssert.AreEqual(@"[
  $('#element'),
  $('#element'),
  $('#element')
]", json);

            Assert.IsTrue(traceWriter.ToString().EndsWith(@"Verbose Serialized JSON: 
[
  $('#element'),
  $('#element'),
  $('#element')
]", StringComparison.Ordinal));
        }

        [Test]
        public void MemoryTraceWriterSerializeTest()
        {
            Staff staff = new Staff();
            staff.Name = "Arnie Admin";
            staff.Roles = new List<string> { "Administrator" };
            staff.StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            JsonConvert.SerializeObject(
                staff,
                new JsonSerializerSettings { TraceWriter = traceWriter, Converters = { new JavaScriptDateTimeConverter() } });

            // 2012-11-11T12:08:42.761 Info Started serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.
            // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.799 Info Finished serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.

            MemoryTraceWriter memoryTraceWriter = (MemoryTraceWriter)traceWriter;
            string output = memoryTraceWriter.ToString();

            Assert.AreEqual(916, output.Length);
            Assert.AreEqual(7, memoryTraceWriter.GetTraceMessages().Count());

            string json = @"Serialized JSON: 
{
  ""Name"": ""Arnie Admin"",
  ""StartDate"": new Date(
    976623132000
  ),
  ""Roles"": [
    ""Administrator""
  ]
}";

            json = StringAssert.Normalize(json);
            output = StringAssert.Normalize(output);

            Assert.IsTrue(output.Contains(json));
        }

        [Test]
        public void MemoryTraceWriterDeserializeTest()
        {
            string json = @"{
  ""Name"": ""Arnie Admin"",
  ""StartDate"": new Date(
    976623132000
  ),
  ""Roles"": [
    ""Administrator""
  ]
}";

            Staff staff = new Staff();
            staff.Name = "Arnie Admin";
            staff.Roles = new List<string> { "Administrator" };
            staff.StartDate = new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc);

            ITraceWriter traceWriter = new MemoryTraceWriter();

            JsonConvert.DeserializeObject<Staff>(
                json,
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter,
                    Converters = { new JavaScriptDateTimeConverter() },
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });

            // 2012-11-11T12:08:42.761 Info Started serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.
            // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
            // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
            // 2012-11-11T12:08:42.799 Info Finished serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.
            // 2013-05-19T00:07:24.360 Verbose Deserialized JSON: 
            // {
            //   "Name": "Arnie Admin",
            //   "StartDate": new Date(
            //     976623132000
            //   ),
            //   "Roles": [
            //     "Administrator"
            //   ]
            // }

            MemoryTraceWriter memoryTraceWriter = (MemoryTraceWriter)traceWriter;
            string output = memoryTraceWriter.ToString();

            Assert.AreEqual(1058, output.Length);
            Assert.AreEqual(7, memoryTraceWriter.GetTraceMessages().Count());

            json = StringAssert.Normalize(json);
            output = StringAssert.Normalize(output);

            Assert.IsTrue(output.Contains(json));
        }

        [Test]
        public void MemoryTraceWriterLimitTest()
        {
            MemoryTraceWriter traceWriter = new MemoryTraceWriter();

            for (int i = 0; i < 1005; i++)
            {
                traceWriter.Trace(TraceLevel.Verbose, (i + 1).ToString(CultureInfo.InvariantCulture), null);
            }

            IList<string> traceMessages = traceWriter.GetTraceMessages().ToList();

            Assert.AreEqual(1000, traceMessages.Count);

            Assert.IsTrue(traceMessages.First().EndsWith(" 6"));
            Assert.IsTrue(traceMessages.Last().EndsWith(" 1005"));
        }

#if !(NET20 || NET35 || NET40 || PORTABLE40 || PORTABLE) || DNXCORE50
        [Test]
        public async Task MemoryTraceWriterThreadSafety_Trace()
        {
            List<Task> tasks = new List<Task>();

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();

            for (int i = 0; i < 20; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    for (int j = 0; j < 1005; j++)
                    {
                        traceWriter.Trace(TraceLevel.Verbose, (j + 1).ToString(CultureInfo.InvariantCulture), null);
                    }
                }));
            }

            await Task.WhenAll(tasks);

            IList<string> traceMessages = traceWriter.GetTraceMessages().ToList();

            Assert.AreEqual(1000, traceMessages.Count);
        }

        [Test]
        public async Task MemoryTraceWriterThreadSafety_ToString()
        {
            List<Task> tasks = new List<Task>();

            MemoryTraceWriter traceWriter = new MemoryTraceWriter();

            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10005; j++)
                {
                    traceWriter.Trace(TraceLevel.Verbose, (j + 1).ToString(CultureInfo.InvariantCulture), null);
                }
            }));

            string s = null;

            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10005; j++)
                {
                    s = traceWriter.ToString();
                }
            }));

            await Task.WhenAll(tasks);

            Assert.IsNotNull(s);
        }
#endif

        [Test]
        public void Serialize()
        {
            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            string json =
                JsonConvert.SerializeObject(
                    new TraceTestObject
                    {
                        StringArray = new[] { "1", "2" },
                        IntList = new List<int> { 1, 2 },
                        Version = new Version(1, 2, 3, 4),
                        StringDictionary =
                            new Dictionary<string, string>
                            {
                                { "1", "!" },
                                { "Two", "!!" },
                                { "III", "!!!" }
                            },
                        Double = 1.1d
                    },
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter,
                        Formatting = Formatting.Indented
                    });

            Assert.AreEqual("Started serializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Started serializing System.Collections.Generic.List`1[System.Int32]. Path 'IntList'.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("Finished serializing System.Collections.Generic.List`1[System.Int32]. Path 'IntList'.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("Started serializing System.String[]. Path 'StringArray'.", traceWriter.TraceRecords[3].Message);
            Assert.AreEqual("Finished serializing System.String[]. Path 'StringArray'.", traceWriter.TraceRecords[4].Message);
            Assert.AreEqual("Started serializing System.Version. Path 'Version'.", traceWriter.TraceRecords[5].Message);
            Assert.AreEqual("Finished serializing System.Version. Path 'Version'.", traceWriter.TraceRecords[6].Message);
            Assert.AreEqual("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path 'StringDictionary'.", traceWriter.TraceRecords[7].Message);
            Assert.AreEqual("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path 'StringDictionary'.", traceWriter.TraceRecords[8].Message);
            Assert.AreEqual("Finished serializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path ''.", traceWriter.TraceRecords[9].Message);

            Assert.IsFalse(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
        }

        [Test]
        public void Deserialize()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            TraceTestObject o2 = JsonConvert.DeserializeObject<TraceTestObject>(
                @"{
  ""IntList"": [
    1,
    2
  ],
  ""StringArray"": [
    ""1"",
    ""2""
  ],
  ""Version"": {
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  },
  ""StringDictionary"": {
    ""1"": ""!"",
    ""Two"": ""!!"",
    ""III"": ""!!!""
  },
  ""Double"": 1.1
}",
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter
                });

            Assert.AreEqual(2, o2.IntList.Count);
            Assert.AreEqual(2, o2.StringArray.Length);
            Assert.AreEqual(1, o2.Version.Major);
            Assert.AreEqual(2, o2.Version.Minor);
            Assert.AreEqual(3, o2.StringDictionary.Count);
            Assert.AreEqual(1.1d, o2.Double);

            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path 'IntList', line 2, position 12.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 14.", traceWriter.TraceRecords[1].Message);
            Assert.IsTrue(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList'"));
            Assert.AreEqual("Started deserializing System.String[]. Path 'StringArray', line 6, position 18.", traceWriter.TraceRecords[3].Message);
            Assert.IsTrue(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.String[]. Path 'StringArray'"));
            Assert.AreEqual("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path 'Version.Major', line 11, position 12.", traceWriter.TraceRecords[5].Message);
            Assert.IsTrue(traceWriter.TraceRecords[6].Message.StartsWith("Started deserializing System.Version. Path 'Version'"));
            Assert.IsTrue(traceWriter.TraceRecords[7].Message.StartsWith("Finished deserializing System.Version. Path 'Version'"));
            Assert.AreEqual("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 8.", traceWriter.TraceRecords[8].Message);
            Assert.IsTrue(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary'"));
            Assert.IsTrue(traceWriter.TraceRecords[10].Message.StartsWith("Finished deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path ''"));

            Assert.IsFalse(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
        }

        [Test]
        public void Populate()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            TraceTestObject o2 = new TraceTestObject();

            JsonConvert.PopulateObject(@"{
  ""IntList"": [
    1,
    2
  ],
  ""StringArray"": [
    ""1"",
    ""2""
  ],
  ""Version"": {
    ""Major"": 1,
    ""Minor"": 2,
    ""Build"": 3,
    ""Revision"": 4,
    ""MajorRevision"": 0,
    ""MinorRevision"": 4
  },
  ""StringDictionary"": {
    ""1"": ""!"",
    ""Two"": ""!!"",
    ""III"": ""!!!""
  },
  ""Double"": 1.1
}",
                o2,
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default
                });

            Assert.AreEqual(2, o2.IntList.Count);
            Assert.AreEqual(2, o2.StringArray.Length);
            Assert.AreEqual(1, o2.Version.Major);
            Assert.AreEqual(2, o2.Version.Minor);
            Assert.AreEqual(3, o2.StringDictionary.Count);
            Assert.AreEqual(1.1d, o2.Double);

            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path 'IntList', line 2, position 12.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 14.", traceWriter.TraceRecords[1].Message);
            Assert.IsTrue(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList'"));
            Assert.AreEqual("Started deserializing System.String[]. Path 'StringArray', line 6, position 18.", traceWriter.TraceRecords[3].Message);
            Assert.IsTrue(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.String[]. Path 'StringArray'"));
            Assert.AreEqual("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path 'Version.Major', line 11, position 12.", traceWriter.TraceRecords[5].Message);
            Assert.IsTrue(traceWriter.TraceRecords[6].Message.StartsWith("Started deserializing System.Version. Path 'Version'"));
            Assert.IsTrue(traceWriter.TraceRecords[7].Message.StartsWith("Finished deserializing System.Version. Path 'Version'"));
            Assert.AreEqual("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 8.", traceWriter.TraceRecords[8].Message);
            Assert.IsTrue(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary'"));
            Assert.IsTrue(traceWriter.TraceRecords[10].Message.StartsWith("Finished deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path ''"));

            Assert.IsFalse(traceWriter.TraceRecords.Any(r => r.Level == TraceLevel.Verbose));
        }

        [Test]
        public void ErrorDeserializing()
        {
            string json = @"{""Integer"":""hi""}";

            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            ExceptionAssert.Throws<Exception>(() =>
            {
                JsonConvert.DeserializeObject<IntegerTestClass>(
                    json,
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            }, "Could not convert string to integer: hi. Path 'Integer', line 1, position 15.");

            Assert.AreEqual(2, traceWriter.TraceRecords.Count);

            Assert.AreEqual(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.IntegerTestClass. Path 'Integer', line 1, position 11.", traceWriter.TraceRecords[0].Message);

            Assert.AreEqual(TraceLevel.Error, traceWriter.TraceRecords[1].Level);
            Assert.AreEqual("Error deserializing Newtonsoft.Json.Tests.Serialization.IntegerTestClass. Could not convert string to integer: hi. Path 'Integer', line 1, position 15.", traceWriter.TraceRecords[1].Message);
        }

        [Test]
        public void ErrorDeserializingNested()
        {
            string json = @"{""IntList"":[1, ""two""]}";

            var traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Info
            };

            ExceptionAssert.Throws<Exception>(() =>
            {
                JsonConvert.DeserializeObject<TraceTestObject>(
                    json,
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            }, "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.");

            Assert.AreEqual(3, traceWriter.TraceRecords.Count);

            Assert.AreEqual(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path 'IntList', line 1, position 11.", traceWriter.TraceRecords[0].Message);

            Assert.AreEqual(TraceLevel.Info, traceWriter.TraceRecords[1].Level);
            Assert.AreEqual("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 1, position 12.", traceWriter.TraceRecords[1].Message);

            Assert.AreEqual(TraceLevel.Error, traceWriter.TraceRecords[2].Level);
            Assert.AreEqual("Error deserializing System.Collections.Generic.IList`1[System.Int32]. Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.", traceWriter.TraceRecords[2].Message);
        }

        [Test]
        public void SerializeDictionarysWithPreserveObjectReferences()
        {
            PreserveReferencesHandlingTests.CircularDictionary circularDictionary = new PreserveReferencesHandlingTests.CircularDictionary();
            circularDictionary.Add("other", new PreserveReferencesHandlingTests.CircularDictionary { { "blah", null } });
            circularDictionary.Add("self", circularDictionary);

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.SerializeObject(
                circularDictionary,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    TraceWriter = traceWriter
                });

            Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference Id '1' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path ''."));
            Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference Id '2' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other'."));
            Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Writing object reference to Id '1' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self'."));
        }

        [Test]
        public void DeserializeDictionarysWithPreserveObjectReferences()
        {
            string json = @"{
  ""$id"": ""1"",
  ""other"": {
    ""$id"": ""2"",
    ""blah"": null
  },
  ""self"": {
    ""$ref"": ""1""
  }
}";

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.DeserializeObject<PreserveReferencesHandlingTests.CircularDictionary>(json,
                new JsonSerializerSettings
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.All,
                    MetadataPropertyHandling = MetadataPropertyHandling.Default,
                    TraceWriter = traceWriter
                });

            Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '1' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other', line 3, position 10."));
            Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '2' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other.blah', line 5, position 11."));
            Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message.StartsWith("Resolved object reference '1' to Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self'")));
        }

        [Test]
        public void WriteTypeNameForObjects()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            IList<object> l = new List<object>
            {
                new Dictionary<string, string> { { "key!", "value!" } },
                new Version(1, 2, 3, 4)
            };

            JsonConvert.SerializeObject(l, Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                TraceWriter = traceWriter
            });

            Assert.AreEqual("Started serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Writing type name '" + ReflectionUtils.GetTypeName(typeof(List<object>), 0, DefaultSerializationBinder.Instance) + "' for System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values'.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("Writing type name '" + ReflectionUtils.GetTypeName(typeof(Dictionary<string, string>), 0, DefaultSerializationBinder.Instance) + "' for System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[3].Message);
            Assert.AreEqual("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[4].Message);
            Assert.AreEqual("Started serializing System.Version. Path '$values[0]'.", traceWriter.TraceRecords[5].Message);
            Assert.AreEqual("Writing type name '" + ReflectionUtils.GetTypeName(typeof(Version), 0, DefaultSerializationBinder.Instance) + "' for System.Version. Path '$values[1]'.", traceWriter.TraceRecords[6].Message);
            Assert.AreEqual("Finished serializing System.Version. Path '$values[1]'.", traceWriter.TraceRecords[7].Message);
            Assert.AreEqual("Finished serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[8].Message);
        }

        [Test]
        public void SerializeConverter()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            IList<DateTime> d = new List<DateTime>
            {
                new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc)
            };

            string json = JsonConvert.SerializeObject(d, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = { new JavaScriptDateTimeConverter() },
                TraceWriter = traceWriter
            });

            Assert.AreEqual("Started serializing System.Collections.Generic.List`1[System.DateTime]. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Started serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("Finished serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path '[0]'.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("Finished serializing System.Collections.Generic.List`1[System.DateTime]. Path ''.", traceWriter.TraceRecords[3].Message);
        }

        [Test]
        public void DeserializeConverter()
        {
            string json = @"[new Date(976623132000)]";

            InMemoryTraceWriter traceWriter =
                new InMemoryTraceWriter
                {
                    LevelFilter = TraceLevel.Verbose
                };

            JsonConvert.DeserializeObject<List<DateTime>>(
                json,
                new JsonSerializerSettings
                {
                    Converters = { new JavaScriptDateTimeConverter() },
                    TraceWriter = traceWriter
                });

            Assert.AreEqual("Started deserializing System.Collections.Generic.List`1[System.DateTime]. Path '', line 1, position 1.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Started deserializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path '[0]', line 1, position 10.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("Finished deserializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path '[0]', line 1, position 23.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("Finished deserializing System.Collections.Generic.List`1[System.DateTime]. Path '', line 1, position 24.", traceWriter.TraceRecords[3].Message);
        }

        [Test]
        public void DeserializeTypeName()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = @"{
  ""$type"": ""System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib"",
  ""$values"": [
    {
      ""$type"": ""System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib"",
      ""key!"": ""value!""
    },
    {
      ""$type"": ""System.Version, mscorlib"",
      ""Major"": 1,
      ""Minor"": 2,
      ""Build"": 3,
      ""Revision"": 4,
      ""MajorRevision"": 0,
      ""MinorRevision"": 4
    }
  ]
}";

            JsonConvert.DeserializeObject(json, null, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                MetadataPropertyHandling = MetadataPropertyHandling.Default,
                TraceWriter = traceWriter
            });

            Assert.AreEqual("Resolved type 'System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib' to System.Collections.Generic.List`1[System.Object]. Path '$type', line 2, position 83.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Started deserializing System.Collections.Generic.List`1[System.Object]. Path '$values', line 3, position 14.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("Resolved type 'System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib' to System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].$type', line 5, position 119.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("Started deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].key!', line 6, position 13.", traceWriter.TraceRecords[3].Message);
            Assert.IsTrue(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'"));
            Assert.AreEqual("Resolved type 'System.Version, mscorlib' to System.Version. Path '$values[1].$type', line 9, position 41.", traceWriter.TraceRecords[5].Message);
            Assert.AreEqual("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path '$values[1].Major', line 10, position 14.", traceWriter.TraceRecords[6].Message);
            Assert.IsTrue(traceWriter.TraceRecords[7].Message.StartsWith("Started deserializing System.Version. Path '$values[1]'"));
            Assert.IsTrue(traceWriter.TraceRecords[8].Message.StartsWith("Finished deserializing System.Version. Path '$values[1]'"));
            Assert.IsTrue(traceWriter.TraceRecords[9].Message.StartsWith("Finished deserializing System.Collections.Generic.List`1[System.Object]. Path '$values'"));
        }

#if !(PORTABLE || DNXCORE50 || PORTABLE40) || NETSTANDARD2_0
        [Test]
        public void DeserializeISerializable()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            ExceptionAssert.Throws<SerializationException>(() =>
            {
                JsonConvert.DeserializeObject<Exception>(
                    "{}",
                    new JsonSerializerSettings
                    {
                        TraceWriter = traceWriter
                    });
            }, "Member 'ClassName' was not found.");

            Assert.IsTrue(traceWriter.TraceRecords[0].Message.StartsWith("Deserializing System.Exception using ISerializable constructor. Path ''"));
            Assert.AreEqual(TraceLevel.Info, traceWriter.TraceRecords[0].Level);
            Assert.AreEqual("Error deserializing System.Exception. Member 'ClassName' was not found. Path '', line 1, position 2.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual(TraceLevel.Error, traceWriter.TraceRecords[1].Level);
        }
#endif

        [Test]
        public void DeserializeMissingMember()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.DeserializeObject<Person>(
                "{'MissingMemberProperty':'!!'}",
                new JsonSerializerSettings
                {
                    TraceWriter = traceWriter
                });

            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.TestObjects.Organization.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Could not find member 'MissingMemberProperty' on Newtonsoft.Json.Tests.TestObjects.Organization.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[1].Message);
            Assert.IsTrue(traceWriter.TraceRecords[2].Message.StartsWith("Finished deserializing Newtonsoft.Json.Tests.TestObjects.Organization.Person. Path ''"));
        }

        [Test]
        public void DeserializeMissingMemberConstructor()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = @"{
  ""Major"": 1,
  ""Minor"": 2,
  ""Build"": 3,
  ""Revision"": 4,
  ""MajorRevision"": 0,
  ""MinorRevision"": 4,
  ""MissingMemberProperty"": null
}";

            JsonConvert.DeserializeObject<Version>(json, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

            Assert.AreEqual("Deserializing System.Version using creator with parameters: Major, Minor, Build, Revision. Path 'Major', line 2, position 10.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("Could not find member 'MissingMemberProperty' on System.Version. Path 'MissingMemberProperty', line 8, position 31.", traceWriter.TraceRecords[1].Message);
            Assert.IsTrue(traceWriter.TraceRecords[2].Message.StartsWith("Started deserializing System.Version. Path ''"));
            Assert.IsTrue(traceWriter.TraceRecords[3].Message.StartsWith("Finished deserializing System.Version. Path ''"));
        }

        [Test]
        public void PublicParameterizedConstructorWithPropertyNameConflictWithAttribute()
        {
            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = @"{name:""1""}";

            PublicParameterizedConstructorWithPropertyNameConflictWithAttribute c = JsonConvert.DeserializeObject<PublicParameterizedConstructorWithPropertyNameConflictWithAttribute>(json, new JsonSerializerSettings
            {
                TraceWriter = traceWriter
            });

            Assert.IsNotNull(c);
            Assert.AreEqual(1, c.Name);

            Assert.AreEqual("Deserializing Newtonsoft.Json.Tests.TestObjects.PublicParameterizedConstructorWithPropertyNameConflictWithAttribute using creator with parameters: name. Path 'name', line 1, position 6.", traceWriter.TraceRecords[0].Message);
        }

        [Test]
        public void ShouldSerializeTestClass()
        {
            ShouldSerializeTestClass c = new ShouldSerializeTestClass();
            c.Age = 29;
            c.Name = "Jim";
            c._shouldSerializeName = true;

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            JsonConvert.SerializeObject(c, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.AreEqual("ShouldSerialize result for property 'Name' on Newtonsoft.Json.Tests.Serialization.ShouldSerializeTestClass: True. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual(TraceLevel.Verbose, traceWriter.TraceRecords[1].Level);

            traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            c._shouldSerializeName = false;

            JsonConvert.SerializeObject(c, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.AreEqual("ShouldSerialize result for property 'Name' on Newtonsoft.Json.Tests.Serialization.ShouldSerializeTestClass: False. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual(TraceLevel.Verbose, traceWriter.TraceRecords[1].Level);
        }

        [Test]
        public void SpecifiedTest()
        {
            SpecifiedTestClass c = new SpecifiedTestClass();
            c.Name = "James";
            c.Age = 27;
            c.NameSpecified = false;

            InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            string json = JsonConvert.SerializeObject(c, Formatting.Indented, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.AreEqual("Started serializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path ''.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("IsSpecified result for property 'Name' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass: False. Path ''.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("IsSpecified result for property 'Weight' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("IsSpecified result for property 'Height' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[3].Message);
            Assert.AreEqual("IsSpecified result for property 'FavoriteNumber' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass: False. Path 'Age'.", traceWriter.TraceRecords[4].Message);
            Assert.AreEqual("Finished serializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path ''.", traceWriter.TraceRecords[5].Message);

            StringAssert.AreEqual(@"{
  ""Age"": 27
}", json);

            traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            SpecifiedTestClass deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path 'Age', line 2, position 8.", traceWriter.TraceRecords[0].Message);
            Assert.IsTrue(traceWriter.TraceRecords[1].Message.StartsWith("Finished deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path ''"));

            Assert.IsNull(deserialized.Name);
            Assert.IsFalse(deserialized.NameSpecified);
            Assert.IsFalse(deserialized.WeightSpecified);
            Assert.IsFalse(deserialized.HeightSpecified);
            Assert.IsFalse(deserialized.FavoriteNumberSpecified);
            Assert.AreEqual(27, deserialized.Age);

            c.NameSpecified = true;
            c.WeightSpecified = true;
            c.HeightSpecified = true;
            c.FavoriteNumber = 23;
            json = JsonConvert.SerializeObject(c, Formatting.Indented);

            StringAssert.AreEqual(@"{
  ""Name"": ""James"",
  ""Age"": 27,
  ""Weight"": 0,
  ""Height"": 0,
  ""FavoriteNumber"": 23
}", json);

            traceWriter = new InMemoryTraceWriter
            {
                LevelFilter = TraceLevel.Verbose
            };

            deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

            Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path 'Name', line 2, position 9.", traceWriter.TraceRecords[0].Message);
            Assert.AreEqual("IsSpecified for property 'Name' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass set to true. Path 'Name', line 2, position 17.", traceWriter.TraceRecords[1].Message);
            Assert.AreEqual("IsSpecified for property 'Weight' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass set to true. Path 'Weight', line 4, position 13.", traceWriter.TraceRecords[2].Message);
            Assert.AreEqual("IsSpecified for property 'Height' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass set to true. Path 'Height', line 5, position 13.", traceWriter.TraceRecords[3].Message);
            Assert.IsTrue(traceWriter.TraceRecords[4].Message.StartsWith("Finished deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path ''"));

            Assert.AreEqual("James", deserialized.Name);
            Assert.IsTrue(deserialized.NameSpecified);
            Assert.IsTrue(deserialized.WeightSpecified);
            Assert.IsTrue(deserialized.HeightSpecified);
            Assert.IsTrue(deserialized.FavoriteNumberSpecified);
            Assert.AreEqual(27, deserialized.Age);
            Assert.AreEqual(23, deserialized.FavoriteNumber);
        }

        [Test]
        public void TraceJsonWriterTest_WriteObjectInObject()
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            JsonTextWriter w = new JsonTextWriter(sw);
            TraceJsonWriter traceWriter = new TraceJsonWriter(w);

            traceWriter.WriteStartObject();
            traceWriter.WritePropertyName("Prop1");
            traceWriter.WriteValue((object)1);
            traceWriter.WriteEndObject();
            traceWriter.Flush();
            traceWriter.Close();

            string json = @"{
  ""Prop1"": 1
}";

            StringAssert.AreEqual("Serialized JSON: " + Environment.NewLine + json, traceWriter.GetSerializedJsonMessage());
        }

#if !(NET20 || NET35 || NET40 || PORTABLE || PORTABLE40)
        [Test]
        public async Task TraceJsonWriterTest_WriteObjectInObjectAsync()
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            JsonTextWriter w = new JsonTextWriter(sw);
            TraceJsonWriter traceWriter = new TraceJsonWriter(w);

            await traceWriter.WriteStartObjectAsync();
            await traceWriter.WritePropertyNameAsync("Prop1");
            await traceWriter.WriteValueAsync((object)1);
            await traceWriter.WriteEndObjectAsync();
            await traceWriter.FlushAsync();
            traceWriter.Close();

            string json = @"{
  ""Prop1"": 1
}";

            StringAssert.AreEqual("Serialized JSON: " + Environment.NewLine + json, traceWriter.GetSerializedJsonMessage());
        }
#endif

#if !(NET20 || NET35 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0
        [Test]
        public void TraceJsonWriterTest()
        {
            StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
            JsonTextWriter w = new JsonTextWriter(sw);
            TraceJsonWriter traceWriter = new TraceJsonWriter(w);

            traceWriter.WriteStartObject();
            traceWriter.WritePropertyName("Array");
            traceWriter.WriteStartArray();
            traceWriter.WriteValue("String!");
            traceWriter.WriteValue(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
            traceWriter.WriteValue(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
            traceWriter.WriteValue(1.1f);
            traceWriter.WriteValue(1.1d);
            traceWriter.WriteValue(1.1m);
            traceWriter.WriteValue(1);
            traceWriter.WriteValue((char)'!');
            traceWriter.WriteValue((short)1);
            traceWriter.WriteValue((ushort)1);
            traceWriter.WriteValue((int)1);
            traceWriter.WriteValue((uint)1);
            traceWriter.WriteValue((sbyte)1);
            traceWriter.WriteValue((byte)1);
            traceWriter.WriteValue((long)1);
            traceWriter.WriteValue((ulong)1);
            traceWriter.WriteValue((bool)true);

            traceWriter.WriteValue((DateTime?)new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc));
            traceWriter.WriteValue((DateTimeOffset?)new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.FromHours(2)));
            traceWriter.WriteValue((float?)1.1f);
            traceWriter.WriteValue((double?)1.1d);
            traceWriter.WriteValue((decimal?)1.1m);
            traceWriter.WriteValue((int?)1);
            traceWriter.WriteValue((char?)'!');
            traceWriter.WriteValue((short?)1);
            traceWriter.WriteValue((ushort?)1);
            traceWriter.WriteValue((int?)1);
            traceWriter.WriteValue((uint?)1);
            traceWriter.WriteValue((sbyte?)1);
            traceWriter.WriteValue((byte?)1);
            traceWriter.WriteValue((long?)1);
            traceWriter.WriteValue((ulong?)1);
            traceWriter.WriteValue((bool?)true);
            traceWriter.WriteValue(BigInteger.Parse("9999999990000000000000000000000000000000000"));

            traceWriter.WriteValue((object)true);
            traceWriter.WriteValue(TimeSpan.FromMinutes(1));
            traceWriter.WriteValue(Guid.Empty);
            traceWriter.WriteValue(new Uri("http://www.google.com/"));
            traceWriter.WriteValue(Encoding.UTF8.GetBytes("String!"));
            traceWriter.WriteRawValue("[1],");
            traceWriter.WriteRaw("[2]");
            traceWriter.WriteNull();
            traceWriter.WriteUndefined();
            traceWriter.WriteStartConstructor("ctor");
            traceWriter.WriteValue(1);
            traceWriter.WriteEndConstructor();
            traceWriter.WriteComment("A comment");
            traceWriter.WriteWhitespace("       ");
            traceWriter.WriteEnd();
            traceWriter.WriteEndObject();
            traceWriter.Flush();
            traceWriter.Close();

            string json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+02:00"",
    1.1,
    1.1,
    1.1,
    1,
    ""!"",
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    1,
    true,
    9999999990000000000000000000000000000000000,
    true,
    ""00:01:00"",
    ""00000000-0000-0000-0000-000000000000"",
    ""http://www.google.com/"",
    ""U3RyaW5nIQ=="",
    [1],[2],
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/       
  ]
}";

            StringAssert.AreEqual("Serialized JSON: " + Environment.NewLine + json, traceWriter.GetSerializedJsonMessage());
        }

        [Test]
        public void TraceJsonReaderTest()
        {
            string json = @"{
  ""Array"": [
    ""String!"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12Z"",
    ""2000-12-12T12:12:12+00:00"",
    ""U3RyaW5nIQ=="",
    1,
    1.1,
    1.2,
    9999999990000000000000000000000000000000000,
    null,
    undefined,
    new ctor(
      1
    )
    /*A comment*/
  ]
}";

            StringReader sw = new StringReader(json);
            JsonTextReader w = new JsonTextReader(sw);
            TraceJsonReader traceReader = new TraceJsonReader(w);

            traceReader.Read();
            Assert.AreEqual(JsonToken.StartObject, traceReader.TokenType);

            traceReader.Read();
            Assert.AreEqual(JsonToken.PropertyName, traceReader.TokenType);
            Assert.AreEqual("Array", traceReader.Value);

            traceReader.Read();
            Assert.AreEqual(JsonToken.StartArray, traceReader.TokenType);
            Assert.AreEqual(null, traceReader.Value);

            traceReader.ReadAsString();
            Assert.AreEqual(JsonToken.String, traceReader.TokenType);
            Assert.AreEqual('"', traceReader.QuoteChar);
            Assert.AreEqual("String!", traceReader.Value);

            // for great code coverage justice!
            traceReader.QuoteChar = '\'';
            Assert.AreEqual('\'', traceReader.QuoteChar);

            traceReader.ReadAsString();
            Assert.AreEqual(JsonToken.String, traceReader.TokenType);
            Assert.AreEqual("2000-12-12T12:12:12Z", traceReader.Value);

            traceReader.ReadAsDateTime();
            Assert.AreEqual(JsonToken.Date, traceReader.TokenType);
            Assert.AreEqual(new DateTime(2000, 12, 12, 12, 12, 12, DateTimeKind.Utc), traceReader.Value);

            traceReader.ReadAsDateTimeOffset();
            Assert.AreEqual(JsonToken.Date, traceReader.TokenType);
            Assert.AreEqual(new DateTimeOffset(2000, 12, 12, 12, 12, 12, TimeSpan.Zero), traceReader.Value);

            traceReader.ReadAsBytes();
            Assert.AreEqual(JsonToken.Bytes, traceReader.TokenType);
            CollectionAssert.AreEqual(Encoding.UTF8.GetBytes("String!"), (byte[])traceReader.Value);

            traceReader.ReadAsInt32();
            Assert.AreEqual(JsonToken.Integer, traceReader.TokenType);
            Assert.AreEqual(1, traceReader.Value);

            traceReader.ReadAsDecimal();
            Assert.AreEqual(JsonToken.Float, traceReader.TokenType);
            Assert.AreEqual(1.1m, traceReader.Value);

            traceReader.ReadAsDouble();
            Assert.AreEqual(JsonToken.Float, traceReader.TokenType);
            Assert.AreEqual(1.2d, traceReader.Value);

            traceReader.Read();
            Assert.AreEqual(JsonToken.Integer, traceReader.TokenType);
            Assert.AreEqual(typeof(BigInteger), traceReader.ValueType);
            Assert.AreEqual(BigInteger.Parse("9999999990000000000000000000000000000000000"), traceReader.Value);

            traceReader.Read();
            Assert.AreEqual(JsonToken.Null, traceReader.TokenType);

            traceReader.Read();
            Assert.AreEqual(JsonToken.Undefined, traceReader.TokenType);

            traceReader.Read();
            Assert.AreEqual(JsonToken.StartConstructor, traceReader.TokenType);

            traceReader.Read();
            Assert.AreEqual(JsonToken.Integer, traceReader.TokenType);
            Assert.AreEqual(1L, traceReader.Value);

            traceReader.Read();
            Assert.AreEqual(JsonToken.EndConstructor, traceReader.TokenType);

            traceReader.Read();
            Assert.AreEqual(JsonToken.Comment, traceReader.TokenType);
            Assert.AreEqual("A comment", traceReader.Value);

            traceReader.Read();
            Assert.AreEqual(JsonToken.EndArray, traceReader.TokenType);

            traceReader.Read();
            Assert.AreEqual(JsonToken.EndObject, traceReader.TokenType);

            Assert.IsFalse(traceReader.Read());

            traceReader.Close();

            StringAssert.AreEqual("Deserialized JSON: " + Environment.NewLine + json, traceReader.GetDeserializedJsonMessage());
        }
#endif
    }

    public class TraceRecord
    {
        public string Message { get; set; }
        public TraceLevel Level { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return Level + " - " + Message;
        }
    }

    public class InMemoryTraceWriter : ITraceWriter
    {
        public TraceLevel LevelFilter { get; set; }
        public IList<TraceRecord> TraceRecords { get; set; }

        public InMemoryTraceWriter()
        {
            LevelFilter = TraceLevel.Verbose;
            TraceRecords = new List<TraceRecord>();
        }

        public void Trace(TraceLevel level, string message, Exception ex)
        {
            TraceRecords.Add(
                new TraceRecord
                {
                    Level = level,
                    Message = message,
                    Exception = ex
                });
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var traceRecord in TraceRecords)
            {
                sb.AppendLine(traceRecord.Message);
            }

            return sb.ToString();
        }
    }

    public class TraceTestObject
    {
        public IList<int> IntList { get; set; }
        public string[] StringArray { get; set; }
        public Version Version { get; set; }
        public IDictionary<string, string> StringDictionary { get; set; }
        public double Double { get; set; }
    }

    public class IntegerTestClass
    {
        public int Integer { get; set; }
    }
}