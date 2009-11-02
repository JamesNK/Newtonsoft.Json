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
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Tests.TestObjects;
using NUnit.Framework;
using Newtonsoft.Json.Serialization;
using System.IO;
using ErrorEventArgs=Newtonsoft.Json.Serialization.ErrorEventArgs;

namespace Newtonsoft.Json.Tests.Serialization
{
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
      Assert.AreEqual("Error message for member 1 = An item with the same key has already been added.", c.Messages[0]);
    }

    [Test]
    public void ErrorSerializingListHandled()
    {
      VersionKeyedCollection c = new VersionKeyedCollection();
      c.Add(new Person
      {
        Name = "Jim",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });
      c.Add(new Person
      {
        Name = "Jimbo",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });
      c.Add(new Person
      {
        Name = "Jimmy",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });
      c.Add(new Person
      {
        Name = "Jim Bean",
        BirthDate = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc),
        LastModified = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc)
      });

      string json = JsonConvert.SerializeObject(c, Formatting.Indented);

      Assert.AreEqual(@"[
  {
    ""Name"": ""Jimbo"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  },
  {
    ""Name"": ""Jim Bean"",
    ""BirthDate"": ""\/Date(978048000000)\/"",
    ""LastModified"": ""\/Date(978048000000)\/""
  }
]", json);

      Assert.AreEqual(2, c.Messages.Count);
      Assert.AreEqual("Error message for member 0 = Index even: 0", c.Messages[0]);
      Assert.AreEqual("Error message for member 2 = Index even: 2", c.Messages[1]);
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

      List<DateTime> c = JsonConvert.DeserializeObject<List<DateTime>>(@"[
        ""2009-09-09T00:00:00Z"",
        ""I am not a date and will error!"",
        [
          1
        ],
        ""1977-02-20T00:00:00Z"",
        null,
        ""2000-12-01T00:00:00Z""
      ]",
        new JsonSerializerSettings
          {
            Error = delegate(object sender, ErrorEventArgs args)
              {
                errors.Add(args.ErrorContext.Error.Message);
                args.ErrorContext.Handled = true;
              },
            Converters = { new IsoDateTimeConverter() }
          });

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
      Assert.AreEqual("The string was not recognized as a valid DateTime. There is a unknown word starting at index 0.", errors[0]);
      Assert.AreEqual("Unexpected token parsing date. Expected String, got StartArray.", errors[1]);
      Assert.AreEqual("Cannot convert null value to System.DateTime.", errors[2]);
    }

    [Test]
    public void DeserializingErrorInDateTimeCollectionWithAttributeWithEventNotCalled()
    {
      bool eventErrorHandlerCalled = false;

      DateTimeErrorObjectCollection c = JsonConvert.DeserializeObject<DateTimeErrorObjectCollection>(@"[
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

      try
      {
        JsonSerializer serializer = new JsonSerializer();
        serializer.Error += delegate(object sender, ErrorEventArgs args)
          {
            // only log an error once
            if (args.CurrentObject == args.ErrorContext.OriginalObject)
              errors.Add(args.ErrorContext.Error.Message);
          };

        serializer.Deserialize(new StringReader(json), typeof(List<List<DateTime>>));
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }

      Assert.AreEqual(1, errors.Count);
      Assert.AreEqual(@"Error converting value ""kjhkjhkjhkjh"" to type 'System.DateTime'.", errors[0]);
    }
  }
}