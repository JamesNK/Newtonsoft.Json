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

#if !(NET35 || NET20 || NETFX_CORE)
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using System.Reflection;
using Newtonsoft.Json.Serialization;
using System.Globalization;
using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Converters
{
    /// <summary>
    /// Converts a F# discriminated union type to and from JSON.
    /// </summary>
    public class DiscriminatedUnionConverter : JsonConverter
    {
        private const string CasePropertyName = "Case";
        private const string FieldsPropertyName = "Fields";

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            Type t = value.GetType();

            object result = FSharpUtils.GetUnionFields(null, value, t, null);
            object info = FSharpUtils.GetUnionCaseInfo(result);
            object fields = FSharpUtils.GetUnionCaseFields(result);
            object caseName = FSharpUtils.GetUnionCaseInfoName(info);

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(CasePropertyName) : CasePropertyName);
            writer.WriteValue((string)caseName);
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(FieldsPropertyName) : FieldsPropertyName);
            serializer.Serialize(writer, fields);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
                return null;

            IEnumerable cases = (IEnumerable)FSharpUtils.GetUnionCases(null, objectType, null);

            ReadAndAssertProperty(reader, CasePropertyName);
            ReadAndAssert(reader);

            string caseName = reader.Value.ToString();

            object matchingCaseInfo = null;
            foreach (object c in cases)
            {
                if ((string)FSharpUtils.GetUnionCaseInfoName(c) == caseName)
                {
                    matchingCaseInfo = c;
                    break;
                }
            }

            if (matchingCaseInfo == null)
                throw new JsonSerializationException("No union type found with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));

            ReadAndAssertProperty(reader, FieldsPropertyName);
            // start array
            ReadAndAssert(reader);
            // first value
            ReadAndAssert(reader);

            PropertyInfo[] fieldProperties = (PropertyInfo[])FSharpUtils.GetUnionCaseInfoFields(matchingCaseInfo);
            List<object> fieldValues = new List<object>();
            foreach (PropertyInfo field in fieldProperties)
            {
                fieldValues.Add(serializer.Deserialize(reader, field.PropertyType));
                ReadAndAssert(reader);
            }

            // end object
            ReadAndAssert(reader);

            return FSharpUtils.MakeUnion(null, matchingCaseInfo, fieldValues.ToArray(), null);
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            if (typeof(IEnumerable).IsAssignableFrom(objectType))
                return false;

            // all fsharp objects have CompilationMappingAttribute
            // get the fsharp assembly from the attribute and initialize latebound methods
            object[] attributes;
#if !(NETFX_CORE || PORTABLE)
            attributes = objectType.GetCustomAttributes(true);
#else
            attributes = objectType.GetTypeInfo().GetCustomAttributes(true).ToArray();
#endif

            bool isFSharpType = false;
            foreach (object attribute in attributes)
            {
                Type attributeType = attribute.GetType();
                if (attributeType.Name == "CompilationMappingAttribute")
                {
                    FSharpUtils.EnsureInitialized(attributeType.Assembly());

                    isFSharpType = true;
                    break;
                }
            }

            if (!isFSharpType)
                return false;

            return (bool)FSharpUtils.IsUnion(null, objectType, null);
        }

        private static void ReadAndAssertProperty(JsonReader reader, string propertyName)
        {
            ReadAndAssert(reader);

            if (reader.TokenType != JsonToken.PropertyName || !string.Equals(reader.Value.ToString(), propertyName, StringComparison.OrdinalIgnoreCase))
                throw new JsonSerializationException("Expected JSON property '{0}'.".FormatWith(CultureInfo.InvariantCulture, propertyName));
        }

        private static void ReadAndAssert(JsonReader reader)
        {
            if (!reader.Read())
                throw new JsonSerializationException("Unexpected end.");
        }
    }
}
#endif