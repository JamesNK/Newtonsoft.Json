using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Converters
{
    [TestFixture]
    public class UnionConverterTests : TestFixtureBase
    {
        [Union]
        public struct IntOrString
        {
            public object Value { get; }
            public IntOrString(int value) { Value = value; }
            public IntOrString(string value) { Value = value; }
        }

        [Test]
        public void TransparentScalarRoundTrip()
        {
            Assert.AreEqual("42", JsonConvert.SerializeObject(new IntOrString(42)));
            Assert.AreEqual("\"hello\"", JsonConvert.SerializeObject(new IntOrString("hello")));
            Assert.AreEqual(42, JsonConvert.DeserializeObject<IntOrString>("42").Value);
            Assert.AreEqual("hello", JsonConvert.DeserializeObject<IntOrString>("\"hello\"").Value);
        }

        [Test]
        public void DetectMarker()
        {
            UnionConverter converter = new UnionConverter();
            Assert.IsTrue(converter.CanConvert(typeof(IntOrString)));
            Assert.IsTrue(converter.CanConvert(typeof(IntOrString?)));
            Assert.IsFalse(converter.CanConvert(typeof(object)));
        }

        [Union]
        public struct Shapes
        {
            public object Value { get; }
            public Shapes(decimal value) { Value = value; }
            public Shapes(string value) { Value = value; }
            public Shapes(bool value) { Value = value; }
            public Shapes(Payload value) { Value = value; }
            public Shapes(int[] value) { Value = value; }
        }

        public class Payload
        {
            public string Name { get; set; }
        }

        [Test]
        public void RoundTripAllShapes()
        {
            string[] jsonValues = { "42.5", "\"hello\"", "true", "{\"Name\":\"test\"}", "[1,2]" };
            Type[] types = { typeof(decimal), typeof(string), typeof(bool), typeof(Payload), typeof(int[]) };
            for (int index = 0; index < jsonValues.Length; index++)
            {
                Shapes union = JsonConvert.DeserializeObject<Shapes>(jsonValues[index]);
                Assert.AreEqual(types[index], union.Value.GetType());
                Assert.AreEqual(jsonValues[index], JsonConvert.SerializeObject(union));
            }
        }

        [Union]
        public struct Overlapping
        {
            public object Value { get; }
            public Overlapping(int value) { Value = value; }
            public Overlapping(long value) { Value = value; }
            public Overlapping(string value) { Value = value; }
        }

        [Test]
        public void AmbiguityOnlyAffectsMatchingShape()
        {
            Assert.AreEqual("42", JsonConvert.SerializeObject(new Overlapping(42)));
            Assert.AreEqual("hello", JsonConvert.DeserializeObject<Overlapping>("\"hello\"").Value);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Overlapping>("42"));
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Overlapping>("true"));
        }

        [Union]
        public struct Numbers
        {
            public object Value { get; }
            public Numbers(int value) { Value = value; }
        }

        [Union]
        public struct NullableNumber
        {
            public object Value { get; }
            public bool Constructed { get; }
            public NullableNumber(int? value) { Value = value; Constructed = true; }
        }

        [Test]
        public void NullAndDefaultStates()
        {
            Assert.AreEqual("null", JsonConvert.SerializeObject(default(Numbers)));
            Assert.IsNull(JsonConvert.DeserializeObject<Numbers>("null").Value);
            Assert.IsNull(JsonConvert.DeserializeObject<Numbers?>("null"));
            NullableNumber nullable = JsonConvert.DeserializeObject<NullableNumber>("null");
            Assert.IsTrue(nullable.Constructed);
            Assert.IsNull(nullable.Value);
            Assert.AreEqual("null", JsonConvert.SerializeObject(nullable));
            Assert.AreEqual(42, JsonConvert.DeserializeObject<NullableNumber>("42").Value);
        }

        [Union]
        public struct Objects
        {
            public object Value { get; }
            public Objects(Payload value) { Value = value; }
            public Objects(Dictionary<string, int> value) { Value = value; }
        }

        [Union]
        public struct Arrays
        {
            public object Value { get; }
            public Arrays(int[] value) { Value = value; }
            public Arrays(List<string> value) { Value = value; }
        }

        [Test]
        public void TypeMetadataDisambiguatesObjectsAndArrays()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string objectJson = JsonConvert.SerializeObject(new Objects(new Payload { Name = "test" }), settings);
            Assert.IsTrue(objectJson.Contains("\"$type\""));
            Objects objectResult = JsonConvert.DeserializeObject<Objects>(objectJson, settings);
            Assert.AreEqual("test", ((Payload)objectResult.Value).Name);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Objects>("{\"Name\":\"test\"}"));

            string arrayJson = JsonConvert.SerializeObject(new Arrays(new int[] { 1, 2 }), settings);
            Assert.IsTrue(arrayJson.Contains("\"$values\""));
            Arrays arrayResult = JsonConvert.DeserializeObject<Arrays>(arrayJson, settings);
            CollectionAssert.AreEqual(new int[] { 1, 2 }, (int[])arrayResult.Value);
        }

        [Union]
        public class Recursive
        {
            public object Value { get; set; }
            public Recursive(bool value) { Value = value; }
            public Recursive(Recursive value) { Value = value; }
        }

        [Test]
        public void RecursiveUnionFlattensAndRejectsCycles()
        {
            Assert.AreEqual("true", JsonConvert.SerializeObject(new Recursive(new Recursive(true))));
            Assert.AreEqual(true, JsonConvert.DeserializeObject<Recursive>("true").Value);
            Recursive cyclic = new Recursive(true);
            cyclic.Value = cyclic;
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(cyclic));
        }

        public class MemberMetadata
        {
            [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
            public Objects Value { get; set; }

            [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
            public List<Arrays> Items { get; set; }
        }

        [Test]
        public void MemberTypeNameHandlingDoesNotEnableUnionMetadata()
        {
            MemberMetadata value = new MemberMetadata
            {
                Value = new Objects(new Payload { Name = "test" }),
                Items = new List<Arrays> { new Arrays(new int[] { 1, 2 }) }
            };
            Assert.AreEqual("{\"Value\":{\"Name\":\"test\"},\"Items\":[[1,2]]}", JsonConvert.SerializeObject(value));

            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto };
            string json = JsonConvert.SerializeObject(value, settings);
            Assert.IsTrue(json.Contains("\"$type\""));
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<MemberMetadata>(json));
            MemberMetadata result = JsonConvert.DeserializeObject<MemberMetadata>(json, settings);
            Assert.AreEqual("test", ((Payload)result.Value.Value).Name);
            CollectionAssert.AreEqual(new int[] { 1, 2 }, (int[])result.Items[0].Value);
        }

        [Test]
        public void DateLookingStringStaysStringWithDateParsingDisabled()
        {
            const string date = "2026-09-21T12:34:56.000Z";
            JsonSerializerSettings settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.None };
            Assert.AreEqual(date, JsonConvert.DeserializeObject<IntOrString>("\"" + date + "\"", settings).Value);
            Assert.AreEqual(date, JsonConvert.DeserializeObject<List<IntOrString>>("[\"" + date + "\"]", settings)[0].Value);
            Assert.AreEqual(date, JsonConvert.DeserializeObject<Dictionary<string, IntOrString>>("{\"value\":\"" + date + "\"}", settings)["value"].Value);
            Assert.AreEqual(date, ((object[])JsonConvert.DeserializeObject<Pair<object[], bool>>("[\"" + date + "\"]", settings).Value)[0]);
        }

        [Test]
        public void DateParseHandlingIsRespected()
        {
            const string json = "\"2026-09-21T12:34:56.000Z\"";
            object expected = JsonConvert.DeserializeObject<object>(json);
            Assert.AreEqual(typeof(DateTime), expected.GetType());
            Assert.AreEqual(expected, JsonConvert.DeserializeObject<Pair<object, bool>>(json).Value);
            Assert.AreEqual(expected, ((object[])JsonConvert.DeserializeObject<Pair<object[], bool>>("[" + json + "]").Value)[0]);
#if HAVE_DATE_TIME_OFFSET
            JsonSerializerSettings settings = new JsonSerializerSettings { DateParseHandling = DateParseHandling.DateTimeOffset };
            expected = JsonConvert.DeserializeObject<object>(json, settings);
            Assert.AreEqual(typeof(DateTimeOffset), expected.GetType());
            Assert.AreEqual(expected, JsonConvert.DeserializeObject<Pair<object, bool>>(json, settings).Value);
            Assert.AreEqual(expected, ((object[])JsonConvert.DeserializeObject<Pair<object[], bool>>("[" + json + "]", settings).Value)[0]);
#endif
        }

        public class DerivedPayload : Payload
        {
            public int Extra { get; set; }
        }

        [Test]
        public void DefaultSerializationUsesRuntimeType()
        {
            Assert.AreEqual("{\"Extra\":42,\"Name\":\"test\"}", JsonConvert.SerializeObject(new Shapes(new DerivedPayload { Name = "test", Extra = 42 })));
        }

        [Union]
        public struct TaggedUnion
        {
            private readonly Payload _value;
            private readonly bool _derived;
            public object Value { get { throw new InvalidOperationException("Use TryGetValue."); } }
            public TaggedUnion(Payload value) { _value = value; _derived = false; }
            public TaggedUnion(DerivedPayload value) { _value = value; _derived = true; }
            public bool TryGetValue(out Payload value) { value = _value; return !_derived; }
            public bool TryGetValue(out DerivedPayload value) { value = _value as DerivedPayload; return _derived; }
        }

        [Test]
        public void TryGetValueSuppliesContainedValue()
        {
            DerivedPayload payload = new DerivedPayload { Name = "test", Extra = 42 };
            Assert.AreEqual("{\"Extra\":42,\"Name\":\"test\"}", JsonConvert.SerializeObject(new TaggedUnion((Payload)payload)));
            Assert.AreEqual("{\"Extra\":42,\"Name\":\"test\"}", JsonConvert.SerializeObject(new TaggedUnion(payload)));
        }

        public class AliasBinder : ISerializationBinder
        {
            public Type ResolvedType { get; set; }
            public int ResolveCount { get; private set; }
            public void BindToName(Type serializedType, out string assemblyName, out string typeName)
            {
                assemblyName = null;
                typeName = "payload";
            }
            public Type BindToType(string assemblyName, string typeName)
            {
                ResolveCount++;
                Assert.AreEqual("payload", typeName);
                return ResolvedType ?? typeof(Payload);
            }
        }

        public class Unrelated
        {
            public static bool Constructed;
            public Unrelated() { Constructed = true; }
        }

        [Test]
        public void BinderRejectsNonCaseBeforeConstruction()
        {
            AliasBinder binder = new AliasBinder();
            JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, SerializationBinder = binder };
            string json = JsonConvert.SerializeObject(new Objects(new Payload { Name = "test" }), settings);
            Assert.AreEqual("{\"$type\":\"payload\",\"Name\":\"test\"}", json);
            Assert.AreEqual("test", ((Payload)JsonConvert.DeserializeObject<Objects>(json, settings).Value).Name);
            Assert.AreEqual(1, binder.ResolveCount);
            Unrelated.Constructed = false;
            binder.ResolvedType = typeof(Unrelated);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Objects>(json, settings));
            Assert.IsFalse(Unrelated.Constructed);
        }

        [Union]
        public struct Pair<TFirst, TSecond>
        {
            public object Value { get; }
            public Pair(TFirst value) { Value = value; }
            public Pair(TSecond value) { Value = value; }
        }

        [Test]
        public void PrimitiveAndGenericCases()
        {
            Guid guid = new Guid("deaf6e94-cc78-48ad-a0f9-642f397a943a");
            Assert.AreEqual(guid, JsonConvert.DeserializeObject<Pair<Guid, bool>>("\"" + guid + "\"").Value);
            Assert.AreEqual(DayOfWeek.Monday, JsonConvert.DeserializeObject<Pair<DayOfWeek, bool>>("1").Value);
            Assert.AreEqual(typeof(DateTime), JsonConvert.DeserializeObject<Pair<DateTime, bool>>("\"2026-09-21T00:00:00Z\"").Value.GetType());
            CollectionAssert.AreEqual(new byte[] { 1, 2 }, (byte[])JsonConvert.DeserializeObject<Pair<byte[], bool>>("\"AQI=\"").Value);
            Assert.AreEqual(42, ((Dictionary<string, int>)JsonConvert.DeserializeObject<Pair<Dictionary<string, int>, bool>>("{\"answer\":42}").Value)["answer"]);
            Assert.AreEqual("hello", JsonConvert.DeserializeObject<Pair<string, string>>("\"hello\"").Value);
        }

        [Test]
        public void FloatingPointSpecialValuesRoundTrip()
        {
            foreach (FloatFormatHandling handling in new[] { FloatFormatHandling.String, FloatFormatHandling.Symbol })
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { FloatFormatHandling = handling };
                foreach (double value in new[] { double.NaN, double.PositiveInfinity, double.NegativeInfinity, 1.5 })
                {
                    string json = JsonConvert.SerializeObject(value, settings);
                    Assert.AreEqual(json, JsonConvert.SerializeObject(new Pair<double, bool>(value), settings));
                    Assert.AreEqual(value, JsonConvert.DeserializeObject<Pair<double, bool>>(json, settings).Value);
                    Assert.AreEqual(json, JsonConvert.SerializeObject(new Pair<double?, bool>((double?)value), settings));
                    Assert.AreEqual(value, JsonConvert.DeserializeObject<Pair<double?, bool>>(json, settings).Value);

                    float singleValue = (float)value;
                    json = JsonConvert.SerializeObject(singleValue, settings);
                    Assert.AreEqual(json, JsonConvert.SerializeObject(new Pair<float, bool>(singleValue), settings));
                    Assert.AreEqual(singleValue, JsonConvert.DeserializeObject<Pair<float, bool>>(json, settings).Value);
                    Assert.AreEqual(json, JsonConvert.SerializeObject(new Pair<float?, bool>((float?)singleValue), settings));
                    Assert.AreEqual(singleValue, JsonConvert.DeserializeObject<Pair<float?, bool>>(json, settings).Value);
                }
            }
        }

        [Test]
        public void FloatingPointSpecialStringsSelectOnlyMatchingCases()
        {
            foreach (string value in new[] { "NaN", "Infinity", "-Infinity" })
            {
                string json = "\"" + value + "\"";
                Assert.AreEqual(JsonConvert.DeserializeObject<double>(json), JsonConvert.DeserializeObject<Pair<double, int>>(json).Value);
                Assert.AreEqual(value, JsonConvert.DeserializeObject<Pair<int, string>>(json).Value);
                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<double, string>>(json));
                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<string, double>>(json));
                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<float?, string>>(json));
                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<float, double>>(json));
                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<decimal, bool>>(json));
            }

            Assert.AreEqual("hello", JsonConvert.DeserializeObject<Pair<double, string>>("\"hello\"").Value);
            Assert.AreEqual("1.5", JsonConvert.DeserializeObject<Pair<double, string>>("\"1.5\"").Value);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<double, bool>>("\"1.5\""));
        }

        [Test]
        public void MutuallyAssignableArrayCases()
        {
            Assert.IsNull(JsonConvert.DeserializeObject<Pair<int[], uint[]>>("null").Value);
            Assert.IsNull(JsonConvert.DeserializeObject<Pair<uint[], int[]>>("null").Value);
            Assert.AreEqual("[1,2]", JsonConvert.SerializeObject(new Pair<int[], uint[]>(new int[] { 1, 2 })));
            Assert.AreEqual("[1,2]", JsonConvert.SerializeObject(new Pair<int[], uint[]>(new uint[] { 1, 2 })));
            Assert.AreEqual("[1,2]", JsonConvert.SerializeObject(new Pair<uint[], int[]>(new int[] { 1, 2 })));
            Assert.AreEqual("[1,2]", JsonConvert.SerializeObject(new Pair<uint[], int[]>(new uint[] { 1, 2 })));
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<int[], uint[]>>("[1,2]"));
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<uint[], int[]>>("[1,2]"));
        }

        [Test]
        public void NumberAmbiguityWithTypeNameHandling()
        {
            foreach (TypeNameHandling mode in new[] { TypeNameHandling.None, TypeNameHandling.Auto, TypeNameHandling.All, TypeNameHandling.Objects, TypeNameHandling.Arrays })
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = mode };
                Assert.AreEqual("42", JsonConvert.SerializeObject(new Overlapping(42), settings));
                ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Overlapping>("42", settings));
            }
        }

        [Test]
        public void TypeNameHandlingModes()
        {
            foreach (TypeNameHandling mode in new[] { TypeNameHandling.Auto, TypeNameHandling.All, TypeNameHandling.Objects })
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = mode };
                string json = JsonConvert.SerializeObject(new Objects(new Payload { Name = "test" }), settings);
                Assert.AreEqual("test", ((Payload)JsonConvert.DeserializeObject<Objects>(json, settings).Value).Name);
            }
            foreach (TypeNameHandling mode in new[] { TypeNameHandling.Auto, TypeNameHandling.All, TypeNameHandling.Arrays })
            {
                JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = mode };
                string json = JsonConvert.SerializeObject(new Arrays(new List<string> { "test" }), settings);
                Assert.AreEqual("test", ((List<string>)JsonConvert.DeserializeObject<Arrays>(json, settings).Value)[0]);
            }
        }

        public class NoMetadata
        {
            [JsonProperty(TypeNameHandling = TypeNameHandling.None)]
            public Shapes Value { get; set; }
        }

        [Test]
        public void MemberTypeNameHandlingDoesNotDisableUnionMetadata()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new AliasBinder { ResolvedType = typeof(DerivedPayload) }
            };
            NoMetadata result = JsonConvert.DeserializeObject<NoMetadata>("{\"Value\":{\"$type\":\"payload\",\"Name\":\"test\"}}", settings);
            Assert.AreEqual(typeof(DerivedPayload), result.Value.Value.GetType());
            Assert.AreEqual("{\"Value\":{\"$type\":\"payload\",\"Extra\":0,\"Name\":\"test\"}}", JsonConvert.SerializeObject(result, settings));
        }

        [Test]
        public void MetadataOrderingAndIgnore()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new AliasBinder(),
                MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead
            };
            const string json = "{\"Name\":\"test\",\"$type\":\"payload\"}";
            Assert.AreEqual("test", ((Payload)JsonConvert.DeserializeObject<Objects>(json, settings).Value).Name);
            settings.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Objects>(json, settings));
            settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Objects>("{\"$type\":\"payload\",\"Name\":\"test\"}", settings));
            settings.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Objects>("{\"$type\":42}", settings));
        }

        [Test]
        public void PreservesContainedReferences()
        {
            Payload payload = new Payload { Name = "test" };
            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            };
            List<Objects> values = new List<Objects> { new Objects(payload), new Objects(payload) };
            string json = JsonConvert.SerializeObject(values, settings);
            Assert.IsTrue(json.Contains("\"$ref\""));
            List<Objects> result = JsonConvert.DeserializeObject<List<Objects>>(json, settings);
            Assert.AreSame(result[0].Value, result[1].Value);
        }

        [Test]
        public void PreservesContainedCollectionReferencesWithoutTypeNames()
        {
            foreach (PreserveReferencesHandling references in new[] { PreserveReferencesHandling.Arrays, PreserveReferencesHandling.All })
            {
                foreach (MetadataPropertyHandling metadata in new[] { MetadataPropertyHandling.Default, MetadataPropertyHandling.ReadAhead })
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        PreserveReferencesHandling = references,
                        MetadataPropertyHandling = metadata
                    };
                    List<int> payload = new List<int> { 1, 2 };
                    string json = JsonConvert.SerializeObject(new Pair<List<int>, bool>(payload), settings);
                    Assert.AreEqual("{\"$id\":\"1\",\"$values\":[1,2]}", json);
                    Pair<List<int>, bool> result = JsonConvert.DeserializeObject<Pair<List<int>, bool>>(json, settings);
                    CollectionAssert.AreEqual(payload, (List<int>)result.Value);

                    List<Pair<List<int>, Payload>> values = new List<Pair<List<int>, Payload>>
                    {
                        new Pair<List<int>, Payload>(payload),
                        new Pair<List<int>, Payload>(payload)
                    };
                    json = JsonConvert.SerializeObject(values, settings);
                    Assert.IsTrue(json.Contains("\"$ref\""));
                    Assert.IsFalse(json.Contains("\"$type\""));
                    List<Pair<List<int>, Payload>> results = JsonConvert.DeserializeObject<List<Pair<List<int>, Payload>>>(json, settings);
                    CollectionAssert.AreEqual(payload, (List<int>)results[0].Value);
                    Assert.AreSame(results[0].Value, results[1].Value);
                }
            }
        }

        [Test]
        public void CollectionMetadataOrderingAndIgnore()
        {
            const string leadingJson = "{\"$id\":\"1\",\"$values\":[1,2]}";
            const string trailingJson = "{\"Name\":\"test\",\"$id\":\"1\",\"$values\":[1,2]}";
            JsonSerializerSettings settings = new JsonSerializerSettings();
            Pair<List<int>, Dictionary<string, object>> result = JsonConvert.DeserializeObject<Pair<List<int>, Dictionary<string, object>>>(trailingJson, settings);
            Assert.AreEqual("test", ((Dictionary<string, object>)result.Value)["Name"]);
            Assert.IsTrue(((Dictionary<string, object>)result.Value).ContainsKey("$values"));

            settings.MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead;
            result = JsonConvert.DeserializeObject<Pair<List<int>, Dictionary<string, object>>>(trailingJson, settings);
            CollectionAssert.AreEqual(new int[] { 1, 2 }, (List<int>)result.Value);

            settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;
            result = JsonConvert.DeserializeObject<Pair<List<int>, Dictionary<string, object>>>(leadingJson, settings);
            Assert.AreEqual("1", ((Dictionary<string, object>)result.Value)["$id"]);
            Assert.IsTrue(((Dictionary<string, object>)result.Value).ContainsKey("$values"));

            settings.MetadataPropertyHandling = MetadataPropertyHandling.Default;
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Arrays>(leadingJson, settings));
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<List<int>, bool>>("{\"$values\":true}", settings));
        }

        public class PayloadConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) { return objectType == typeof(Payload); }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { writer.WriteValue(((Payload)value).Name); }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new Payload { Name = (string)reader.Value };
            }
        }

        [Test]
        public void SettingsCaseConverter()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new PayloadConverter());
            Assert.AreEqual("\"test\"", JsonConvert.SerializeObject(new Pair<Payload, bool>(new Payload { Name = "test" }), settings));
            Assert.AreEqual("{\"Extra\":0,\"Name\":\"test\"}", JsonConvert.SerializeObject(new Pair<Payload, bool>(new DerivedPayload { Name = "test" }), settings));
            Assert.AreEqual("test", ((Payload)JsonConvert.DeserializeObject<Pair<Payload, bool>>("\"test\"", settings).Value).Name);
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Pair<Payload, bool>>("true", settings));
        }

        [JsonConverter(typeof(AttributedPayloadConverter))]
        public class AttributedPayload : Payload
        {
        }

        public class AttributedPayloadConverter : PayloadConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                return new AttributedPayload { Name = (string)reader.Value };
            }
        }

        [Test]
        public void AttributeCaseConverter()
        {
            Assert.AreEqual("\"test\"", JsonConvert.SerializeObject(new Pair<AttributedPayload, bool>(new AttributedPayload { Name = "test" })));
            Assert.AreEqual("test", ((AttributedPayload)JsonConvert.DeserializeObject<Pair<AttributedPayload, bool>>("\"test\"").Value).Name);
        }

        public class ScalarConverter : JsonConverter
        {
            public JsonToken LastToken { get; private set; }
            public override bool CanConvert(Type objectType) { return objectType == typeof(IntOrString); }
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer) { writer.WriteValue("override"); }
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                LastToken = reader.TokenType;
                return new IntOrString("override");
            }
        }

        [Test]
        public void UnionConverterCanBeOverridden()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            ScalarConverter converter = new ScalarConverter();
            settings.Converters.Add(converter);
            Assert.AreEqual("\"override\"", JsonConvert.SerializeObject(new IntOrString(42), settings));
            Assert.AreEqual("override", JsonConvert.DeserializeObject<IntOrString>("false", settings).Value);
            Assert.AreEqual("override", JsonConvert.DeserializeObject<IntOrString>("\"2026-09-21T00:00:00Z\"", settings).Value);
            Assert.AreEqual(JsonToken.Date, converter.LastToken);
        }

