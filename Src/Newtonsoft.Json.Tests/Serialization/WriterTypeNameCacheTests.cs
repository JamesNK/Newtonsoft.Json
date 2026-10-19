#nullable enable
#if NET6_0_OR_GREATER
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Xunit;

namespace Newtonsoft.Json.Tests.Serialization
{
    public class WriterTypeNameCacheTests
    {
        [Theory]
        [InlineData(TypeNameHandling.Auto)]
        [InlineData(TypeNameHandling.Objects)]
        [InlineData(TypeNameHandling.All)]
        public void RepeatedRuntimeTypes_MatchUncachedBinderBytesAndRoundTrip(TypeNameHandling handling)
        {
            var repeated = new Payload { Value = 7 };
            var graph = new Graph
            {
                Items = new List<object>
                {
                    repeated, repeated, new Payload { Value = 8 },
                    new List<Payload> { new Payload { Value = 9 } },
                    new Dictionary<string, List<Payload>> { ["items"] = new List<Payload> { repeated } }
                }
            };
            graph.Self = graph;
            JsonSerializer cached = CreateSerializer(handling);
            cached.PreserveReferencesHandling = PreserveReferencesHandling.All;
            JsonSerializer uncached = CreateSerializer(handling);
            uncached.PreserveReferencesHandling = PreserveReferencesHandling.All;
            uncached.SerializationBinder = new ForwardingBinder();

            string json = Serialize(cached, graph);
            Assert.Equal(Serialize(uncached, graph), json);
            using var reader = new JsonTextReader(new StringReader(json));
            Graph result = Assert.IsType<Graph>(cached.Deserialize<Graph>(reader));
            Assert.Same(result, result.Self);
            Assert.Same(result.Items[0], result.Items[1]);
            Assert.Equal(7, Assert.IsType<Payload>(result.Items[0]).Value);
        }

        [Fact]
        public void Converter_ChangesFormatAfterWarmWrite_UsesCurrentFormat()
        {
            JsonSerializer serializer = CreateSerializer();
            string json = Serialize(serializer, new Script((writer, proxy) =>
            {
                writer.WriteStartArray();
                proxy.Serialize(writer, new Payload());
                proxy.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
                proxy.Serialize(writer, new Payload());
                proxy.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple;
                proxy.Serialize(writer, new Payload());
                writer.WriteEndArray();
            }));
            Assert.Equal(new[] { Name(TypeNameAssemblyFormatHandling.Simple), Name(TypeNameAssemblyFormatHandling.Full), Name(TypeNameAssemblyFormatHandling.Simple) }, Names(json));
        }

        [Fact]
        public void Converter_ReplacesBinderAfterWarmWrite_UsesEachAliasThenRestoresDefault()
        {
            JsonSerializer serializer = CreateSerializer();
            ISerializationBinder original = serializer.SerializationBinder;
            var binder = new ChangingBinder();
            string json = Serialize(serializer, new Script((writer, proxy) =>
            {
                writer.WriteStartArray();
                proxy.Serialize(writer, new Payload());
                proxy.SerializationBinder = binder;
                proxy.Serialize(writer, new Payload());
                proxy.Serialize(writer, new Payload());
                proxy.SerializationBinder = original;
                proxy.Serialize(writer, new Payload());
                writer.WriteEndArray();
            }));
            Assert.Equal(new[] { Name(TypeNameAssemblyFormatHandling.Simple), "Alias1", "Alias2", Name(TypeNameAssemblyFormatHandling.Simple) }, Names(json));
            Assert.Equal(2, binder.Calls);
        }

        [Fact]
        public void DefaultBinderSubclass_IsInvokedForEveryMetadataProperty()
        {
            JsonSerializer serializer = CreateSerializer();
            var binder = new ChangingDefaultBinder();
            serializer.SerializationBinder = binder;
            Assert.Equal(new[] { "Subclass1", "Subclass2" }, Names(Serialize(serializer, new[] { new Payload(), new Payload() })));
            Assert.Equal(2, binder.Calls);
        }

        [Fact]
        public void MutableTypeDelegator_CustomContractReadsCurrentNameForEveryWrite()
        {
            var type = new MutableType(typeof(Payload));
            JsonSerializer serializer = CreateSerializer();
            serializer.ContractResolver = new MutableTypeResolver(type);
            string json = Serialize(serializer, new Script((writer, proxy) =>
            {
                writer.WriteStartArray();
                type.CurrentName = "First";
                proxy.Serialize(writer, new Payload());
                type.CurrentName = "Second";
                proxy.Serialize(writer, new Payload());
                writer.WriteEndArray();
            }));
            string assembly = typeof(Payload).Assembly.GetName().Name ?? throw new InvalidOperationException();
            Assert.Equal(new[] { "First, " + assembly, "Second, " + assembly }, Names(json));
        }

        [Fact]
        public void CustomBinder_HandledNameFailure_RetriesNextObject()
        {
            JsonSerializer serializer = CreateSerializer();
            var binder = new ChangingBinder { ThrowFirst = true };
            serializer.SerializationBinder = binder;
            int errors = 0;
            serializer.Error += (_, args) =>
            {
                errors++;
                Assert.IsType<InvalidOperationException>(args.ErrorContext.Error);
                args.ErrorContext.Handled = true;
            };
            JArray values = JArray.Parse(Serialize(serializer, new[] { new Payload(), new Payload() }));
            Assert.Equal(1, errors);
            Assert.Equal(2, binder.Calls);
            Assert.Null(values[0]["$type"]);
            Assert.Equal("Alias2", (string?)values[1]["$type"]);
        }

