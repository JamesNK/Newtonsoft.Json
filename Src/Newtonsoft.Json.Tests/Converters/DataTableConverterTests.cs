#if !SILVERLIGHT
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
using NUnit.Framework;
using Newtonsoft.Json.Tests.TestObjects;
using System.Data;

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
    ""item"": ""item 0""
  },
  {
    ""id"": 1,
    ""item"": ""item 1""
  }
]";

      DataTable deserializedDataTable = JsonConvert.DeserializeObject<DataTable>(json);
      Assert.IsNotNull(deserializedDataTable);

      Assert.AreEqual(string.Empty, deserializedDataTable.TableName);
      Assert.AreEqual(2, deserializedDataTable.Columns.Count);
      Assert.AreEqual("id", deserializedDataTable.Columns[0].ColumnName);
      Assert.AreEqual(typeof(long), deserializedDataTable.Columns[0].DataType);
      Assert.AreEqual("item", deserializedDataTable.Columns[1].ColumnName);
      Assert.AreEqual(typeof(string), deserializedDataTable.Columns[1].DataType);

      Assert.AreEqual(2, deserializedDataTable.Rows.Count);

      DataRow dr1 = deserializedDataTable.Rows[0];
      Assert.AreEqual(0, dr1["id"]);
      Assert.AreEqual("item 0", dr1["item"]);

      DataRow dr2 = deserializedDataTable.Rows[1];
      Assert.AreEqual(1, dr2["id"]);
      Assert.AreEqual("item 1", dr2["item"]);
    }

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

      // populate one row with values.
      DataRow myNewRow = myTable.NewRow();

      myNewRow["StringCol"] = "Item Name";
      myNewRow["Int32Col"] = 2147483647;
      myNewRow["BooleanCol"] = true;
      myNewRow["TimeSpanCol"] = new TimeSpan(10, 22, 10, 15, 100);
      myNewRow["DateTimeCol"] = new DateTime(2000, 12, 29, 0, 0, 0, DateTimeKind.Utc);
      myNewRow["DecimalCol"] = 64.0021;
      myTable.Rows.Add(myNewRow);

      string json = JsonConvert.SerializeObject(myTable, Formatting.Indented);
      Assert.AreEqual(@"[
  {
    ""StringCol"": ""Item Name"",
    ""Int32Col"": 2147483647,
    ""BooleanCol"": true,
    ""TimeSpanCol"": ""10.22:10:15.1000000"",
    ""DateTimeCol"": ""\/Date(978048000000)\/"",
    ""DecimalCol"": 64.0021
  }
]", json);
    }

    public class TestDataTableConverter : JsonConverter
    {
      public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
      {
        DataTable d = (DataTable) value;
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
        return (objectType == typeof (DataTable));
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
  }
}
#endif