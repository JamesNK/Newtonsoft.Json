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

namespace Newtonsoft.Json.Serialization
{
    internal partial class TraceJsonReader
    {
        public override async Task<bool> ReadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            bool value = await _innerReader.ReadAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default(CancellationToken))
        {
            int? value = await _innerReader.ReadAsInt32Async(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            bool? value = await _innerReader.ReadAsBooleanAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<byte[]> ReadAsBytesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            byte[] value = await _innerReader.ReadAsBytesAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            DateTime? value = await _innerReader.ReadAsDateTimeAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            DateTimeOffset? value = await _innerReader.ReadAsDateTimeOffsetAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            decimal? value = await _innerReader.ReadAsDecimalAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            double? value = await _innerReader.ReadAsDoubleAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }

        public override async Task<string> ReadAsStringAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            string value = await _innerReader.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            _textWriter.WriteToken(_innerReader, false, false, true);
            return value;
        }
    }
}

#endif
