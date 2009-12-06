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
      writer.Flush();
      
      string bson = MiscellaneousUtils.BytesToHex(ms.ToArray());
      Assert.AreEqual("0F-00-00-00-10-42-6C-61-68-00-01-00-00-00-00", bson);
    }

    [Test]
    [ExpectedException(typeof(JsonWriterException), ExpectedMessage = "Cannot flush BsonWriter until JSON is complete.")]
    public void FlushInsideObject()
    {
      MemoryStream ms = new MemoryStream();
      BsonWriter writer = new BsonWriter(ms);

      writer.WriteStartObject();
      writer.WritePropertyName("Blah");
      writer.WriteValue(1);
      writer.Flush();
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
  }
}