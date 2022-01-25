#if HAVE_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Represents a wrapper writer that throws on all synchronous operations.
    /// </summary>
    public sealed class JsonNoSyncWriter : JsonWriter
    {
        private readonly JsonWriter _writer;

        /// <summary>
        /// Creates a new instance of <see cref="JsonNoSyncWriter" />.
        /// </summary>
        /// <param name="writer">The reader to wrap.</param>
        public JsonNoSyncWriter(JsonWriter writer)
        {
            _writer = writer;
        }

        /// <summary>
        /// Gets the wrapped <see cref="JsonWriter" />.
        /// </summary>
        public JsonWriter UnderlyingWriter => _writer;

        /// <inheritdoc/>
        public override void Flush() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteComment(string? text) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteEnd() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteEndArray() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteEndConstructor() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteEndObject() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteNull() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WritePropertyName(string name) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WritePropertyName(string name, bool escape) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteRaw(string? json) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteRawValue(string? json) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteStartArray() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteStartConstructor(string name) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteStartObject() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteUndefined() => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(bool value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(bool? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(byte value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(byte? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(byte[]? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(char value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(char? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(DateTime value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(DateTime? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(DateTimeOffset value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(DateTimeOffset? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(decimal value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(decimal? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(double value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(double? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(float value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(float? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(Guid value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(Guid? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(int value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(int? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(long value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(long? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(object? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(sbyte? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(short value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(short? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(string? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(TimeSpan value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(TimeSpan? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(uint value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(uint? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(ulong value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(ulong? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteValue(Uri? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(ushort value) => throw new NotSupportedException();
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override void WriteValue(ushort? value) => throw new NotSupportedException();
        /// <inheritdoc/>
        public override void WriteWhitespace(string ws) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Close() => _writer.Close(); // XXX: this writes in some circumstances
        /// <inheritdoc/>
        public override Task CloseAsync(CancellationToken cancellationToken = default) => _writer.CloseAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task FlushAsync(CancellationToken cancellationToken = default) => _writer.FlushAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteCommentAsync(string? text, CancellationToken cancellationToken = default) => _writer.WriteCommentAsync(text, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteEndArrayAsync(CancellationToken cancellationToken = default) => _writer.WriteEndArrayAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteEndAsync(CancellationToken cancellationToken = default) => _writer.WriteEndAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteEndConstructorAsync(CancellationToken cancellationToken = default) => _writer.WriteEndConstructorAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteEndObjectAsync(CancellationToken cancellationToken = default) => _writer.WriteEndObjectAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteNullAsync(CancellationToken cancellationToken = default) => _writer.WriteNullAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WritePropertyNameAsync(string name, bool escape, CancellationToken cancellationToken = default) => _writer.WritePropertyNameAsync(name, escape, cancellationToken);
        /// <inheritdoc/>
        public override Task WritePropertyNameAsync(string name, CancellationToken cancellationToken = default) => _writer.WritePropertyNameAsync(name, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteRawAsync(string? json, CancellationToken cancellationToken = default) => _writer.WriteRawAsync(json, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteRawValueAsync(string? json, CancellationToken cancellationToken = default) => _writer.WriteRawValueAsync(json, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteStartArrayAsync(CancellationToken cancellationToken = default) => _writer.WriteStartArrayAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteStartConstructorAsync(string name, CancellationToken cancellationToken = default) => _writer.WriteStartConstructorAsync(name, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteStartObjectAsync(CancellationToken cancellationToken = default) => _writer.WriteStartObjectAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteUndefinedAsync(CancellationToken cancellationToken = default) => _writer.WriteUndefinedAsync(cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(bool value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(bool? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(byte value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(byte? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(byte[]? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(char value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(char? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(DateTime value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(DateTime? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(DateTimeOffset value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(DateTimeOffset? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(decimal value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(decimal? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(double value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(double? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(float value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(float? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(Guid value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(Guid? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(int value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(int? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(long value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(long? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(object? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(sbyte value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(sbyte? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(short value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(short? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(string? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(TimeSpan value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(TimeSpan? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(uint value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(uint? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ulong value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ulong? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteValueAsync(Uri? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ushort value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        [CLSCompliant(false)]
        public override Task WriteValueAsync(ushort? value, CancellationToken cancellationToken = default) => _writer.WriteValueAsync(value, cancellationToken);
        /// <inheritdoc/>
        public override Task WriteWhitespaceAsync(string ws, CancellationToken cancellationToken = default) => _writer.WriteWhitespaceAsync(ws, cancellationToken);
    }
}

#endif
