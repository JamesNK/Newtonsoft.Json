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
#if HAVE_ASYNC
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Converters
{
    public partial class KeyValuePairConverter : JsonConverter
    {
        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public override async Task WriteJsonAsync(JsonWriter writer, object? value, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            if (value == null)
            {
                await writer.WriteNullAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(value.GetType());

            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync((resolver != null) ? resolver.GetResolvedPropertyName(KeyName) : KeyName, cancellationToken).ConfigureAwait(false);
            await serializer.SerializeAsync(writer, reflectionObject.GetValue(value, KeyName), reflectionObject.GetType(KeyName), cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync((resolver != null) ? resolver.GetResolvedPropertyName(ValueName) : ValueName, cancellationToken).ConfigureAwait(false);
            await serializer.SerializeAsync(writer, reflectionObject.GetValue(value, ValueName), reflectionObject.GetType(ValueName), cancellationToken).ConfigureAwait(false);
            await writer.WriteEndObjectAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>The object value.</returns>
        public override async Task<object?> ReadJsonAsync(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionUtils.IsNullableType(objectType))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
                }

                return null;
            }

            object? key = null;
            object? value = null;

            await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            Type t = ReflectionUtils.IsNullableType(objectType)
                ? Nullable.GetUnderlyingType(objectType)!
                : objectType;

            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(t);
            JsonContract keyContract = serializer.ContractResolver.ResolveContract(reflectionObject.GetType(KeyName));
            JsonContract valueContract = serializer.ContractResolver.ResolveContract(reflectionObject.GetType(ValueName));

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value!.ToString()!;
                if (string.Equals(propertyName, KeyName, StringComparison.OrdinalIgnoreCase))
                {
                    await reader.ReadForTypeAndAssertAsync(keyContract, false, cancellationToken).ConfigureAwait(false);

                    key = await serializer.DeserializeAsync(reader, keyContract.UnderlyingType, cancellationToken).ConfigureAwait(false);
                }
                else if (string.Equals(propertyName, ValueName, StringComparison.OrdinalIgnoreCase))
                {
                    await reader.ReadForTypeAndAssertAsync(valueContract, false, cancellationToken).ConfigureAwait(false);

                    value = await serializer.DeserializeAsync(reader, valueContract.UnderlyingType, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    await reader.SkipAsync(cancellationToken).ConfigureAwait(false);
                }

                await reader.ReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            return reflectionObject.Creator!(key, value);
        }

    }
}
#endif