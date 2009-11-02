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
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.Collections;

namespace Newtonsoft.Json.Serialization
{
  /// <summary>
  /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
  /// </summary>
  public class JsonDictionaryContract : JsonContract
  {
    internal Type DictionaryKeyType { get; private set; }
    internal Type DictionaryValueType { get; private set; }

    private readonly Type _genericCollectionDefinitionType;
    private Type _genericWrapperType;
    private MemberHandler<object> _genericWrapperCreator;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDictionaryContract"/> class.
    /// </summary>
    /// <param name="underlyingType">The underlying type for the contract.</param>
    public JsonDictionaryContract(Type underlyingType)
      : base(underlyingType)
    {
      Type keyType;
      Type valueType;
      if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<,>), out _genericCollectionDefinitionType))
      {
        keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
        valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];
      }
      else
      {
        ReflectionUtils.GetDictionaryKeyValueTypes(UnderlyingType, out keyType, out valueType);
      }

      DictionaryKeyType = keyType;
      DictionaryValueType = valueType;

      if (IsTypeGenericDictionaryInterface(UnderlyingType))
      {
        CreatedType = ReflectionUtils.MakeGenericType(typeof(Dictionary<,>), keyType, valueType);
      }
      else
      {
        CreatedType = underlyingType;
      }
    }

    internal IWrappedDictionary CreateWrapper(object dictionary)
    {
      if (dictionary is IDictionary)
        return new DictionaryWrapper<object, object>((IDictionary)dictionary);

      if (_genericWrapperType == null)
      {
        _genericWrapperType = ReflectionUtils.MakeGenericType(typeof(DictionaryWrapper<,>), DictionaryKeyType, DictionaryValueType);

        ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { _genericCollectionDefinitionType });
#if !PocketPC
        _genericWrapperCreator = LateBoundDelegateFactory.CreateMethodHandler(genericWrapperConstructor);
#else
        _genericWrapperCreator = (target, args) => genericWrapperConstructor.Invoke(new[] { args[0] });
#endif
      }

      return (IWrappedDictionary)_genericWrapperCreator(null, dictionary);
    }

    private bool IsTypeGenericDictionaryInterface(Type type)
    {
      if (!type.IsGenericType)
        return false;

      Type genericDefinition = type.GetGenericTypeDefinition();

      return (genericDefinition == typeof(IDictionary<,>));
    }
  }
}