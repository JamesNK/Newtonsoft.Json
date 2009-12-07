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
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Bson
{
  public class BsonWriter : JTokenWriter
  {
    private readonly Stream _stream;
    private readonly BinaryWriter _writer;


    public BsonWriter(Stream stream)
    {
      ValidationUtils.ArgumentNotNull(stream, "stream");
      _stream = stream;
      _writer = new BinaryWriter(stream);
    }

    public override void Flush()
    {
      _writer.Flush();
    }

    private void WriteToken(JToken t)
    {
      switch (t.Type)
      {
        case JTokenType.Object:
          {
            int size = CalculateSize(t);
            _writer.Write(size);
            foreach (JProperty property in t)
            {
              _writer.Write((sbyte)GetTypeNumber(property.Value));
              WriteString(property.Name);
              WriteToken(property.Value);
            }
            _writer.Write((byte)0);
          }
          break;
        case JTokenType.Array:
          {
            int size = CalculateSize(t);
            _writer.Write(size);
            int index = 0;
            foreach (JToken c in t)
            {
              _writer.Write((sbyte)GetTypeNumber(c));
              WriteString(index.ToString());
              WriteToken(c);
              index++;
            }
            _writer.Write((byte)0);
          }
          break;
        case JTokenType.Integer:
          _writer.Write(Convert.ToInt32(((JValue)t).Value));
          break;
        case JTokenType.Float:
          _writer.Write(Convert.ToDouble(((JValue)t).Value));
          break;
        case JTokenType.String:
          WriteStringWithLength((string)t);
          break;
        case JTokenType.Boolean:
          _writer.Write((bool)t);
          break;
        case JTokenType.Null:
        case JTokenType.Undefined:
          break;
        case JTokenType.Date:
          DateTime dateTime = (DateTime) t;
          dateTime = dateTime.ToUniversalTime();
          long ticks = JsonConvert.ConvertDateTimeToJavaScriptTicks(dateTime);
          _writer.Write(ticks);
          break;
        case JTokenType.Bytes:
          byte[] data = (byte[])t;
          _writer.Write(data.Length);
          _writer.Write((byte)BsonBinaryType.Data);
          _writer.Write(data);
          break;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private void WriteString(string s)
    {
      byte[] bytes = Encoding.UTF8.GetBytes(s);
      _writer.Write(bytes);
      _writer.Write((byte)0);
    }

    private void WriteStringWithLength(string s)
    {
      _writer.Write(CalculateSizeWithLength(s, false));
      WriteString(s);
    }

    private BsonType GetTypeNumber(JToken t)
    {
      switch (t.Type)
      {
        case JTokenType.Object:
          return BsonType.Object;
        case JTokenType.Array:
          return BsonType.Array;
        case JTokenType.Integer:
          return BsonType.Integer;
        case JTokenType.Float:
          return BsonType.Number;
        case JTokenType.String:
          return BsonType.String;
        case JTokenType.Boolean:
          return BsonType.Boolean;
        case JTokenType.Null:
        case JTokenType.Undefined:
          return BsonType.Null;
        case JTokenType.Date:
          return BsonType.Date;
        case JTokenType.Bytes:
          return BsonType.Binary;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private int CalculateSize(string s)
    {
      int ret;
      if (s != null)
        ret = Encoding.UTF8.GetByteCount(s);
      else
        ret = 0;
      return ret + 1;
    }

    private int CalculateSizeWithLength(string s, bool includeSize)
    {
      int baseSize = (includeSize)
        ? 5 // size bytes + terminator
        : 1; // terminator

      int ret;
      if (s != null)
        ret = Encoding.UTF8.GetByteCount(s);
      else
        ret = 0;
      return baseSize + ret;
    }

    private int CalculateSize(JToken t)
    {
      switch (t.Type)
      {
        case JTokenType.Object:
          {
            int bases = 4;
            foreach (JProperty p in t)
            {
              bases += CalculateSize(p);
            }
            bases += 1;
            return bases;
          }
        case JTokenType.Array:
          {
            int bases = 4;
            int index = 0;
            foreach (JToken c in t)
            {
              bases += 1;
              bases += CalculateSize(index.ToString());
              bases += CalculateSize(c);
              index++;
            }
            bases += 1;
            return bases;
          }
        case JTokenType.Constructor:
          throw new JsonWriterException("Cannot write JSON constructor as BSON.");
        case JTokenType.Property:
          JProperty property = (JProperty) t;
          int ss = 1;
          ss += CalculateSize(property.Name);
          ss += CalculateSize(property.Value);
          return ss;
        case JTokenType.Comment:
          throw new JsonWriterException("Cannot write JSON comment as BSON.");
        case JTokenType.Integer:
          return 4;
        case JTokenType.Float:
          return 8;
        case JTokenType.String:
          string s = (string)t;

          return CalculateSizeWithLength(s, true);
        case JTokenType.Boolean:
          return 1;
        case JTokenType.Null:
        case JTokenType.Undefined:
          return 0;
        case JTokenType.Date:
          return 8;
        case JTokenType.Raw:
          throw new JsonWriterException("Cannot write raw JSON as BSON.");
        case JTokenType.Bytes:
          byte[] data = (byte[]) t;
          return 4 + 1 + data.Length;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    protected override void WriteEnd(JsonToken token)
    {
      base.WriteEnd(token);

      if (Top == 0)
      {
        WriteToken(Token);
      }
    }
  }
}