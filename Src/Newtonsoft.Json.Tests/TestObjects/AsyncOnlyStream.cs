#if HAVE_ASYNC

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if DNXCORE50
using Xunit;
using Test = Xunit.FactAttribute;
using Assert = Newtonsoft.Json.Tests.XUnitAssert;
using TestCase = Xunit.InlineDataAttribute;
#else
using NUnit.Framework;
#endif

namespace Newtonsoft.Json.Tests.TestObjects
{
    internal class AsyncOnlyStream : System.IO.Stream
    {
        public AsyncOnlyStream()
        {
            innerStream = new MemoryStream();
        }

        private MemoryStream innerStream;

        public override bool CanRead { get => innerStream.CanRead; }
        public override bool CanSeek { get => innerStream.CanSeek; }
        public override bool CanWrite { get => innerStream.CanWrite; }
        public override long Length { get => innerStream.Length; }
        public override long Position { get => innerStream.Position; set => innerStream.Position = value; }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            return innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            Assert.Fail("Synchronous Read() called.");
            return 0;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, System.Threading.CancellationToken cancellationToken)
        {
            return innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Assert.Fail("Synchronous Write() called.");
        }

        public override Task FlushAsync(System.Threading.CancellationToken cancellationToken)
        {
            return innerStream.FlushAsync(cancellationToken);
        }

        public override void Flush()
        {
            Assert.Fail("Synchronous Flush() called.");
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            innerStream.SetLength(value);
        }
    }
}

#endif