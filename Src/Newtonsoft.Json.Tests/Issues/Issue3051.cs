using System;
using System.Collections.Generic;
using System.Linq;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue3051
    {
        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3051
        public void Guid_InObjectTypedListProperty_RoundTripsWithTypeNameHandlingAll()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            Record recordPre = new Record
            {
                RecordIdentifiers = new List<object> { Guid.NewGuid() }
            };

            string json = JsonConvert.SerializeObject(recordPre, settings);

            Record recordPost = JsonConvert.DeserializeObject<Record>(json, settings);

            Assert.IsNotNull(recordPost);
            Assert.IsNotNull(recordPost.RecordIdentifiers);
            Assert.AreEqual(1, recordPost.RecordIdentifiers.Count);

            object postValue = recordPost.RecordIdentifiers.Single();

            Assert.AreEqual(typeof(Guid), postValue.GetType());
            Assert.AreEqual(recordPre.RecordIdentifiers.Single(), postValue);
        }

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3051
        public void TimeSpan_InObjectTypedListProperty_RoundTripsWithTypeNameHandlingAll()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            Record recordPre = new Record
            {
                RecordIdentifiers = new List<object> { TimeSpan.FromMinutes(90) }
            };

            string json = JsonConvert.SerializeObject(recordPre, settings);

            Record recordPost = JsonConvert.DeserializeObject<Record>(json, settings);

            object postValue = recordPost.RecordIdentifiers.Single();

            Assert.AreEqual(typeof(TimeSpan), postValue.GetType());
            Assert.AreEqual(recordPre.RecordIdentifiers.Single(), postValue);
        }

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3051
        public void Uri_InObjectTypedListProperty_RoundTripsWithTypeNameHandlingAll()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            Record recordPre = new Record
            {
                RecordIdentifiers = new List<object> { new Uri("https://www.newtonsoft.com/json") }
            };

            string json = JsonConvert.SerializeObject(recordPre, settings);

            Record recordPost = JsonConvert.DeserializeObject<Record>(json, settings);

            object postValue = recordPost.RecordIdentifiers.Single();

            Assert.AreEqual(typeof(Uri), postValue.GetType());
            Assert.AreEqual(recordPre.RecordIdentifiers.Single(), postValue);
        }

        [Test]
        //https://github.com/JamesNK/Newtonsoft.Json/issues/3051
        public void DateTimeOffset_InObjectTypedListProperty_RoundTripsWithTypeNameHandlingAll()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };

            DateTimeOffset dateTimeOffset = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.FromHours(5));

            Record recordPre = new Record
            {
                RecordIdentifiers = new List<object> { dateTimeOffset }
            };

            string json = JsonConvert.SerializeObject(recordPre, settings);

            Record recordPost = JsonConvert.DeserializeObject<Record>(json, settings);

            object postValue = recordPost.RecordIdentifiers.Single();

            Assert.AreEqual(typeof(DateTimeOffset), postValue.GetType());
            Assert.AreEqual(dateTimeOffset, postValue);
        }

        public class Record
        {
            public IReadOnlyList<object> RecordIdentifiers { get; set; }
        }
    }
}
