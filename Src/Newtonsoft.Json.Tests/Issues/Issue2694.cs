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

#if !(NET20 || NET35 || NET40 || PORTABLE || PORTABLE40) || NETSTANDARD1_3 || NETSTANDARD2_0 || NET6_0_OR_GREATER
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
#if DNXCORE50
using System.Reflection;
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.Issues
{
    [TestFixture]
    public class Issue2694 : TestFixtureBase
    {
#if NET6_0_OR_GREATER
        [Test]
        public async Task Test_DisposeAsync()
        {
            MemoryStream ms = new MemoryStream();
            Stream s = new AsyncOnlyStream(ms);
            StreamWriter sr = new StreamWriter(s, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 2, leaveOpen: true);
            await using (JsonTextWriter writer = new JsonTextWriter(sr))
            {
                await writer.WriteStartObjectAsync();
            }

            string json = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("{}", json);
        }
#endif

        [Test]
        public async Task Test_CloseAsync()
        {
            MemoryStream ms = new MemoryStream();
            Stream s = new AsyncOnlyStream(ms);
            StreamWriter sr = new StreamWriter(s, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 2, leaveOpen: true);
            JsonTextWriter writer = new JsonTextWriter(sr);
            await writer.WriteStartObjectAsync();

            await writer.CloseAsync();

            string json = Encoding.UTF8.GetString(ms.ToArray());
            Assert.AreEqual("{}", json);
        }

        public class AsyncOnlyStream : Stream
        {
            private readonly Stream _innerStream;
            private int _unflushedContentLength;

            public AsyncOnlyStream(Stream innerStream)
            {
                _innerStream = innerStream;
            }

            public override void Flush()
            {
                // It's ok to call Flush if the content was already processed with FlushAsync.
                if (_unflushedContentLength > 0)
                {
                    throw new Exception($"Flush when there is {_unflushedContentLength} bytes buffered.");
                }
            }

            public override Task FlushAsync(CancellationToken cancellationToken)
            {
                _unflushedContentLength = 0;
                return _innerStream.FlushAsync(cancellationToken);
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _innerStream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _innerStream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }

            public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                _unflushedContentLength += count;
                return _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            }

            public override bool CanRead => _innerStream.CanRead;
            public override bool CanSeek => _innerStream.CanSeek;
            public override bool CanWrite => _innerStream.CanWrite;
            public override long Length => _innerStream.Length;

            public override long Position
            {
                get => _innerStream.Position;
                set => _innerStream.Position = value;
            }
        }
    }
}
#endif