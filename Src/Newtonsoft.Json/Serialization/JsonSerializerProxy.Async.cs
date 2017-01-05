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

#if !(NET20 || NET35 || NET40 || PORTABLE40)

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Serialization
{
    internal partial class JsonSerializerProxy
    {
        internal override Task<object> DeserializeInternalAsync(JsonReader reader, Type objectType, CancellationToken cancellationToken)
        {
            return _serializerReader != null
                ? _serializerReader.DeserializeAsync(reader, objectType, false, cancellationToken)
                : _serializer.DeserializeAsync(reader, objectType, cancellationToken);
        }

        internal override Task SerializeInternalAsync(JsonWriter jsonWriter, object value, Type rootType, CancellationToken cancellationToken)
        {
            return _serializerWriter != null
                ? _serializerWriter.SerializeAsync(jsonWriter, value, rootType, cancellationToken)
                : _serializer.SerializeAsync(jsonWriter, value, cancellationToken);
        }

        internal override Task PopulateInternalAsync(JsonReader reader, object target, CancellationToken cancellationToken)
        {
            return _serializerReader != null
                ? _serializerReader.PopulateAsync(reader, target, cancellationToken)
                : _serializer.PopulateAsync(reader, target, cancellationToken);
        }
    }
}

#endif