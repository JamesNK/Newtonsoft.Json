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
#if !(NET20 || NET35 || PORTABLE)
using System.Numerics;
#endif
using System.Text;
using System.Text.RegularExpressions;
#if NETFX_CORE
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#elif DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Tests.TestObjects;
using System.Globalization;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json.Tests.Bson
{
    [TestFixture]
    public class BsonWriterTests : TestFixtureBase
    {
        [Test]
        public void CloseOutput()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            Assert.IsTrue(ms.CanRead);
            writer.Close();
            Assert.IsFalse(ms.CanRead);

            ms = new MemoryStream();
            writer = new BsonWriter(ms) { CloseOutput = false };

            Assert.IsTrue(ms.CanRead);
            writer.Close();
            Assert.IsTrue(ms.CanRead);
        }

        [Test]
        public void WriteSingleObject()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("Blah");
            writer.WriteValue(1);
            writer.WriteEndObject();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00", bson);
        }

#if !NET20
        [Test]
        public void WriteValues()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartArray();
            writer.WriteValue(long.MaxValue);
            writer.WriteValue((ulong)long.MaxValue);
            writer.WriteValue(int.MaxValue);
            writer.WriteValue((uint)int.MaxValue);
            writer.WriteValue(byte.MaxValue);
            writer.WriteValue(sbyte.MaxValue);
            writer.WriteValue('a');
            writer.WriteValue(decimal.MaxValue);
            writer.WriteValue(double.MaxValue);
            writer.WriteValue(float.MaxValue);
            writer.WriteValue(true);
            writer.WriteValue(new byte[] { 0, 1, 2, 3, 4 });
            writer.WriteValue(new DateTimeOffset(2000, 12, 29, 12, 30, 0, TimeSpan.Zero));
            writer.WriteValue(new DateTime(2000, 12, 29, 12, 30, 0, DateTimeKind.Utc));
            writer.WriteEnd();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("8C-00-00-00-12-30-00-FF-FF-FF-FF-FF-FF-FF-7F-12-31-00-FF-FF-FF-FF-FF-FF-FF-7F-10-32-00-FF-FF-FF-7F-10-33-00-FF-FF-FF-7F-10-34-00-FF-00-00-00-10-35-00-7F-00-00-00-02-36-00-02-00-00-00-61-00-01-37-00-00-00-00-00-00-00-F0-45-01-38-00-FF-FF-FF-FF-FF-FF-EF-7F-01-39-00-00-00-00-E0-FF-FF-EF-47-08-31-30-00-01-05-31-31-00-05-00-00-00-00-00-01-02-03-04-09-31-32-00-40-C5-E2-BA-E3-00-00-00-09-31-33-00-40-C5-E2-BA-E3-00-00-00-00", bson);
        }
