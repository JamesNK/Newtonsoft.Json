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

namespace Newtonsoft.Json.Serialization
{
  internal class JsonSerializerProxy : JsonSerializer
  {
    private readonly JsonSerializerReader _serializerReader;
    private readonly JsonSerializerWriter _serializerWriter;
    private readonly JsonSerializer _serializer;

    public override IReferenceResolver ReferenceResolver
    {
      get { return _serializer.ReferenceResolver; }
      set { _serializer.ReferenceResolver = value; }
    }

    public override JsonConverterCollection Converters
    {
      get { return _serializer.Converters; }
    }

    public override DefaultValueHandling DefaultValueHandling
    {
      get { return _serializer.DefaultValueHandling; }
      set { _serializer.DefaultValueHandling = value; }
    }

    public override IMappingResolver MappingResolver
    {
      get { return _serializer.MappingResolver; }
      set { _serializer.MappingResolver = value; }
    }

    public override MissingMemberHandling MissingMemberHandling
    {
      get { return _serializer.MissingMemberHandling; }
      set { _serializer.MissingMemberHandling = value; }
    }

    public override NullValueHandling NullValueHandling
    {
      get { return _serializer.NullValueHandling; }
      set { _serializer.NullValueHandling = value; }
    }

    public override ObjectCreationHandling ObjectCreationHandling
    {
      get { return _serializer.ObjectCreationHandling; }
      set { _serializer.ObjectCreationHandling = value; }
    }

    public override ReferenceLoopHandling ReferenceLoopHandling
    {
      get { return _serializer.ReferenceLoopHandling; }
      set { _serializer.ReferenceLoopHandling = value; }
    }

    public override PreserveReferencesHandling PreserveReferencesHandling
    {
      get { return _serializer.PreserveReferencesHandling; }
      set { _serializer.PreserveReferencesHandling = value; }
    }

    public JsonSerializerProxy(JsonSerializerReader serializerReader)
    {
      ValidationUtils.ArgumentNotNull(serializerReader, "serializerReader");

      _serializerReader = serializerReader;
      _serializer = serializerReader._serializer;
    }

    public JsonSerializerProxy(JsonSerializerWriter serializerWriter)
    {
      ValidationUtils.ArgumentNotNull(serializerWriter, "serializerWriter");

      _serializerWriter = serializerWriter;
      _serializer = serializerWriter._serializer;
    }

    internal override object DeserializeInternal(JsonReader reader, Type objectType)
    {
      if (_serializerReader != null)
        return _serializerReader.Deserialize(reader, objectType);
      else
        return _serializer.Deserialize(reader, objectType);
    }

    internal override void PopulateInternal(JsonReader reader, object target)
    {
      if (_serializerReader != null)
        _serializerReader.Populate(reader, target);
      else
        _serializer.Populate(reader, target);
    }

    internal override void SerializeInternal(JsonWriter jsonWriter, object value)
    {
      if (_serializerWriter != null)
        _serializerWriter.Serialize(jsonWriter, value);
      else
        _serializer.Serialize(jsonWriter, value);
    }
  }
}