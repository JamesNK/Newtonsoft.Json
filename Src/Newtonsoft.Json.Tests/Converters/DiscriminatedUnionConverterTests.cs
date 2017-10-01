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

#if !(NET40 || NET35 || NET20 || DNXCORE50) || NETSTANDARD2_0
using System.Diagnostics;
using System.Reflection;
using Microsoft.FSharp.Core;
using Microsoft.FSharp.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Tests.TestObjects;
using Newtonsoft.Json.Tests.TestObjects.GeometricForms;
using Newtonsoft.Json.Tests.TestObjects.Money;
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
    public class DiscriminatedUnionConverterTests : TestFixtureBase
    {
        public class DoubleDoubleConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                double d = (double)value;

                writer.WriteValue(d * 2);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                double d = (double)reader.Value;

                return d / 2;
            }

            public override bool CanConvert(Type objectType)
            {
                return objectType == typeof(double);
            }
        }

        [Test]
        public void SerializeUnionWithConverter()
        {
            string json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0), new DoubleDoubleConverter());

            Assert.AreEqual(@"{""Case"":""Rectangle"",""Fields"":[20.0,10.0]}", json);

            Shape c = JsonConvert.DeserializeObject<Shape>(json, new DoubleDoubleConverter());
            Assert.AreEqual(true, c.IsRectangle);

            Shape.Rectangle r = (Shape.Rectangle)c;

            Assert.AreEqual(5.0, r.length);
            Assert.AreEqual(10.0, r.width);
        }

        [Test]
        public void SerializeBasicUnion()
        {
            string json = JsonConvert.SerializeObject(Currency.AUD);

            Assert.AreEqual(@"{""Case"":""AUD""}", json);
        }

        [Test]
        public void SerializePerformance()
        {
            List<Shape> values = new List<Shape>
            {
                Shape.NewRectangle(10.0, 5.0),
                Shape.NewCircle(7.5)
            };

            string json = JsonConvert.SerializeObject(values, Formatting.Indented);

            Stopwatch ts = new Stopwatch();
            ts.Start();

            for (int i = 0; i < 100; i++)
            {
                JsonConvert.SerializeObject(values);
            }

            ts.Stop();

            Console.WriteLine(ts.Elapsed.TotalSeconds);
        }

        [Test]
        public void DeserializePerformance()
        {
            string json = @"[
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]},
  {""Case"":""Rectangle"",""Fields"":[10.0,5.0]}
]";

            JsonConvert.DeserializeObject<List<Shape>>(json);

            Stopwatch ts = new Stopwatch();
            ts.Start();

            for (int i = 0; i < 100; i++)
            {
                JsonConvert.DeserializeObject<List<Shape>>(json);
            }

            ts.Stop();

            Console.WriteLine(ts.Elapsed.TotalSeconds);
        }

        [Test]
        public void SerializeUnionWithFields()
        {
            string json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0));

            Assert.AreEqual(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}", json);
        }

        [Test]
        public void DeserializeBasicUnion()
        {
            Currency c = JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD""}");
            Assert.AreEqual(Currency.AUD, c);

            c = JsonConvert.DeserializeObject<Currency>(@"{""Case"":""EUR""}");
            Assert.AreEqual(Currency.EUR, c);

            c = JsonConvert.DeserializeObject<Currency>(@"null");
            Assert.AreEqual(null, c);
        }

        [Test]
        public void DeserializeUnionWithFields()
        {
            Shape c = JsonConvert.DeserializeObject<Shape>(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}");
            Assert.AreEqual(true, c.IsRectangle);

            Shape.Rectangle r = (Shape.Rectangle)c;

            Assert.AreEqual(5.0, r.length);
            Assert.AreEqual(10.0, r.width);
        }

        public class Union
        {
            public List<UnionCase> Cases;
            public Converter<object, int> TagReader { get; set; }
        }

        public class UnionCase
        {
            public int Tag;
            public string Name;
            public PropertyInfo[] Fields;
            public Converter<object, object[]> FieldReader;
            public Converter<object[], object> Constructor;
        }

        private Union CreateUnion(Type t)
        {
            Union u = new Union();

            u.TagReader = (s) => FSharpValue.PreComputeUnionTagReader(t, null).Invoke(s);
            u.Cases = new List<UnionCase>();

            UnionCaseInfo[] cases = FSharpType.GetUnionCases(t, null);

            foreach (UnionCaseInfo unionCaseInfo in cases)
            {
                UnionCase unionCase = new UnionCase();
                unionCase.Tag = unionCaseInfo.Tag;
                unionCase.Name = unionCaseInfo.Name;
                unionCase.Fields = unionCaseInfo.GetFields();
                unionCase.FieldReader = (s) => FSharpValue.PreComputeUnionReader(unionCaseInfo, null).Invoke(s);
                unionCase.Constructor = (s) => FSharpValue.PreComputeUnionConstructor(unionCaseInfo, null).Invoke(s);

                u.Cases.Add(unionCase);
            }

            return u;
        }

        [Test]
        public void Serialize()
        {
            Shape value = Shape.NewRectangle(10.0, 5.0);

            Union union = CreateUnion(value.GetType());

            int tag = union.TagReader.Invoke(value);

            UnionCase caseInfo = union.Cases.Single(c => c.Tag == tag);

            object[] fields = caseInfo.FieldReader.Invoke(value);

            Assert.AreEqual(10d, fields[0]);
            Assert.AreEqual(5d, fields[1]);
        }

        [Test]
        public void Deserialize()
        {
            Union union = CreateUnion(typeof(Shape.Rectangle));

            UnionCase caseInfo = union.Cases.Single(c => c.Name == "Rectangle");

            Shape.Rectangle value = (Shape.Rectangle)caseInfo.Constructor.Invoke(new object[]
            {
                10.0, 5.0
            });

            Assert.AreEqual("Newtonsoft.Json.Tests.TestObjects.GeometricForms.Shape+Rectangle", value.ToString());
            Assert.AreEqual(10, value.width);
            Assert.AreEqual(5, value.length);
        }

        [Test]
        public void DeserializeBasicUnion_NoMatch()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""abcdefg"",""Fields"":[]}"), "No union type found with the name 'abcdefg'. Path 'Case', line 1, position 17.");
        }

        [Test]
        public void DeserializeBasicUnion_MismatchedFieldCount()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":[1]}"), "The number of field values does not match the number of properties defined by union 'AUD'. Path '', line 1, position 27.");
        }

        [Test]
        public void DeserializeBasicUnion_NoCaseName()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Currency>(@"{""Fields"":[1]}"), "No 'Case' property with union name found. Path '', line 1, position 14.");
        }

        [Test]
        public void DeserializeBasicUnion_UnexpectedEnd()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Currency>(@"{""Case"":"), "Unexpected end when reading JSON. Path 'Case', line 1, position 8.");
        }

        [Test]
        public void DeserializeBasicUnion_FieldsObject()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Currency>(@"{""Case"":""AUD"",""Fields"":{}}"), "Union fields must been an array. Path 'Fields', line 1, position 24.");
        }

        [Test]
        public void DeserializeBasicUnion_UnexpectedProperty()
        {
            ExceptionAssert.Throws<JsonSerializationException>(() => JsonConvert.DeserializeObject<Currency>(@"{""Case123"":""AUD""}"), "Unexpected property 'Case123' found when reading union. Path 'Case123', line 1, position 11.");
        }

        [Test]
        public void SerializeUnionWithTypeNameHandlingAndReferenceTracking()
        {
            string json = JsonConvert.SerializeObject(Shape.NewRectangle(10.0, 5.0), new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                TypeNameHandling = TypeNameHandling.All
            });

            Assert.AreEqual(@"{""Case"":""Rectangle"",""Fields"":[10.0,5.0]}", json);

            Shape c = JsonConvert.DeserializeObject<Shape>(json);
            Assert.AreEqual(true, c.IsRectangle);

            Shape.Rectangle r = (Shape.Rectangle)c;

            Assert.AreEqual(5.0, r.length);
            Assert.AreEqual(10.0, r.width);
        }
    }
}

#endif