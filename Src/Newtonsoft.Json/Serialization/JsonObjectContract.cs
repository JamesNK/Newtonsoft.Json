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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="System.Type"/> used by the <see cref="JsonSerializer"/>.
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
        /// Gets a collection of <see cref="JsonProperty"/> instances that define the parameters used with <see cref="JsonObjectContract.OverrideCreator"/>.
        /// </summary>
        public JsonPropertyCollection CreatorParameters
        {
            get
            {
                if (_creatorParameters == null)
                {
                    _creatorParameters = new JsonPropertyCollection(UnderlyingType);
                }

                return _creatorParameters;
            }
        }

        /// <summary>
        /// Gets or sets the function used to create the object. When set this function will override <see cref="JsonContract.DefaultCreator"/>.
        /// This function is called with a collection of arguments which are defined by the <see cref="JsonObjectContract.CreatorParameters"/> collection.
        /// </summary>
        /// <value>The function used to create the object.</value>
        public ObjectConstructor<object> OverrideCreator
        {
            get { return _overrideCreator; }
            set { _overrideCreator = value; }
        }

        internal ObjectConstructor<object> ParameterizedCreator
        {
            get { return _parameterizedCreator; }
            set { _parameterizedCreator = value; }
        }

        /// <summary>
        /// Gets or sets the extension data setter.
        /// </summary>
        public ExtensionDataSetter ExtensionDataSetter { get; set; }

        /// <summary>
        /// Gets or sets the extension data getter.
        /// </summary>
        public ExtensionDataGetter ExtensionDataGetter { get; set; }

        /// <summary>
        /// Gets or sets the extension data value type.
        /// </summary>
        public Type ExtensionDataValueType
        {
            get { return _extensionDataValueType; }
            set
            {
                _extensionDataValueType = value;
                ExtensionDataIsJToken = (value != null && typeof(JToken).IsAssignableFrom(value));
            }
        }

        /// <summary>
        /// Gets or sets the extension data name resolver.
        /// </summary>
        /// <value>The extension data name resolver.</value>
        public Func<string, string> ExtensionDataNameResolver { get; set; }

        internal bool ExtensionDataIsJToken;
        private bool? _hasRequiredOrDefaultValueProperties;
        private ObjectConstructor<object> _overrideCreator;
        private ObjectConstructor<object> _parameterizedCreator;
        private JsonPropertyCollection _creatorParameters;
        private Type _extensionDataValueType;

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
                            if (property.Required != Required.Default || (property.DefaultValueHandling & DefaultValueHandling.Populate) == DefaultValueHandling.Populate)
                            {
                                _hasRequiredOrDefaultValueProperties = true;
                                break;
                            }
                        }
                    }
                }

                return _hasRequiredOrDefaultValueProperties.GetValueOrDefault();
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
        }

#if HAVE_BINARY_SERIALIZATION
#if HAVE_SECURITY_SAFE_CRITICAL_ATTRIBUTE
        [SecuritySafeCritical]
#endif
        internal object GetUninitializedObject()
        {
            // we should never get here if the environment is not fully trusted, check just in case
            if (!JsonTypeReflector.FullyTrusted)
            {
                throw new JsonException("Insufficient permissions. Creating an uninitialized '{0}' type requires full trust.".FormatWith(CultureInfo.InvariantCulture, NonNullableUnderlyingType));
            }

            return FormatterServices.GetUninitializedObject(NonNullableUnderlyingType);
        }
#endif
    }
}