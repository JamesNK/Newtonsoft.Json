using System;
using Newtonsoft.Json;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Converters
{
    public class DistinctReadAndWriteConverterTests : TestFixtureBase
    {
        [Test]
        public void SerializeString()
        {
            var settings = new JsonSerializerSettings()
            {
                Converters = { new ReadConverter(), new WriteConverter() }
            };
            var data = "testdata";
            var serialized = JsonConvert.SerializeObject(data, settings);
            Assert.AreEqual("\"C:testdata\"", serialized);
            var result = JsonConvert.DeserializeObject<string>(serialized, settings);
            Assert.AreEqual("testdata", result);
        }
    }

    public class ReadConverter : JsonConverter
    {
        public override bool CanWrite => false;
        public override bool CanRead => true;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value?.ToString()?.Substring(2);
        }
    }

    public class WriteConverter : JsonConverter
    {
        public override bool CanWrite => true;
        public override bool CanRead => false;
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue("C:" + value);
        }
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
