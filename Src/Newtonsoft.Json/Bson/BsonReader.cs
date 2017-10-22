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
using System.Text;
using System.IO;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;

namespace Newtonsoft.Json.Bson
{
    /// <summary>
    /// Represents a reader that provides fast, non-cached, forward-only access to serialized BSON data.
    /// </summary>
    [Obsolete("BSON reading and writing has been moved to its own package. See https://www.nuget.org/packages/Newtonsoft.Json.Bson for more details.")]
    public class BsonReader : JsonReader
    {
        private const int MaxCharBytesSize = 128;
        private static readonly byte[] SeqRange1 = new byte[] { 0, 127 }; // range of 1-byte sequence
        private static readonly byte[] SeqRange2 = new byte[] { 194, 223 }; // range of 2-byte sequence
        private static readonly byte[] SeqRange3 = new byte[] { 224, 239 }; // range of 3-byte sequence
        private static readonly byte[] SeqRange4 = new byte[] { 240, 244 }; // range of 4-byte sequence

        private readonly BinaryReader _reader;
        private readonly List<ContainerContext> _stack;

        private byte[] _byteBuffer;
        private char[] _charBuffer;

        private BsonType _currentElementType;
        private BsonReaderState _bsonReaderState;
        private ContainerContext _currentContext;

        private bool _readRootValueAsArray;
        private bool _jsonNet35BinaryCompatibility;
        private DateTimeKind _dateTimeKindHandling;

        private enum BsonReaderState
        {
            Normal = 0,
            ReferenceStart = 1,
            ReferenceRef = 2,
            ReferenceId = 3,
            CodeWScopeStart = 4,
            CodeWScopeCode = 5,
            CodeWScopeScope = 6,
            CodeWScopeScopeObject = 7,
            CodeWScopeScopeEnd = 8
        }

        private class ContainerContext
        {
            public readonly BsonType Type;
            public int Length;
            public int Position;

