#region License

// Copyright (c) 2007 James Newton-King
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
// associated documentation files (the "Software"), to deal in the Software without restriction,
// including without limitation the rights to use, copy, modify, merge, publish, distribute,
// sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT
// NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM,
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT
// OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion

#if NET35 || NET40 || NET45

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class ProjectionDrivenConverterTests : TestFixtureBase
    {
        public class RelatedNestedClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public DateTimeOffset Created { get; set; } = DateTimeOffset.Now.UtcDateTime;
        }

        public class TestObjectClass
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public TestObjectClass Parent { get; set; }
            public string Ignored { get; set; }
            public IEnumerable<RelatedNestedClass> Attachments { get; set; }
        }

        [Test]
        public void SerializeProjection()
        {
            var settings = new JsonSerializerSettings()
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Converters = new List<JsonConverter>() {
                    new IsoDateTimeConverter() { DateTimeStyles = System.Globalization.DateTimeStyles.AdjustToUniversal, DateTimeFormat = IsoDateTimeConverter.UtcDateTimeFormat },
                    new ProjectionConverter(new Dictionary<Type, HashSet<string>>() {
                        { typeof(TestObjectClass), new HashSet<string>() { "Id", "Title", "Parent.Title", "Attachments.Name", "Attachments.Created" } }
                    })
                },
            };

            var testObject = new TestObjectClass()
            {
                Id = 12,
                Title = "MyTestExample",
                Attachments = new List<RelatedNestedClass>() {
                    new RelatedNestedClass() { Id = 1, Name = "Attachment1", Created = DateTimeOffset.Parse("2017-07-27T13:57:59.0359181Z")},
                    new RelatedNestedClass() { Id = 2, Name = "Attachment2", Created = DateTimeOffset.Parse("2017-07-27T13:57:59.0359181+00:00")},
                    new RelatedNestedClass() { Id = 3, Name = "Attachment3", Created = DateTimeOffset.Parse("2017-07-27T15:57:59.0359181+02:00")},
                }
            };
            testObject.Parent = testObject;
            var output = JsonConvert.SerializeObject(testObject, settings);
            Assert.AreEqual(output, "{\"Id\":12,\"Title\":\"MyTestExample\",\"Parent\":{\"Title\":\"MyTestExample\"},\"Attachments\":[{\"Name\":\"Attachment1\",\"Created\":\"2017-07-27T13:57:59.0359181Z\"},{\"Name\":\"Attachment2\",\"Created\":\"2017-07-27T13:57:59.0359181Z\"},{\"Name\":\"Attachment3\",\"Created\":\"2017-07-27T13:57:59.0359181Z\"}]}");
        }
    }
}

#endif