#endif

        [Test]
        public void WriteDouble()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartArray();
            writer.WriteValue(99.99d);
            writer.WriteEnd();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("10-00-00-00-01-30-00-8F-C2-F5-28-5C-FF-58-40-00", bson);
        }

        [Test]
        public void WriteGuid()
        {
            Guid g = new Guid("D821EED7-4B5C-43C9-8AC2-6928E579B705");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartArray();
            writer.WriteValue(g);
            writer.WriteEnd();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("1D-00-00-00-05-30-00-10-00-00-00-04-D7-EE-21-D8-5C-4B-C9-43-8A-C2-69-28-E5-79-B7-05-00", bson);
        }

        [Test]
        public void WriteArrayBsonFromSite()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartArray();
            writer.WriteValue("a");
            writer.WriteValue("b");
            writer.WriteValue("c");
            writer.WriteEndArray();

            writer.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            string expected = "20-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-02-32-00-02-00-00-00-63-00-00";
            string bson = BytesToHex(ms.ToArray());

            Assert.AreEqual(expected, bson);
        }

        [Test]
        public void WriteBytes()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello world!");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartArray();
            writer.WriteValue("a");
            writer.WriteValue("b");
            writer.WriteValue(data);
            writer.WriteEndArray();

            writer.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            string expected = "2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-00-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00";
            string bson = BytesToHex(ms.ToArray());

            Assert.AreEqual(expected, bson);

            BsonReader reader = new BsonReader(new MemoryStream(ms.ToArray()));
            reader.ReadRootValueAsArray = true;
            reader.Read();
            reader.Read();
            reader.Read();
            reader.Read();
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(data, (byte[])reader.Value);
        }

        [Test]
        public void WriteNestedArray()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.WriteStartObject();

            writer.WritePropertyName("_id");
            writer.WriteValue(HexToBytes("4A-78-93-79-17-22-00-00-00-00-61-CF"));

            writer.WritePropertyName("a");
            writer.WriteStartArray();
            for (int i = 1; i <= 8; i++)
            {
                double value = (i != 5)
                    ? Convert.ToDouble(i)
                    : 5.78960446186581E+77d;

                writer.WriteValue(value);
            }
            writer.WriteEndArray();

            writer.WritePropertyName("b");
            writer.WriteValue("test");

            writer.WriteEndObject();

            writer.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            string expected = "87-00-00-00-05-5F-69-64-00-0C-00-00-00-00-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";
            string bson = BytesToHex(ms.ToArray());

            Assert.AreEqual(expected, bson);
        }

        [Test]
        public void WriteSerializedStore()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            Store s1 = new Store();
            s1.Color = StoreColor.White;
            s1.Cost = 999.59m;
            s1.Employees = int.MaxValue - 1;
            s1.Open = true;
            s1.product.Add(new Product
            {
                ExpiryDate = new DateTime(2000, 9, 28, 3, 59, 58, DateTimeKind.Local),
                Name = "BSON!",
                Price = -0.1m,
                Sizes = new[] { "First", "Second" }
            });
            s1.Establised = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Local);

            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, s1);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);
            Store s2 = (Store)serializer.Deserialize(reader, typeof(Store));

            Assert.AreNotEqual(s1, s2);
            Assert.AreEqual(s1.Color, s2.Color);
            Assert.AreEqual(s1.Cost, s2.Cost);
            Assert.AreEqual(s1.Employees, s2.Employees);
            Assert.AreEqual(s1.Escape, s2.Escape);
            Assert.AreEqual(s1.Establised, s2.Establised);
            Assert.AreEqual(s1.Mottos.Count, s2.Mottos.Count);
            Assert.AreEqual(s1.Mottos.First(), s2.Mottos.First());
            Assert.AreEqual(s1.Mottos.Last(), s2.Mottos.Last());
            Assert.AreEqual(s1.Open, s2.Open);
            Assert.AreEqual(s1.product.Count, s2.product.Count);
            Assert.AreEqual(s1.RoomsPerFloor.Length, s2.RoomsPerFloor.Length);
            Assert.AreEqual(s1.Symbol, s2.Symbol);
            Assert.AreEqual(s1.Width, s2.Width);

            MemoryStream ms1 = new MemoryStream();
            BsonWriter writer1 = new BsonWriter(ms1);

            serializer.Serialize(writer1, s1);

            CollectionAssert.AreEquivalent(ms.ToArray(), ms1.ToArray());
        }

        [Test]
        public void WriteLargeStrings()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            StringBuilder largeStringBuilder = new StringBuilder();
            for (int i = 0; i < 100; i++)
            {
                if (i > 0)
                {
                    largeStringBuilder.Append("-");
                }

                largeStringBuilder.Append(i.ToString(CultureInfo.InvariantCulture));
            }
            string largeString = largeStringBuilder.ToString();

            writer.WriteStartObject();
            writer.WritePropertyName(largeString);
            writer.WriteValue(largeString);
            writer.WriteEndObject();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("4E-02-00-00-02-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-22-01-00-00-30-2D-31-2D-32-2D-33-2D-34-2D-35-2D-36-2D-37-2D-38-2D-39-2D-31-30-2D-31-31-2D-31-32-2D-31-33-2D-31-34-2D-31-35-2D-31-36-2D-31-37-2D-31-38-2D-31-39-2D-32-30-2D-32-31-2D-32-32-2D-32-33-2D-32-34-2D-32-35-2D-32-36-2D-32-37-2D-32-38-2D-32-39-2D-33-30-2D-33-31-2D-33-32-2D-33-33-2D-33-34-2D-33-35-2D-33-36-2D-33-37-2D-33-38-2D-33-39-2D-34-30-2D-34-31-2D-34-32-2D-34-33-2D-34-34-2D-34-35-2D-34-36-2D-34-37-2D-34-38-2D-34-39-2D-35-30-2D-35-31-2D-35-32-2D-35-33-2D-35-34-2D-35-35-2D-35-36-2D-35-37-2D-35-38-2D-35-39-2D-36-30-2D-36-31-2D-36-32-2D-36-33-2D-36-34-2D-36-35-2D-36-36-2D-36-37-2D-36-38-2D-36-39-2D-37-30-2D-37-31-2D-37-32-2D-37-33-2D-37-34-2D-37-35-2D-37-36-2D-37-37-2D-37-38-2D-37-39-2D-38-30-2D-38-31-2D-38-32-2D-38-33-2D-38-34-2D-38-35-2D-38-36-2D-38-37-2D-38-38-2D-38-39-2D-39-30-2D-39-31-2D-39-32-2D-39-33-2D-39-34-2D-39-35-2D-39-36-2D-39-37-2D-39-38-2D-39-39-00-00", bson);
        }

        [Test]
        public void SerializeGoogleGeoCode()
        {
            string json = @"{
  ""name"": ""1600 Amphitheatre Parkway, Mountain View, CA, USA"",
  ""Status"": {
    ""code"": 200,
    ""request"": ""geocode""
  },
  ""Placemark"": [
    {
      ""address"": ""1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA"",
      ""AddressDetails"": {
        ""Country"": {
          ""CountryNameCode"": ""US"",
          ""AdministrativeArea"": {
            ""AdministrativeAreaName"": ""CA"",
            ""SubAdministrativeArea"": {
              ""SubAdministrativeAreaName"": ""Santa Clara"",
              ""Locality"": {
                ""LocalityName"": ""Mountain View"",
                ""Thoroughfare"": {
                  ""ThoroughfareName"": ""1600 Amphitheatre Pkwy""
                },
                ""PostalCode"": {
                  ""PostalCodeNumber"": ""94043""
                }
              }
            }
          }
        },
        ""Accuracy"": 8
      },
      ""Point"": {
        ""coordinates"": [-122.083739, 37.423021, 0]
      }
    }
  ]
}";

            GoogleMapGeocoderStructure jsonGoogleMapGeocoder = JsonConvert.DeserializeObject<GoogleMapGeocoderStructure>(json);

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            JsonSerializer serializer = new JsonSerializer();
            serializer.Serialize(writer, jsonGoogleMapGeocoder);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);
            GoogleMapGeocoderStructure bsonGoogleMapGeocoder = (GoogleMapGeocoderStructure)serializer.Deserialize(reader, typeof(GoogleMapGeocoderStructure));

            Assert.IsNotNull(bsonGoogleMapGeocoder);
            Assert.AreEqual("1600 Amphitheatre Parkway, Mountain View, CA, USA", bsonGoogleMapGeocoder.Name);
            Assert.AreEqual("200", bsonGoogleMapGeocoder.Status.Code);
            Assert.AreEqual("geocode", bsonGoogleMapGeocoder.Status.Request);

            IList<Placemark> placemarks = bsonGoogleMapGeocoder.Placemark;
            Assert.IsNotNull(placemarks);
            Assert.AreEqual(1, placemarks.Count);

            Placemark placemark = placemarks[0];
            Assert.AreEqual("1600 Amphitheatre Pkwy, Mountain View, CA 94043, USA", placemark.Address);
            Assert.AreEqual(8, placemark.AddressDetails.Accuracy);
            Assert.AreEqual("US", placemark.AddressDetails.Country.CountryNameCode);
            Assert.AreEqual("CA", placemark.AddressDetails.Country.AdministrativeArea.AdministrativeAreaName);
            Assert.AreEqual("Santa Clara", placemark.AddressDetails.Country.AdministrativeArea.SubAdministrativeArea.SubAdministrativeAreaName);
            Assert.AreEqual("Mountain View", placemark.AddressDetails.Country.AdministrativeArea.SubAdministrativeArea.Locality.LocalityName);
            Assert.AreEqual("1600 Amphitheatre Pkwy", placemark.AddressDetails.Country.AdministrativeArea.SubAdministrativeArea.Locality.Thoroughfare.ThoroughfareName);
            Assert.AreEqual("94043", placemark.AddressDetails.Country.AdministrativeArea.SubAdministrativeArea.Locality.PostalCode.PostalCodeNumber);
            Assert.AreEqual(-122.083739m, placemark.Point.Coordinates[0]);
            Assert.AreEqual(37.423021m, placemark.Point.Coordinates[1]);
            Assert.AreEqual(0m, placemark.Point.Coordinates[2]);
        }

        [Test]
        public void WriteEmptyStrings()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("");
            writer.WriteValue("");
            writer.WriteEndObject();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("0C-00-00-00-02-00-01-00-00-00-00-00", bson);
        }

        [Test]
        public void WriteComment()
        {
            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                MemoryStream ms = new MemoryStream();
                BsonWriter writer = new BsonWriter(ms);

                writer.WriteStartArray();
                writer.WriteComment("fail");
            }, "Cannot write JSON comment as BSON. Path ''.");
        }

        [Test]
        public void WriteConstructor()
        {
            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                MemoryStream ms = new MemoryStream();
                BsonWriter writer = new BsonWriter(ms);

                writer.WriteStartArray();
                writer.WriteStartConstructor("fail");
            }, "Cannot write JSON constructor as BSON. Path ''.");
        }

        [Test]
        public void WriteRaw()
        {
            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                MemoryStream ms = new MemoryStream();
                BsonWriter writer = new BsonWriter(ms);

                writer.WriteStartArray();
                writer.WriteRaw("fail");
            }, "Cannot write raw JSON as BSON. Path ''.");
        }

        [Test]
        public void WriteRawValue()
        {
            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                MemoryStream ms = new MemoryStream();
                BsonWriter writer = new BsonWriter(ms);

                writer.WriteStartArray();
                writer.WriteRawValue("fail");
            }, "Cannot write raw JSON as BSON. Path ''.");
        }

        [Test]
        public void Example()
        {
            Product p = new Product();
            p.ExpiryDate = DateTime.Parse("2009-04-05T14:45:00Z");
            p.Name = "Carlos' Spicy Wieners";
            p.Price = 9.95m;
            p.Sizes = new[] { "Small", "Medium", "Large" };

            MemoryStream ms = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();

            // serialize product to BSON
            BsonWriter writer = new BsonWriter(ms);
            serializer.Serialize(writer, p);

            Console.WriteLine(BitConverter.ToString(ms.ToArray()));
            // 7C-00-00-00-02-4E-61-6D-65-00-16-00-00-00-43-61-72-6C-
            // 6F-73-27-20-53-70-69-63-79-20-57-69-65-6E-65-72-73-00-
            // 09-45-78-70-69-72-79-44-61-74-65-00-E0-51-BD-76-20-01-
            // 00-00-01-50-72-69-63-65-00-66-66-66-66-66-E6-23-40-04-
            // 53-69-7A-65-73-00-2D-00-00-00-02-30-00-06-00-00-00-53-
            // 6D-61-6C-6C-00-02-31-00-07-00-00-00-4D-65-64-69-75-6D-
            // 00-02-32-00-06-00-00-00-4C-61-72-67-65-00-00-00

            ms.Seek(0, SeekOrigin.Begin);

            // deserialize product from BSON
            BsonReader reader = new BsonReader(ms);
            Product deserializedProduct = serializer.Deserialize<Product>(reader);

            Console.WriteLine(deserializedProduct.Name);
            // Carlos' Spicy Wieners

            Assert.AreEqual("Carlos' Spicy Wieners", deserializedProduct.Name);
            Assert.AreEqual(9.95m, deserializedProduct.Price);
            Assert.AreEqual(3, deserializedProduct.Sizes.Length);
        }

        [Test]
        public void WriteOid()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            byte[] oid = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 };

            writer.WriteStartObject();
            writer.WritePropertyName("_oid");
            writer.WriteObjectId(oid);
            writer.WriteEndObject();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("17-00-00-00-07-5F-6F-69-64-00-01-02-03-04-05-06-07-08-09-0A-0B-0C-00", bson);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEquivalent(oid, (byte[])reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);
        }

        [Test]
        public void WriteOidPlusContent()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("_id");
            writer.WriteObjectId(HexToBytes("4ABBED9D1D8B0F0218000001"));
            writer.WritePropertyName("test");
            writer.WriteValue("1234£56");
            writer.WriteEndObject();

            byte[] expected = HexToBytes("29000000075F6964004ABBED9D1D8B0F02180000010274657374000900000031323334C2A335360000");

            CollectionAssert.AreEquivalent(expected, ms.ToArray());
        }

        [Test]
        public void WriteRegexPlusContent()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("regex");
            writer.WriteRegex("abc", "i");
            writer.WritePropertyName("test");
            writer.WriteRegex(string.Empty, null);
            writer.WriteEndObject();

            byte[] expected = HexToBytes("1A-00-00-00-0B-72-65-67-65-78-00-61-62-63-00-69-00-0B-74-65-73-74-00-00-00-00");

            CollectionAssert.AreEquivalent(expected, ms.ToArray());
        }

        [Test]
        public void SerializeEmptyAndNullStrings()
        {
            Product p = new Product();
            p.ExpiryDate = DateTime.Parse("2009-04-05T14:45:00Z");
            p.Name = null;
            p.Price = 9.95m;
            p.Sizes = new[] { "Small", "", null };

            MemoryStream ms = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();

            BsonWriter writer = new BsonWriter(ms);
            serializer.Serialize(writer, p);

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            Product deserializedProduct = serializer.Deserialize<Product>(reader);

            Console.WriteLine(deserializedProduct.Name);

            Assert.AreEqual(null, deserializedProduct.Name);
            Assert.AreEqual(9.95m, deserializedProduct.Price);
            Assert.AreEqual(3, deserializedProduct.Sizes.Length);
            Assert.AreEqual("Small", deserializedProduct.Sizes[0]);
            Assert.AreEqual("", deserializedProduct.Sizes[1]);
            Assert.AreEqual(null, deserializedProduct.Sizes[2]);
        }

        [Test]
        public void WriteReadEmptyAndNullStrings()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartArray();
            writer.WriteValue("Content!");
            writer.WriteValue("");
            writer.WriteValue((string)null);
            writer.WriteEndArray();

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("Content!", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.String, reader.TokenType);
            Assert.AreEqual("", reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Null, reader.TokenType);
            Assert.AreEqual(null, reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void WriteDateTimes()
        {
            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);
            writer.DateTimeKindHandling = DateTimeKind.Unspecified;

            writer.WriteStartArray();
            writer.WriteValue(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc));
            writer.WriteValue(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Local));
            writer.WriteValue(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Unspecified));
            writer.WriteEndArray();

            ms.Seek(0, SeekOrigin.Begin);

            BsonReader reader = new BsonReader(ms);
            reader.ReadRootValueAsArray = true;
            reader.DateTimeKindHandling = DateTimeKind.Utc;

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartArray, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc), reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc), reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Date, reader.TokenType);
            Assert.AreEqual(new DateTime(2000, 10, 12, 20, 55, 0, DateTimeKind.Utc), reader.Value);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndArray, reader.TokenType);

            Assert.IsFalse(reader.Read());
        }

        [Test]
        public void WriteValueOutsideOfObjectOrArray()
        {
            ExceptionAssert.Throws<JsonWriterException>(() =>
            {
                MemoryStream stream = new MemoryStream();

                using (BsonWriter writer = new BsonWriter(stream))
                {
                    writer.WriteValue("test");
                    writer.Flush();
                }
            }, "Error writing String value. BSON must start with an Object or Array. Path ''.");
        }

        [Test]
        public void DateTimeZoneHandling()
        {
            MemoryStream ms = new MemoryStream();
            JsonWriter writer = new BsonWriter(ms)
            {
                DateTimeZoneHandling = Json.DateTimeZoneHandling.Utc
            };

            writer.WriteStartArray();
            writer.WriteValue(new DateTime(2000, 1, 1, 1, 1, 1, DateTimeKind.Unspecified));
            writer.WriteEndArray();

            Assert.AreEqual("10-00-00-00-09-30-00-C8-88-07-6B-DC-00-00-00-00", (BitConverter.ToString(ms.ToArray())));
        }

        public class RegexTestClass
        {
            public Regex Regex { get; set; }
        }

        [Test]
        public void SerializeDeserializeRegex()
        {
            Regex r1 = new Regex("(hi)", RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
            RegexTestClass c = new RegexTestClass { Regex = r1 };

            MemoryStream ms = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();

            BsonWriter writer = new BsonWriter(ms);
            serializer.Serialize(writer, c);

            string hex = BitConverter.ToString(ms.ToArray());

            Assert.AreEqual("15-00-00-00-0B-52-65-67-65-78-00-28-68-69-29-00-69-75-78-00-00", hex);

            JObject o = (JObject)JObject.ReadFrom(new BsonReader(new MemoryStream(ms.ToArray())));

            StringAssert.AreEqual(@"{
  ""Regex"": ""/(hi)/iux""
}", o.ToString());
        }

        [Test]
        public void SerializeByteArray_ErrorWhenTopLevel()
        {
            byte[] b = Encoding.UTF8.GetBytes("Hello world");

            MemoryStream ms = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();

            BsonWriter writer = new BsonWriter(ms);

            ExceptionAssert.Throws<JsonWriterException>(() => { serializer.Serialize(writer, b); }, "Error writing Binary value. BSON must start with an Object or Array. Path ''.");
        }

        public class GuidTestClass
        {
            public Guid AGuid { get; set; }
        }

        public class StringTestClass
        {
            public string AGuid { get; set; }
        }

        [Test]
        public void WriteReadGuid()
        {
            GuidTestClass c = new GuidTestClass();
            c.AGuid = new Guid("af45dccf-df13-44fe-82be-6212c09eda84");

            MemoryStream ms = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();

            BsonWriter writer = new BsonWriter(ms);

            serializer.Serialize(writer, c);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);

            GuidTestClass c2 = serializer.Deserialize<GuidTestClass>(reader);

            Assert.AreEqual(c.AGuid, c2.AGuid);
        }

        [Test]
        public void WriteStringReadGuid()
        {
            StringTestClass c = new StringTestClass();
            c.AGuid = new Guid("af45dccf-df13-44fe-82be-6212c09eda84").ToString();

            MemoryStream ms = new MemoryStream();
            JsonSerializer serializer = new JsonSerializer();

            BsonWriter writer = new BsonWriter(ms);

            serializer.Serialize(writer, c);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);

            GuidTestClass c2 = serializer.Deserialize<GuidTestClass>(reader);

            Assert.AreEqual(c.AGuid, c2.AGuid.ToString());
        }

