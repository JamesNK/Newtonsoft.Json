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
using System.Linq;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Serialization;

namespace Newtonsoft.Json.Schema
{
  /// <summary>
  /// Generates a <see cref="JsonSchema"/> from a specified <see cref="Type"/>.
  /// </summary>
  public class JsonSchemaGenerator
  {
    /// <summary>
    /// Gets or sets how undefined schemas are handled by the serializer.
    /// </summary>
    public UndefinedSchemaIdHandling UndefinedSchemaIdHandling { get; set; }

    private IContractResolver _contractResolver;
    /// <summary>
    /// Gets or sets the contract resolver.
    /// </summary>
    /// <value>The contract resolver.</value>
    public IContractResolver ContractResolver
    {
      get
      {
        if (_contractResolver == null)
          return DefaultContractResolver.Instance;

        return _contractResolver;
      }
      set { _contractResolver = value; }
    }

    private class TypeSchema
    {
      public Type Type { get; private set; }
      public JsonSchema Schema { get; private set;}

      public TypeSchema(Type type, JsonSchema schema)
      {
        ValidationUtils.ArgumentNotNull(type, "type");
        ValidationUtils.ArgumentNotNull(schema, "schema");

        Type = type;
        Schema = schema;
      }
    }

    private JsonSchemaResolver _resolver;
    private IList<TypeSchema> _stack = new List<TypeSchema>();
    private JsonSchema _currentSchema;

    private JsonSchema CurrentSchema
    {
      get { return _currentSchema; }
    }

    private void Push(TypeSchema typeSchema)
    {
      _currentSchema = typeSchema.Schema;
      _stack.Add(typeSchema);
      _resolver.LoadedSchemas.Add(typeSchema.Schema);
    }

    private TypeSchema Pop()
    {
      TypeSchema popped = _stack[_stack.Count - 1];
      _stack.RemoveAt(_stack.Count - 1);
      TypeSchema newValue = _stack.LastOrDefault();
      if (newValue != null)
      {
        _currentSchema = newValue.Schema;
      }
      else
      {
        _currentSchema = null;
      }

      return popped;
    }

    /// <summary>
    /// Generate a <see cref="JsonSchema"/> from the specified type.
    /// </summary>
    /// <param name="type">The type to generate a <see cref="JsonSchema"/> from.</param>
    /// <returns>A <see cref="JsonSchema"/> generated from the specified type.</returns>
    public JsonSchema Generate(Type type)
    {
      return Generate(type, new JsonSchemaResolver(), false);
    }

    /// <summary>
    /// Generate a <see cref="JsonSchema"/> from the specified type.
    /// </summary>
    /// <param name="type">The type to generate a <see cref="JsonSchema"/> from.</param>
    /// <param name="resolver">The <see cref="JsonSchemaResolver"/> used to resolve schema references.</param>
    /// <returns>A <see cref="JsonSchema"/> generated from the specified type.</returns>
    public JsonSchema Generate(Type type, JsonSchemaResolver resolver)
    {
      return Generate(type, resolver, false);
    }

    /// <summary>
    /// Generate a <see cref="JsonSchema"/> from the specified type.
    /// </summary>
    /// <param name="type">The type to generate a <see cref="JsonSchema"/> from.</param>
    /// <param name="rootSchemaNullable">Specify whether the generated root <see cref="JsonSchema"/> will be nullable.</param>
    /// <returns>A <see cref="JsonSchema"/> generated from the specified type.</returns>
    public JsonSchema Generate(Type type, bool rootSchemaNullable)
    {
      return Generate(type, new JsonSchemaResolver(), rootSchemaNullable);
    }

    /// <summary>
    /// Generate a <see cref="JsonSchema"/> from the specified type.
    /// </summary>
    /// <param name="type">The type to generate a <see cref="JsonSchema"/> from.</param>
    /// <param name="resolver">The <see cref="JsonSchemaResolver"/> used to resolve schema references.</param>
    /// <param name="rootSchemaNullable">Specify whether the generated root <see cref="JsonSchema"/> will be nullable.</param>
    /// <returns>A <see cref="JsonSchema"/> generated from the specified type.</returns>
    public JsonSchema Generate(Type type, JsonSchemaResolver resolver, bool rootSchemaNullable)
    {
      ValidationUtils.ArgumentNotNull(type, "type");
      ValidationUtils.ArgumentNotNull(resolver, "resolver");

      _resolver = resolver;

      return GenerateInternal(type, (!rootSchemaNullable) ? Required.Always : Required.Default, false);
    }

    private string GetTitle(Type type)
    {
      JsonContainerAttribute containerAttribute = JsonTypeReflector.GetJsonContainerAttribute(type);

      if (containerAttribute != null && !string.IsNullOrEmpty(containerAttribute.Title))
        return containerAttribute.Title;

      return null;
    }

