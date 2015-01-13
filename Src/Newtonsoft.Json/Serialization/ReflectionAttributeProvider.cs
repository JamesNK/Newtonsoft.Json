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
using System.Reflection;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Provides methods to get attributes from a <see cref="Type"/>, <see cref="MemberInfo"/>, <see cref="ParameterInfo"/> or <see cref="Assembly"/>.
    /// </summary>
    public class ReflectionAttributeProvider : IAttributeProvider
    {
        private readonly object _attributeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionAttributeProvider"/> class.
        /// </summary>
        /// <param name="attributeProvider">The instance to get attributes for. This parameter should be a <see cref="Type"/>, <see cref="MemberInfo"/>, <see cref="ParameterInfo"/> or <see cref="Assembly"/>.</param>
        public ReflectionAttributeProvider(object attributeProvider)
        {
            ValidationUtils.ArgumentNotNull(attributeProvider, "attributeProvider");
            _attributeProvider = attributeProvider;
        }

        /// <summary>
        /// Returns a collection of all of the attributes, or an empty collection if there are no attributes.
        /// </summary>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>A collection of <see cref="Attribute"/>s, or an empty collection.</returns>
        public IList<Attribute> GetAttributes(bool inherit)
        {
            return ReflectionUtils.GetAttributes(_attributeProvider, null, inherit);
        }

        /// <summary>
        /// Returns a collection of attributes, identified by type, or an empty collection if there are no attributes.
        /// </summary>
        /// <param name="attributeType">The type of the attributes.</param>
        /// <param name="inherit">When true, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>A collection of <see cref="Attribute"/>s, or an empty collection.</returns>
        public IList<Attribute> GetAttributes(Type attributeType, bool inherit)
        {
            return ReflectionUtils.GetAttributes(_attributeProvider, attributeType, inherit);
        }
    }
}