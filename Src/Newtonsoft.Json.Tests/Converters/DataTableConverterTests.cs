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

using System.IO;
using System.Text;
using Newtonsoft.Json.Bson;
using Newtonsoft.Json.Linq;
#if !(NETFX_CORE || PORTABLE || PORTABLE40)
using System;
using System.Collections.Generic;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestPlatform.UnitTestFramework;
using TestFixture = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestPlatform.UnitTestFramework.TestMethodAttribute;
#endif
#if !NETFX_CORE
using System.Data;
#endif
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
    public class DataTableConverterTests : TestFixtureBase
    {
        [Test]
        public void Deserialize()
        {
            string json = @"[
  {
    ""id"": 0,
    ""item"": ""item 0"",
    ""DataTableCol"": [
      {
        ""NestedStringCol"": ""0!""
      }
    ],
    ""ArrayCol"": [
      0
    ],
    ""DateCol"": ""2000-12-29T00:00:00Z""
  },
  {
    ""id"": 1,
    ""item"": ""item 1"",
    ""DataTableCol"": [
      {
        ""NestedStringCol"": ""1!""
      }
    ],
    ""ArrayCol"": [
      1
    ],
    ""DateCol"": ""2000-12-29T00:00:00Z""
  }
]";

            DataTable deserializedDataTable = JsonConvert.DeserializeObject<DataTable>(json);
            Assert.IsNotNull(deserializedDataTable);

            Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
            Assert.AreEqual(5, deserializedDataTable.Columns.Count);
            Assert.AreEqual("id", deserializedDataTable.Columns[0].ColumnName);
            Assert.AreEqual(typeof(long), deserializedDataTable.Columns[0].DataType);
            Assert.AreEqual("item", deserializedDataTable.Columns[1].ColumnName);
            Assert.AreEqual(typeof(string), deserializedDataTable.Columns[1].DataType);
            Assert.AreEqual("DataTableCol", deserializedDataTable.Columns[2].ColumnName);
            Assert.AreEqual(typeof(DataTable), deserializedDataTable.Columns[2].DataType);
            Assert.AreEqual("ArrayCol", deserializedDataTable.Columns[3].ColumnName);
            Assert.AreEqual(typeof(long[]), deserializedDataTable.Columns[3].DataType);
            Assert.AreEqual("DateCol", deserializedDataTable.Columns[4].ColumnName);
            Assert.AreEqual(typeof(DateTime), deserializedDataTable.Columns[4].DataType);

            Assert.AreEqual(2, deserializedDataTable.Rows.Count);

            DataRow dr1 = deserializedDataTable.Rows[0];
            Assert.AreEqual(0, dr1["id"]);
            Assert.AreEqual("item 0", dr1["item"]);
            Assert.AreEqual("0!", ((DataTable)dr1["DataTableCol"]).Rows[0]["NestedStringCol"]);
            Assert.AreEqual(0, ((long[])dr1["ArrayCol"])[0]);
            Assert.AreEqual(new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc), dr1["DateCol"]);

            DataRow dr2 = deserializedDataTable.Rows[1];
            Assert.AreEqual(1, dr2["id"]);
            Assert.AreEqual("item 1", dr2["item"]);
            Assert.AreEqual("1!", ((DataTable)dr2["DataTableCol"]).Rows[0]["NestedStringCol"]);
            Assert.AreEqual(1, ((long[])dr2["ArrayCol"])[0]);
            Assert.AreEqual(new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc), dr2["DateCol"]);
        }