    private string GetDescription(Type type)
    {
      JsonContainerAttribute containerAttribute = JsonTypeReflector.GetJsonContainerAttribute(type);

      if (containerAttribute != null && !string.IsNullOrEmpty(containerAttribute.Description))
        return containerAttribute.Description;

#if !PocketPC
      DescriptionAttribute descriptionAttribute = ReflectionUtils.GetAttribute<DescriptionAttribute>(type);
      if (descriptionAttribute != null)
        return descriptionAttribute.Description;
#endif

      return null;
    }

    private string GetTypeId(Type type, bool explicitOnly)
    {
      JsonContainerAttribute containerAttribute = JsonTypeReflector.GetJsonContainerAttribute(type);

      if (containerAttribute != null && !string.IsNullOrEmpty(containerAttribute.Id))
        return containerAttribute.Id;

      if (explicitOnly)
        return null;

      switch (UndefinedSchemaIdHandling)
      {
        case UndefinedSchemaIdHandling.UseTypeName:
          return type.FullName;
        case UndefinedSchemaIdHandling.UseAssemblyQualifiedName:
          return type.AssemblyQualifiedName;
        default:
          return null;
      }
    }

    private JsonSchema GenerateInternal(Type type, Required valueRequired, bool optional)
    {
      ValidationUtils.ArgumentNotNull(type, "type");

      string resolvedId = GetTypeId(type, false);
      string explicitId = GetTypeId(type, true);

      if (!string.IsNullOrEmpty(resolvedId))
      {
        JsonSchema resolvedSchema = _resolver.GetSchema(resolvedId);
        if (resolvedSchema != null)
        {
          // resolved schema is not null but referencing member allows nulls
          // change resolved schema to allow nulls. hacky but what are ya gonna do?
          if (valueRequired != Required.Always && !HasFlag(resolvedSchema.Type, JsonSchemaType.Null))
            resolvedSchema.Type |= JsonSchemaType.Null;
          if (optional && resolvedSchema.Optional != true)
            resolvedSchema.Optional = true;

          return resolvedSchema;
        }
      }

      // test for unresolved circular reference
      if (_stack.Any(tc => tc.Type == type))
      {
        throw new Exception("Unresolved circular reference for type '{0}'. Explicitly define an Id for the type using a JsonObject/JsonArray attribute or automatically generate a type Id using the UndefinedSchemaIdHandling property.".FormatWith(CultureInfo.InvariantCulture, type));
      }

      Push(new TypeSchema(type, new JsonSchema()));

      if (explicitId != null)
        CurrentSchema.Id = explicitId;

      if (optional)
        CurrentSchema.Optional = true;
      CurrentSchema.Title = GetTitle(type);
      CurrentSchema.Description = GetDescription(type);

      JsonContract contract = ContractResolver.ResolveContract(type);
      if (contract.Converter != null || contract.InternalConverter != null)
      {
        // todo: Add GetSchema to JsonConverter and use here?
        CurrentSchema.Type = JsonSchemaType.Any;
      }
      else if (contract is JsonDictionaryContract)
      {
        CurrentSchema.Type = AddNullType(JsonSchemaType.Object, valueRequired);

        Type keyType;
        Type valueType;
        ReflectionUtils.GetDictionaryKeyValueTypes(type, out keyType, out valueType);

        if (keyType != null)
        {
          // can be converted to a string
          if (typeof (IConvertible).IsAssignableFrom(keyType))
          {
            CurrentSchema.AdditionalProperties = GenerateInternal(valueType, Required.Default, false);
          }
        }
      }
      else if (contract is JsonArrayContract)
      {
        CurrentSchema.Type = AddNullType(JsonSchemaType.Array, valueRequired);

        CurrentSchema.Id = GetTypeId(type, false);

        JsonArrayAttribute arrayAttribute = JsonTypeReflector.GetJsonContainerAttribute(type) as JsonArrayAttribute;
        bool allowNullItem = (arrayAttribute != null) ? arrayAttribute.AllowNullItems : true;

        Type collectionItemType = ReflectionUtils.GetCollectionItemType(type);
        if (collectionItemType != null)
        {
          CurrentSchema.Items = new List<JsonSchema>();
          CurrentSchema.Items.Add(GenerateInternal(collectionItemType, (!allowNullItem) ? Required.Always : Required.Default, false));
        }
      }
      else if (contract is JsonPrimitiveContract)
      {
        CurrentSchema.Type = GetJsonSchemaType(type, valueRequired);

        if (CurrentSchema.Type == JsonSchemaType.Integer && type.IsEnum && !type.IsDefined(typeof(FlagsAttribute), true))
        {
          CurrentSchema.Enum = new List<JToken>();
          CurrentSchema.Options = new Dictionary<JToken, string>();

          EnumValues<long> enumValues = EnumUtils.GetNamesAndValues<long>(type);
          foreach (EnumValue<long> enumValue in enumValues)
          {
            JToken value = JToken.FromObject(enumValue.Value);

            CurrentSchema.Enum.Add(value);
            CurrentSchema.Options.Add(value, enumValue.Name);
          }
        }
      }
      else if (contract is JsonObjectContract)
      {
        CurrentSchema.Type = AddNullType(JsonSchemaType.Object, valueRequired);
        CurrentSchema.Id = GetTypeId(type, false);
        GenerateObjectSchema(type, (JsonObjectContract)contract);
      }
#if !SILVERLIGHT && !PocketPC
      else if (contract is JsonISerializableContract)
      {
        CurrentSchema.Type = AddNullType(JsonSchemaType.Object, valueRequired);
        CurrentSchema.Id = GetTypeId(type, false);
        GenerateISerializableContract(type, (JsonISerializableContract) contract);
      }
#endif
      else if (contract is JsonStringContract)
      {
        JsonSchemaType schemaType = (!ReflectionUtils.IsNullable(contract.UnderlyingType))
                                      ? JsonSchemaType.String
                                      : AddNullType(JsonSchemaType.String, valueRequired);

        CurrentSchema.Type = schemaType;
      }
      else if (contract is JsonLinqContract)
      {
        CurrentSchema.Type = JsonSchemaType.Any;
      }
      else
      {
        throw new Exception("Unexpected contract type: {0}".FormatWith(CultureInfo.InvariantCulture, contract));
      }

      return Pop().Schema;
    }

