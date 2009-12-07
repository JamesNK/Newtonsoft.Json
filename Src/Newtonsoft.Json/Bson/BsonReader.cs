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
  /// <summary>
  /// Represents a reader that provides fast, non-cached, forward-only access to serialized Json data.
  /// </summary>
  public class BsonReader : JsonReader
  {
    private readonly BinaryReader _reader;
    private readonly bool _rootTypeIsArray;
    private readonly List<ContainerContext> _stack;

    private BsonType _currentElementType;
    private BsonReaderState _bsonReaderState;

    private enum BsonReaderState
    {
      Normal,
      ReferenceStart,
      ReferenceRef,
      ReferenceId,
      CodeWScopeStart,
      CodeWScopeCode,
      CodeWScopeScope,
      CodeWScopeScopeObject,
      CodeWScopeScopeEnd
    }

    private class ContainerContext
    {
      public JTokenType Type { get; private set; }
      public int Length { get; set; }
      private int _position;
      public int Position
      {
        get { return _position; }
        set
        {
          if (value > Length && Length != 0)
            throw new JsonReaderException("Read past end of current container context.");

          _position = value;
        }
      }

      public ContainerContext(JTokenType type)
      {
        Type = type;
      }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BsonReader"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    public BsonReader(Stream stream) : this(stream, false)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BsonReader"/> class.
    /// </summary>
    /// <param name="stream">The stream.</param>
    /// <param name="rootTypeIsArray">if set to <c>true</c> the root object will be read as a JSON array.</param>
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

    /// <summary>
    /// Reads the next JSON token from the stream as a <see cref="T:Byte[]"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:Byte[]"/> or a null reference if the next JSON token is null.
    /// </returns>
    public override byte[] ReadAsBytes()
    {
      Read();
      if (TokenType != JsonToken.Bytes)
        throw new JsonReaderException("Error reading bytes. Expected bytes but got {0}.".FormatWith(CultureInfo.InvariantCulture, TokenType));

      return (byte[])Value;
    }

    /// <summary>
    /// Reads the next JSON token from the stream.
    /// </summary>
    /// <returns>
    /// true if the next token was read successfully; false if there are no more tokens to read.
    /// </returns>
    public override bool Read()
    {
      try
      {
        switch (_bsonReaderState)
        {
          case BsonReaderState.Normal:
            return ReadNormal();
          case BsonReaderState.ReferenceStart:
          case BsonReaderState.ReferenceRef:
          case BsonReaderState.ReferenceId:
            return ReadReference();
          case BsonReaderState.CodeWScopeStart:
          case BsonReaderState.CodeWScopeCode:
          case BsonReaderState.CodeWScopeScope:
          case BsonReaderState.CodeWScopeScopeObject:
          case BsonReaderState.CodeWScopeScopeEnd:
            return ReadCodeWScope();
          default:
            throw new JsonReaderException("Unexpected state: {0}".FormatWith(CultureInfo.InvariantCulture, _bsonReaderState));
        }
      }
      catch (EndOfStreamException)
      {
        return false;
      }
    }

    private bool ReadCodeWScope()
    {
      switch (_bsonReaderState)
      {
        case BsonReaderState.CodeWScopeStart:
          SetToken(JsonToken.PropertyName, "$code");
          _bsonReaderState = BsonReaderState.CodeWScopeCode;
          return true;
        case BsonReaderState.CodeWScopeCode:
          // total CodeWScope size - not used
          ReadInt32();

          SetToken(JsonToken.String, ReadLengthString());
          _bsonReaderState = BsonReaderState.CodeWScopeScope;
          return true;
        case BsonReaderState.CodeWScopeScope:
          if (CurrentState == State.PostValue)
          {
            SetToken(JsonToken.PropertyName, "$scope");
            return true;
          }
          else
          {
            SetToken(JsonToken.StartObject);
            _bsonReaderState = BsonReaderState.CodeWScopeScopeObject;

            ContainerContext newContext = new ContainerContext(JTokenType.Object);
            _stack.Add(newContext);
            newContext.Length = ReadInt32();

            return true;
          }
        case BsonReaderState.CodeWScopeScopeObject:
          bool result = ReadNormal();
          if (result && TokenType == JsonToken.EndObject)
            _bsonReaderState = BsonReaderState.CodeWScopeScopeEnd;

          return result;
        case BsonReaderState.CodeWScopeScopeEnd:
          SetToken(JsonToken.EndObject);
          _bsonReaderState = BsonReaderState.Normal;
          return true;
        default:
          throw new ArgumentOutOfRangeException();
      }
    }

    private bool ReadReference()
    {
      switch (CurrentState)
      {
        case State.ObjectStart:
          {
            SetToken(JsonToken.PropertyName, "$ref");
            _bsonReaderState = BsonReaderState.ReferenceRef;
            return true;
          }
        case State.Property:
          {
            if (_bsonReaderState == BsonReaderState.ReferenceRef)
            {
              SetToken(JsonToken.String, ReadLengthString());
              return true;
            }
            else if (_bsonReaderState == BsonReaderState.ReferenceId)
            {
              SetToken(JsonToken.Bytes, ReadBytes(12));
              return true;
            }
            else
            {
              throw new JsonReaderException("Unexpected state when reading BSON reference: " + _bsonReaderState);
            }
          }
        case State.PostValue:
          {
            if (_bsonReaderState == BsonReaderState.ReferenceRef)
            {
              SetToken(JsonToken.PropertyName, "$id");
              _bsonReaderState = BsonReaderState.ReferenceId;
              return true;
            }
            else if (_bsonReaderState == BsonReaderState.ReferenceId)
            {
              SetToken(JsonToken.EndObject);
              _bsonReaderState = BsonReaderState.Normal;
              return true;
            }
            else
            {
              throw new JsonReaderException("Unexpected state when reading BSON reference: " + _bsonReaderState);
            }
          }
        default:
          throw new JsonReaderException("Unexpected state when reading BSON reference: " + CurrentState);
      }
    }

    private bool ReadNormal()
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
        case State.Closed:
          return false;
        case State.Property:
          {
            ReadType(_currentElementType);
            return true;
          }
        case State.ArrayStart:
          ReadElement();
          ReadType(_currentElementType);
          return true;
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
          else
          {
            throw new JsonReaderException("Read past end of current container context.");
          }
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
        case BsonType.Symbol:
          SetToken(JsonToken.String, ReadLengthString());
          break;
        case BsonType.Object:
          {
            SetToken(JsonToken.StartObject);

            ContainerContext newContext = new ContainerContext(JTokenType.Object);
            _stack.Add(newContext);
            newContext.Length = ReadInt32();
            break;
          }
        case BsonType.Array:
          {
            SetToken(JsonToken.StartArray);

            ContainerContext newContext = new ContainerContext(JTokenType.Array);
            _stack.Add(newContext);
            newContext.Length = ReadInt32();
            break;
          }
        case BsonType.Binary:
          SetToken(JsonToken.Bytes, ReadBinary());
          break;
        case BsonType.Undefined:
          SetToken(JsonToken.Undefined);
          break;
        case BsonType.Oid:
          byte[] oid = ReadBytes(12);
          SetToken(JsonToken.Bytes, oid);
          break;
        case BsonType.Boolean:
          bool b = Convert.ToBoolean(ReadByte());
          SetToken(JsonToken.Boolean, b);
          break;
        case BsonType.Date:
          long ticks = ReadInt64();
          DateTime dateTime = JsonConvert.ConvertJavaScriptTicksToDateTime(ticks);
          SetToken(JsonToken.Date, dateTime);
          break;
        case BsonType.Null:
          SetToken(JsonToken.Null);
          break;
        case BsonType.Regex:
          string expression = ReadString();
          string modifiers = ReadString();

          string regex = @"/" + expression + @"/" + modifiers;
          SetToken(JsonToken.String, regex);
          break;
        case BsonType.Reference:
          SetToken(JsonToken.StartObject);
          _bsonReaderState = BsonReaderState.ReferenceStart;
          break;
        case BsonType.Code:
          SetToken(JsonToken.String, ReadLengthString());
          break;
        case BsonType.CodeWScope:
          SetToken(JsonToken.StartObject);
          _bsonReaderState = BsonReaderState.CodeWScopeStart;
          break;
        case BsonType.Integer:
          SetToken(JsonToken.Integer, (long)ReadInt32());
          break;
        case BsonType.TimeStamp:
        case BsonType.Long:
          SetToken(JsonToken.Integer, ReadInt64());
          break;
        default:
          throw new ArgumentOutOfRangeException("type", "Unexpected BsonType value: " + type);
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

    private long ReadInt64()
    {
      MovePosition(8);
      return _reader.ReadInt64();
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
