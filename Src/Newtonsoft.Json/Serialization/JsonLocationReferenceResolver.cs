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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
    public class JsonLocationReferenceResolver : IReferenceResolver
    {
        private JsonSerializerInternalBase ResolveInternalContext(object context)
        {
            JsonSerializerInternalBase internalSerializer;

            if (context is JsonSerializerInternalBase)
                internalSerializer = (JsonSerializerInternalBase)context;
            else if (context is JsonSerializerProxy)
                internalSerializer = ((JsonSerializerProxy)context).GetInternalSerializer();
            else
                throw new JsonException("The DefaultReferenceResolver can only be used internally.");

            return internalSerializer;
        }

        private BidirectionalDictionary<string, object> GetMappings(object context)
        {
            return ResolveInternalContext(context).DefaultReferenceMappings;
        }

        public object ResolveReference(object context, string reference)
        {
            object value;
            GetMappings(context).TryGetByFirst(reference, out value);
            return value;
        }

        public string GetReference(object context, object value)
        {
            JsonSerializerInternalBase resolvedContext = ResolveInternalContext(context);
            var mappings = GetMappings(context);

            string reference;
            if (!mappings.TryGetBySecond(value, out reference))
            {
                reference = BuildPointer(resolvedContext, value);
                mappings.Set(reference, value);

                // don't write reference
                return null;
            }

            return reference;
        }

        private string BuildPointer(JsonSerializerInternalBase internalContext, object value)
        {
            List<JsonPosition> currentPositions = internalContext.GetCurrentPositions().ToList();

            // deserializer reads into an object to access metadata properties
            // need ignore the bottom level property
            if (internalContext is JsonSerializerInternalReader)
            {
                JsonContract contract = internalContext.Serializer.ContractResolver.ResolveContract(value.GetType());
                switch (contract.ContractType)
                {
                    case JsonContractType.Object:
                    case JsonContractType.Dictionary:
                    case JsonContractType.Dynamic:
                    case JsonContractType.Serializable:
                        // should always be bigger than 1, but just in case...
                        if (currentPositions.Count > 0)
                            currentPositions.RemoveAt(currentPositions.Count - 1);
                        break;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("#");
            foreach (JsonPosition position in currentPositions)
            {
                sb.Append("/" + (position.HasIndex ? position.Position.ToString(CultureInfo.InvariantCulture) : position.PropertyName));
            }

            return sb.ToString();
        }

        public void AddReference(object context, string reference, object value)
        {
            if (reference != null)
                throw new JsonException("Reference should be null.");


            JsonSerializerInternalBase resolvedContext = ResolveInternalContext(context);

            string locationReference = BuildPointer(resolvedContext, value);

            GetMappings(context).Set(locationReference, value);
        }

        public bool IsReferenced(object context, object value)
        {
            string reference;
            return GetMappings(context).TryGetBySecond(value, out reference);
        }
    }
}