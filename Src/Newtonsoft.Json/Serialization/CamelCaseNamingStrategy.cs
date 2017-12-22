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
    /// A camel case naming strategy.
    /// </summary>
    public class CamelCaseNamingStrategy : NamingStrategy
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseNamingStrategy"/> class.
        /// </summary>
        /// <param name="processDictionaryKeys">
        /// A flag indicating whether dictionary keys should be processed.
        /// </param>
        /// <param name="overrideSpecifiedNames">
        /// A flag indicating whether explicitly specified property names should be processed,
        /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>.
        /// </param>
        public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames)
        {
            ProcessDictionaryKeys = processDictionaryKeys;
            OverrideSpecifiedNames = overrideSpecifiedNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseNamingStrategy"/> class.
        /// </summary>
        /// <param name="processDictionaryKeys">
        /// A flag indicating whether dictionary keys should be processed.
        /// </param>
        /// <param name="overrideSpecifiedNames">
        /// A flag indicating whether explicitly specified property names should be processed,
        /// e.g. a property name customized with a <see cref="JsonPropertyAttribute"/>.
        /// </param>
        /// <param name="processExtensionDataNames">
        /// A flag indicating whether extension data names should be processed.
        /// </param>
        public CamelCaseNamingStrategy(bool processDictionaryKeys, bool overrideSpecifiedNames, bool processExtensionDataNames)
            : this(processDictionaryKeys, overrideSpecifiedNames)
        {
            ProcessExtensionDataNames = processExtensionDataNames;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CamelCaseNamingStrategy"/> class.
        /// </summary>
        public CamelCaseNamingStrategy()
        {
        }

        /// <summary>
        /// Resolves the specified property name.
        /// </summary>
        /// <param name="name">The property name to resolve.</param>
        /// <returns>The resolved property name.</returns>
        protected override string ResolvePropertyName(string name)
        {
            return StringUtils.ToCamelCase(name);
        }
    }
}