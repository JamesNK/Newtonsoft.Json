using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class ConcurrencyTests : TestFixtureBase
    {
        DefaultContractResolver defaultContractResolver = new DefaultContractResolver();

        [Test]
        public void ShouldNotFailWhenPopulatingDefaultValuesConccurently()
        {
            var root = new Root
            {
                Objects = new List<LargeObject>()
            };

            for (int i = 0; i < 1000; i++)
            {
                root.Objects.Add(new LargeObject());
            }

            var bytes = Serialize(root);

            Console.WriteLine("setup done");
            for (var i = 0; i < 20; i++)
            {
                Console.WriteLine();
                Console.WriteLine("round: " + i);
                defaultContractResolver = new DefaultContractResolver();
                Parallel.For(0, 100, x =>
                {
                    var @case = (Root)Deserialize(bytes, typeof(Root));
                    if (@case.Events == null)
                    {
                        Console.WriteLine("Not ok on " + Thread.CurrentThread.ManagedThreadId);
                        Assert.NotNull(@case.Events);
                    }
                    else
                    {
                        Console.WriteLine("Ok on " + Thread.CurrentThread.ManagedThreadId);
                    }
                });
            }

        }

        public class Root
        {
            public List<object> Events { get; set; }
            public List<LargeObject> Objects { get; set; }
        }

        public class LargeObject
        {
            public int    A1 { get; set; }   
            public string B1 { get; set; }   
            public double C1 { get; set; }   
            public int    A2 { get; set; }   
            public string B2 { get; set; }   
            public double C2 { get; set; }   
            public int    A3 { get; set; }   
            public string B3 { get; set; }   
            public double C3 { get; set; }   
            public int    A4 { get; set; }   
            public string B4 { get; set; }   
            public double C4 { get; set; }   
            public int    A5 { get; set; }   
            public string B5 { get; set; }   
            public double C5 { get; set; }   
            public int    A6 { get; set; }   
            public string B6 { get; set; }   
            public double C6 { get; set; }   
            public int    A7 { get; set; }   
            public string B7 { get; set; }   
            public double C7 { get; set; }   
            public int    A8 { get; set; }   
            public string B8 { get; set; }   
            public double C8 { get; set; }   
            public int    A9 { get; set; }   
            public string B9 { get; set; }   
            public double C9 { get; set; }   
            public int    A10 { get; set; }   
            public string B10 { get; set; }   
            public double C10 { get; set; }   
            public int    A11 { get; set; }   
            public string B11 { get; set; }   
            public double C11 { get; set; }   
            public int    A12 { get; set; }   
            public string B12 { get; set; }   
            public double C12 { get; set; }   
            public int    A13 { get; set; }   
            public string B13 { get; set; }   
            public double C13 { get; set; }   
            public int    A14 { get; set; }   
            public string B14 { get; set; }   
            public double C14 { get; set; }   
            public int    A15 { get; set; }   
            public string B15 { get; set; }   
            public double C15 { get; set; }   
            public int    A16 { get; set; }   
            public string B16 { get; set; }   
            public double C16 { get; set; }   
            public int    A17 { get; set; }   
            public string B17 { get; set; }   
            public double C17 { get; set; }   
            public int    A18 { get; set; }   
            public string B18 { get; set; }   
            public double C18 { get; set; }   
            public int    A19 { get; set; }   
            public string B19 { get; set; }   
            public double C19 { get; set; }   
        }

        public JsonSerializer CreateSerializer()
        {
            return JsonSerializer.Create(new JsonSerializerSettings
            {
                ContractResolver = defaultContractResolver,
            });
        }

        public byte[] Serialize(object obj)
        {
            using (var outStream = new MemoryStream())
            using (var bsonWriter = new BsonWriter(outStream))
            {
                CreateSerializer().Serialize(bsonWriter, obj);
                return outStream.ToArray();
            }
        }

        public object Deserialize(byte[] data, Type type)
        {
            using (var inStream = new MemoryStream(data))
            using (var bsonReader = new BsonReader(inStream))
            {
                return CreateSerializer().Deserialize(bsonReader, type);
            }
        }

        public class DefaultContractResolver : Json.Serialization.DefaultContractResolver
        {
            readonly object mutex = new object();
            readonly ConcurrentDictionary<Type, JsonContract> contracts = new ConcurrentDictionary<Type, JsonContract>();  

            public DefaultContractResolver() : base(shareCache: false)
            {
            }

            public override JsonContract ResolveContract(Type type)
            {
                lock (mutex)
                {
                    return contracts.GetOrAdd(type, key => base.ResolveContract(type));
                }
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);

                if (typeof(Root).IsAssignableFrom(member.DeclaringType) && property.PropertyName == "Events")
                {
                    property.ValueProvider = new SetOnlyValueProvider<Root>((root, o) =>
                    {
                        Console.WriteLine("setvalue: " + Thread.CurrentThread.ManagedThreadId);
                        root.Events = new List<object>();
                    });

                    property.Readable = false;
                    property.DefaultValueHandling = DefaultValueHandling.Populate;
                }

                return property;
            }
        }

        public class SetOnlyValueProvider<T> : IValueProvider
        {
            readonly Action<T, object> provider;

            public SetOnlyValueProvider(Action<T, object> provider)
            {
                this.provider = provider;
            }

            public void SetValue(object target, object value)
            {
                provider((T)target, value);
            }

            public object GetValue(object target)
            {
                throw new Exception();
            }
        }
    }
}