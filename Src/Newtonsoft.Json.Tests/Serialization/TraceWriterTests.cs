using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Tests.TestObjects;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Tests.Serialization
{
  public class Staff
  {
    public string Name { get; set; }
    public DateTime StartDate { get; set; }
    public IList<string> Roles { get; set; }
  }

  [TestFixture]
  public class TraceWriterTests : TestFixtureBase
  {
#if !(SILVERLIGHT || PORTABLE || NETFX_CORE || PORTABLE40)
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

        Assert.AreEqual(@"Newtonsoft.Json Verbose: 0 : Verbose!
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
    public void MemoryTraceWriterTest()
    {
      Staff staff = new Staff();
      staff.Name = "Arnie Admin";
      staff.Roles = new List<string> {"Administrator"};
      staff.StartDate = DateTime.Now;

      ITraceWriter traceWriter = new MemoryTraceWriter();

      JsonConvert.SerializeObject(
        staff,
        new JsonSerializerSettings {TraceWriter = traceWriter, Converters = {new JavaScriptDateTimeConverter()}});

      Console.WriteLine(traceWriter);
      // 2012-11-11T12:08:42.761 Info Started serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.
      // 2012-11-11T12:08:42.785 Info Started serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
      // 2012-11-11T12:08:42.791 Info Finished serializing System.DateTime with converter Newtonsoft.Json.Converters.JavaScriptDateTimeConverter. Path 'StartDate'.
      // 2012-11-11T12:08:42.797 Info Started serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
      // 2012-11-11T12:08:42.798 Info Finished serializing System.Collections.Generic.List`1[System.String]. Path 'Roles'.
      // 2012-11-11T12:08:42.799 Info Finished serializing Newtonsoft.Json.Tests.Serialization.Staff. Path ''.

      MemoryTraceWriter memoryTraceWriter = (MemoryTraceWriter)traceWriter;

      Assert.AreEqual(743, memoryTraceWriter.ToString().Length);
      Assert.AreEqual(6, memoryTraceWriter.GetTraceMessages().Count());
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
              StringArray = new[] {"1", "2"},
              IntList = new List<int> {1, 2},
              Version = new Version(1, 2, 3, 4),
              StringDictionary =
                new Dictionary<string, string>
                  {
                    {"1", "!"},
                    {"Two", "!!"},
                    {"III", "!!!"}
                  }
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
  }
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

      Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path 'IntList', line 2, position 13.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("Started deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 2, position 15.", traceWriter.TraceRecords[1].Message);
      Assert.AreEqual("Finished deserializing System.Collections.Generic.IList`1[System.Int32]. Path 'IntList', line 5, position 4.", traceWriter.TraceRecords[2].Message);
      Assert.AreEqual("Started deserializing System.String[]. Path 'StringArray', line 6, position 19.", traceWriter.TraceRecords[3].Message);
      Assert.AreEqual("Finished deserializing System.String[]. Path 'StringArray', line 9, position 4.", traceWriter.TraceRecords[4].Message);
      Assert.AreEqual("Deserializing System.Version using a non-default constructor 'Void .ctor(Int32, Int32, Int32, Int32)'. Path 'Version.Major', line 11, position 13.", traceWriter.TraceRecords[5].Message);
      Assert.AreEqual("Started deserializing System.Version. Path 'Version', line 17, position 4.", traceWriter.TraceRecords[6].Message);
      Assert.AreEqual("Finished deserializing System.Version. Path 'Version', line 17, position 4.", traceWriter.TraceRecords[7].Message);
      Assert.AreEqual("Started deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary.1', line 19, position 9.", traceWriter.TraceRecords[8].Message);
      Assert.AreEqual("Finished deserializing System.Collections.Generic.IDictionary`2[System.String,System.String]. Path 'StringDictionary', line 22, position 4.", traceWriter.TraceRecords[9].Message);
      Assert.AreEqual("Finished deserializing Newtonsoft.Json.Tests.Serialization.TraceTestObject. Path '', line 23, position 2.", traceWriter.TraceRecords[10].Message);
    }

    [Test]
    public void ErrorDeserializing()
    {
      string json = @"{""Integer"":""hi""}";

      var traceWriter = new InMemoryTraceWriter
                          {
                            LevelFilter = TraceLevel.Info
                          };

      ExceptionAssert.Throws<Exception>(
        "Could not convert string to integer: hi. Path 'Integer', line 1, position 15.",
        () =>
          {
            JsonConvert.DeserializeObject<IntegerTestClass>(
              json,
              new JsonSerializerSettings
                {
                  TraceWriter = traceWriter
                });
          });

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

      ExceptionAssert.Throws<Exception>(
        "Could not convert string to integer: two. Path 'IntList[1]', line 1, position 20.",
        () =>
        {
          JsonConvert.DeserializeObject<TraceTestObject>(
            json,
            new JsonSerializerSettings
            {
              TraceWriter = traceWriter
            });
        });

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
          TraceWriter = traceWriter
        });

      Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '1' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other', line 3, position 11."));
      Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Read object reference Id '2' for Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'other.blah', line 5, position 12."));
      Assert.IsTrue(traceWriter.TraceRecords.Any(r => r.Message == "Resolved object reference '1' to Newtonsoft.Json.Tests.Serialization.PreserveReferencesHandlingTests+CircularDictionary. Path 'self', line 9, position 4."));
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
                            new Dictionary<string, string> { {"key!", "value!"}},
                            new Version(1, 2, 3, 4)
                          };

      JsonConvert.SerializeObject(l, Formatting.Indented, new JsonSerializerSettings
      {
        TypeNameHandling = TypeNameHandling.All,
        TraceWriter = traceWriter
      });

      Assert.AreEqual("Started serializing System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("Writing type name 'System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib' for System.Collections.Generic.List`1[System.Object]. Path ''.", traceWriter.TraceRecords[1].Message);
      Assert.AreEqual("Started serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values'.", traceWriter.TraceRecords[2].Message);
      Assert.AreEqual("Writing type name 'System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib' for System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[3].Message);
      Assert.AreEqual("Finished serializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]'.", traceWriter.TraceRecords[4].Message);
      Assert.AreEqual("Started serializing System.Version. Path '$values[0]'.", traceWriter.TraceRecords[5].Message);
      Assert.AreEqual("Writing type name 'System.Version, mscorlib' for System.Version. Path '$values[1]'.", traceWriter.TraceRecords[6].Message);
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
            Converters = {new JavaScriptDateTimeConverter()},
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
                                                    TraceWriter = traceWriter
                                                  });

      Assert.AreEqual("Resolved type 'System.Collections.Generic.List`1[[System.Object, mscorlib]], mscorlib' to System.Collections.Generic.List`1[System.Object]. Path '$type', line 2, position 84.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("Started deserializing System.Collections.Generic.List`1[System.Object]. Path '$values', line 3, position 15.", traceWriter.TraceRecords[1].Message);
      Assert.AreEqual("Resolved type 'System.Collections.Generic.Dictionary`2[[System.String, mscorlib],[System.String, mscorlib]], mscorlib' to System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].$type', line 5, position 120.", traceWriter.TraceRecords[2].Message);
      Assert.AreEqual("Started deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0].key!', line 6, position 14.", traceWriter.TraceRecords[3].Message);
      Assert.AreEqual("Finished deserializing System.Collections.Generic.Dictionary`2[System.String,System.String]. Path '$values[0]', line 7, position 6.", traceWriter.TraceRecords[4].Message);
      Assert.AreEqual("Resolved type 'System.Version, mscorlib' to System.Version. Path '$values[1].$type', line 9, position 42.", traceWriter.TraceRecords[5].Message);
      Assert.AreEqual("Deserializing System.Version using a non-default constructor 'Void .ctor(Int32, Int32, Int32, Int32)'. Path '$values[1].Major', line 10, position 15.", traceWriter.TraceRecords[6].Message);
      Assert.AreEqual("Started deserializing System.Version. Path '$values[1]', line 16, position 6.", traceWriter.TraceRecords[7].Message);
      Assert.AreEqual("Finished deserializing System.Version. Path '$values[1]', line 16, position 6.", traceWriter.TraceRecords[8].Message);
      Assert.AreEqual("Finished deserializing System.Collections.Generic.List`1[System.Object]. Path '$values', line 17, position 4.", traceWriter.TraceRecords[9].Message);
    }

