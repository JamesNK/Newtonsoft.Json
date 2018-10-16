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

#if HAVE_ASYNC

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json
{
    public abstract partial class JsonReader
    {
        /// <summary>
        /// Asynchronously reads the next JSON token from the source.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns <c>true</c> if the next token was read successfully; <c>false</c> if there are no more tokens to read.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<bool>() ?? Read().ToAsync();
        }

        /// <summary>
        /// Asynchronously skips the children of the current token.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public async Task SkipAsync(CancellationToken cancellationToken = default)
        {
            if (TokenType == JsonToken.PropertyName)
            {
                await ReadAsync(cancellationToken).ConfigureAwait(false);
            }

            if (JsonTokenUtils.IsStartToken(TokenType))
            {
                int depth = Depth;

                while (await ReadAsync(cancellationToken).ConfigureAwait(false) && depth < Depth)
                {
                }
            }
        }

        internal async Task ReaderReadAndAssertAsync(CancellationToken cancellationToken)
        {
            if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                throw CreateUnexpectedEndException();
            }
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="bool"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="bool"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<bool?>() ?? Task.FromResult(ReadAsBoolean());
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="byte"/>[].
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="byte"/>[]. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<byte[]>() ?? Task.FromResult(ReadAsBytes());
        }

        internal async Task<byte[]> ReadArrayIntoByteArrayAsync(CancellationToken cancellationToken)
        {
            List<byte> buffer = new List<byte>();

            while (true)
            {
                if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    SetToken(JsonToken.None);
                }

                if (ReadArrayElementIntoByteArrayReportDone(buffer))
                {
                    byte[] d = buffer.ToArray();
                    SetToken(JsonToken.Bytes, d, false);
                    return d;
                }
            }
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTime"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTime"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<DateTime?>() ?? Task.FromResult(ReadAsDateTime());
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="DateTimeOffset"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<DateTimeOffset?>() ?? Task.FromResult(ReadAsDateTimeOffset());
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="decimal"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="decimal"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<decimal?>() ?? Task.FromResult(ReadAsDecimal());
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="double"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="double"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadAsDouble());
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="Nullable{T}"/> of <see cref="int"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="Nullable{T}"/> of <see cref="int"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<int?>() ?? Task.FromResult(ReadAsInt32());
        }

        /// <summary>
        /// Asynchronously reads the next JSON token from the source as a <see cref="string"/>.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task{TResult}"/> that represents the asynchronous read. The <see cref="Task{TResult}.Result"/>
        /// property returns the <see cref="string"/>. This result will be <c>null</c> at the end of an array.</returns>
        /// <remarks>The default behaviour is to execute synchronously, returning an already-completed task. Derived
        /// classes can override this behaviour for true asynchronicity.</remarks>
        public virtual Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<string>() ?? Task.FromResult(ReadAsString());
        }

        internal async Task<bool> ReadAndMoveToContentAsync(CancellationToken cancellationToken)
        {
            return await ReadAsync(cancellationToken).ConfigureAwait(false) && await MoveToContentAsync(cancellationToken).ConfigureAwait(false);
        }

        internal Task<bool> MoveToContentAsync(CancellationToken cancellationToken)
        {
            switch (TokenType)
            {
                case JsonToken.None:
                case JsonToken.Comment:
                    return MoveToContentFromNonContentAsync(cancellationToken);
                default:
                    return AsyncUtils.True;
            }
        }

        private async Task<bool> MoveToContentFromNonContentAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    return false;
                }

                switch (TokenType)
                {
                    case JsonToken.None:
                    case JsonToken.Comment:
                        break;
                    default:
                        return true;
                }
            }
        }
    }
}

#endif