            public ContainerContext(BsonType type)
            {
                Type = type;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether binary data reading should be compatible with incorrect Json.NET 3.5 written binary.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if binary data reading will be compatible with incorrect Json.NET 3.5 written binary; otherwise, <c>false</c>.
        /// </value>
        [Obsolete("JsonNet35BinaryCompatibility will be removed in a future version of Json.NET.")]
        public bool JsonNet35BinaryCompatibility
        {
            get => _jsonNet35BinaryCompatibility;
            set => _jsonNet35BinaryCompatibility = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the root object will be read as a JSON array.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if the root object will be read as a JSON array; otherwise, <c>false</c>.
        /// </value>
        public bool ReadRootValueAsArray
        {
            get => _readRootValueAsArray;
            set => _readRootValueAsArray = value;
        }

        /// <summary>
        /// Gets or sets the <see cref="DateTimeKind" /> used when reading <see cref="DateTime"/> values from BSON.
        /// </summary>
        /// <value>The <see cref="DateTimeKind" /> used when reading <see cref="DateTime"/> values from BSON.</value>
        public DateTimeKind DateTimeKindHandling
        {
            get => _dateTimeKindHandling;
            set => _dateTimeKindHandling = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonReader"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the BSON data to read.</param>
        public BsonReader(Stream stream)
            : this(stream, false, DateTimeKind.Local)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonReader"/> class.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> containing the BSON data to read.</param>
        public BsonReader(BinaryReader reader)
            : this(reader, false, DateTimeKind.Local)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonReader"/> class.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> containing the BSON data to read.</param>
        /// <param name="readRootValueAsArray">if set to <c>true</c> the root object will be read as a JSON array.</param>
        /// <param name="dateTimeKindHandling">The <see cref="DateTimeKind" /> used when reading <see cref="DateTime"/> values from BSON.</param>
        public BsonReader(Stream stream, bool readRootValueAsArray, DateTimeKind dateTimeKindHandling)
        {
            ValidationUtils.ArgumentNotNull(stream, nameof(stream));
            _reader = new BinaryReader(stream);
            _stack = new List<ContainerContext>();
            _readRootValueAsArray = readRootValueAsArray;
            _dateTimeKindHandling = dateTimeKindHandling;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonReader"/> class.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryReader"/> containing the BSON data to read.</param>
        /// <param name="readRootValueAsArray">if set to <c>true</c> the root object will be read as a JSON array.</param>
        /// <param name="dateTimeKindHandling">The <see cref="DateTimeKind" /> used when reading <see cref="DateTime"/> values from BSON.</param>
        public BsonReader(BinaryReader reader, bool readRootValueAsArray, DateTimeKind dateTimeKindHandling)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            _reader = reader;
            _stack = new List<ContainerContext>();
            _readRootValueAsArray = readRootValueAsArray;
            _dateTimeKindHandling = dateTimeKindHandling;
        }

        private string ReadElement()
        {
            _currentElementType = ReadType();
            string elementName = ReadString();
            return elementName;
        }

        /// <summary>
        /// Reads the next JSON token from the underlying <see cref="Stream"/>.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.
        /// </returns>
        public override bool Read()
        {
            try
            {
                bool success;

                switch (_bsonReaderState)
                {
                    case BsonReaderState.Normal:
                        success = ReadNormal();
                        break;
                    case BsonReaderState.ReferenceStart:
                    case BsonReaderState.ReferenceRef:
                    case BsonReaderState.ReferenceId:
                        success = ReadReference();
                        break;
                    case BsonReaderState.CodeWScopeStart:
                    case BsonReaderState.CodeWScopeCode:
                    case BsonReaderState.CodeWScopeScope:
                    case BsonReaderState.CodeWScopeScopeObject:
                    case BsonReaderState.CodeWScopeScopeEnd:
                        success = ReadCodeWScope();
                        break;
                    default:
                        throw JsonReaderException.Create(this, "Unexpected state: {0}".FormatWith(CultureInfo.InvariantCulture, _bsonReaderState));
                }

                if (!success)
                {
                    SetToken(JsonToken.None);
                    return false;
                }

                return true;
            }
            catch (EndOfStreamException)
            {
                SetToken(JsonToken.None);
                return false;
            }
        }

        /// <summary>
        /// Changes the reader's state to <see cref="JsonReader.State.Closed"/>.
        /// If <see cref="JsonReader.CloseInput"/> is set to <c>true</c>, the underlying <see cref="Stream"/> is also closed.
        /// </summary>
        public override void Close()
        {
            base.Close();

            if (CloseInput)
            {
#if HAVE_STREAM_READER_WRITER_CLOSE
                _reader?.Close();
#else
                _reader?.Dispose();
#endif
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

                        ContainerContext newContext = new ContainerContext(BsonType.Object);
                        PushContext(newContext);
                        newContext.Length = ReadInt32();

                        return true;
                    }
                case BsonReaderState.CodeWScopeScopeObject:
                    bool result = ReadNormal();
                    if (result && TokenType == JsonToken.EndObject)
                    {
                        _bsonReaderState = BsonReaderState.CodeWScopeScopeEnd;
                    }

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
                    SetToken(JsonToken.PropertyName, JsonTypeReflector.RefPropertyName);
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
                        throw JsonReaderException.Create(this, "Unexpected state when reading BSON reference: " + _bsonReaderState);
                    }
                }
                case State.PostValue:
                {
                    if (_bsonReaderState == BsonReaderState.ReferenceRef)
                    {
                        SetToken(JsonToken.PropertyName, JsonTypeReflector.IdPropertyName);
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
                        throw JsonReaderException.Create(this, "Unexpected state when reading BSON reference: " + _bsonReaderState);
                    }
                }
                default:
                    throw JsonReaderException.Create(this, "Unexpected state when reading BSON reference: " + CurrentState);
            }
        }