#if !NET20
        [Test]
        public void DeserializeParseHandling()
        {
            string json = @"[
  {
    ""DateCol"": ""2000-12-29T00:00:00Z"",
    ""FloatCol"": 99.9999999999999999999
  },
  {
    ""DateCol"": ""2000-12-29T00:00:00Z"",
    ""FloatCol"": 99.9999999999999999999
  }
]";

            DataTable deserializedDataTable = JsonConvert.DeserializeObject<DataTable>(json, new JsonSerializerSettings
            {
                DateParseHandling = DateParseHandling.DateTimeOffset,
                FloatParseHandling = FloatParseHandling.Decimal
            });
            Assert.IsNotNull(deserializedDataTable);

            Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
            Assert.AreEqual(2, deserializedDataTable.Columns.Count);
            Assert.AreEqual("DateCol", deserializedDataTable.Columns[0].ColumnName);
            Assert.AreEqual(typeof(DateTimeOffset), deserializedDataTable.Columns[0].DataType);
            Assert.AreEqual("FloatCol", deserializedDataTable.Columns[1].ColumnName);
            Assert.AreEqual(typeof(decimal), deserializedDataTable.Columns[1].DataType);

            Assert.AreEqual(2, deserializedDataTable.Rows.Count);

            DataRow dr1 = deserializedDataTable.Rows[0];
            Assert.AreEqual(new DateTimeOffset(2000, 12, 29, 0, 0, 0, TimeSpan.Zero), dr1["DateCol"]);
            Assert.AreEqual(99.9999999999999999999m, dr1["FloatCol"]);

            DataRow dr2 = deserializedDataTable.Rows[1];
            Assert.AreEqual(new DateTimeOffset(2000, 12, 29, 0, 0, 0, TimeSpan.Zero), dr2["DateCol"]);
            Assert.AreEqual(99.9999999999999999999m, dr2["FloatCol"]);
        }