#if !(NETFX_CORE || PORTABLE || SILVERLIGHT || PORTABLE40)
    [Test]
    public void DeserializeISerializable()
    {
      InMemoryTraceWriter traceWriter = new InMemoryTraceWriter
                                          {
                                            LevelFilter = TraceLevel.Verbose
                                          };

      ExceptionAssert.Throws<SerializationException>(
        "Member 'ClassName' was not found.",
        () =>
          {
            JsonConvert.DeserializeObject<Exception>(
              "{}",
              new JsonSerializerSettings
                {
                  TraceWriter = traceWriter
                });
          });

      Assert.AreEqual("Deserializing System.Exception using ISerializable constructor. Path '', line 1, position 2.", traceWriter.TraceRecords[0].Message);
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

      Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.TestObjects.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("Could not find member 'MissingMemberProperty' on Newtonsoft.Json.Tests.TestObjects.Person. Path 'MissingMemberProperty', line 1, position 25.", traceWriter.TraceRecords[1].Message);
      Assert.AreEqual("Finished deserializing Newtonsoft.Json.Tests.TestObjects.Person. Path '', line 1, position 30.", traceWriter.TraceRecords[2].Message);
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

      Assert.AreEqual("Deserializing System.Version using a non-default constructor 'Void .ctor(Int32, Int32, Int32, Int32)'. Path 'Major', line 2, position 11.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("Could not find member 'MissingMemberProperty' on System.Version. Path 'MissingMemberProperty', line 8, position 32.", traceWriter.TraceRecords[1].Message);
      Assert.AreEqual("Started deserializing System.Version. Path '', line 9, position 2.", traceWriter.TraceRecords[2].Message);
      Assert.AreEqual("Finished deserializing System.Version. Path '', line 9, position 2.", traceWriter.TraceRecords[3].Message);
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

      JsonConvert.SerializeObject(c, new JsonSerializerSettings {TraceWriter = traceWriter});

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

      Assert.AreEqual(@"{
  ""Age"": 27
}", json);

      traceWriter = new InMemoryTraceWriter
      {
        LevelFilter = TraceLevel.Verbose
      };

      SpecifiedTestClass deserialized = JsonConvert.DeserializeObject<SpecifiedTestClass>(json, new JsonSerializerSettings { TraceWriter = traceWriter });

      Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path 'Age', line 2, position 9.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("Finished deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path '', line 3, position 2.", traceWriter.TraceRecords[1].Message);

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

      Assert.AreEqual(@"{
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

      Assert.AreEqual("Started deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path 'Name', line 2, position 10.", traceWriter.TraceRecords[0].Message);
      Assert.AreEqual("IsSpecified for property 'Name' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass set to true. Path 'Name', line 2, position 18.", traceWriter.TraceRecords[1].Message);
      Assert.AreEqual("IsSpecified for property 'Weight' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass set to true. Path 'Weight', line 4, position 14.", traceWriter.TraceRecords[2].Message);
      Assert.AreEqual("IsSpecified for property 'Height' on Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass set to true. Path 'Height', line 5, position 14.", traceWriter.TraceRecords[3].Message);
      Assert.AreEqual("Finished deserializing Newtonsoft.Json.Tests.Serialization.SpecifiedTestClass. Path '', line 7, position 2.", traceWriter.TraceRecords[4].Message);

      Assert.AreEqual("James", deserialized.Name);
      Assert.IsTrue(deserialized.NameSpecified);
      Assert.IsTrue(deserialized.WeightSpecified);
      Assert.IsTrue(deserialized.HeightSpecified);
      Assert.IsTrue(deserialized.FavoriteNumberSpecified);
      Assert.AreEqual(27, deserialized.Age);
      Assert.AreEqual(23, deserialized.FavoriteNumber);
    }
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
  }

  public class IntegerTestClass
  {
    public int Integer { get; set; }
  }
}