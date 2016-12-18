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
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    public abstract partial class CustomCreationConverter<T>
    {
        private bool? _safeCreate;

        private bool SafeAsync
        {
            get
            {
                if (_safeCreate.HasValue)
                {
                    // In most cases we assume that we can't do any asynchronous operations safely in any
                    // derived class. In this case though the whole point is for users to override Create,
                    // and it's safe as long as they haven't also overridden Read, which would be relatively
                    // rare. Therefore check that Read hasn't been overridden.
                    MethodInfo baseMethod = typeof(CustomCreationConverter<T>).GetMethod("ReadJsonAsync");
                    _safeCreate = GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .All(m => m.GetBaseDefinition() != baseMethod);
                }

                return _safeCreate.GetValueOrDefault();
            }
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
        /// <remarks>If a derived class overrides <see cref="ReadJson"/> this will execute synchronously, returning an
        /// already-completed task, unless that class also overrides this method.</remarks>
        public override Task<object> ReadJsonAsync(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return SafeAsync
                ? DoReadJsonAsync(reader, objectType, serializer, cancellationToken)
                : base.ReadJsonAsync(reader, objectType, existingValue, serializer, cancellationToken);
        }

        private Task<object> DoReadJsonAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        {
            return reader.TokenType == JsonToken.Null
                ? cancellationToken.CancelledOrNullAsync()
                : ReadJsonNotNullAsync(reader, objectType, serializer, cancellationToken);
        }

        private async Task<object> ReadJsonNotNullAsync(JsonReader reader, Type objectType, JsonSerializer serializer, CancellationToken cancellationToken)
        { 
            T value = Create(objectType);
            if (value == null)
            {
                throw new JsonSerializationException("No object created.");
            }

            await serializer.PopulateAsync(reader, value, cancellationToken).ConfigureAwait(false);
            return value;
        }
    }
}

#endif