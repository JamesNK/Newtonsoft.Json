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
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class DiscriminatedUnionConverterImpl
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

    public partial class DiscriminatedUnionConverter
    {
        private bool SafeAsync => GetType() == typeof(DiscriminatedUnionConverter);

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
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            Type unionType = UnionTypeLookupCache.Get(value.GetType());
            Union union = UnionCache.Get(unionType);

            int tag = (int)union.TagReader.Invoke(value);
            UnionCase caseInfo = union.Cases.Single(c => c.Tag == tag);

            await writer.WriteStartObjectAsync(cancellationToken).ConfigureAwait(false);
            await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(CasePropertyName) : CasePropertyName, cancellationToken).ConfigureAwait(false);
            await writer.WriteValueAsync(caseInfo.Name, cancellationToken).ConfigureAwait(false);
            if (caseInfo.Fields != null && caseInfo.Fields.Length > 0)
            {
                object[] fields = (object[])caseInfo.FieldReader.Invoke(value);

                await writer.WritePropertyNameAsync(resolver != null ? resolver.GetResolvedPropertyName(FieldsPropertyName) : FieldsPropertyName, cancellationToken).ConfigureAwait(false);
                await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);
                foreach (object field in fields)
                {
                    await serializer.SerializeAsync(writer, field, cancellationToken).ConfigureAwait(false);
                }
                await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
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
                : ReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        internal Task<object> DoReadJsonAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            return reader.TokenType == JsonToken.Null
                ? cancellationToken.CancelledOrNullAsync()
                : ReadJsonNotNullAsync(reader, objectType, serializer, cancellationToken);
        }

        private async Task<object> ReadJsonNotNullAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            UnionCase caseInfo = null;
            string caseName = null;
            JArray fields = null;

            // start object
            await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();
                if (string.Equals(propertyName, CasePropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);

                    Union union = UnionCache.Get(objectType);

                    caseName = reader.Value.ToString();

                    caseInfo = union.Cases.SingleOrDefault(c => c.Name == caseName);

                    if (caseInfo == null)
                    {
                        throw JsonSerializationException.Create(reader, "No union type found with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
                    }
                }
                else if (string.Equals(propertyName, FieldsPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
                    if (reader.TokenType != JsonToken.StartArray)
                    {
                        throw JsonSerializationException.Create(reader, "Union fields must been an array.");
                    }

                    fields = (JArray)JToken.ReadFrom(reader);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Unexpected property '{0}' found when reading union.".FormatWith(CultureInfo.InvariantCulture, propertyName));
                }

                await reader.ReaderReadAndAssertAsync(cancellationToken).ConfigureAwait(false);
            }

            if (caseInfo == null)
            {
                throw JsonSerializationException.Create(reader, "No '{0}' property with union name found.".FormatWith(CultureInfo.InvariantCulture, CasePropertyName));
            }

            object[] typedFieldValues = new object[caseInfo.Fields.Length];

            if (caseInfo.Fields.Length > 0 && fields == null)
            {
                throw JsonSerializationException.Create(reader, "No '{0}' property with union fields found.".FormatWith(CultureInfo.InvariantCulture, FieldsPropertyName));
            }

            if (fields != null)
            {
                if (caseInfo.Fields.Length != fields.Count)
                {
                    throw JsonSerializationException.Create(reader, "The number of field values does not match the number of properties defined by union '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
                }

                for (int i = 0; i < fields.Count; i++)
                {
                    JToken t = fields[i];
                    PropertyInfo fieldProperty = caseInfo.Fields[i];

                    typedFieldValues[i] = t.ToObject(fieldProperty.PropertyType, serializer);
                }
            }

            object[] args = { typedFieldValues };

            return caseInfo.Constructor.Invoke(args);
        }
    }
}

#endif