    private JsonSchemaType AddNullType(JsonSchemaType type, Required valueRequired)
    {
      if (valueRequired != Required.Always)
        return type | JsonSchemaType.Null;

      return type;
    }

    private void GenerateObjectSchema(Type type, JsonObjectContract contract)
    {
      CurrentSchema.Properties = new Dictionary<string, JsonSchema>();
      foreach (JsonProperty property in contract.Properties)
      {
        if (!property.Ignored)
        {
          bool optional = property.NullValueHandling == NullValueHandling.Ignore ||
                          property.DefaultValueHandling == DefaultValueHandling.Ignore;

          JsonSchema propertySchema = GenerateInternal(property.PropertyType, property.Required, optional);

          if (property.DefaultValue != null)
            propertySchema.Default = JToken.FromObject(property.DefaultValue);

          CurrentSchema.Properties.Add(property.PropertyName, propertySchema);
        }
      }

      if (type.IsSealed)
        CurrentSchema.AllowAdditionalProperties = false;
    }

#if !SILVERLIGHT && !PocketPC
    private void GenerateISerializableContract(Type type, JsonISerializableContract contract)
    {
      CurrentSchema.AllowAdditionalProperties = true;
    }
#endif

    internal static bool HasFlag(JsonSchemaType? value, JsonSchemaType flag)
    {
      // default value is Any
      if (value == null)
        return true;

      return ((value & flag) == flag);
    }

    private JsonSchemaType GetJsonSchemaType(Type type, Required valueRequired)
    {
      JsonSchemaType schemaType = JsonSchemaType.None;
      if (valueRequired != Required.Always && ReflectionUtils.IsNullable(type))
      {
        schemaType = JsonSchemaType.Null;
        if (ReflectionUtils.IsNullableType(type))
          type = Nullable.GetUnderlyingType(type);
      }

      TypeCode typeCode = Type.GetTypeCode(type);

      switch (typeCode)
      {
        case TypeCode.Empty:
        case TypeCode.Object:
          return schemaType | JsonSchemaType.String;
        case TypeCode.DBNull:
          return schemaType | JsonSchemaType.Null;
        case TypeCode.Boolean:
          return schemaType | JsonSchemaType.Boolean;
        case TypeCode.Char:
          return schemaType | JsonSchemaType.String;
        case TypeCode.SByte:
        case TypeCode.Byte:
        case TypeCode.Int16:
        case TypeCode.UInt16:
        case TypeCode.Int32:
        case TypeCode.UInt32:
        case TypeCode.Int64:
        case TypeCode.UInt64:
          return schemaType | JsonSchemaType.Integer;
        case TypeCode.Single:
        case TypeCode.Double:
        case TypeCode.Decimal:
          return schemaType | JsonSchemaType.Float;
        // convert to string?
        case TypeCode.DateTime:
          return schemaType | JsonSchemaType.String;
        case TypeCode.String:
          return schemaType | JsonSchemaType.String;
        default:
          throw new Exception("Unexpected type code '{0}' for type '{1}'.".FormatWith(CultureInfo.InvariantCulture, typeCode, type));
      }
    }
  }
}
