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
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities
{
    internal partial class Base64Encoder
    {
        public async Task EncodeAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
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

            if (count > buffer.Length - index)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (_leftOverBytesCount > 0)
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
                    return;
                }

                int num2 = Convert.ToBase64CharArray(_leftOverBytes, 0, 3, _charsLine, 0);
                await WriteCharsAsync(_charsLine, 0, num2, cancellationToken).ConfigureAwait(false);
            }

            _leftOverBytesCount = count % 3;
            if (_leftOverBytesCount > 0)
            {
                count -= _leftOverBytesCount;
                if (_leftOverBytes == null)
                {
                    _leftOverBytes = new byte[3];
                }
                for (int i = 0; i < _leftOverBytesCount; i++)
                {
                    _leftOverBytes[i] = buffer[index + count + i];
                }
            }

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
                return cancellationToken.CancelledAsync();

            if (_leftOverBytesCount > 0)
            {
                int count = Convert.ToBase64CharArray(_leftOverBytes, 0, _leftOverBytesCount, _charsLine, 0);
                _leftOverBytesCount = 0;
                return WriteCharsAsync(_charsLine, 0, count, cancellationToken);
            }

            return AsyncUtils.CompletedTask;
        }
    }
}

#endif
