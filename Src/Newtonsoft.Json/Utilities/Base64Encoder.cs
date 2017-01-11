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
using System.IO;
#if HAVE_ASYNC
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Newtonsoft.Json.Utilities
{
    internal class Base64Encoder
    {
        private const int Base64LineSize = 76;
        private const int LineSizeInBytes = 57;

        private readonly char[] _charsLine = new char[Base64LineSize];
        private readonly TextWriter _writer;

        private byte[] _leftOverBytes;
        private int _leftOverBytesCount;

        public Base64Encoder(TextWriter writer)
        {
            ValidationUtils.ArgumentNotNull(writer, nameof(writer));
            _writer = writer;
        }

        private void ValidateEncode(byte[] buffer, int index, int count)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count > (buffer.Length - index))
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
        }

        public void Encode(byte[] buffer, int index, int count)
        {
            ValidateEncode(buffer, index, count);

            if (_leftOverBytesCount > 0)
            {
                if(FulfillFromLeftover(buffer, index, ref count))
                {
                    return;
                }

                int num2 = Convert.ToBase64CharArray(_leftOverBytes, 0, 3, _charsLine, 0);
                WriteChars(_charsLine, 0, num2);
            }

            StoreLeftOverBytes(buffer, index, ref count);

            int num4 = index + count;
            int length = LineSizeInBytes;
            while (index < num4)
            {
                if ((index + length) > num4)
                {
                    length = num4 - index;
                }
                int num6 = Convert.ToBase64CharArray(buffer, index, length, _charsLine, 0);
                WriteChars(_charsLine, 0, num6);
                index += length;
            }
        }

        private void StoreLeftOverBytes(byte[] buffer, int index, ref int count)
        {
            int leftOverBytesCount = count % 3;
            if (leftOverBytesCount > 0)
            {
                count -= leftOverBytesCount;
                if (_leftOverBytes == null)
                {
                    _leftOverBytes = new byte[3];
                }

                for (int i = 0; i < leftOverBytesCount; i++)
                {
                    _leftOverBytes[i] = buffer[index + count + i];
                }
            }

            _leftOverBytesCount = leftOverBytesCount;
        }

        private bool FulfillFromLeftover(byte[] buffer, int index, ref int count)
        {
            int leftOverBytesCount = _leftOverBytesCount;
            while (leftOverBytesCount < 3 && count > 0)
            {
                _leftOverBytes[leftOverBytesCount++] = buffer[index++];
                count--;
            }

            if (count == 0 && leftOverBytesCount < 3)
            {
                _leftOverBytesCount = leftOverBytesCount;
                return true;
            }

            return false;
        }

        public void Flush()
        {
            if (_leftOverBytesCount > 0)
            {
                int count = Convert.ToBase64CharArray(_leftOverBytes, 0, _leftOverBytesCount, _charsLine, 0);
                WriteChars(_charsLine, 0, count);
                _leftOverBytesCount = 0;
            }
        }

        private void WriteChars(char[] chars, int index, int count)
        {
            _writer.Write(chars, index, count);
        }

#if HAVE_ASYNC

        public async Task EncodeAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            ValidateEncode(buffer, index, count);

            if (_leftOverBytesCount > 0)
            {
                if (FulfillFromLeftover(buffer, index, ref count))
                {
                    return;
                }

                int num2 = Convert.ToBase64CharArray(_leftOverBytes, 0, 3, _charsLine, 0);
                await WriteCharsAsync(_charsLine, 0, num2, cancellationToken).ConfigureAwait(false);
            }

            StoreLeftOverBytes(buffer, index, ref count);

            int num4 = index + count;
            int length = LineSizeInBytes;
            while (index < num4)
            {
                if (index + length > num4)
                {
                    length = num4 - index;
                }
                int num6 = Convert.ToBase64CharArray(buffer, index, length, _charsLine, 0);
                await WriteCharsAsync(_charsLine, 0, num6, cancellationToken).ConfigureAwait(false);
                index += length;
            }
        }

        private Task WriteCharsAsync(char[] chars, int index, int count, CancellationToken cancellationToken)
        {
            return _writer.WriteAsync(chars, index, count, cancellationToken);
        }

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return cancellationToken.FromCanceled();
            }

            if (_leftOverBytesCount > 0)
            {
                int count = Convert.ToBase64CharArray(_leftOverBytes, 0, _leftOverBytesCount, _charsLine, 0);
                _leftOverBytesCount = 0;
                return WriteCharsAsync(_charsLine, 0, count, cancellationToken);
            }

            return AsyncUtils.CompletedTask;
        }

#endif

    }
}