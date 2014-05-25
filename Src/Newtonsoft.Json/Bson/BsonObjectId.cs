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

using System;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Bson
{
    /// <summary>
    /// Represents a BSON Oid (object id).
    /// </summary>
    public class BsonObjectId
    {
        /// <summary>
        /// Gets or sets the value of the Oid.
        /// </summary>
        /// <value>The value of the Oid.</value>
        public byte[] Value { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonObjectId"/> class.
        /// </summary>
        /// <param name="value">The Oid value.</param>
        public BsonObjectId(byte[] value)
        {
            ValidationUtils.ArgumentNotNull(value, "value");
            if (value.Length != 12)
                throw new ArgumentException("An ObjectId must be 12 bytes", "value");

            Value = value;
        }
    }
}