        private bool ReadNormal()
        {
            switch (CurrentState)
            {
                case State.Start:
                {
                    JsonToken token = (!_readRootValueAsArray) ? JsonToken.StartObject : JsonToken.StartArray;
                    BsonType type = (!_readRootValueAsArray) ? BsonType.Object : BsonType.Array;

                    SetToken(token);
                    ContainerContext newContext = new ContainerContext(type);
                    PushContext(newContext);
                    newContext.Length = ReadInt32();
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
                case State.ObjectStart:
                case State.ArrayStart:
                case State.PostValue:
                    ContainerContext context = _currentContext;
                    if (context == null)
                    {
                        return false;
                    }

                    int lengthMinusEnd = context.Length - 1;

                    if (context.Position < lengthMinusEnd)
                    {
                        if (context.Type == BsonType.Array)
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
                        {
                            throw JsonReaderException.Create(this, "Unexpected end of object byte value.");
                        }

                        PopContext();
                        if (_currentContext != null)
                        {
                            MovePosition(context.Length);
                        }

                        JsonToken endToken = (context.Type == BsonType.Object) ? JsonToken.EndObject : JsonToken.EndArray;
                        SetToken(endToken);
                        return true;
                    }
                    else
                    {
                        throw JsonReaderException.Create(this, "Read past end of current container context.");
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

        private void PopContext()
        {
            _stack.RemoveAt(_stack.Count - 1);
            if (_stack.Count == 0)
            {
                _currentContext = null;
            }
            else
            {
                _currentContext = _stack[_stack.Count - 1];
            }
        }

        private void PushContext(ContainerContext newContext)
        {
            _stack.Add(newContext);
            _currentContext = newContext;
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
                    double d = ReadDouble();

                    if (_floatParseHandling == FloatParseHandling.Decimal)
                    {
                        SetToken(JsonToken.Float, Convert.ToDecimal(d, CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        SetToken(JsonToken.Float, d);
                    }
                    break;
                case BsonType.String:
                case BsonType.Symbol:
                    SetToken(JsonToken.String, ReadLengthString());
                    break;
                case BsonType.Object:
                {
                    SetToken(JsonToken.StartObject);

                    ContainerContext newContext = new ContainerContext(BsonType.Object);
                    PushContext(newContext);
                    newContext.Length = ReadInt32();
                    break;
                }
                case BsonType.Array:
                {
                    SetToken(JsonToken.StartArray);

                    ContainerContext newContext = new ContainerContext(BsonType.Array);
                    PushContext(newContext);
                    newContext.Length = ReadInt32();
                    break;
                }
                case BsonType.Binary:
                    BsonBinaryType binaryType;
                    byte[] data = ReadBinary(out binaryType);

                    object value = (binaryType != BsonBinaryType.Uuid)
                        ? data
                        : (object)new Guid(data);

                    SetToken(JsonToken.Bytes, value);
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
                    DateTime utcDateTime = DateTimeUtils.ConvertJavaScriptTicksToDateTime(ticks);

                    DateTime dateTime;
                    switch (DateTimeKindHandling)
                    {
                        case DateTimeKind.Unspecified:
                            dateTime = DateTime.SpecifyKind(utcDateTime, DateTimeKind.Unspecified);
                            break;
                        case DateTimeKind.Local:
                            dateTime = utcDateTime.ToLocalTime();
                            break;
                        default:
                            dateTime = utcDateTime;
                            break;
                    }

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
                    throw new ArgumentOutOfRangeException(nameof(type), "Unexpected BsonType value: " + type);
            }
        }

        private byte[] ReadBinary(out BsonBinaryType binaryType)
        {
            int dataLength = ReadInt32();

            binaryType = (BsonBinaryType)ReadByte();

#pragma warning disable 612,618
            // the old binary type has the data length repeated in the data for some reason
            if (binaryType == BsonBinaryType.BinaryOld && !_jsonNet35BinaryCompatibility)
            {
                dataLength = ReadInt32();
            }
#pragma warning restore 612,618

            return ReadBytes(dataLength);
        }

        private string ReadString()
        {
            EnsureBuffers();

            StringBuilder builder = null;

            int totalBytesRead = 0;
            // used in case of left over multibyte characters in the buffer
            int offset = 0;
            while (true)
            {
                int count = offset;
                byte b;
                while (count < MaxCharBytesSize && (b = _reader.ReadByte()) > 0)
                {
                    _byteBuffer[count++] = b;
                }
                int byteCount = count - offset;
                totalBytesRead += byteCount;

                if (count < MaxCharBytesSize && builder == null)
                {
                    // pref optimization to avoid reading into a string builder
                    // if string is smaller than the buffer then return it directly
                    int length = Encoding.UTF8.GetChars(_byteBuffer, 0, byteCount, _charBuffer, 0);

                    MovePosition(totalBytesRead + 1);
                    return new string(_charBuffer, 0, length);
                }
                else
                {
                    // calculate the index of the end of the last full character in the buffer
                    int lastFullCharStop = GetLastFullCharStop(count - 1);

                    int charCount = Encoding.UTF8.GetChars(_byteBuffer, 0, lastFullCharStop + 1, _charBuffer, 0);

                    if (builder == null)
                    {
                        builder = new StringBuilder(MaxCharBytesSize * 2);
                    }

                    builder.Append(_charBuffer, 0, charCount);

                    if (lastFullCharStop < byteCount - 1)
                    {
                        offset = byteCount - lastFullCharStop - 1;
                        // copy left over multi byte characters to beginning of buffer for next iteration
                        Array.Copy(_byteBuffer, lastFullCharStop + 1, _byteBuffer, 0, offset);
                    }
                    else
                    {
                        // reached end of string
                        if (count < MaxCharBytesSize)
                        {
                            MovePosition(totalBytesRead + 1);
                            return builder.ToString();
                        }

                        offset = 0;
                    }
                }
            }
        }

        private string ReadLengthString()
        {
            int length = ReadInt32();

            MovePosition(length);

            string s = GetString(length - 1);
            _reader.ReadByte();

            return s;
        }

        private string GetString(int length)
        {
            if (length == 0)
            {
                return string.Empty;
            }

            EnsureBuffers();

            StringBuilder builder = null;

            int totalBytesRead = 0;

            // used in case of left over multibyte characters in the buffer
            int offset = 0;
            do
            {
                int count = ((length - totalBytesRead) > MaxCharBytesSize - offset)
                    ? MaxCharBytesSize - offset
                    : length - totalBytesRead;

                int byteCount = _reader.Read(_byteBuffer, offset, count);

                if (byteCount == 0)
                {
                    throw new EndOfStreamException("Unable to read beyond the end of the stream.");
                }

                totalBytesRead += byteCount;

                // Above, byteCount is how many bytes we read this time.
                // Below, byteCount is how many bytes are in the _byteBuffer.
                byteCount += offset;

                if (byteCount == length)
                {
                    // pref optimization to avoid reading into a string builder
                    // first iteration and all bytes read then return string directly
                    int charCount = Encoding.UTF8.GetChars(_byteBuffer, 0, byteCount, _charBuffer, 0);
                    return new string(_charBuffer, 0, charCount);
                }
                else
                {
                    int lastFullCharStop = GetLastFullCharStop(byteCount - 1);

                    if (builder == null)
                    {
                        builder = new StringBuilder(length);
                    }

                    int charCount = Encoding.UTF8.GetChars(_byteBuffer, 0, lastFullCharStop + 1, _charBuffer, 0);
                    builder.Append(_charBuffer, 0, charCount);

                    if (lastFullCharStop < byteCount - 1)
                    {
                        offset = byteCount - lastFullCharStop - 1;
                        // copy left over multi byte characters to beginning of buffer for next iteration
                        Array.Copy(_byteBuffer, lastFullCharStop + 1, _byteBuffer, 0, offset);
                    }
                    else
                    {
                        offset = 0;
                    }
                }
            } while (totalBytesRead < length);

            return builder.ToString();
        }

        private int GetLastFullCharStop(int start)
        {
            int lookbackPos = start;
            int bis = 0;
            while (lookbackPos >= 0)
            {
                bis = BytesInSequence(_byteBuffer[lookbackPos]);
                if (bis == 0)
                {
                    lookbackPos--;
                    continue;
                }
                else if (bis == 1)
                {
                    break;
                }
                else
                {
                    lookbackPos--;
                    break;
                }
            }
            if (bis == start - lookbackPos)
            {
                //Full character.
                return start;
            }
            else
            {
                return lookbackPos;
            }
        }

        private int BytesInSequence(byte b)
        {
            if (b <= SeqRange1[1])
            {
                return 1;
            }
            if (b >= SeqRange2[0] && b <= SeqRange2[1])
            {
                return 2;
            }
            if (b >= SeqRange3[0] && b <= SeqRange3[1])
            {
                return 3;
            }
            if (b >= SeqRange4[0] && b <= SeqRange4[1])
            {
                return 4;
            }
            return 0;
        }

        private void EnsureBuffers()
        {
            if (_byteBuffer == null)
            {
                _byteBuffer = new byte[MaxCharBytesSize];
            }
            if (_charBuffer == null)
            {
                int charBufferSize = Encoding.UTF8.GetMaxCharCount(MaxCharBytesSize);
                _charBuffer = new char[charBufferSize];
            }
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

        private void MovePosition(int count)
        {
            _currentContext.Position += count;
        }

        private byte[] ReadBytes(int count)
        {
            MovePosition(count);
            return _reader.ReadBytes(count);
        }
    }
}