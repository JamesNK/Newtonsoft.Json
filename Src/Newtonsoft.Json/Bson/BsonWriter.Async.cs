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
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
    public partial class BsonWriter
    {
        private readonly bool _safeAsync;

        /// <summary>
        /// Asynchronously flushes whatever is in the buffer to the destination and also flushes the destination.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Because BSON documents are written as a single unit, only <see cref="FlushAsync"/>,
        /// <see cref="CloseAsync"/> and the final <see cref="WriteEndAsync(JsonToken,CancellationToken)"/>,
        /// <see cref="WriteEndArrayAsync"/> or <see cref="WriteEndObjectAsync"/>
        /// that finishes writing the document will write asynchronously. Derived classes will not write asynchronously.</remarks>
        public override Task FlushAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _safeAsync ? _writer.FlushAsync(cancellationToken) : base.FlushAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the specified end token.
        /// </summary>
        /// <param name="token">The end token to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Because BSON documents are written as a single unit, only <see cref="FlushAsync"/>,
        /// <see cref="CloseAsync"/> and the final <see cref="WriteEndAsync(JsonToken,CancellationToken)"/>,
        /// <see cref="WriteEndArrayAsync"/> or <see cref="WriteEndObjectAsync"/>
        /// that finishes writing the document will write asynchronously. Derived classes will not write asynchronously.</remarks>
        protected override Task WriteEndAsync(JsonToken token, CancellationToken cancellationToken)
        {
            if (!_safeAsync)
            {
                return base.WriteEndAsync(token, cancellationToken);
            }

            RemoveParent();
            if (Top == 0)
            {
                return _writer.WriteTokenAsync(_root, cancellationToken);
            }

            return AsyncUtils.CompletedTask;
        }

        /// <summary>
        /// Asynchronously writes the end of the current JSON object or array.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Because BSON documents are written as a single unit, only <see cref="FlushAsync"/>,
        /// <see cref="CloseAsync"/> and the final <see cref="WriteEndAsync(JsonToken,CancellationToken)"/>,
        /// <see cref="WriteEndArrayAsync"/> or <see cref="WriteEndObjectAsync"/>
        /// that finishes writing the document will write asynchronously. Derived classes will not write asynchronously.</remarks>
        public override Task WriteEndAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _safeAsync ? WriteEndInternalAsync(cancellationToken) : base.WriteEndAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the end of an array.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Because BSON documents are written as a single unit, only <see cref="FlushAsync"/>,
        /// <see cref="CloseAsync"/> and the final <see cref="WriteEndAsync(JsonToken,CancellationToken)"/>,
        /// <see cref="WriteEndArrayAsync"/> or <see cref="WriteEndObjectAsync"/>
        /// that finishes writing the document will write asynchronously. Derived classes will not write asynchronously.</remarks>
        public override Task WriteEndArrayAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _safeAsync ? InternalWriteEndAsync(JsonContainerType.Array, cancellationToken) : base.WriteEndArrayAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously writes the end of a JSON object.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Because BSON documents are written as a single unit, only <see cref="FlushAsync"/>,
        /// <see cref="CloseAsync"/> and the final <see cref="WriteEndAsync(JsonToken,CancellationToken)"/>,
        /// <see cref="WriteEndArrayAsync"/> or <see cref="WriteEndObjectAsync"/>
        /// that finishes writing the document will write asynchronously. Derived classes will not write asynchronously.</remarks>
        public override Task WriteEndObjectAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return _safeAsync ? InternalWriteEndAsync(JsonContainerType.Object, cancellationToken) : base.WriteEndObjectAsync(cancellationToken);
        }

        /// <summary>
        /// Asynchronously closes this writer.
        /// If <see cref="JsonWriter.CloseOutput"/> is set to <c>true</c>, the destination is also closed.
        /// </summary>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>Because BSON documents are written as a single unit, only <see cref="FlushAsync"/>,
        /// <see cref="CloseAsync"/> and the final <see cref="WriteEndAsync(JsonToken,CancellationToken)"/>,
        /// <see cref="WriteEndArrayAsync"/> or <see cref="WriteEndObjectAsync"/>
        /// that finishes writing the document will write asynchronously. Derived classes will not write asynchronously.</remarks>
        public override async Task CloseAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            while (Top > 0)
            {
                await WriteEndAsync(cancellationToken).ConfigureAwait(false);
            }

            if (CloseOutput)
            {
                _writer?.Close();
            }
        }
    }
}

#endif
