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
using System.Globalization;

namespace Newtonsoft.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> to use the specified <see cref="JsonConverter"/> when serializing the member or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class JsonConverterAttribute : Attribute
    {
        private readonly Type _converterType;

        /// <summary>
        /// Gets the type of the converter.
        /// </summary>
        /// <value>The type of the converter.</value>
        public Type ConverterType
        {
            get { return _converterType; }
        }

        /// <summary>
        /// The parameter list to use when constructing the JsonConverter described by ConverterType.  
        /// If null, the default constructor is used.
        /// </summary>
        public object[] ConverterParameters { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the converter.</param>
        public JsonConverterAttribute(Type converterType)
        {
            if (converterType == null)
                throw new ArgumentNullException("converterType");

            _converterType = converterType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the converter.</param>
        /// <param name="converterParameters">Parameter list to use when constructing the JsonConverter.  Can be null.</param>
        public JsonConverterAttribute(Type converterType, params object[] converterParameters)
        : this(converterType)
        {
            ConverterParameters = converterParameters;
        }
    }
}