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

using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Bson;

namespace Newtonsoft.Json.Converters
{
    internal sealed partial class BsonObjectIdConverterImpl
    {
        public override Task WriteJsonAsync(JsonWriter writer, object value, JsonSerializer serializer, CancellationToken cancellationToken = default(CancellationToken))
        {
            return DoWriteJsonAsync(writer, value, cancellationToken);
        }
    }

    public partial class BsonObjectIdConverter
    {
        private bool SafeAsync => GetType() == typeof(BsonObjectIdConverter);

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
                ? DoWriteJsonAsync(writer, value, cancellationToken)
                : base.WriteJsonAsync(writer, value, serializer, cancellationToken);
        }

        internal async Task DoWriteJsonAsync(JsonWriter writer, object value, CancellationToken cancellationToken)
        {
            BsonObjectId objectId = (BsonObjectId)value;

            BsonWriter bsonWriter = writer as BsonWriter;
            if (bsonWriter != null)
            {
                bsonWriter.WriteObjectId(objectId.Value);
            }
            else
            {
                await writer.WriteValueAsync(objectId.Value, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
#endif
