#if HAVE_ASYNC

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json
{
    /// <summary>
    /// A bolt-on interface for <see cref="JsonConverter"/>s
    /// to convert an object to and from JSON asynchronously.
    /// </summary>
    /// <remarks>
    /// This interface is helpful when extending existing
    /// converters which one cannot recompile.
    /// </remarks>
    public interface IJsonAsyncConverter
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="JsonReader"/> to read from.
        /// </param>
        /// <param name="objectType">
        /// Type of the object.
        /// </param>
        /// <param name="existingValue">
        /// The existing value of object being read.
        /// </param>
        /// <param name="serializer">
        /// The calling serializer.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The object value.
        /// </returns>
        Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken);

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="JsonWriter"/> to write to.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="serializer">
        /// The calling serializer.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// It is permissible for a <see cref="JsonAsyncConverter"/> to implement
    /// only the <see cref="JsonConverter.ReadJson"/> method when the converter
    /// works with the current token and does not need to read anything.
    /// </remarks>
    public abstract class JsonAsyncConverter : JsonConverter, IJsonAsyncConverter
    {
        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">
        /// The <see cref="JsonReader"/> to read from.
        /// </param>
        /// <param name="objectType">
        /// Type of the object.
        /// </param>
        /// <param name="existingValue">
        /// The existing value of object being read.
        /// </param>
        /// <param name="serializer">
        /// The calling serializer.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        /// <returns>
        /// The object value.
        /// </returns>
        public virtual Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            return Task.FromResult(ReadJson(reader, objectType, existingValue, serializer));
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="JsonWriter"/> to write to.
        /// </param>
        /// <param name="value">
        /// The value.
        /// </param>
        /// <param name="serializer">
        /// The calling serializer.
        /// </param>
        /// <param name="cancellationToken">
        /// The cancellation token.
        /// </param>
        public abstract Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken);
    }
}

#endif
