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
using System.Globalization;
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Bson
{
  public class BsonReader : JsonReader
  {
    private readonly BinaryReader _reader;
    private readonly bool _rootTypeIsArray;
    private readonly List<ContainerContext> _stack;

    private BsonType _currentElementType;


    private class ContainerContext
    {
      public JTokenType Type { get; private set; }
      public int Length { get; set; }
      public int Position { get; set; }

      public ContainerContext(JTokenType type)
      {
        Type = type;
      }
    }

    public BsonReader(Stream stream) : this(stream, false)
    {
    }

    public BsonReader(Stream stream, bool rootTypeIsArray)
    {
      ValidationUtils.ArgumentNotNull(stream, "stream");
      _reader = new BinaryReader(stream);
      _stack = new List<ContainerContext>();
      _rootTypeIsArray = rootTypeIsArray;
    }

    private string ReadElement()
    {
      _currentElementType = ReadType();
      string elementName = ReadString();
      return elementName;
    }

    public override byte[] ReadAsBytes()
    {
      Read();
      if (TokenType != JsonToken.Bytes)
        throw new JsonReaderException("Error reading bytes. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));

      return (byte[])Value;
    }

    public override bool Read()
    {
      try
      {
        switch (CurrentState)
        {
          case State.Start:
            {
              JsonToken token = (!_rootTypeIsArray) ? JsonToken.StartObject : JsonToken.StartArray;
              JTokenType type = (!_rootTypeIsArray) ? JTokenType.Object : JTokenType.Array;

              SetToken(token);
              ContainerContext newContext = new ContainerContext(type);
              _stack.Add(newContext);
              newContext.Length = ReadInt32();
              return true;
            }
          case State.ObjectStart:
            {
              SetToken(JsonToken.PropertyName, ReadElement());
              return true;
            }
          case State.Complete:
            break;
          case State.Property:
            {
              ReadType(_currentElementType);
              return true;
            }
          case State.Object:
            break;
          case State.ArrayStart:
            ReadElement();
            ReadType(_currentElementType);
            return true;
          case State.Array:
            break;
          case State.Closed:
            break;
          case State.PostValue:
            ContainerContext context = GetCurrentContext();
            if (context == null)
              return false;

            int lengthMinusEnd = context.Length - 1;

            if (context.Position < lengthMinusEnd)
            {
              if (context.Type == JTokenType.Array)
              {
                ReadElement();
                ReadType(_currentElementType);
                return true;
              }
              else
              {
                SetToken(JsonToken.PropertyName, ReadElement());
                return true;
              }
            }
            else if (context.Position == lengthMinusEnd)
            {
              if (ReadByte() != 0)
                throw new JsonReaderException("Unexpected end of object byte value.");

              _stack.RemoveAt(_stack.Count - 1);
              if (_stack.Count != 0)
                MovePosition(context.Length);

              JsonToken endToken = (context.Type == JTokenType.Object) ? JsonToken.EndObject : JsonToken.EndArray;
              SetToken(endToken);
              return true;
            }
            break;
          case State.ConstructorStart:
            break;
          case State.Constructor:
            break;
          case State.Error:
            break;
          case State.Finished:
            break;
          default:
            throw new ArgumentOutOfRangeException();
        }

        return false;
      }
      catch (EndOfStreamException)
      {
        return false;
      }
    }

    private byte ReadByte()
    {
      MovePosition(1);
      return _reader.ReadByte();
    }

    private void ReadType(BsonType type)
    {
      switch (type)
      {
        case BsonType.Number:
          SetToken(JsonToken.Float, ReadDouble());
          break;
        case BsonType.String:
          SetToken(JsonToken.String, ReadLengthString());
          break;
        case BsonType.Object:
          break;
        case BsonType.Array:
          break;
        case BsonType.Binary:
          SetToken(JsonToken.Bytes, ReadBinary());
          break;
        case BsonType.Undefined:
          break;
        case BsonType.Oid:
          break;
        case BsonType.Boolean:
          break;
        case BsonType.Date:
          break;
        case BsonType.Null:
          break;
        case BsonType.Regex:
          break;
        case BsonType.Reference:
          break;
        case BsonType.Code:
          break;
        case BsonType.Symbol:
          break;
        case BsonType.CodeWScope:
          break;
        case BsonType.Integer:
          SetToken(JsonToken.Integer, (long)ReadInt32());
          break;
        case BsonType.TimeStamp:
          break;
        case BsonType.MinKey:
          break;
        case BsonType.MaxKey:
          break;
        default:
          throw new ArgumentOutOfRangeException("type");
      }
    }

    private byte[] ReadBinary()
    {
      int dataLength = ReadInt32();

      // BsonBinaryType not used
      ReadByte();

      return ReadBytes(dataLength);
    }

    private string ReadString()
    {
      List<byte> buff = new List<byte>();
      byte b = _reader.ReadByte();
      while (b != 0)
      {
        buff.Add(b);
        b = _reader.ReadByte();
      }

      MovePosition(buff.Count + 1);
      string ret = Encoding.UTF8.GetString(buff.ToArray());
      return ret;
    }

    private string ReadLengthString()
    {
      int length = ReadInt32();

      MovePosition(length);
      byte[] buffer = _reader.ReadBytes(length - 1);
      _reader.ReadByte();

      return Encoding.UTF8.GetString(buffer);
    }

    private double ReadDouble()
    {
      MovePosition(8);
      return _reader.ReadDouble();
    }

    private int ReadInt32()
    {
      MovePosition(4);
      return _reader.ReadInt32();
    }

    private BsonType ReadType()
    {
      MovePosition(1);
      return (BsonType)_reader.ReadSByte();
    }

    private ContainerContext GetCurrentContext()
    {
      int count = _stack.Count;
      if (count == 0)
        return null;

      return _stack[count - 1];
    }

    private void MovePosition(int count)
    {
      ContainerContext context = GetCurrentContext();
      context.Position += count;
    }

    private byte[] ReadBytes(int count)
    {
      MovePosition(count);
      return _reader.ReadBytes(count);
    }
  }
}
