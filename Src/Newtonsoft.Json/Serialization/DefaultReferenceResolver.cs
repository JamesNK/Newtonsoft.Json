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

namespace Newtonsoft.Json.Serialization
{
  internal class DefaultReferenceResolver : IReferenceResolver
  {
    private int _referenceCount;

    private BidirectionalDictionary<string, object> GetMappings(object context)
    {
      JsonSerializerInternalBase internalSerializer;

      if (context is JsonSerializerInternalBase)
        internalSerializer = (JsonSerializerInternalBase) context;
      else if (context is JsonSerializerProxy)
        internalSerializer = ((JsonSerializerProxy) context).GetInternalSerializer();
      else
        throw new JsonException("The DefaultReferenceResolver can only be used internally.");

      return internalSerializer.DefaultReferenceMappings;
    }

    public object ResolveReference(object context, string reference)
    {
      object value;
      GetMappings(context).TryGetByFirst(reference, out value);
      return value;
    }

    public string GetReference(object context, object value)
    {
      var mappings = GetMappings(context);

      string reference;
      if (!mappings.TryGetBySecond(value, out reference))
      {
        _referenceCount++;
        reference = _referenceCount.ToString(CultureInfo.InvariantCulture);
        mappings.Set(reference, value);
      }

      return reference;
    }

    public void AddReference(object context, string reference, object value)
    {
      GetMappings(context).Set(reference, value);
    }

    public bool IsReferenced(object context, object value)
    {
      string reference;
      return GetMappings(context).TryGetBySecond(value, out reference);
    }
  }
}