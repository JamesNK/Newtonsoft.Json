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
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
    /// </summary>
    public class JsonObjectContract : JsonContainerContract
    {
        /// <summary>
        /// Gets or sets the object member serialization.
        /// </summary>
        /// <value>The member object serialization.</value>
        public MemberSerialization MemberSerialization { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the object's properties are required.
        /// </summary>
        /// <value>
        /// 	A value indicating whether the object's properties are required.
        /// </value>
        public Required? ItemRequired { get; set; }

        /// <summary>
        /// Gets the object's properties.
        /// </summary>
        /// <value>The object's properties.</value>
        public JsonPropertyCollection Properties { get; private set; }

        /// <summary>
        /// Gets the constructor parameters required for any non-default constructor
        /// </summary>
        public JsonPropertyCollection ConstructorParameters { get; private set; }

        /// <summary>
        /// Gets or sets the override constructor used to create the object.
        /// This is set when a constructor is marked up using the
        /// JsonConstructor attribute.
        /// </summary>
        /// <value>The override constructor.</value>
        public ConstructorInfo OverrideConstructor { get; set; }

        /// <summary>
        /// Gets or sets the parametrized constructor used to create the object.
        /// </summary>
        /// <value>The parametrized constructor.</value>
        public ConstructorInfo ParametrizedConstructor { get; set; }

        /// <summary>
        /// Gets or sets the extension data setter.
        /// </summary>
        public ExtensionDataSetter ExtensionDataSetter { get; set; }

        /// <summary>
        /// Gets or sets the extension data getter.
        /// </summary>
        public ExtensionDataGetter ExtensionDataGetter { get; set; }

        private bool? _hasRequiredOrDefaultValueProperties;

        internal bool HasRequiredOrDefaultValueProperties
        {
            get
            {
                if (_hasRequiredOrDefaultValueProperties == null)
                {
                    _hasRequiredOrDefaultValueProperties = false;

                    if (ItemRequired.GetValueOrDefault(Required.Default) != Required.Default)
                    {
                        _hasRequiredOrDefaultValueProperties = true;
                    }
                    else
                    {
                        foreach (JsonProperty property in Properties)
                        {
                            if (property.Required != Required.Default || ((property.DefaultValueHandling & DefaultValueHandling.Populate) == DefaultValueHandling.Populate) && property.Writable)
                            {
                                _hasRequiredOrDefaultValueProperties = true;
                                break;
                            }
                        }
                    }
                }

                return _hasRequiredOrDefaultValueProperties.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonObjectContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public JsonObjectContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Object;

            Properties = new JsonPropertyCollection(UnderlyingType);
            ConstructorParameters = new JsonPropertyCollection(UnderlyingType);
        }

#if !(NETFX_CORE || PORTABLE40 || PORTABLE)
#if !(NET20 || NET35)
        [SecuritySafeCritical]
#endif
        internal object GetUninitializedObject()
        {
            // we should never get here if the environment is not fully trusted, check just in case
            if (!JsonTypeReflector.FullyTrusted)
                throw new JsonException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, NonNullableUnderlyingType));

            return FormatterServices.GetUninitializedObject(NonNullableUnderlyingType);
        }
#endif
    }
}