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
using System.Linq;
using System.Text;
using NUnit.Framework;
using Newtonsoft.Json.Bson;
using System.IO;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Bson
{
  public class BsonWriterTests : TestFixtureBase
  {
    [Test]
    public void WriteSingleObject()
    {
      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);

      writer.WriteStartObject();
      writer.WritePropertyName("Blah");
      writer.WriteValue(1);
      writer.WriteEndObject();
      
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());
      Assert.AreEqual("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00", bson);
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
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());

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

      string expected = "2B-00-00-00-02-30-00-02-00-00-00-61-00-02-31-00-02-00-00-00-62-00-05-32-00-0C-00-00-00-02-48-65-6C-6C-6F-20-77-6F-72-6C-64-21-00";
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());

      Assert.AreEqual(expected, bson);
    }

    [Test]
    public void WriteNestedArray()
    {
      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);
      writer.WriteStartObject();

      writer.WritePropertyName("_id");
      writer.WriteValue(MiscellaneousUtils.HexToBytes("4A-78-93-79-17-22-00-00-00-00-61-CF"));

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

      string expected = "87-00-00-00-05-5F-69-64-00-0C-00-00-00-02-4A-78-93-79-17-22-00-00-00-00-61-CF-04-61-00-5D-00-00-00-01-30-00-00-00-00-00-00-00-F0-3F-01-31-00-00-00-00-00-00-00-00-40-01-32-00-00-00-00-00-00-00-08-40-01-33-00-00-00-00-00-00-00-10-40-01-34-00-00-00-00-00-00-00-14-50-01-35-00-00-00-00-00-00-00-18-40-01-36-00-00-00-00-00-00-00-1C-40-01-37-00-00-00-00-00-00-00-20-40-00-02-62-00-05-00-00-00-74-65-73-74-00-00";
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());

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
                         ExpiryDate = new DateTime(2000, 9, 28, 3, 59, 58, DateTimeKind.Utc),
                         Name = "BSON!",
                         Price = -0.1m,
                         Sizes = new [] { "First", "Second" }
                       });

      JsonSerializer serializer = new JsonSerializer();
      serializer.Serialize(writer, s1);

      ms.Seek(0, SeekOrigin.Begin);
      BsonReader reader = new BsonReader(ms);
      Store s2 = (Store)serializer.Deserialize(reader, typeof (Store));

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

      Assert.AreEqual(ms.ToArray(), ms1.ToArray());

      string s = JsonConvert.SerializeObject(s1);
      byte[] textData = Encoding.UTF8.GetBytes(s);

      int l1 = textData.Length;
      int l2 = ms.ToArray().Length;

      Console.WriteLine(l1);
      Console.WriteLine(l2);
    }
  }
}