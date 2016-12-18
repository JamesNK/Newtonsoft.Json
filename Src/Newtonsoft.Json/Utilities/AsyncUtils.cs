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
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities
{
    internal static class AsyncUtils
    {
        private static readonly Action _nop = () => { };
        // Pre-allocate to avoid wasted allocations.
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<bool> True = Task.FromResult(true);
        public static readonly Task<object> NullTask = Task.FromResult<object>(null);

        internal static Task<bool> ToAsync(this bool value)
        {
            return value ? True : False;
        }

        public static Task CancelIfRequesedAsync(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? CancelledAsync(cancellationToken) : null;
        }

        public static Task<T> CancelIfRequesedAsync<T>(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? CancelledAsync<T>(cancellationToken) : null;
        }

        public static Task<object> CancelledOrNullAsync(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? CancelledAsync<object>(cancellationToken) : NullTask;
        }

        public static Task CancelledAsync(this CancellationToken cancellationToken)
        {
            Debug.Assert(cancellationToken.IsCancellationRequested);
            return new Task(_nop, cancellationToken);
        }

        public static Task<T> CancelledAsync<T>(this CancellationToken cancellationToken)
        {
            Debug.Assert(cancellationToken.IsCancellationRequested);
            return new Task<T>(() => default(T), cancellationToken);
        }

        // Task.Delay(0) is optimised as a cached task within the framework, and indeed
        // the same cached task that CompletedTask returns as of 4.6, but we'll add our
        // own property for previous frameworks.
        internal static Task CompletedTask
        {
            get { return Task.Delay(0); }
        }

        public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? CancelledAsync(cancellationToken) : writer.WriteAsync(value);
        }
        public static Task WriteAsync(this TextWriter writer, string value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? CancelledAsync(cancellationToken) : writer.WriteAsync(value);
        }
        public static Task WriteAsync(this TextWriter writer, char[] value, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? CancelledAsync(cancellationToken) : writer.WriteAsync(value);
        }
        public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested
                ? CancelledAsync(cancellationToken) : writer.WriteAsync(value, start, count);
        }
        public static Task WriteLineAsync(this TextWriter writer, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? CancelledAsync(cancellationToken) : writer.WriteLineAsync();
        }

        public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            Debug.Assert(reader != null);
            return cancellationToken.IsCancellationRequested ? CancelledAsync<int>(cancellationToken) : reader.ReadAsync(buffer, index, count);
        }
    }
}

#endif
