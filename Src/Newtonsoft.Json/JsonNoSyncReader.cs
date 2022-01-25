#if HAVE_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Represents a wrapper reader that throws on all synchronous operations.
    /// </summary>
    public sealed class JsonNoSyncReader : JsonReader, IJsonLineInfo
    {
        private readonly JsonReader _reader;

        /// <summary>
        /// Creates a new instance of <see cref="JsonNoSyncReader" />.
        /// </summary>
        /// <param name="reader">The reader to wrap.</param>
        public JsonNoSyncReader(JsonReader reader)
        {
            _reader = reader;
        }

        /// <inheritdoc/>
        public override bool Read() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override bool? ReadAsBoolean() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override byte[] ReadAsBytes() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override DateTime? ReadAsDateTime() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override decimal? ReadAsDecimal() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override double? ReadAsDouble() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override int? ReadAsInt32() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override string ReadAsString() => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Close() => _reader.Close();
        /// <inheritdoc/>
        public override int Depth => _reader.Depth;
        /// <inheritdoc/>
        public override string Path => _reader.Path;
        /// <inheritdoc/>
        public override char QuoteChar { get => _reader.QuoteChar; protected internal set => throw new NotImplementedException(); }
        /// <inheritdoc/>
        public override Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default) => _reader.ReadAsBooleanAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellationToken = default) => _reader.ReadAsBytesAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default) => _reader.ReadAsDateTimeAsync(cancellationToken);
        /// <inheritdoc/>
        public override DateTimeOffset? ReadAsDateTimeOffset() => _reader.ReadAsDateTimeOffset();
        /// <inheritdoc/>
        public override Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default) => _reader.ReadAsDateTimeOffsetAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default) => _reader.ReadAsDecimalAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default) => _reader.ReadAsDoubleAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default) => _reader.ReadAsInt32Async(cancellationToken);
        /// <inheritdoc/>
        public override Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default) => _reader.ReadAsStringAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task<bool> ReadAsync(CancellationToken cancellationToken = default) => _reader.ReadAsync(cancellationToken);

        /// <inheritdoc/>
        public override JsonToken TokenType => _reader.TokenType;
        /// <inheritdoc/>
        public override object? Value => _reader.Value;
        /// <inheritdoc/>
        public override Type? ValueType => _reader.ValueType;

        bool IJsonLineInfo.HasLineInfo()
        {
            return _reader is IJsonLineInfo lineInfo && lineInfo.HasLineInfo();
        }

        int IJsonLineInfo.LineNumber =>(_reader is IJsonLineInfo lineInfo) ? lineInfo.LineNumber : 0;

        int IJsonLineInfo.LinePosition =>(_reader is IJsonLineInfo lineInfo) ? lineInfo.LinePosition : 0;
    }
}

#endif
