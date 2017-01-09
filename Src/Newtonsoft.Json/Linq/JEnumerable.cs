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
using System.Collections.Generic;
#if !HAVE_LINQ
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Newtonsoft.Json.Utilities;
using System.Collections;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Represents a collection of <see cref="JToken"/> objects.
    /// </summary>
    /// <typeparam name="T">The type of token.</typeparam>
    public struct JEnumerable<T> : IJEnumerable<T>, IEquatable<JEnumerable<T>> where T : JToken
    {
        /// <summary>
        /// An empty collection of <see cref="JToken"/> objects.
        /// </summary>
        public static readonly JEnumerable<T> Empty = new JEnumerable<T>(Enumerable.Empty<T>());

        private readonly IEnumerable<T> _enumerable;

        /// <summary>
        /// Initializes a new instance of the <see cref="JEnumerable{T}"/> struct.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        public JEnumerable(IEnumerable<T> enumerable)
        {
            ValidationUtils.ArgumentNotNull(enumerable, nameof(enumerable));

            _enumerable = enumerable;
        }

        /// <summary>
        /// Returns an enumerator that can be used to iterate through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="IEnumerator{T}"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            return (_enumerable ?? Empty).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Gets the <see cref="IJEnumerable{T}"/> of <see cref="JToken"/> with the specified key.
        /// </summary>
        /// <value></value>
        public IJEnumerable<JToken> this[object key]
        {
            get
            {
                if (_enumerable == null)
                {
                    return JEnumerable<JToken>.Empty;
                }

                return new JEnumerable<JToken>(_enumerable.Values<T, JToken>(key));
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="JEnumerable{T}"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="JEnumerable{T}"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="JEnumerable{T}"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(JEnumerable<T> other)
        {
            return Equals(_enumerable, other._enumerable);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is JEnumerable<T>)
            {
                return Equals((JEnumerable<T>)obj);
            }

            return false;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            if (_enumerable == null)
            {
                return 0;
            }

            return _enumerable.GetHashCode();
        }
    }
}