#endif

        [Test]
        public void Serialize()
        {
            // create a new DataTable.
            DataTable myTable = new DataTable("blah");

            // create DataColumn objects of data types.
            DataColumn colString = new DataColumn("StringCol");
            colString.DataType = typeof(string);
            myTable.Columns.Add(colString);

            DataColumn colInt32 = new DataColumn("Int32Col");
            colInt32.DataType = typeof(int);
            myTable.Columns.Add(colInt32);

            DataColumn colBoolean = new DataColumn("BooleanCol");
            colBoolean.DataType = typeof(bool);
            myTable.Columns.Add(colBoolean);

            DataColumn colTimeSpan = new DataColumn("TimeSpanCol");
            colTimeSpan.DataType = typeof(TimeSpan);
            myTable.Columns.Add(colTimeSpan);

            DataColumn colDateTime = new DataColumn("DateTimeCol");
            colDateTime.DataType = typeof(DateTime);
            colDateTime.DateTimeMode = DataSetDateTime.Utc;
            myTable.Columns.Add(colDateTime);

            DataColumn colDecimal = new DataColumn("DecimalCol");
            colDecimal.DataType = typeof(decimal);
            myTable.Columns.Add(colDecimal);

            DataColumn colDataTable = new DataColumn("DataTableCol");
            colDataTable.DataType = typeof(DataTable);
            myTable.Columns.Add(colDataTable);

            DataColumn colArray = new DataColumn("ArrayCol");
            colArray.DataType = typeof(int[]);
            myTable.Columns.Add(colArray);

            DataColumn colBytes = new DataColumn("BytesCol");
            colBytes.DataType = typeof(byte[]);
            myTable.Columns.Add(colBytes);

            // populate one row with values.
            DataRow myNewRow = myTable.NewRow();

            myNewRow["StringCol"] = "Item Name";
            myNewRow["Int32Col"] = 2147483647;
            myNewRow["BooleanCol"] = true;
            myNewRow["TimeSpanCol"] = new TimeSpan(10, 22, 10, 15, 100);
            myNewRow["DateTimeCol"] = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc);
            myNewRow["DecimalCol"] = 64.0021;
            myNewRow["ArrayCol"] = new[] { 1 };
            myNewRow["BytesCol"] = Encoding.UTF8.GetBytes("Hello world");

            DataTable nestedTable = new DataTable("Nested");
            DataColumn nestedColString = new DataColumn("NestedStringCol");
            nestedColString.DataType = typeof(string);
            nestedTable.Columns.Add(nestedColString);
            DataRow myNewNestedRow = nestedTable.NewRow();
            myNewNestedRow["NestedStringCol"] = "Nested!";
            nestedTable.Rows.Add(myNewNestedRow);

            myNewRow["DataTableCol"] = nestedTable;
            myTable.Rows.Add(myNewRow);

            string json = JsonConvert.SerializeObject(myTable, Formatting.Indented);
            Assert.AreEqual(@"[
  {
    ""StringCol"": ""Item Name"",
    ""Int32Col"": 2147483647,
    ""BooleanCol"": true,
    ""TimeSpanCol"": ""10.22:10:15.1000000"",
    ""DateTimeCol"": ""2000-12-29T00:00:00Z"",
    ""DecimalCol"": 64.0021,
    ""DataTableCol"": [
      {
        ""NestedStringCol"": ""Nested!""
      }
    ],
    ""ArrayCol"": [
      1
    ],
    ""BytesCol"": ""SGVsbG8gd29ybGQ=""
  }
]", json);
        }

        public class TestDataTableConverter : JsonConverter
        {
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                DataTable d = (DataTable)value;
                writer.WriteValue(d.TableName);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                //reader.Read();
                DataTable d = new DataTable((string)reader.Value);

                return d;
            }

            public override bool CanConvert(Type objectType)
            {
                return (objectType == typeof(DataTable));
            }
        }

        [Test]
        public void PassedInJsonConverterOverridesInternalConverter()
        {
            DataTable t1 = new DataTable("Custom");

            string json = JsonConvert.SerializeObject(t1, Formatting.Indented, new TestDataTableConverter());
            Assert.AreEqual(@"""Custom""", json);

            DataTable t2 = JsonConvert.DeserializeObject<DataTable>(json, new TestDataTableConverter());
            Assert.AreEqual(t1.TableName, t2.TableName);
        }

        [Test]
        public void RoundtripBsonBytes()
        {
            Guid g = new Guid("EDE9A599-A7D9-44A9-9243-7C287049DD20");

            var table = new DataTable();
            table.Columns.Add("data", typeof(byte[]));
            table.Columns.Add("id", typeof(Guid));
            table.Rows.Add(Encoding.UTF8.GetBytes("Hello world!"), g);

            JsonSerializer serializer = new JsonSerializer();

            MemoryStream ms = new MemoryStream();
            BsonWriter bw = new BsonWriter(ms);

            serializer.Serialize(bw, table);

            JToken o = JToken.ReadFrom(new BsonReader(new MemoryStream(ms.ToArray())) { ReadRootValueAsArray = true });
            Assert.AreEqual(@"[
  {
    ""data"": ""SGVsbG8gd29ybGQh"",
    ""id"": ""ede9a599-a7d9-44a9-9243-7c287049dd20""
  }
]", o.ToString());

            DataTable deserializedDataTable = serializer.Deserialize<DataTable>(new BsonReader(new MemoryStream(ms.ToArray())) { ReadRootValueAsArray = true });

            Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
            Assert.AreEqual(2, deserializedDataTable.Columns.Count);
            Assert.AreEqual("data", deserializedDataTable.Columns[0].ColumnName);
            Assert.AreEqual(typeof(byte[]), deserializedDataTable.Columns[0].DataType);
            Assert.AreEqual("id", deserializedDataTable.Columns[1].ColumnName);
            Assert.AreEqual(typeof(Guid), deserializedDataTable.Columns[1].DataType);

            Assert.AreEqual(1, deserializedDataTable.Rows.Count);

            DataRow dr1 = deserializedDataTable.Rows[0];
            CollectionAssert.AreEquivalent(Encoding.UTF8.GetBytes("Hello world!"), (byte[])dr1["data"]);
            Assert.AreEqual(g, (Guid)dr1["id"]);
        }

        [Test]
        public void SerializeDataTableWithNull()
        {
            var table = new DataTable();
            table.Columns.Add("item");
            table.Columns.Add("price", typeof(double));
            table.Rows.Add("shirt", 49.99);
            table.Rows.Add("pants", 54.99);
            table.Rows.Add("shoes"); // no price

            var json = JsonConvert.SerializeObject(table);
            Assert.AreEqual(@"["
                            + @"{""item"":""shirt"",""price"":49.99},"
                            + @"{""item"":""pants"",""price"":54.99},"
                            + @"{""item"":""shoes"",""price"":null}]", json);
        }

        [Test]
        public void SerializeDataTableWithNullAndIgnoreNullHandling()
        {
            var table = new DataTable();
            table.Columns.Add("item");
            table.Columns.Add("price", typeof(double));
            table.Rows.Add("shirt", 49.99);
            table.Rows.Add("pants", 54.99);
            table.Rows.Add("shoes"); // no price

            var json = JsonConvert.SerializeObject(table, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            Assert.AreEqual(@"["
                            + @"{""item"":""shirt"",""price"":49.99},"
                            + @"{""item"":""pants"",""price"":54.99},"
                            + @"{""item"":""shoes""}]", json);
        }

        [Test]
        public void DerializeDataTableWithImplicitNull()
        {
            const string json = @"["
                                + @"{""item"":""shirt"",""price"":49.99},"
                                + @"{""item"":""pants"",""price"":54.99},"
                                + @"{""item"":""shoes""}]";
            var table = JsonConvert.DeserializeObject<DataTable>(json);
            Assert.AreEqual("shirt", table.Rows[0]["item"]);
            Assert.AreEqual("pants", table.Rows[1]["item"]);
            Assert.AreEqual("shoes", table.Rows[2]["item"]);
            Assert.AreEqual(49.99, (double)table.Rows[0]["price"], 0.01);
            Assert.AreEqual(54.99, (double)table.Rows[1]["price"], 0.01);
            CustomAssert.IsInstanceOfType(typeof(System.DBNull), table.Rows[2]["price"]);
        }

        [Test]
        public void DerializeDataTableWithExplicitNull()
        {
            const string json = @"["
                                + @"{""item"":""shirt"",""price"":49.99},"
                                + @"{""item"":""pants"",""price"":54.99},"
                                + @"{""item"":""shoes"",""price"":null}]";
            var table = JsonConvert.DeserializeObject<DataTable>(json);
            Assert.AreEqual("shirt", table.Rows[0]["item"]);
            Assert.AreEqual("pants", table.Rows[1]["item"]);
            Assert.AreEqual("shoes", table.Rows[2]["item"]);
            Assert.AreEqual(49.99, (double)table.Rows[0]["price"], 0.01);
            Assert.AreEqual(54.99, (double)table.Rows[1]["price"], 0.01);
            CustomAssert.IsInstanceOfType(typeof(System.DBNull), table.Rows[2]["price"]);
        }

        [Test]
        public void SerializeKeyValuePairWithDataTableKey()
        {
            DataTable table = new DataTable();
            DataColumn idColumn = new DataColumn("id", typeof(int));
            idColumn.AutoIncrement = true;

            DataColumn itemColumn = new DataColumn("item");
            table.Columns.Add(idColumn);
            table.Columns.Add(itemColumn);

            DataRow r = table.NewRow();
            r["item"] = "item!";
            r.EndEdit();
            table.Rows.Add(r);

            KeyValuePair<DataTable, int> pair = new KeyValuePair<DataTable, int>(table, 1);
            string serializedpair = JsonConvert.SerializeObject(pair, Formatting.Indented);

            Assert.AreEqual(@"{
  ""Key"": [
    {
      ""id"": 0,
      ""item"": ""item!""
    }
  ],
  ""Value"": 1
}", serializedpair);

            var pair2 = (KeyValuePair<DataTable, int>)JsonConvert.DeserializeObject(serializedpair, typeof(KeyValuePair<DataTable, int>));

            Assert.AreEqual(1, pair2.Value);
            Assert.AreEqual(1, pair2.Key.Rows.Count);
            Assert.AreEqual("item!", pair2.Key.Rows[0]["item"]);
        }


        [Test]
        public void SerializedTypedDataTable()
        {
            CustomerDataSet.CustomersDataTable dt = new CustomerDataSet.CustomersDataTable();
            dt.AddCustomersRow("432");

            string json = JsonConvert.SerializeObject(dt, Formatting.Indented);

            Assert.AreEqual(@"[
  {
    ""CustomerID"": ""432""
  }
]", json);
        }

        [Test]
        public void DeserializedTypedDataTable()
        {
            string json = @"[
  {
    ""CustomerID"": ""432""
  }
]";

            var dt = JsonConvert.DeserializeObject<CustomerDataSet.CustomersDataTable>(json);

            Assert.AreEqual("432", dt[0].CustomerID);
        }
    }
}

#endif