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
using System.Collections;
using System.Collections.Generic;
using System.IO;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Text;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace Newtonsoft.Json.Bson
{
    public partial class BsonWriter : JsonWriter
    {
        /// <summary>
        /// Writes a <see cref="Byte"/>[] value that represents a BSON object id.
        /// </summary>
        /// <param name="value">The Object ID value to write.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public async Task WriteObjectIdAsync(byte[] value, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            if (value.Length != 12)
            {
                throw JsonWriterException.Create(this, "An object id must be 12 bytes", null);
            }

            // hack to update the writer state
            await SetWriteStateAsync(JsonToken.Undefined, null, cancellationToken).ConfigureAwait(false);
            AddValue(value, BsonType.Oid);
        }


        /// <summary>
        /// Writes a BSON regex.
        /// </summary>
        /// <param name="pattern">The regex pattern.</param>
        /// <param name="options">The regex options.</param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
        public async Task WriteRegexAsync(string pattern, string options, CancellationToken cancellationToken)
        {
            ValidationUtils.ArgumentNotNull(pattern, nameof(pattern));

            // hack to update the writer state
            await SetWriteStateAsync(JsonToken.Undefined, null, cancellationToken).ConfigureAwait(false);
            AddToken(new BsonRegex(pattern, options));
        }
    }
}

#endif