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

using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Used to resolve references when serializing and deserializing JSON by the <see cref="JsonSerializer"/>.
    /// </summary>
    public class DefaultReferenceResolver : IReferenceResolver
    {
        private sealed class ReferenceEqualsEqualityComparer : IEqualityComparer<object>
        {
            bool IEqualityComparer<object>.Equals(object x, object y)
            {
                return ReferenceEquals(x, y);
            }

            int IEqualityComparer<object>.GetHashCode(object obj)
            {
                // put objects in a bucket based on their reference
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        private BidirectionalDictionary<string, object> _mappings = new BidirectionalDictionary<string, object>(
            EqualityComparer<string>.Default,
            new ReferenceEqualsEqualityComparer(),
            "A different value already has the Id '{0}'.",
            "A different Id has already been assigned for value '{0}'. This error may be caused by an object being reused multiple times during deserialization and can be fixed with the setting ObjectCreationHandling.Replace.");
        private int _referenceCount;

        /// <summary>
        /// Resolves a reference to its object.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="reference">The reference to resolve.</param>
        /// <returns>The object that was resolved from the reference.</returns>
        public virtual object ResolveReference(object context, string reference)
        {
            _mappings.TryGetByFirst(reference, out object value);
            return value;
        }

        /// <summary>
        /// Gets the reference for the specified object.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The object to get a reference for.</param>
        /// <returns>The reference to the object.</returns>
        public virtual string GetReference(object context, object value)
        {
            return _mappings.TryGetBySecond(value, out string reference) ? reference : AddReference(context, value);
        }

        /// <summary>
        /// Adds a reference to the specified object.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The object to reference.</param>
        /// <returns>The reference to the object.</returns>
        public virtual string AddReference(object context, object value)
        {
            var reference = (++_referenceCount).ToString(CultureInfo.InvariantCulture);
            AddReference(context, reference, value);
            return reference;
        }

        /// <summary>
        /// Adds a reference to the specified object.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="reference">The reference.</param>
        /// <param name="value">The object to reference.</param>
        public virtual void AddReference(object context, string reference, object value)
        {
            _mappings.Set(reference, value);
        }

        /// <summary>
        /// Determines whether the specified object is referenced.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="value">The object to test for a reference.</param>
        /// <returns>
        /// 	<c>true</c> if the specified object is referenced; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool IsReferenced(object context, object value)
        {
            return _mappings.TryGetBySecond(value, out _);
        }
    }
}