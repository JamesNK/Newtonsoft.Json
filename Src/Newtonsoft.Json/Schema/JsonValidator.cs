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
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
using System.Numerics;
#endif
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Utilities;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Newtonsoft.Json
{
    /// <summary>
    /// Represents a reader that provides <see cref="JsonSchema"/> validation.
    /// </summary>
    public class JsonValidator
    {
        /// <summary>
        /// Validates the specified <see cref="JToken"/>.
        /// </summary>
        /// <param name="source">The source <see cref="JToken"/> to test.</param>
        /// <param name="schema">The schema to test with.</param>
        public void Validate(JToken source, JsonSchema schema)
        {
            if (source == null)
                return;
            
            ValidateToken(source, schema);
        }

        private void ValidateToken(JToken source, JsonSchema schema)
        {
            switch (source.Type)
            {
                case JTokenType.Object:
                    ValidateObject(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.Array:
                    ValidateArray(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.Constructor:
                    break;
                case JTokenType.Property:
                    ValidateToken(((JProperty)source).Value, schema);
                    break;
                case JTokenType.Raw:
                    break;
                case JTokenType.Integer:
                    ValidateInteger(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.Float:
                    ValidateFloat(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.String:
                    ValidateString(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.Boolean:
                    ValidateBoolean(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.Null:
                    ValidateNull(source, schema);
                    ValidateEnum(source, schema);
                    ValidateSets(source, schema);
                    break;
                case JTokenType.Undefined:
                case JTokenType.Date:
                case JTokenType.Bytes:
                case JTokenType.Comment:
                    // these have no equivalent in JSON schema
                    break;
                case JTokenType.None:
                    // no content, do nothing
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool TestType(JToken token, JsonSchema currentSchema, JsonSchemaType currentType)
        {
            if (!JsonSchemaGenerator.HasFlag(currentSchema.Type, currentType))
            {
                RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, currentSchema.Type, currentType), token, currentSchema);
                return false;
            }

            return true;
        }

        private void ValidateObject(JToken source, JsonSchema schema)
        {
            if (!TestType(source, schema, JsonSchemaType.Object))
                return;

            IList<string> childProperties = new List<string>();

            foreach (JToken child in source)
            {
                if (child.Type == JTokenType.Comment)
                    continue;

                if (child.Type != JTokenType.Property)
                {
                    RaiseError("Invalid type. Expected {0} but got {1}.".FormatWith(CultureInfo.InvariantCulture, JTokenType.Property, child.Type), child, schema);
                    continue;
                }

                JProperty sourceProperty = (JProperty)child;
                JsonSchema propertySchema;
                bool foundSchemaMatch = false;

                childProperties.Add(sourceProperty.Name);

                if (schema.Properties != null && schema.Properties.TryGetValue(sourceProperty.Name, out propertySchema))
                {
                    ValidateToken(child, propertySchema);
                    foundSchemaMatch = true;
                }

                if (schema.PatternProperties != null)
                {
                    foreach (KeyValuePair<string, JsonSchema> patternProperty in schema.PatternProperties)
                    {
                        if (Regex.IsMatch(sourceProperty.Name, patternProperty.Key))
                        {
                            ValidateToken(child, patternProperty.Value);
                            foundSchemaMatch = true;
                        }
                    }
                }

                if (!foundSchemaMatch)
                {
                    if (schema.AllowAdditionalProperties)
                    {
                        if (schema.AdditionalProperties != null)
                        {
                            ValidateToken(child, schema.AdditionalProperties);
                        }
                    }
                    else
                    {
                        RaiseError("Property '{0}' has not been defined and the schema does not allow additional properties.".FormatWith(CultureInfo.InvariantCulture, sourceProperty.Name), child, schema);
                    }
                }
            }

            if (schema.Required != null)
            {
                List<string> unmatchedRequiredProperties = schema.Required.Where(v => !childProperties.Contains(v)).ToList();

                if (unmatchedRequiredProperties.Count > 0)
                    RaiseError("Required properties are missing from object: {0}.".FormatWith(CultureInfo.InvariantCulture, string.Join(", ", unmatchedRequiredProperties.ToArray())), source, schema);
            }

            if (schema.MaximumProperties != null && childProperties.Count > schema.MaximumProperties)
                RaiseError("Property count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, childProperties.Count, schema.MaximumProperties), source, schema);

            if (schema.MinimumProperties != null && childProperties.Count < schema.MinimumProperties)
                RaiseError("Property count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, childProperties.Count, schema.MinimumProperties), source, schema);

            if (schema.Dependencies != null)
            {
                foreach (KeyValuePair<string, JsonSchema> dep in schema.Dependencies)
                {
                    if (childProperties.Contains(dep.Key))
                        ValidateObject(source, dep.Value);
                }
            }
        }
        
        private void ValidateArray(JToken source, JsonSchema schema)
        {
            if (!TestType(source, schema, JsonSchemaType.Array))
                return;
                
            int arrayItemCount = 0;
            IList<JToken> uniqueItems = null;

            if (schema.UniqueItems)
                uniqueItems = new List<JToken>();

            foreach (JToken child in source)
            {
                if (child.Type == JTokenType.Comment)
                    continue;

                JsonSchema arrayItemSchema = null;
                arrayItemCount++;

                if (uniqueItems != null)
                {
                    if (uniqueItems.Contains(child, JToken.EqualityComparer))
                        RaiseError("Non-unique array item at index {0}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount - 1), child, schema);

                    uniqueItems.Add(child);
                }

                if (!schema.PositionalItemsValidation)
                {
                    if (schema.Items != null && schema.Items.Count > 0)
                    {
                        arrayItemSchema = schema.Items[0];
                    }
                }
                else
                {
                    if (schema.Items != null &&
                         schema.Items.Count > 0 &&
                         schema.Items.Count > (arrayItemCount - 1))
                    {
                        arrayItemSchema = schema.Items[arrayItemCount - 1];
                    }
                    else
                    {
                        if (schema.AllowAdditionalItems)
                        {
                            if (schema.AdditionalItems != null)
                            {
                                arrayItemSchema = schema.AdditionalItems;
                            }
                        }
                        else
                        {
                            RaiseError("Index {0} has not been defined and the schema does not allow additional items.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount), child, schema);
                        }
                    }
                }

                if (arrayItemSchema != null)
                    ValidateToken(child, arrayItemSchema);
            }

            if (schema.MaximumItems != null && arrayItemCount > schema.MaximumItems)
                RaiseError("Array item count {0} exceeds maximum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MaximumItems), source, schema);

            if (schema.MinimumItems != null && arrayItemCount < schema.MinimumItems)
                RaiseError("Array item count {0} is less than minimum count of {1}.".FormatWith(CultureInfo.InvariantCulture, arrayItemCount, schema.MinimumItems), source, schema);
        }

        private void ValidateNull(JToken source, JsonSchema schema)
        {
            if (schema == null)
                return;

            if (!TestType(source, schema, JsonSchemaType.Null))
                return;
        }

        private void ValidateBoolean(JToken source, JsonSchema schema)
        {
            if (schema == null)
                return;

            if (!TestType(source, schema, JsonSchemaType.Boolean))
                return;
        }

        private void ValidateString(JToken source, JsonSchema schema)
        {
            if (schema == null)
                return;

            if (!TestType(source, schema, JsonSchemaType.String))
                return;

            string value = (string)source;

            if (schema.MaximumLength != null || schema.MinimumLength != null)
            {
                int[] unicodeString = StringInfo.ParseCombiningCharacters(value);

                if (schema.MaximumLength != null && unicodeString.Length > schema.MaximumLength)
                    RaiseError("String '{0}' exceeds maximum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MaximumLength), source, schema);

                if (schema.MinimumLength != null && unicodeString.Length < schema.MinimumLength)
                    RaiseError("String '{0}' is less than minimum length of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MinimumLength), source, schema);
            }

            if (schema.Pattern != null)
            {
                if (!Regex.IsMatch(value, schema.Pattern))
                    RaiseError("String '{0}' does not match regex pattern '{1}'.".FormatWith(CultureInfo.InvariantCulture, value, schema.Pattern), source, schema);
            }
        }

        private void ValidateInteger(JToken source, JsonSchema schema)
        {
            if (schema == null)
                return;

            if (!TestType(source, schema, JsonSchemaType.Integer))
                return;

            object value = ((JValue)source).Value;

            if (schema.Maximum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Maximum) > 0)
                    RaiseError("Integer {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), source, schema);
                if (schema.ExclusiveMaximum != null && (bool)schema.ExclusiveMaximum && JValue.Compare(JTokenType.Integer, value, schema.Maximum) == 0)
                    RaiseError("Integer {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Maximum), source, schema);
            }

            if (schema.Minimum != null)
            {
                if (JValue.Compare(JTokenType.Integer, value, schema.Minimum) < 0)
                    RaiseError("Integer {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), source, schema);
                if (schema.ExclusiveMinimum != null && (bool)schema.ExclusiveMinimum && JValue.Compare(JTokenType.Integer, value, schema.Minimum) == 0)
                    RaiseError("Integer {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, value, schema.Minimum), source, schema);
            }

            if (schema.MultipleOf != null)
            {
                bool notMultiple = false;
#if !(NET20 || NET35 || PORTABLE40 || PORTABLE)
                if (value is BigInteger)
                {
                    // not that this will lose any decimal point on MultipleOf
                    // so manually raise an error if MultipleOf is not an integer and value is not zero
                    BigInteger i = (BigInteger)value;
                    bool multipleNonInteger = !Math.Abs(schema.MultipleOf.Value - Math.Truncate(schema.MultipleOf.Value)).Equals(0);
                    if (multipleNonInteger)
                        notMultiple = i != 0;
                    else
                        notMultiple = i % new BigInteger(schema.MultipleOf.Value) != 0;
                }
                else
#endif
                    notMultiple = !IsZero(Convert.ToInt64(value, CultureInfo.InvariantCulture) % schema.MultipleOf.Value);

                if (notMultiple)
                    RaiseError("Integer {0} is not a multiple of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.MultipleOf), source, schema);
            }
        }

        private void ValidateFloat(JToken source, JsonSchema schema)
        {
            if (schema == null)
                return;

            if (!TestType(source, schema, JsonSchemaType.Float))
                return;

            double value = Convert.ToDouble(source, CultureInfo.InvariantCulture);

            if (schema.Maximum != null)
            {
                if (value > schema.Maximum)
                    RaiseError("Float {0} exceeds maximum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), source, schema);
                if (schema.ExclusiveMaximum != null && (bool)schema.ExclusiveMaximum && value == schema.Maximum)
                    RaiseError("Float {0} equals maximum value of {1} and exclusive maximum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Maximum), source, schema);
            }

            if (schema.Minimum != null)
            {
                if (value < schema.Minimum)
                    RaiseError("Float {0} is less than minimum value of {1}.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), source, schema);
                if (schema.ExclusiveMinimum != null && (bool)schema.ExclusiveMinimum && value == schema.Minimum)
                    RaiseError("Float {0} equals minimum value of {1} and exclusive minimum is true.".FormatWith(CultureInfo.InvariantCulture, JsonConvert.ToString(value), schema.Minimum), source, schema);
            }

            if (schema.MultipleOf != null)
            {
                double remainder = FloatingPointRemainder(value, schema.MultipleOf.Value);

                if (!IsZero(remainder))
                    RaiseError("Float {0} is not a multiple of {1}.".FormatWith(CultureInfo.InvariantCulture, value, schema.MultipleOf), source, schema);
            }
        }

        private void ValidateEnum(JToken source, JsonSchema schema)
        {
            if (schema.Enum != null)
            {
                if (!schema.Enum.ContainsValue(source, JToken.EqualityComparer))
                {
                    StringWriter sw = new StringWriter(CultureInfo.InvariantCulture);
                    source.WriteTo(new JsonTextWriter(sw));

                    RaiseError("Value {0} is not defined in enum.".FormatWith(CultureInfo.InvariantCulture, sw.ToString()), source, schema);
                }
            }
        }

        private void ValidateSets(JToken source, JsonSchema schema)
        {
            if (schema.AllOf != null)
            {
                ValidateAllOf(source, schema);
            }
            if (schema.OneOf != null)
            {
                ValidateOneOf(source, schema);
            }
            if (schema.AnyOf != null)
            {
                ValidateAnyOf(source, schema);
            }
            if (schema.NotOf != null)
            {
                ValidateNot(source, schema);
            }
        }

        private void ValidateAllOf(JToken source, JsonSchema schema)
        {
            IList<JsonSchemaException> previousValidationResult = _storedValidationResult;
            IList<JsonSchemaException> setValidationResult = new List<JsonSchemaException>();
            _storedValidationResult = setValidationResult;

            foreach (JsonSchema childSet in schema.AllOf)
            {
                ValidateToken(source, childSet);
            }

            _storedValidationResult = previousValidationResult;

            if (setValidationResult.Count > 0)
                RaiseError("allOf condition failed {0}.".FormatWith(CultureInfo.InvariantCulture, schema.AllOf), source, schema);
        }

        private void ValidateOneOf(JToken source, JsonSchema schema)
        {
            IList<JsonSchemaException> previousValidationResult = _storedValidationResult;
            IList<JsonSchemaException> setValidationResult = new List<JsonSchemaException>();
            bool foundOne = false;
            bool foundMultiple = false;

            _storedValidationResult = setValidationResult;

            foreach (JsonSchema childSet in schema.OneOf)
            {
                ValidateToken(source, childSet);

                if (foundOne && setValidationResult.Count == 0)
                    foundMultiple = true;
                else if (setValidationResult.Count == 0)
                    foundOne = true;

                setValidationResult.Clear();
            }

            _storedValidationResult = previousValidationResult;

            if (!foundOne || foundMultiple)
                RaiseError("oneOf condition failed {0}.".FormatWith(CultureInfo.InvariantCulture, schema.AllOf), source, schema);
        }

        private void ValidateAnyOf(JToken source, JsonSchema schema)
        {
            IList<JsonSchemaException> previousValidationResult = _storedValidationResult;
            IList<JsonSchemaException> setValidationResult = new List<JsonSchemaException>();
            bool foundOne = false;

            _storedValidationResult = setValidationResult;

            foreach (JsonSchema childSet in schema.AnyOf)
            {
                ValidateToken(source, childSet);

                if (setValidationResult.Count == 0)
                {
                    foundOne = true;
                    break;
                }

                setValidationResult.Clear();
            }

            _storedValidationResult = previousValidationResult;

            if (!foundOne)
                RaiseError("anyOf condition failed {0}.".FormatWith(CultureInfo.InvariantCulture, schema.AllOf), source, schema);
        }

        private void ValidateNot(JToken source, JsonSchema schema)
        {
            IList<JsonSchemaException> previousValidationResult = _storedValidationResult;
            IList<JsonSchemaException> setValidationResult = new List<JsonSchemaException>();

            _storedValidationResult = setValidationResult;

            ValidateToken(source, schema.NotOf);

            _storedValidationResult = previousValidationResult;

            if (setValidationResult.Count == 0)
                RaiseError("not condition failed {0}.".FormatWith(CultureInfo.InvariantCulture, schema.AllOf), source, schema);
        }

        private static double FloatingPointRemainder(double dividend, double divisor)
        {
            return dividend - Math.Floor(dividend / divisor) * divisor;
        }

        private static bool IsZero(double value)
        {
            const double epsilon = 2.2204460492503131e-016;

            return Math.Abs(value) < 20.0 * epsilon;
        }

        private IList<JsonSchemaException> _storedValidationResult;

        /// <summary>
        /// Sets an event handler for receiving schema validation errors.
        /// </summary>
        public event ValidationEventHandler ValidationEventHandler;

        private void RaiseError(string message, JToken token, JsonSchema schema)
        {
            IJsonLineInfo lineInfo = token;

            string exceptionMessage = (lineInfo.HasLineInfo())
                ? message + " Line {0}, position {1}.".FormatWith(CultureInfo.InvariantCulture, lineInfo.LineNumber, lineInfo.LinePosition)
                : message;

            OnValidationEvent(new JsonSchemaException(exceptionMessage, null, token.Path, lineInfo.LineNumber, lineInfo.LinePosition));
        }

        private void OnValidationEvent(JsonSchemaException exception)
        {
            if (_storedValidationResult != null)
            {
                _storedValidationResult.Add(exception);
            }
            else
            {
            ValidationEventHandler handler = ValidationEventHandler;
            if (handler != null)
                handler(this, new ValidationEventArgs(exception));
            else
                throw exception;
            }
        }
    }
}
