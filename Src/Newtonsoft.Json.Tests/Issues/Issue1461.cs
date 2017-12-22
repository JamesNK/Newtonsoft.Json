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

#if !(NET20 || NET35)
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Xml;
using Newtonsoft.Json.Serialization;
#if !NET20
using System.Xml.Linq;
#endif
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue1461 : TestFixtureBase
    {
        [Test]
        public void Test()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                Converters = new JsonConverter[] { new IdJsonConverter() },
                TraceWriter = new TraceWriter(),
            };

            TestObject test = new TestObject { Id = "test" };

            JsonSerializer serializer = JsonSerializer.Create(settings);

            MemoryStream stream = new MemoryStream();

            StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8);
            using (JsonTextWriter writer = new JsonTextWriter(streamWriter))
            {
                writer.CloseOutput = false;
                serializer.Serialize(writer, test);
                writer.Flush();
            }
            stream.Position = 0;

            StreamReader reader = new StreamReader(stream);
            Assert.AreEqual(@"{""Id"":""test""}", reader.ReadToEnd());
        }

        private class TestObject
        {
            public Id Id { get; set; }
        }

        private class TraceWriter : ITraceWriter
        {
            public TraceLevel LevelFilter => TraceLevel.Verbose;

            public void Trace(TraceLevel level, string message, Exception ex)
            {
                Console.WriteLine(message);
            }
        }

        private class IdJsonConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => typeof(Id) == objectType;

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (reader.TokenType == JsonToken.Integer)
                    return new Id((long)reader.Value);

                string str = reader.Value as string;
                Guid guid;
                return Guid.TryParse(str, out guid) ? new Id(guid) : new Id(str);
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value == null)
                {
                    writer.WriteNull();
                    return;
                }

                Id id = (Id)value;
                writer.WriteValue(id.Value);
            }
        }

        private class Id : IEquatable<Id>
        {
            internal object Value { get; set; }

            public Id(string id) { Value = id; }
            public Id(long id) { Value = id; }
            public Id(Guid id) { Value = id; }

            public static implicit operator Id(string id) => new Id(id);
            public static implicit operator Id(long id) => new Id(id);
            public static implicit operator Id(Guid id) => new Id(id);

            public static implicit operator string(Id id) => (string)id.Value;
            public static implicit operator long(Id id) => (long)id.Value;
            public static implicit operator Guid(Id id) => (Guid)id.Value;

            public override string ToString()
            {
                return Value.ToString();
            }

            public bool Equals(Id other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Value, other.Value);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                return obj.GetType() == this.GetType() && Equals((Id)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Value?.GetHashCode() ?? 0) * 397);
                }
            }

            public static bool operator ==(Id left, Id right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(Id left, Id right)
            {
                return !Equals(left, right);
            }
        }
    }
}
#endif