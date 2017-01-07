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

namespace Newtonsoft.Json.Bson
{
    // This is not a intended as a general-purpose binary writer, caution should be exercised
    // if adapting for other uses.
    internal class AsyncBinaryWriter : BinaryWriter
    {
        private static readonly byte[] TrueFalse = { 1, 0 };
        private byte[] _buffer;

        public AsyncBinaryWriter(Stream stream)
            : base(stream)
        {
        }

        private byte[] Buffer => _buffer ?? (_buffer = new byte[8]);

        public Task FlushAsync(CancellationToken cancellationToken)
        {
            return OutStream.FlushAsync(cancellationToken);
        }

        public Task WriteAsync(bool value, CancellationToken cancellationToken)
        {
            return OutStream.WriteAsync(TrueFalse, value ? 0 : 1, 1, cancellationToken);
        }

        public Task WriteAsync(int value, CancellationToken cancellationToken)
        {
            byte[] buffer = Buffer;
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            return OutStream.WriteAsync(buffer, 0, 4, cancellationToken);
        }

        public Task WriteAsync(long value, CancellationToken cancellationToken)
        {
            byte[] buffer = Buffer;
            buffer[0] = (byte)value;
            buffer[1] = (byte)(value >> 8);
            buffer[2] = (byte)(value >> 16);
            buffer[3] = (byte)(value >> 24);
            buffer[4] = (byte)(value >> 32);
            buffer[5] = (byte)(value >> 40);
            buffer[6] = (byte)(value >> 48);
            buffer[7] = (byte)(value >> 56);
            return OutStream.WriteAsync(buffer, 0, 8, cancellationToken);
        }

        public Task WriteAsync(byte value, CancellationToken cancellationToken)
        {
            byte[] buffer = Buffer;
            buffer[0] = value;
            return OutStream.WriteAsync(buffer, 0, 1, cancellationToken);
        }

        public Task WriteAsync(sbyte value, CancellationToken cancellationToken)
        {
            return WriteAsync((byte)value, cancellationToken);
        }

        public Task WriteAsync(double value, CancellationToken cancellationToken)
        {
            return WriteAsync(BitConverter.DoubleToInt64Bits(value), cancellationToken);
        }

        public Task WriteAsync(byte[] buffer, CancellationToken cancellationToken)
        {
            return OutStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
        }

        public Task WriteAsync(byte[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            return OutStream.WriteAsync(buffer, index, count, cancellationToken);
        }
    }

    internal class AsyncBinaryWriterOwningWriter : AsyncBinaryWriter
    {
        private readonly BinaryWriter _writer;

        public AsyncBinaryWriterOwningWriter(BinaryWriter writer)
            : base(writer.BaseStream)
        {
            Debug.Assert(writer.GetType() == typeof(BinaryWriter));
            _writer = writer;
        }

#if !(DOTNET || PORTABLE40 || PORTABLE)
        public override void Close()
        {
            // Don't call base.Close(). Let this writer decide
            // whether or not to close the stream.
            _writer.Close();
        }
#endif

        protected override void Dispose(bool disposing)
        {
            // Don't call base.Dispose(disposing). Let this writer decide
            // whether or not to close the stream.
            _writer.Dispose();
        }
    }
}

#endif
