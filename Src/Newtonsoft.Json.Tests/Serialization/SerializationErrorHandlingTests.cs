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

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Tests.TestObjects;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
using System.IO;
using ErrorEventArgs=Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json.Tests.Serialization
{
  [TestFixture]
  public class SerializationErrorHandlingTests : TestFixtureBase
  {
    [Test]
    public void ErrorDeserializingListHandled()
    {
      string json = @"[
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]";

      VersionKeyedCollection c = JsonConvert.DeserializeObject<VersionKeyedCollection>(json);
      Assert.AreEqual(1, c.Count);
      Assert.AreEqual(1, c.Messages.Count);
      Assert.AreEqual("[1] - Error message for member 1 = An item with the same key has already been added.", c.Messages[0]);
    }

    [Test]
    public void DeserializingErrorInChildObject()
    {
      ListErrorObjectCollection c = JsonConvert.DeserializeObject<ListErrorObjectCollection>(@"[
  {
    ""Member"": ""Value1"",
    ""Member2"": null
  },
  {
    ""Member"": ""Value2""
  },
  {
    ""ThrowError"": ""Value"",
    ""Object"": {
      ""Array"": [
        1,
        2
      ]
    }
  },
  {
    ""ThrowError"": ""Handle this!"",
    ""Member"": ""Value3""
  }
]");

      Assert.AreEqual(3, c.Count);
      Assert.AreEqual("Value1", c[0].Member);
      Assert.AreEqual("Value2", c[1].Member);
      Assert.AreEqual("Value3", c[2].Member);
      Assert.AreEqual("Handle this!", c[2].ThrowError);
    }

    [Test]
    public void SerializingErrorIn3DArray()
    {
      ListErrorObject[,,] c = new ListErrorObject[,,]
        {
          {
            {
              new ListErrorObject
                {
                  Member = "Value1",
                  ThrowError = "Handle this!",
                  Member2 = "Member1"
                },
              new ListErrorObject
                {
                  Member = "Value2",
                  Member2 = "Member2"
                },
              new ListErrorObject
                {
                  Member = "Value3",
                  ThrowError = "Handle that!",
                  Member2 = "Member3"
                }
            },
            {
              new ListErrorObject
                {
                  Member = "Value1",
                  ThrowError = "Handle this!",
                  Member2 = "Member1"
                },
              new ListErrorObject
                {
                  Member = "Value2",
                  Member2 = "Member2"
                },
              new ListErrorObject
                {
                  Member = "Value3",
                  ThrowError = "Handle that!",
                  Member2 = "Member3"
                }
            }
          }
        };

      string json = JsonConvert.SerializeObject(c, new JsonSerializerSettings
        {
          Formatting = Formatting.Indented,
          Error = (s, e) =>
            {
              if (e.CurrentObject.GetType().IsArray)
                e.ErrorContext.Handled = true;
            }
        });

      Assert.AreEqual(@"[
  [
    [
      {
        ""Member"": ""Value1"",
        ""ThrowError"": ""Handle this!"",
        ""Member2"": ""Member1""
      },
      {
        ""Member"": ""Value2""
      },
      {
        ""Member"": ""Value3"",
        ""ThrowError"": ""Handle that!"",
        ""Member2"": ""Member3""
      }
    ],
    [
      {
        ""Member"": ""Value1"",
        ""ThrowError"": ""Handle this!"",
        ""Member2"": ""Member1""
      },
      {
        ""Member"": ""Value2""
      },
      {
        ""Member"": ""Value3"",
        ""ThrowError"": ""Handle that!"",
        ""Member2"": ""Member3""
      }
    ]
  ]
]", json);
    }

    [Test]
    public void SerializingErrorInChildObject()
    {
      ListErrorObjectCollection c = new ListErrorObjectCollection
        {
          new ListErrorObject
            {
              Member = "Value1",
              ThrowError = "Handle this!",
              Member2 = "Member1"
            },
          new ListErrorObject
            {
              Member = "Value2",
              Member2 = "Member2"
            },
          new ListErrorObject
            {
              Member = "Value3",
              ThrowError = "Handle that!",
              Member2 = "Member3"
            }
        };

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"[
  {
    ""Member"": ""Value1"",
    ""ThrowError"": ""Handle this!"",
    ""Member2"": ""Member1""
  },
  {
    ""Member"": ""Value2""
  },
  {
    ""Member"": ""Value3"",
    ""ThrowError"": ""Handle that!"",
    ""Member2"": ""Member3""
  }
]", json);
    }

    [Test]
    public void DeserializingErrorInDateTimeCollection()
    {
      DateTimeErrorObjectCollection c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(@"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]", new IsoDateTimeConverter());

      Assert.AreEqual(3, c.Count);
      Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
      Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
      Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);
    }

    [Test]
    public void DeserializingErrorHandlingUsingEvent()
    {
      List<string> errors = new List<string>();

      JsonSerializer serializer = JsonSerializer.Create(new JsonSerializerSettings
        {
          Error = delegate(object sender, ErrorEventArgs args)
            {
              errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
              args.ErrorContext.Handled = true;
            },
          Converters = {new IsoDateTimeConverter()}
        });
      var c = serializer.Deserialize<List<DateTime>>(new JsonTextReader(new StringReader(@"[
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
#if !(NET20 || NET35 || WINDOWS_PHONE)
      Assert.AreEqual("[1] - 1 - The string was not recognized as a valid DateTime. There is an unknown word starting at index 0.", errors[0]);
#else
      Assert.AreEqual("[1] - 1 - The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.", errors[0]);
#endif
      Assert.AreEqual("[2] - 2 - Unexpected token parsing date. Expected String, got StartArray. Path '[2]', line 4, position 10.", errors[1]);
      Assert.AreEqual("[4] - 4 - Cannot convert null value to System.DateTime. Path '[4]', line 8, position 13.", errors[2]);
    }

    [Test]
    public void DeserializingErrorInDateTimeCollectionWithAttributeWithEventNotCalled()
    {
      bool eventErrorHandlerCalled = false;

      DateTimeErrorObjectCollection c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(
        @"[
  ""2009-09-09T00:00:00Z"",
  ""kjhkjhkjhkjh"",
  [
    1
  ],
  ""1977-02-20T00:00:00Z"",
  null,
  ""2000-12-01T00:00:00Z""
]",
        new JsonSerializerSettings
          {
            Error = (s, a) => eventErrorHandlerCalled = true,
            Converters =
              {
                new IsoDateTimeConverter()
              }
          });

      Assert.AreEqual(3, c.Count);
      Assert.AreEqual(new DateTime(2009, 9, 9, 0, 0, 0, DateTimeKind.Utc), c[0]);
      Assert.AreEqual(new DateTime(1977, 2, 20, 0, 0, 0, DateTimeKind.Utc), c[1]);
      Assert.AreEqual(new DateTime(2000, 12, 1, 0, 0, 0, DateTimeKind.Utc), c[2]);

      Assert.AreEqual(false, eventErrorHandlerCalled);
    }

    [Test]
    public void SerializePerson()
    {
      PersonError person = new PersonError
        {
          Name = "George Michael Bluth",
          Age = 16,
          Roles = null,
          Title = "Mister Manager"
        };

      string json = JsonConvert.SerializeObject(person, Formatting.Indented);

      Console.WriteLine(json);
      //{
      //  "Name": "George Michael Bluth",
      //  "Age": 16,
      //  "Title": "Mister Manager"
      //}

      Assert.AreEqual(@"{
  ""Name"": ""George Michael Bluth"",
  ""Age"": 16,
  ""Title"": ""Mister Manager""
}", json);
    }

    [Test]
    public void DeserializeNestedUnhandled()
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
              errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
          };

        serializer.Deserialize(new StringReader(json), typeof (List<List<DateTime>>));
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
    public void MultipleRequiredPropertyErrors()
    {
      string json = "{}";
      List<string> errors = new List<string>();
      JsonSerializer serializer = new JsonSerializer();
      serializer.Error += delegate(object sender, ErrorEventArgs args)
        {
          errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
          args.ErrorContext.Handled = true;
        };
      serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof (MyTypeWithRequiredMembers));

      Assert.AreEqual(2, errors.Count);
      Assert.AreEqual(" - Required1 - Required property 'Required1' not found in JSON. Path '', line 1, position 2.", errors[0]);
      Assert.AreEqual(" - Required2 - Required property 'Required2' not found in JSON. Path '', line 1, position 2.", errors[1]);
    }

    [Test]
    public void HandlingArrayErrors()
    {
      string json = "[\"a\",\"b\",\"45\",34]";

      List<string> errors = new List<string>();

      JsonSerializer serializer = new JsonSerializer();
      serializer.Error += delegate(object sender, ErrorEventArgs args)
        {
          errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
          args.ErrorContext.Handled = true;
        };

      serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof (int[]));

      Assert.AreEqual(2, errors.Count);
      Assert.AreEqual("[0] - 0 - Could not convert string to integer: a. Path '[0]', line 1, position 4.", errors[0]);
      Assert.AreEqual("[1] - 1 - Could not convert string to integer: b. Path '[1]', line 1, position 8.", errors[1]);
    }

    [Test]
    public void HandlingMultidimensionalArrayErrors()
    {
      string json = "[[\"a\",\"45\"],[\"b\",34]]";

      List<string> errors = new List<string>();

      JsonSerializer serializer = new JsonSerializer();
      serializer.Error += delegate(object sender, ErrorEventArgs args)
        {
          errors.Add(args.ErrorContext.Path + " - " + args.ErrorContext.Member + " - " + args.ErrorContext.Error.Message);
          args.ErrorContext.Handled = true;
        };

      serializer.Deserialize(new JsonTextReader(new StringReader(json)), typeof (int[,]));

      Assert.AreEqual(2, errors.Count);
      Assert.AreEqual("[0][0] - 0 - Could not convert string to integer: a. Path '[0][0]', line 1, position 5.", errors[0]);
      Assert.AreEqual("[1][0] - 0 - Could not convert string to integer: b. Path '[1][0]', line 1, position 16.", errors[1]);
    }

    [Test]
    public void ErrorHandlingAndAvoidingRecursiveDepthError()
    {
      string json = "{'A':{'A':{'A':{'A':{'A':{}}}}}}";
      JsonSerializer serializer = new JsonSerializer() {};
      IList<string> errors = new List<string>();
      serializer.Error += (sender, e) =>
        {
          e.ErrorContext.Handled = true;
          errors.Add(e.ErrorContext.Path);
        };

      serializer.Deserialize<Nest>(new JsonTextReader(new StringReader(json)) {MaxDepth = 3});

      Assert.AreEqual(1, errors.Count);
      Assert.AreEqual("A.A.A", errors[0]);
    }

    public class Nest
    {
      public Nest A { get; set; }
    }

    [Test]
    public void InfiniteErrorHandlingLoopFromInputError()
    {
      IList<string> errors = new List<string>();

      JsonSerializer serializer = new JsonSerializer();
      serializer.Error += (sender, e) =>
        {
          errors.Add(e.ErrorContext.Error.Message);
          e.ErrorContext.Handled = true;
        };

      ErrorPerson[] result = serializer.Deserialize<ErrorPerson[]>(new JsonTextReader(new ThrowingReader()));

      Assert.IsNull(result);
      Assert.AreEqual(3, errors.Count);
      Assert.AreEqual("too far", errors[0]);
      Assert.AreEqual("too far", errors[1]);
      Assert.AreEqual("Infinite loop detected from error handling. Path '[1023]', line 1, position 65536.", errors[2]);
    }

    [Test]
    public void InfiniteLoopArrayHandling()
    {
      IList<string> errors = new List<string>();

      object o = JsonConvert.DeserializeObject(
        "[0,x]",
        typeof (int[]),
        new JsonSerializerSettings
          {
            Error = (sender, arg) =>
              {
                errors.Add(arg.ErrorContext.Error.Message);
                arg.ErrorContext.Handled = true;
              }
          });

      Assert.IsNull(o);

      Assert.AreEqual(3, errors.Count);
      Assert.AreEqual("Unexpected character encountered while parsing value: x. Path '[0]', line 1, position 3.", errors[0]);
      Assert.AreEqual("Unexpected character encountered while parsing value: x. Path '[0]', line 1, position 3.", errors[1]);
      Assert.AreEqual("Infinite loop detected from error handling. Path '[0]', line 1, position 3.", errors[2]);
    }

    [Test]
    public void InfiniteLoopArrayHandlingInObject()
    {
      IList<string> errors = new List<string>();

      Dictionary<string, int[]> o = JsonConvert.DeserializeObject<Dictionary<string, int[]>>(
        "{'badarray':[0,x,2],'goodarray':[0,1,2]}",
        new JsonSerializerSettings
          {
            Error = (sender, arg) =>
              {
                errors.Add(arg.ErrorContext.Error.Message);
                arg.ErrorContext.Handled = true;
              }
          });

      Assert.IsNull(o);

      Assert.AreEqual(4, errors.Count);
      Assert.AreEqual("Unexpected character encountered while parsing value: x. Path 'badarray[0]', line 1, position 15.", errors[0]);
      Assert.AreEqual("Unexpected character encountered while parsing value: x. Path 'badarray[0]', line 1, position 15.", errors[1]);
      Assert.AreEqual("Infinite loop detected from error handling. Path 'badarray[0]', line 1, position 15.", errors[2]);
      Assert.AreEqual("Unexpected character encountered while parsing value: x. Path 'badarray[0]', line 1, position 15.", errors[3]);
    }

    [Test]
    public void ErrorHandlingEndOfContent()
    {
      IList<string> errors = new List<string>();

      const string input = "{\"events\":[{\"code\":64411},{\"code\":64411,\"prio";

      const int maxDepth = 256;
      using (var jsonTextReader = new JsonTextReader(new StringReader(input)) {MaxDepth = maxDepth})
      {
        JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings {MaxDepth = maxDepth});
        jsonSerializer.Error += (sender, e) =>
          {
            errors.Add(e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
          };

        LogMessage logMessage = jsonSerializer.Deserialize<LogMessage>(jsonTextReader);

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
    public void ErrorHandlingEndOfContentDictionary()
    {
      IList<string> errors = new List<string>();

      const string input = "{\"events\":{\"code\":64411},\"events2\":{\"code\":64412,";

      const int maxDepth = 256;
      using (var jsonTextReader = new JsonTextReader(new StringReader(input)) {MaxDepth = maxDepth})
      {
        JsonSerializer jsonSerializer = JsonSerializer.Create(new JsonSerializerSettings {MaxDepth = maxDepth});
        jsonSerializer.Error += (sender, e) =>
          {
            errors.Add(e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
          };

        IDictionary<string, LogEvent> logEvents = jsonSerializer.Deserialize<IDictionary<string, LogEvent>>(jsonTextReader);

        Assert.IsNotNull(logEvents);
        Assert.AreEqual(2, logEvents.Count);
        Assert.AreEqual("64411", logEvents["events"].Code);
        Assert.AreEqual("64412", logEvents["events2"].Code);
      }

      Assert.AreEqual(2, errors.Count);
      Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[0]);
      Assert.AreEqual(@"Unexpected end when deserializing object. Path 'events2.code', line 1, position 49.", errors[1]);
    }

#if !(NET35 || NET20 || PORTABLE40)
    [Test]
    public void ErrorHandlingEndOfContentDynamic()
    {
      IList<string> errors = new List<string>();

      string json = @"{
  ""Explicit"": true,
  ""Decimal"": 99.9,
  ""Int"": 1,
  ""ChildObject"": {
    ""Integer"": 123";

      TestDynamicObject newDynamicObject = JsonConvert.DeserializeObject<TestDynamicObject>(json, new JsonSerializerSettings
        {
          Error = (sender, e) =>
          {
            errors.Add(e.ErrorContext.Error.Message);
            e.ErrorContext.Handled = true;
          }
        });
      Assert.AreEqual(true, newDynamicObject.Explicit);

      dynamic d = newDynamicObject;

      Assert.AreEqual(99.9, d.Decimal);
      Assert.AreEqual(1, d.Int);
      Assert.AreEqual(123, d.ChildObject.Integer);

      Assert.AreEqual(2, errors.Count);
      Assert.AreEqual(@"Unexpected end when deserializing object. Path 'ChildObject.Integer', line 6, position 19.", errors[0]);
      Assert.AreEqual(@"Unexpected end when deserializing object. Path 'ChildObject.Integer', line 6, position 19.", errors[1]);
    }
#endif

    [Test]
    public void WriteEndOnPropertyState()
    {
      JsonSerializerSettings settings = new JsonSerializerSettings();
      settings.Error += (obj, args) =>
                          {
                            args.ErrorContext.Handled = true;
                          };

      var data = new List<ErrorPerson2>()
                   {
                     new ErrorPerson2 {FirstName = "Scott", LastName = "Hanselman"},
                     new ErrorPerson2 {FirstName = "Scott", LastName = "Hunter"},
                     new ErrorPerson2 {FirstName = "Scott", LastName = "Guthrie"},
                   };

      Dictionary<string, IEnumerable<IErrorPerson2>> dictionary = data.GroupBy(person => person.FirstName).ToDictionary(group => @group.Key, group => @group.Cast<IErrorPerson2>());
      string output = JsonConvert.SerializeObject(dictionary, Formatting.None, settings);
      Assert.AreEqual(@"{""Scott"":[]}", output);
    }

    [Test]
    public void WriteEndOnPropertyState2()
    {
      JsonSerializerSettings settings = new JsonSerializerSettings();
      settings.Error += (obj, args) =>
        {
          args.ErrorContext.Handled = true;
        };

      var data = new List<ErrorPerson2>
        {
          new ErrorPerson2 {FirstName = "Scott", LastName = "Hanselman"},
          new ErrorPerson2 {FirstName = "Scott", LastName = "Hunter"},
          new ErrorPerson2 {FirstName = "Scott", LastName = "Guthrie"},
          new ErrorPerson2 {FirstName = "James", LastName = "Newton-King"},
        };

      Dictionary<string, IEnumerable<IErrorPerson2>> dictionary = data.GroupBy(person => person.FirstName).ToDictionary(group => @group.Key, group => @group.Cast<IErrorPerson2>());
      string output = JsonConvert.SerializeObject(dictionary, Formatting.None, settings);

      Assert.AreEqual(@"{""Scott"":[],""James"":[]}", output);
    }
  }

  interface IErrorPerson2
  {

  }

  class ErrorPerson2//:IPerson - oops! Forgot to implement the person interface
  {
    public string LastName { get; set; }
    public string FirstName { get; set; }
  }

  public class ThrowingReader : TextReader
  {
    private int _position = 0;
    private static string element = "{\"FirstName\":\"Din\",\"LastName\":\"Rav\",\"Item\":{\"ItemName\":\"temp\"}}";
    private bool _firstRead = true;
    private bool _readComma = false;

    public ThrowingReader()
    {
    }

    public override int Read(char[] buffer, int index, int count)
    {
      char[] temp = new char[buffer.Length];
      int charsRead = 0;
      if (_firstRead)
      {
        charsRead = new StringReader("[").Read(temp, index, count);
        _firstRead = false;
      }
      else
      {
        if (_readComma)
        {
          charsRead = new StringReader(",").Read(temp, index, count);
          _readComma = false;
        }
        else
        {
          charsRead = new StringReader(element).Read(temp, index, count);
          _readComma = true;
        }
      }

      _position += charsRead;
      if (_position > 65536)
      {
        throw new Exception("too far");
      }
      Array.Copy(temp, index, buffer, index, charsRead);
      return charsRead;
    }
  }

  public class ErrorPerson
  {
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public ErrorItem Item { get; set; }
  }

  public class ErrorItem
  {
    public string ItemName { get; set; }
  }

  [JsonObject]
  public class MyTypeWithRequiredMembers
  {
    [JsonProperty(Required = Required.AllowNull)] public string Required1;
    [JsonProperty(Required = Required.AllowNull)] public string Required2;
  }

  public class LogMessage
  {
    public string DeviceId { get; set; }
    public IList<LogEvent> Events { get; set; }
  }

  public class LogEvent
  {
    public string Code { get; set; }
    public int Priority { get; set; }
  }
}