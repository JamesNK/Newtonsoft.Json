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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Text;
using Newtonsoft.Json.Converters;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests
{
    [TestFixture]
    public class JsonConvertAsyncTests : TestFixtureBase
    {
        public class NameTableTestClass
        {
            public string Value { get; set; }
        }

        public class NameTableTestClassConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                reader.Read();
                reader.Read();

                JsonTextReader jsonTextReader = (JsonTextReader)reader;
                Assert.IsNotNull(jsonTextReader.NameTable);

                string s = serializer.Deserialize<string>(reader);
                Assert.AreEqual("hi", s);
                Assert.IsNotNull(jsonTextReader.NameTable);

                NameTableTestClass o = new NameTableTestClass
                {
                    Value = s
                };

                return o;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(NameTableTestClass);
            }
        }

        [Test]
        public async Task NameTableTestAsync()
        {
            StringReader sr = new StringReader("{'property':'hi'}");
            JsonTextReader jsonTextReader = new JsonTextReader(sr);

            Assert.IsNull(jsonTextReader.NameTable);

            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new NameTableTestClassConverter());
            NameTableTestClass o = await serializer.DeserializeAsync<NameTableTestClass>(jsonTextReader);

            Assert.IsNull(jsonTextReader.NameTable);
            Assert.AreEqual("hi", o.Value);
        }

        [Test]
        public async Task DefaultSettings_CreateAsync()
        {
            try
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                IList<int> l = new List<int> { 1, 2, 3 };

                StringWriter sw = new StringWriter();
                JsonSerializer serializer = JsonSerializer.CreateDefault();
                await serializer.SerializeAsync(sw, l);

                StringAssert.AreEqual(@"[
  1,
  2,
  3
]", sw.ToString());

                sw = new StringWriter();
                serializer.Formatting = Formatting.None;
                await serializer.SerializeAsync(sw, l);

                Assert.AreEqual(@"[1,2,3]", sw.ToString());

                sw = new StringWriter();
                serializer = new JsonSerializer();
                await serializer.SerializeAsync(sw, l);

                Assert.AreEqual(@"[1,2,3]", sw.ToString());

                sw = new StringWriter();
                serializer = JsonSerializer.Create();
                await serializer.SerializeAsync(sw, l);

                Assert.AreEqual(@"[1,2,3]", sw.ToString());
            }
            finally
            {
                JsonConvert.DefaultSettings = null;
            }
        }

        [Test]
        public async Task DefaultSettings_CreateWithSettingsAsync()
        {
            try
            {
                JsonConvert.DefaultSettings = () => new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                IList<int> l = new List<int> { 1, 2, 3 };

                StringWriter sw = new StringWriter();
                JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
                {
                    Converters = { new IntConverter() }
                });
                await serializer.SerializeAsync(sw, l);

                StringAssert.AreEqual(@"[
  2,
  4,
  6
]", sw.ToString());

                sw = new StringWriter();
                serializer.Converters.Clear();
                await serializer.SerializeAsync(sw, l);

                StringAssert.AreEqual(@"[
  1,
  2,
  3
]", sw.ToString());

                sw = new StringWriter();
                serializer = JsonSerializer.Create(new JsonSerializerSettings { Formatting = Formatting.Indented });
                await serializer.SerializeAsync(sw, l);

                StringAssert.AreEqual(@"[
  1,
  2,
  3
]", sw.ToString());
            }
            finally
            {
                JsonConvert.DefaultSettings = null;
            }
        }

        public class IntConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                int i = (int)value;
                writer.WriteValue(i * 2);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(int);
            }
        }

        [Test]
        public async Task WriteDateTimeAsync()
        {
            await Task.WhenAll(
                TestDateTimeFormatAsync(DateTime.MaxValue),
                TestDateTimeFormatAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Local)),
                TestDateTimeFormatAsync(new DateTime(2000, 1, 1, 1, 1, 1, 999, DateTimeKind.Local)),
                TestDateTimeFormatAsync(new DateTime(634663873826822481, DateTimeKind.Local)),
                TestDateTimeFormatAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified)),
                TestDateTimeFormatAsync(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Utc)),
                TestDateTimeFormatAsync(new DateTime(621355968000000000, DateTimeKind.Utc)),
                TestDateTimeFormatAsync(DateTime.MinValue),
                TestDateTimeFormatAsync(default(DateTime)),
                TestDateTimeFormatAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.Zero)),
                TestDateTimeFormatAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(1))),
                TestDateTimeFormatAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(1.5))),
                TestDateTimeFormatAsync(new DateTimeOffset(2000, 1, 1, 1, 1, 1, TimeSpan.FromHours(13))),
                TestDateTimeFormatAsync(new DateTimeOffset(634663873826822481, TimeSpan.Zero)),
                TestDateTimeFormatAsync(DateTimeOffset.MinValue),
                TestDateTimeFormatAsync(DateTimeOffset.MaxValue),
                TestDateTimeFormatAsync(default(DateTimeOffset))
                );
        }

        private static async Task TestDateTimeFormatAsync<T>(T value)
        {
            JsonConverter converter = new IsoDateTimeConverter();
            string date = await WriteAsync(value, converter);

            Console.WriteLine(converter.GetType().Name + ": " + date);

            T parsed = await ReadAsync<T>(date, converter);

            try
            {
                Assert.AreEqual(value, parsed);
            }
            catch (Exception)
            {
                // JavaScript ticks aren't as precise, recheck after rounding
                long valueTicks = GetTicks(value);
                long parsedTicks = GetTicks(parsed);

                valueTicks = valueTicks / 10000 * 10000;

                Assert.AreEqual(valueTicks, parsedTicks);
            }
        }

        public static long GetTicks(object value)
        {
            return value is DateTime ? ((DateTime)value).Ticks : ((DateTimeOffset)value).Ticks;
        }

        public static async Task<string> WriteAsync(object value, JsonConverter converter)
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            await converter.WriteJsonAsync(writer, value, null);

            await writer.FlushAsync();
            return sw.ToString();
        }

        public static async Task<T> ReadAsync<T>(string text, JsonConverter converter)
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(text));
            await reader.ReadAsStringAsync();

            return (T)await converter.ReadJsonAsync(reader, typeof(T), null, null);
        }

        [Test]
        public async Task MaximumDateTimeOffsetLengthAsync()
        {
            DateTimeOffset dt = new DateTimeOffset(2000, 12, 31, 20, 59, 59, new TimeSpan(0, 11, 33, 0, 0));
            dt = dt.AddTicks(9999999);

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            await writer.WriteValueAsync(dt);
            await writer.FlushAsync();

            Assert.AreEqual(@"""2000-12-31T20:59:59.9999999+11:33""", sw.ToString());
        }

        [Test]
        public async Task MaximumDateTimeLengthAsync()
        {
            DateTime dt = new DateTime(2000, 12, 31, 20, 59, 59, DateTimeKind.Local);
            dt = dt.AddTicks(9999999);

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            await writer.WriteValueAsync(dt);
            await writer.FlushAsync();
        }

        [Test]
        public async Task MaximumDateTimeMicrosoftDateFormatLengthAsync()
        {
            DateTime dt = DateTime.MaxValue;

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);
            writer.DateFormatHandling = DateFormatHandling.MicrosoftDateFormat;

            await writer.WriteValueAsync(dt);
            await writer.FlushAsync();
        }

        [Test]
        public async Task ParseIsoDateAsync()
        {
            StringReader sr = new StringReader(@"""2014-02-14T14:25:02-13:00""");

            JsonReader jsonReader = new JsonTextReader(sr);

            Assert.IsTrue(await jsonReader.ReadAsync());
            Assert.AreEqual(typeof(DateTime), jsonReader.ValueType);
        }

        //[Test]
        public async Task StackOverflowTestAsync()
        {
            StringBuilder sb = new StringBuilder();

            int depth = 900;
            for (int i = 0; i < depth; i++)
            {
                sb.Append("{'A':");
            }

            // invalid json
            sb.Append("{***}");
            for (int i = 0; i < depth; i++)
            {
                sb.Append("}");
            }

            string json = sb.ToString();
            JsonSerializer serializer = new JsonSerializer();
            await serializer.DeserializeAsync<Nest>(new JsonTextReader(new StringReader(json)));
        }

        public class Nest
        {
            public Nest A { get; set; }
        }

        public class ClobberMyProperties
        {
            [JsonConverter(typeof(ClobberingJsonConverter), "Uno", 1)]
            public string One { get; set; }

            [JsonConverter(typeof(ClobberingJsonConverter), "Dos", 2)]
            public string Two { get; set; }

            [JsonConverter(typeof(ClobberingJsonConverter), "Tres")]
            public string Three { get; set; }

            public string Four { get; set; }
        }

        public class ClobberingJsonConverter : JsonConverter
        {
            public string ClobberValueString { get; private set; }

            public int ClobberValueInt { get; private set; }

            public ClobberingJsonConverter(string clobberValueString, int clobberValueInt)
            {
                ClobberValueString = clobberValueString;
                ClobberValueInt = clobberValueInt;
            }

            public ClobberingJsonConverter(string clobberValueString)
                : this(clobberValueString, 1337)
            {
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(ClobberValueString + "-" + ClobberValueInt.ToString() + "-" + value.ToString());
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(string);
            }
        }

        public class IncorrectJsonConvertParameters
        {
            /// <summary>
            /// We deliberately use the wrong number/type of arguments for ClobberingJsonConverter to ensure an 
            /// exception is thrown.
            /// </summary>
            [JsonConverter(typeof(ClobberingJsonConverter), "Uno", "Blammo")]
            public string One { get; set; }
        }
        
        public class OverloadsJsonConverterer : JsonConverter
        {
            private readonly string _type;
            
            // constructor with Type argument

            public OverloadsJsonConverterer(Type typeParam)
            {
                _type = "Type";
            }
            
            public OverloadsJsonConverterer(object objectParam)
            {
                _type = string.Format("object({0})", objectParam.GetType().FullName);
            }

            // primitive type conversions

            public OverloadsJsonConverterer(byte byteParam)
            {
                _type = "byte";
            }

            public OverloadsJsonConverterer(short shortParam)
            {
                _type = "short";
            }

            public OverloadsJsonConverterer(int intParam)
            {
                _type = "int";
            }

            public OverloadsJsonConverterer(long longParam)
            {
                _type = "long";
            }

            public OverloadsJsonConverterer(double doubleParam)
            {
                _type = "double";
            }

            // params argument

            public OverloadsJsonConverterer(params int[] intParams)
            {
                _type = "int[]";
            }

            public OverloadsJsonConverterer(bool[] intParams)
            {
                _type = "bool[]";
            }

            // closest type resolution

            public OverloadsJsonConverterer(IEnumerable<string> iEnumerableParam)
            {
                _type = "IEnumerable<string>";
            }

            public OverloadsJsonConverterer(IList<string> iListParam)
            {
                _type = "IList<string>";
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(_type);
            }
            
            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(int);
            }
            
        }

        public class OverloadWithTypeParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), typeof(int))]
            public int Overload { get; set; }
        }

        public class OverloadWithUnhandledParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), "str")]
            public int Overload { get; set; }
        }

        public class OverloadWithIntParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 1)]
            public int Overload { get; set; }
        }

        public class OverloadWithUIntParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 1U)]
            public int Overload { get; set; }
        }

        public class OverloadWithLongParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 1L)]
            public int Overload { get; set; }
        }

        public class OverloadWithULongParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 1UL)]
            public int Overload { get; set; }
        }
        
        public class OverloadWithShortParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), (short)1)]
            public int Overload { get; set; }
        }

        public class OverloadWithUShortParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), (ushort)1)]
            public int Overload { get; set; }
        }

        public class OverloadWithSByteParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), (sbyte)1)]
            public int Overload { get; set; }
        }
        
        public class OverloadWithByteParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), (byte)1)]
            public int Overload { get; set; }
        }

        public class OverloadWithCharParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 'a')]
            public int Overload { get; set; }
        }

        public class OverloadWithBoolParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), true)]
            public int Overload { get; set; }
        }

        public class OverloadWithFloatParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 1.5f)]
            public int Overload { get; set; }
        }

        public class OverloadWithDoubleParameter
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), 1.5)]
            public int Overload { get; set; }
        }
        
        public class OverloadWithArrayParameters
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), new int[] { 1, 2, 3 })]
            public int WithParams { get; set; }

            [JsonConverter(typeof(OverloadsJsonConverterer), new bool[] { true, false })]
            public int WithoutParams { get; set; }
        }

        public class OverloadWithBaseType
        {
            [JsonConverter(typeof(OverloadsJsonConverterer), new object[] { new string[] { "a", "b", "c" } })]
            public int Overload { get; set; }
        }

        public class Measurements
        {
            [JsonProperty(ItemConverterType = typeof(RoundingJsonConverter))]
            public List<double> Positions { get; set; }

            [JsonProperty(ItemConverterType = typeof(RoundingJsonConverter), ItemConverterParameters = new object[] { 0, MidpointRounding.ToEven })]
            public List<double> Loads { get; set; }

            [JsonConverter(typeof(RoundingJsonConverter), 4)]
            public double Gain { get; set; }
        }

        public class RoundingJsonConverter : JsonConverter
        {
            int _precision;
            MidpointRounding _rounding;

            public RoundingJsonConverter()
                : this(2)
            {
            }

            public RoundingJsonConverter(int precision)
                : this(precision, MidpointRounding.AwayFromZero)
            {
            }

            public RoundingJsonConverter(int precision, MidpointRounding rounding)
            {
                _precision = precision;
                _rounding = rounding;
            }

            public override bool CanRead
            {
                get { return false; }
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(double);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                writer.WriteValue(Math.Round((double)value, _precision, _rounding));
            }
        }

        public class GenericBaseClass<O, T>
        {
            public virtual T Data { get; set; }
        }

        public class GenericIntermediateClass<O> : GenericBaseClass<O, string>
        {
            public override string Data { get; set; }
        }

        public class NonGenericChildClass : GenericIntermediateClass<int>
        {
        }
    }
}

#endif
