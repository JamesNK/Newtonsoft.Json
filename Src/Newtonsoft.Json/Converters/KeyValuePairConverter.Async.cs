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
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class KeyValuePairConverterImpl
    {
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoReadJsonAsync(reader, objectType, serializer, cancellationToken);
        }

        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteJsonAsync(writer, value, serializer, cancellationToken);
        }
    }

    public partial class KeyValuePairConverter
    {
        private bool SafeAsync => GetType() == typeof(KeyValuePairConverter);

        /// <summary>
        /// Asynchronously writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous write operation.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken)) => SafeAsync
            ? DoWriteJsonAsync(writer, value, serializer, cancellationToken)
            : base.WriteJsonAsync(writer, value, serializer, cancellationToken);

        internal async Task DoWriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(value.GetType());

            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(KeyName) : KeyName, cancellationToken).ConfigureAwait(false);
            await serializer.SerializeAsync(writer, reflectionObject.GetValue(value, KeyName), reflectionObject.GetType(KeyName), cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(ValueName) : ValueName, cancellationToken).ConfigureAwait(false);
            await serializer.SerializeAsync(writer, reflectionObject.GetValue(value, ValueName), reflectionObject.GetType(ValueName), cancellationToken).ConfigureAwait(false);
            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Asynchronously reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A task that represents the asynchronous read operation. The value of the Result property is the object read.</returns>
        /// <remarks>Derived classes must override this method to get asynchronous behaviour. Otherwise it will
        /// execute synchronously, returning an already-completed task.</remarks>
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync
                ? DoReadJsonAsync(reader, objectType, serializer, cancellationToken)
                : base.ReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        internal Task<object> DoReadJsonAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionUtils.IsNullableType(objectType))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
                }

                return cancellationToken.CancelledOrNullAsync();
            }

            return ReadJsonNotNullAsync(reader, objectType, serializer, cancellationToken);
        }

        private async Task<object> ReadJsonNotNullAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            object key = null;
            object value = null;

            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            Type t = ReflectionUtils.IsNullableType(objectType)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(t);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();
                if (string.Equals(propertyName, KeyName, StringComparison.OrdinalIgnoreCase))
                {
                    await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    key = serializer.Deserialize(reader, reflectionObject.GetType(KeyName));
                }
                else if (string.Equals(propertyName, ValueName, StringComparison.OrdinalIgnoreCase))
                {
                    await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    value = serializer.Deserialize(reader, reflectionObject.GetType(ValueName));
                }
                else
                {
                    await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                }

                await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            return reflectionObject.Creator(key, value);
        }
    }
}

#endif
