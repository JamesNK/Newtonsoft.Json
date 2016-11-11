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

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Resolves member mappings for a type, camel casing property names.
    /// </summary>
    public class CamelCasePropertyNamesContractResolver : DefaultContractResolver
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCasePropertyNamesContractResolver"/> class.
        /// </summary>
        public CamelCasePropertyNamesContractResolver()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCasePropertyNamesContractResolver"/> class.
        /// </summary>
        /// <param name="shareCache">
        /// If set to <c>true</c> the <see cref="CamelCasePropertyNamesContractResolver"/> will use a cached shared with other resolvers of the same type.
        /// Sharing the cache will significantly improve performance with multiple resolver instances because expensive reflection will only
        /// happen once. This setting can cause unexpected behavior if different instances of the resolver are suppose to produce different
        /// results. When set to false it is highly recommended to reuse <see cref="CamelCasePropertyNamesContractResolver"/> instances with the <see cref="JsonSerializer"/>.
        /// </param>
        public CamelCasePropertyNamesContractResolver(bool shareCache)
#pragma warning disable 612,618
            : base(shareCache)
#pragma warning restore 612,618
        {
            NamingStrategy = new CamelCaseNamingStrategy
            {
                ProcessDictionaryKeys = true,
                OverrideSpecifiedNames = true
            };
        }

        /// <summary>
        /// Resolves the name of the property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The property name camel cased.</returns>
        protected override string ResolvePropertyName(string propertyName)
        {
            // lower case the first letter (or more) of the passed in name
            return StringUtils.ToCamelCase(propertyName);
        }
    }
}