        [Fact]
        public void MutableTypeDelegator_HandledNameFailure_RetriesNextObject()
        {
            var type = new MutableType(typeof(Payload));
            JsonSerializer serializer = CreateSerializer();
            serializer.ContractResolver = new MutableTypeResolver(type);
            type.ThrowNextNameRead = true;
            int errors = 0;
            serializer.Error += (_, args) => { errors++; args.ErrorContext.Handled = true; };
            JArray values = JArray.Parse(Serialize(serializer, new[] { new Payload(), new Payload() }));
            Assert.Equal(1, errors);
            Assert.Null(values[0]["$type"]);
            Assert.Equal(Name(TypeNameAssemblyFormatHandling.Simple), (string?)values[1]["$type"]);
        }

        [Fact]
        public void EscapedConverterProxy_LaterSerializationHonorsChangedBinder()
        {
            JsonSerializer serializer = CreateSerializer();
            JsonSerializer? escaped = null;
            Serialize(serializer, new Script((writer, proxy) =>
            {
                escaped = proxy;
                proxy.Serialize(writer, new Payload());
            }));
            JsonSerializer retainedProxy = Assert.IsAssignableFrom<JsonSerializer>(escaped);
            var binder = new ChangingBinder();
            retainedProxy.SerializationBinder = binder;
            JObject value = JObject.Parse(Serialize(retainedProxy, new Payload()));
            Assert.Equal("Alias1", (string?)value["$type"]);
            Assert.Equal(1, binder.Calls);
        }

        [Fact]
        public void RepeatedTypeNames_PreserveVerboseTraceEvents()
        {
            var trace = new MemoryTraceWriter { LevelFilter = TraceLevel.Verbose };
            JsonSerializer serializer = CreateSerializer();
            serializer.TraceWriter = trace;
            Serialize(serializer, new[] { new Payload(), new Payload(), new Payload() });
            string[] messages = trace.GetTraceMessages().Where(message => message.Contains("Writing type name '", StringComparison.Ordinal)).ToArray();
            Assert.Equal(3, messages.Length);
            Assert.All(messages, message => Assert.Contains(Name(TypeNameAssemblyFormatHandling.Simple), message));
        }

        private static JsonSerializer CreateSerializer(TypeNameHandling handling = TypeNameHandling.Objects)
        {
            var serializer = JsonSerializer.Create(new JsonSerializerSettings
            {
                TypeNameHandling = handling,
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple
            });
            serializer.Converters.Add(new ScriptConverter());
            return serializer;
        }

        private static string Serialize(JsonSerializer serializer, object value)
        {
            using var text = new StringWriter(CultureInfo.InvariantCulture);
            using var writer = new JsonTextWriter(text);
            serializer.Serialize(writer, value);
            writer.Flush();
            return text.ToString();
        }

        private static string Name(TypeNameAssemblyFormatHandling format) =>
            ReflectionUtils.GetTypeName(typeof(Payload), format, DefaultSerializationBinder.Instance);

        private static string?[] Names(string json) => JArray.Parse(json).Select(item => (string?)item["$type"]).ToArray();

        public class Payload { public int Value { get; set; } }
        public class Graph { public List<object> Items { get; set; } = new List<object>(); public Graph? Self { get; set; } }

        private sealed class Script
        {
            internal Action<JsonWriter, JsonSerializer> Write { get; }
            internal Script(Action<JsonWriter, JsonSerializer> write) { Write = write; }
        }

        private sealed class ScriptConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => objectType == typeof(Script);
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is Script script) { script.Write(writer, serializer); }
                else { throw new InvalidOperationException(); }
            }
            public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer) => throw new NotSupportedException();
        }

        private sealed class ForwardingBinder : ISerializationBinder
        {
            public Type BindToType(string? assemblyName, string typeName) => DefaultSerializationBinder.Instance.BindToType(assemblyName, typeName);
            public void BindToName(Type serializedType, out string? assemblyName, out string? typeName) => DefaultSerializationBinder.Instance.BindToName(serializedType, out assemblyName, out typeName);
        }

        private sealed class ChangingBinder : ISerializationBinder
        {
            internal int Calls { get; private set; }
            internal bool ThrowFirst { get; set; }
            public Type BindToType(string? assemblyName, string typeName) => typeof(Payload);
            public void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
            {
                Calls++;
                if (ThrowFirst && Calls == 1) { throw new InvalidOperationException("Expected first name failure."); }
                assemblyName = null;
                typeName = "Alias" + Calls.ToString(CultureInfo.InvariantCulture);
            }
        }

        private sealed class ChangingDefaultBinder : DefaultSerializationBinder
        {
            internal int Calls { get; private set; }
            public override void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
            {
                assemblyName = null;
                typeName = "Subclass" + (++Calls).ToString(CultureInfo.InvariantCulture);
            }
        }

        private sealed class MutableType : TypeDelegator
        {
            internal string? CurrentName { get; set; }
            internal bool ThrowNextNameRead { get; set; }
            internal MutableType(Type type) : base(type) { CurrentName = type.FullName; }
            public override string? FullName
            {
                get
                {
                    if (ThrowNextNameRead) { ThrowNextNameRead = false; throw new InvalidOperationException("Expected mutable Type failure."); }
                    return CurrentName;
                }
            }
        }

        private sealed class MutableTypeResolver : IContractResolver
        {
            private readonly JsonContract _contract;
            private readonly DefaultContractResolver _fallback = new DefaultContractResolver();
            internal MutableTypeResolver(Type type) { _contract = new JsonObjectContract(type); }
            public JsonContract ResolveContract(Type type) => type == typeof(Payload) ? _contract : _fallback.ResolveContract(type);
        }
    }
}
#endif
