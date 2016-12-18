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

#if !(NET20 || NET35 || DOTNET || PORTABLE40 || PORTABLE)

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class EntityKeyMemberConverterImpl
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

    public partial class EntityKeyMemberConverter
    {
        private bool SafeAsync => GetType() == typeof(EntityKeyMemberConverter);

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
        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync
                ? DoWriteJsonAsync(writer, value, serializer, cancellationToken)
                : base.WriteJsonAsync(writer, value, serializer, cancellationToken);
        }

        internal async Task DoWriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            EnsureReflectionObject(value.GetType());

            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            string keyName = (string)_reflectionObject.GetValue(value, KeyPropertyName);
            object keyValue = _reflectionObject.GetValue(value, ValuePropertyName);

            Type keyValueType = keyValue?.GetType();

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(KeyPropertyName) : KeyPropertyName, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(keyName, cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(TypePropertyName) : TypePropertyName, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(keyValueType?.FullName, cancellationToken).ConfigureAwait(false);

            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(ValuePropertyName) : ValuePropertyName, cancellationToken).ConfigureAwait(false);

            if (keyValueType != null)
            {
                string valueJson;
                if (JsonSerializerInternalWriter.TryConvertToString(keyValue, keyValueType, out valueJson))
                {
                    await writer.WriteValueAsync(valueJson, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await writer.WriteValueAsync(keyValue, cancellationToken).ConfigureAwait(false);
                }
            }
            else
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
            }

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

        internal async Task<object> DoReadJsonAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            EnsureReflectionObject(objectType);

            object entityKeyMember = _reflectionObject.Creator();

            await ReadAndAssertPropertyAsync(reader, KeyPropertyName, cancellationToken).ConfigureAwait(false);
            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            _reflectionObject.SetValue(entityKeyMember, KeyPropertyName, reader.Value.ToString());

            await ReadAndAssertPropertyAsync(reader, TypePropertyName, cancellationToken).ConfigureAwait(false);
            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            string type = reader.Value.ToString();

            Type t = Type.GetType(type);

            await ReadAndAssertPropertyAsync(reader, ValuePropertyName, cancellationToken).ConfigureAwait(false);
            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            _reflectionObject.SetValue(entityKeyMember, ValuePropertyName, await serializer.DeserializeAsync(reader, t, cancellationToken).ConfigureAwait(false));

            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            return entityKeyMember;
        }

        private static async Task ReadAndAssertPropertyAsync(JsonReader reader, string propertyName, CancellationToken cancellationToken)
        {
            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            if (reader.TokenType != JsonToken.PropertyName || !string.Equals(reader.Value.ToString(), propertyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, propertyName));
            }
        }
    }
}

#endif
