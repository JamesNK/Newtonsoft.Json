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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
    internal partial class BsonBinaryWriter
    {
        private readonly AsyncBinaryWriter _asyncWriter;

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            if (_asyncWriter == null)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return cancellationToken.FromCanceled();
                }

                Flush();
                return AsyncUtils.CompletedTask;
            }

            return _asyncWriter.FlushAsync(cancellationToken);
        }

        public Task WriteTokenAsync(BsonToken t, CancellationToken cancellationToken)
        {
            if (_asyncWriter == null)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return cancellationToken.FromCanceled();
                }

                WriteToken(t);
                return AsyncUtils.CompletedTask;
            }

            CalculateSize(t);
            return WriteTokenInternalAsync(t, cancellationToken);
        }

        private Task WriteTokenInternalAsync(BsonToken t, CancellationToken cancellationToken)
        {
            switch (t.Type)
            {
                case BsonType.Object:
                    return WriteObjectAsync((BsonObject)t, cancellationToken);
                case BsonType.Array:
                    return WriteArrayAsync((BsonArray)t, cancellationToken);
                case BsonType.Integer:
                    return _asyncWriter.WriteAsync(Convert.ToInt32(((BsonValue)t).Value, CultureInfo.InvariantCulture), cancellationToken);
                case BsonType.Long:
                    return _asyncWriter.WriteAsync(Convert.ToInt64(((BsonValue)t).Value, CultureInfo.InvariantCulture), cancellationToken);
                case BsonType.Number:
                    return _asyncWriter.WriteAsync(Convert.ToDouble(((BsonValue)t).Value, CultureInfo.InvariantCulture), cancellationToken);
                case BsonType.String:
                    BsonString bsonString = (BsonString)t;
                    return WriteStringAsync((string)bsonString.Value, bsonString.ByteCount, bsonString.CalculatedSize - 4, cancellationToken);
                case BsonType.Boolean:
                    return _asyncWriter.WriteAsync((bool)((BsonValue)t).Value, cancellationToken);
                case BsonType.Null:
                case BsonType.Undefined:
                    return AsyncUtils.CompletedTask;
                case BsonType.Date:
                    BsonValue value = (BsonValue)t;

                    long ticks;

                    if (value.Value is DateTime)
                    {
                        DateTime dateTime = (DateTime)value.Value;
                        if (DateTimeKindHandling == DateTimeKind.Utc)
                        {
                            dateTime = dateTime.ToUniversalTime();
                        }
                        else if (DateTimeKindHandling == DateTimeKind.Local)
                        {
                            dateTime = dateTime.ToLocalTime();
                        }

                        ticks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTime, false);
                    }
                    else
                    {
                        DateTimeOffset dateTimeOffset = (DateTimeOffset)value.Value;
                        ticks = DateTimeUtils.ConvertDateTimeToJavaScriptTicks(dateTimeOffset.UtcDateTime, dateTimeOffset.Offset);
                    }

                    return _asyncWriter.WriteAsync(ticks, cancellationToken);
                case BsonType.Binary:
                    return WriteBinaryAsync((BsonBinary)t, cancellationToken);
                case BsonType.Oid:
                    return _asyncWriter.WriteAsync((byte[])((BsonValue)t).Value, cancellationToken);
                case BsonType.Regex:
                    return WriteRegexAsync((BsonRegex)t, cancellationToken);
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), "Unexpected token when writing BSON: {0}".FormatWith(CultureInfo.InvariantCulture, t.Type));
            }
        }

        private async Task WriteObjectAsync(BsonObject value, CancellationToken cancellationToken)
        {
            await _asyncWriter.WriteAsync(value.CalculatedSize, cancellationToken).ConfigureAwait(false);
            foreach (BsonProperty property in value)
            {
                await _asyncWriter.WriteAsync((sbyte)property.Value.Type, cancellationToken).ConfigureAwait(false);
                await WriteStringAsync((string)property.Name.Value, property.Name.ByteCount, null, cancellationToken).ConfigureAwait(false);
                BsonType propertyType = property.Value.Type;
                if (propertyType != BsonType.Null & propertyType != BsonType.Undefined)
                {
                    await WriteTokenInternalAsync(property.Value, cancellationToken).ConfigureAwait(false);
                }
            }

            await _asyncWriter.WriteAsync((byte)0, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteArrayAsync(BsonArray value, CancellationToken cancellationToken)
        {
            await _asyncWriter.WriteAsync(value.CalculatedSize, cancellationToken).ConfigureAwait(false);
            ulong index = 0;
            foreach (BsonToken c in value)
            {
                await _asyncWriter.WriteAsync((sbyte)c.Type, cancellationToken).ConfigureAwait(false);
                await WriteStringAsync(index.ToString(CultureInfo.InvariantCulture), MathUtils.IntLength(index), null, cancellationToken).ConfigureAwait(false);
                BsonType type = c.Type;
                if (type != BsonType.Null & type != BsonType.Undefined)
                {
                    await WriteTokenInternalAsync(c, cancellationToken).ConfigureAwait(false);
                }
                index++;
            }

            await _asyncWriter.WriteAsync((byte)0, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteBinaryAsync(BsonBinary value, CancellationToken cancellationToken)
        {
            byte[] data = (byte[])value.Value;
            await _asyncWriter.WriteAsync(data.Length, cancellationToken).ConfigureAwait(false);
            await _asyncWriter.WriteAsync((byte)value.BinaryType, cancellationToken).ConfigureAwait(false);
            await _asyncWriter.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteRegexAsync(BsonRegex value, CancellationToken cancellationToken)
        {
            await WriteStringAsync((string)value.Pattern.Value, value.Pattern.ByteCount, null, cancellationToken).ConfigureAwait(false);
            await WriteStringAsync((string)value.Options.Value, value.Options.ByteCount, null, cancellationToken).ConfigureAwait(false);
        }

        private async Task WriteStringAsync(string s, int byteCount, int? calculatedlengthPrefix, CancellationToken cancellationToken)
        {
            if (calculatedlengthPrefix != null)
            {
                await _asyncWriter.WriteAsync(calculatedlengthPrefix.GetValueOrDefault(), cancellationToken).ConfigureAwait(false);
            }

            await WriteUtf8BytesAsync(s, byteCount, cancellationToken).ConfigureAwait(false);
            await _asyncWriter.WriteAsync((byte)0, cancellationToken).ConfigureAwait(false);
        }

        private Task WriteUtf8BytesAsync(string s, int byteCount, CancellationToken cancellationToken)
        {
            if (s == null)
            {
                return AsyncUtils.CompletedTask;
            }

            if (byteCount <= 256)
            {
                if (_largeByteBuffer == null)
                {
                    _largeByteBuffer = new byte[256];
                }

                Encoding.GetBytes(s, 0, s.Length, _largeByteBuffer, 0);
                return _asyncWriter.WriteAsync(_largeByteBuffer, 0, byteCount, cancellationToken);
            }

            byte[] bytes = Encoding.GetBytes(s);
            return _asyncWriter.WriteAsync(bytes, cancellationToken);
        }
    }
}

#endif