#nullable enable
        [Union]
        public class NullableReference
        {
            public object? Value { get; }
            public NullableReference(string? value) { Value = value; }
        }

        [Union]
        public class NonNullableReference
        {
            public object Value { get; }
            public NonNullableReference(string value) { Value = value; }
        }
#nullable restore

        [Test]
        public void NullableReferenceAnnotations()
        {
            Assert.IsNotNull(JsonConvert.DeserializeObject<NullableReference>("null"));
            Assert.IsNull(JsonConvert.DeserializeObject<NullableReference>("null").Value);
            Assert.IsNull(JsonConvert.DeserializeObject<NonNullableReference>("null"));
            Assert.AreEqual("null", JsonConvert.SerializeObject((NullableReference)null));
        }

        public class InterfaceOnly : IUnion
        {
            public object Value { get { return 42; } }
        }

        public class DerivedUnion : NonNullableReference
        {
            public DerivedUnion() : base("test") { }
        }

        [Test]
        public void MarkerNotInheritedAndInterfaceNotSufficient()
        {
            UnionConverter converter = new UnionConverter();
            Assert.IsFalse(converter.CanConvert(typeof(InterfaceOnly)));
            Assert.IsFalse(converter.CanConvert(typeof(DerivedUnion)));
            Assert.AreEqual("{\"Value\":42}", JsonConvert.SerializeObject(new InterfaceOnly()));
        }

        [Test]
        public void InvalidUnionPayloadIsRejected()
        {
            Recursive invalid = new Recursive(true) { Value = new Payload() };
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(invalid));
        }

        [Test]
        public void NamingStrategyAndReaderPosition()
        {
            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            Assert.AreEqual("{\"name\":\"test\"}", JsonConvert.SerializeObject(new Shapes(new Payload { Name = "test" }), settings));
            using (JsonTextReader reader = new JsonTextReader(new StringReader("42 \"hello\"")) { SupportMultipleContent = true })
            {
                JsonSerializer serializer = new JsonSerializer();
                Assert.AreEqual(42, serializer.Deserialize<IntOrString>(reader).Value);
                Assert.IsTrue(reader.Read());
                Assert.AreEqual("hello", serializer.Deserialize<IntOrString>(reader).Value);
            }
        }

        [Test]
        public void ExcessiveUnionDepthIsRejected()
        {
            Recursive value = new Recursive(true);
            for (int depth = 0; depth < 10; depth++)
            {
                value = new Recursive(value);
            }
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.SerializeObject(value, new JsonSerializerSettings { MaxDepth = 4 }));
        }

        [Test]
        public void UnionDepthIsResetAfterFailure()
        {
            UnionConverter converter = new UnionConverter();
            JsonSerializer serializer = new JsonSerializer { MaxDepth = 1 };
            using (StringWriter text = new StringWriter())
            using (JsonTextWriter writer = new JsonTextWriter(text))
            {
                ExceptionAssert.Throws<JsonSerializationException>(() => converter.WriteJson(writer, new Recursive(new Recursive(true)), serializer));
                converter.WriteJson(writer, new Recursive(true), serializer);
                Assert.AreEqual("true", text.ToString());
            }
        }
    }
}

#if !NET11_0_OR_GREATER
namespace System.Runtime.CompilerServices
{
    internal interface IUnion
    {
        object Value { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    internal sealed class UnionAttribute : Attribute
    {
    }
}
#endif