#if !(NET20 || NET35 || PORTABLE || PORTABLE40)
        [Test]
        public void WriteBigInteger()
        {
            BigInteger i = BigInteger.Parse("1999999999999999999999999999999999999999999999999999999999990");

            MemoryStream ms = new MemoryStream();
            BsonWriter writer = new BsonWriter(ms);

            writer.WriteStartObject();
            writer.WritePropertyName("Blah");
            writer.WriteValue(i);
            writer.WriteEndObject();

            string bson = BytesToHex(ms.ToArray());
            Assert.AreEqual("2A-00-00-00-05-42-6C-61-68-00-1A-00-00-00-00-F6-FF-FF-FF-FF-FF-FF-1F-B2-21-CB-28-59-84-C4-AE-03-8A-44-34-2F-4C-4E-9E-3E-01-00", bson);

            ms.Seek(0, SeekOrigin.Begin);
            BsonReader reader = new BsonReader(ms);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.StartObject, reader.TokenType);
            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.PropertyName, reader.TokenType);

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.Bytes, reader.TokenType);
            CollectionAssert.AreEqual(new byte[] { 246, 255, 255, 255, 255, 255, 255, 31, 178, 33, 203, 40, 89, 132, 196, 174, 3, 138, 68, 52, 47, 76, 78, 158, 62, 1 }, (byte[])reader.Value);
            Assert.AreEqual(i, new BigInteger((byte[])reader.Value));

            Assert.IsTrue(reader.Read());
            Assert.AreEqual(JsonToken.EndObject, reader.TokenType);

            Assert.IsFalse(reader.Read());
        }
#endif
    }
}