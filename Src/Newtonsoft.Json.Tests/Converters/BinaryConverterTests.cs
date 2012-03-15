using System;
using System.Collections.Generic;
#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
using System.Data.Linq;
#endif
#if !(SILVERLIGHT || NETFX_CORE)
using System.Data.SqlTypes;
#endif
using System.Linq;
using System.Text;
using Newtonsoft.Json.Converters;
#if !NETFX_CORE
using NUnit.Framework;
#else
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestFixture = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using Test = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
#endif
using Newtonsoft.Json.Tests.TestObjects;

namespace Newtonsoft.Json.Tests.Converters
{
  [TestFixture]
  public class BinaryConverterTests : TestFixtureBase
  {
    private static readonly byte[] TestData = Encoding.UTF8.GetBytes("This is some test data!!!");

    public class ByteArrayClass
    {
      public byte[] ByteArray { get; set; }
      public byte[] NullByteArray { get; set; }
    }

#if !SILVERLIGHT && !PocketPC && !NET20 && !NETFX_CORE
    [Test]
    public void DeserializeBinaryClass()
    {
      string json = @"{
  ""Binary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullBinary"": null
}";

      BinaryClass binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

      Assert.AreEqual(new Binary(TestData), binaryClass.Binary);
      Assert.AreEqual(null, binaryClass.NullBinary);
    }

    [Test]
    public void DeserializeBinaryClassFromJsonArray()
    {
      string json = @"{
  ""Binary"": [0, 1, 2, 3],
  ""NullBinary"": null
}";

      BinaryClass binaryClass = JsonConvert.DeserializeObject<BinaryClass>(json, new BinaryConverter());

      Assert.AreEqual(new byte[] { 0, 1, 2, 3 }, binaryClass.Binary.ToArray());
      Assert.AreEqual(null, binaryClass.NullBinary);
    }

    public class BinaryClass
    {
      public Binary Binary { get; set; }
      public Binary NullBinary { get; set; }
    }

    [Test]
    public void SerializeBinaryClass()
    {
      BinaryClass binaryClass = new BinaryClass();
      binaryClass.Binary = new Binary(TestData);
      binaryClass.NullBinary = null;

      string json = JsonConvert.SerializeObject(binaryClass, Formatting.Indented, new BinaryConverter());

      Assert.AreEqual(@"{
  ""Binary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullBinary"": null
}", json);
    }
#endif

    [Test]
    public void SerializeByteArrayClass()
    {
      ByteArrayClass byteArrayClass = new ByteArrayClass();
      byteArrayClass.ByteArray = TestData;
      byteArrayClass.NullByteArray = null;

      string json = JsonConvert.SerializeObject(byteArrayClass, Formatting.Indented, new BinaryConverter());

      Assert.AreEqual(@"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}", json);
    }

#if !(SILVERLIGHT || NETFX_CORE)
    public class SqlBinaryClass
    {
      public SqlBinary SqlBinary { get; set; }
      public SqlBinary? NullableSqlBinary1 { get; set; }
      public SqlBinary? NullableSqlBinary2 { get; set; }
    }

    [Test]
    public void SerializeSqlBinaryClass()
    {
      SqlBinaryClass sqlBinaryClass = new SqlBinaryClass();
      sqlBinaryClass.SqlBinary = new SqlBinary(TestData);
      sqlBinaryClass.NullableSqlBinary1 = new SqlBinary(TestData);
      sqlBinaryClass.NullableSqlBinary2 = null;

      string json = JsonConvert.SerializeObject(sqlBinaryClass, Formatting.Indented, new BinaryConverter());

      Assert.AreEqual(@"{
  ""SqlBinary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary1"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary2"": null
}", json);
    }

    [Test]
    public void DeserializeSqlBinaryClass()
    {
      string json = @"{
  ""SqlBinary"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary1"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullableSqlBinary2"": null
}";

      SqlBinaryClass sqlBinaryClass = JsonConvert.DeserializeObject<SqlBinaryClass>(json, new BinaryConverter());

      Assert.AreEqual(new SqlBinary(TestData), sqlBinaryClass.SqlBinary);
      Assert.AreEqual(new SqlBinary(TestData), sqlBinaryClass.NullableSqlBinary1);
      Assert.AreEqual(null, sqlBinaryClass.NullableSqlBinary2);
    }
#endif

    [Test]
    public void DeserializeByteArrayClass()
    {
      string json = @"{
  ""ByteArray"": ""VGhpcyBpcyBzb21lIHRlc3QgZGF0YSEhIQ=="",
  ""NullByteArray"": null
}";

      ByteArrayClass byteArrayClass = JsonConvert.DeserializeObject<ByteArrayClass>(json, new BinaryConverter());

      CollectionAssert.AreEquivalent(TestData, byteArrayClass.ByteArray);
      Assert.AreEqual(null, byteArrayClass.NullByteArray);
    }

    [Test]
    public void DeserializeByteArrayFromJsonArray()
    {
      string json = @"{
  ""ByteArray"": [0, 1, 2, 3],
  ""NullByteArray"": null
}";

      ByteArrayClass c = JsonConvert.DeserializeObject<ByteArrayClass>(json);
      Assert.IsNotNull(c.ByteArray);
      Assert.AreEqual(4, c.ByteArray.Length);
      CollectionAssert.AreEquivalent(new byte[] { 0, 1, 2, 3 }, c.ByteArray);
    }
  }
}