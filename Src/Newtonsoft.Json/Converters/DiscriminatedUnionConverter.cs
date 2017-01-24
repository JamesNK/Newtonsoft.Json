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

#if HAVE_FSHARP_TYPES
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
#if !HAVE_LINQ
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
        #region UnionDefinition
        internal class Union
        {
            public List<UnionCase> Cases;
            public FSharpFunction TagReader { get; set; }
        }

        internal class UnionCase
        {
            public int Tag;
            public string Name;
            public PropertyInfo[] Fields;
            public FSharpFunction FieldReader;
            public FSharpFunction Constructor;
        }
        #endregion

        private const string CasePropertyName = "Case";
        private const string FieldsPropertyName = "Fields";

        private static readonly ThreadSafeStore<Type, Union> UnionCache = new ThreadSafeStore<Type, Union>(CreateUnion);
        private static readonly ThreadSafeStore<Type, Type> UnionTypeLookupCache = new ThreadSafeStore<Type, Type>(CreateUnionTypeLookup);

        private static Type CreateUnionTypeLookup(Type t)
        {
            // this lookup is because cases with fields are derived from union type
            // need to get declaring type to avoid duplicate Unions in cache

            // hacky but I can't find an API to get the declaring type without GetUnionCases
            object[] cases = (object[])FSharpUtils.GetUnionCases(null, t, null);

            object caseInfo = cases.First();

            Type unionType = (Type)FSharpUtils.GetUnionCaseInfoDeclaringType(caseInfo);
            return unionType;
        }

        private static Union CreateUnion(Type t)
        {
            Union u = new Union();

            u.TagReader = (FSharpFunction)FSharpUtils.PreComputeUnionTagReader(null, t, null);
            u.Cases = new List<UnionCase>();

            object[] cases = (object[])FSharpUtils.GetUnionCases(null, t, null);

            foreach (object unionCaseInfo in cases)
            {
                UnionCase unionCase = new UnionCase();
                unionCase.Tag = (int)FSharpUtils.GetUnionCaseInfoTag(unionCaseInfo);
                unionCase.Name = (string)FSharpUtils.GetUnionCaseInfoName(unionCaseInfo);
                unionCase.Fields = (PropertyInfo[])FSharpUtils.GetUnionCaseInfoFields(unionCaseInfo);
                unionCase.FieldReader = (FSharpFunction)FSharpUtils.PreComputeUnionReader(null, unionCaseInfo, null);
                unionCase.Constructor = (FSharpFunction)FSharpUtils.PreComputeUnionConstructor(null, unionCaseInfo, null);

                u.Cases.Add(unionCase);
            }

            return u;
        }

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            Type unionType = UnionTypeLookupCache.Get(value.GetType());
            Union union = UnionCache.Get(unionType);

            int tag = (int)union.TagReader.Invoke(value);
            UnionCase caseInfo = union.Cases.Single(c => c.Tag == tag);

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(CasePropertyName) : CasePropertyName);
            writer.WriteValue(caseInfo.Name);
            if (caseInfo.Fields != null && caseInfo.Fields.Length > 0)
            {
                object[] fields = (object[])caseInfo.FieldReader.Invoke(value);

                writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(FieldsPropertyName) : FieldsPropertyName);
                writer.WriteStartArray();
                foreach (object field in fields)
                {
                    serializer.Serialize(writer, field);
                }
                writer.WriteEndArray();
            }
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
            {
                return null;
            }

            UnionCase caseInfo = null;
            string caseName = null;
            JArray fields = null;

            // start object
            reader.ReadAndAssert();

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value.ToString();
                if (string.Equals(propertyName, CasePropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    reader.ReadAndAssert();

                    Union union = UnionCache.Get(objectType);

                    caseName = reader.Value.ToString();

                    caseInfo = union.Cases.SingleOrDefault(c => c.Name == caseName);

                    if (caseInfo == null)
                    {
                        throw JsonSerializationException.Create(reader, "No union type found with the name '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
                    }
                }
                else if (string.Equals(propertyName, FieldsPropertyName, StringComparison.OrdinalIgnoreCase))
                {
                    reader.ReadAndAssert();
                    if (reader.TokenType != JsonToken.StartArray)
                    {
                        throw JsonSerializationException.Create(reader, "Union fields must been an array.");
                    }

                    fields = (JArray)JToken.ReadFrom(reader);
                }
                else
                {
                    throw JsonSerializationException.Create(reader, "Unexpected property '{0}' found when reading union.".FormatWith(CultureInfo.InvariantCulture, propertyName));
                }

                reader.ReadAndAssert();
            }

            if (caseInfo == null)
            {
                throw JsonSerializationException.Create(reader, "No '{0}' property with union name found.".FormatWith(CultureInfo.InvariantCulture, CasePropertyName));
            }

            object[] typedFieldValues = new object[caseInfo.Fields.Length];

            if (caseInfo.Fields.Length > 0 && fields == null)
            {
                throw JsonSerializationException.Create(reader, "No '{0}' property with union fields found.".FormatWith(CultureInfo.InvariantCulture, FieldsPropertyName));
            }

            if (fields != null)
            {
                if (caseInfo.Fields.Length != fields.Count)
                {
                    throw JsonSerializationException.Create(reader, "The number of field values does not match the number of properties defined by union '{0}'.".FormatWith(CultureInfo.InvariantCulture, caseName));
                }

                for (int i = 0; i < fields.Count; i++)
                {
                    JToken t = fields[i];
                    PropertyInfo fieldProperty = caseInfo.Fields[i];

                    typedFieldValues[i] = t.ToObject(fieldProperty.PropertyType, serializer);
                }
            }

            object[] args = { typedFieldValues };

            return caseInfo.Constructor.Invoke(args);
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
            {
                return false;
            }

            // all fsharp objects have CompilationMappingAttribute
            // get the fsharp assembly from the attribute and initialize latebound methods
            object[] attributes;
#if HAVE_FULL_REFLECTION
            attributes = objectType.GetCustomAttributes(true);
#else
            attributes = objectType.GetTypeInfo().GetCustomAttributes(true).ToArray();
#endif

            bool isFSharpType = false;
            foreach (object attribute in attributes)
            {
                Type attributeType = attribute.GetType();
                if (attributeType.FullName == "Microsoft.FSharp.Core.CompilationMappingAttribute")
                {
                    FSharpUtils.EnsureInitialized(attributeType.Assembly());

                    isFSharpType = true;
                    break;
                }
            }

            if (!isFSharpType)
            {
                return false;
            }

            return (bool)FSharpUtils.IsUnion(null, objectType, null);
        }
    }
}

#endif