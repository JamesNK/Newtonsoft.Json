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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Newtonsoft.Json.Utilities
{
  internal static class ImmutableCollectionsUtils
  {
    internal class ImmutableCollectionTypeInfo
    {
      public ImmutableCollectionTypeInfo(string contractTypeName, string createdTypeName, string builderTypeName)
      {
        ContractTypeName = contractTypeName;
        CreatedTypeName = createdTypeName;
        BuilderTypeName = builderTypeName;
      }

      public string ContractTypeName { get; set; }
      public string CreatedTypeName { get; set; }
      public string BuilderTypeName { get; set; }
    }

    private const string ImmutableListGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableList`1";
    private const string ImmutableQueueGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableQueue`1";
    private const string ImmutableStackGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableStack`1";
    private const string ImmutableSetGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableSet`1";

    private const string ImmutableArrayTypeName = "System.Collections.Immutable.ImmutableArray";
    private const string ImmutableArrayGenericTypeName = "System.Collections.Immutable.ImmutableArray`1";

    private const string ImmutableListTypeName = "System.Collections.Immutable.ImmutableList";
    private const string ImmutableListGenericTypeName = "System.Collections.Immutable.ImmutableList`1";

    private const string ImmutableQueueTypeName = "System.Collections.Immutable.ImmutableQueue";
    private const string ImmutableQueueGenericTypeName = "System.Collections.Immutable.ImmutableQueue`1";

    private const string ImmutableStackTypeName = "System.Collections.Immutable.ImmutableStack";
    private const string ImmutableStackGenericTypeName = "System.Collections.Immutable.ImmutableStack`1";

    private const string ImmutableSortedSetTypeName = "System.Collections.Immutable.ImmutableSortedSet";
    private const string ImmutableSortedSetGenericTypeName = "System.Collections.Immutable.ImmutableSortedSet`1";

    private const string ImmutableHashSetTypeName = "System.Collections.Immutable.ImmutableHashSet";
    private const string ImmutableHashSetGenericTypeName = "System.Collections.Immutable.ImmutableHashSet`1";

    private static readonly IList<ImmutableCollectionTypeInfo> ArrayContractImmutableCollectionDefinitions = new List<ImmutableCollectionTypeInfo>
      {
        new ImmutableCollectionTypeInfo(ImmutableListGenericInterfaceTypeName, ImmutableListGenericTypeName, ImmutableListTypeName),
        new ImmutableCollectionTypeInfo(ImmutableListGenericTypeName, ImmutableListGenericTypeName, ImmutableListTypeName),
        new ImmutableCollectionTypeInfo(ImmutableQueueGenericInterfaceTypeName, ImmutableQueueGenericTypeName, ImmutableQueueTypeName),
        new ImmutableCollectionTypeInfo(ImmutableQueueGenericTypeName, ImmutableQueueGenericTypeName, ImmutableQueueTypeName),
        new ImmutableCollectionTypeInfo(ImmutableStackGenericInterfaceTypeName, ImmutableStackGenericTypeName, ImmutableStackTypeName),
        new ImmutableCollectionTypeInfo(ImmutableStackGenericTypeName, ImmutableStackGenericTypeName, ImmutableStackTypeName),
        new ImmutableCollectionTypeInfo(ImmutableSetGenericInterfaceTypeName, ImmutableSortedSetGenericTypeName, ImmutableSortedSetTypeName),
        new ImmutableCollectionTypeInfo(ImmutableSortedSetGenericTypeName, ImmutableSortedSetGenericTypeName, ImmutableSortedSetTypeName),
        new ImmutableCollectionTypeInfo(ImmutableHashSetGenericTypeName, ImmutableHashSetGenericTypeName, ImmutableHashSetTypeName),
        new ImmutableCollectionTypeInfo(ImmutableArrayGenericTypeName, ImmutableArrayGenericTypeName, ImmutableArrayTypeName)
      };

    private const string ImmutableDictionaryGenericInterfaceTypeName = "System.Collections.Immutable.IImmutableDictionary`2";

    private const string ImmutableDictionaryTypeName = "System.Collections.Immutable.ImmutableDictionary";
    private const string ImmutableDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableDictionary`2";

    private const string ImmutableSortedDictionaryTypeName = "System.Collections.Immutable.ImmutableSortedDictionary";
    private const string ImmutableSortedDictionaryGenericTypeName = "System.Collections.Immutable.ImmutableSortedDictionary`2";

    private static readonly IList<ImmutableCollectionTypeInfo> DictionaryContractImmutableCollectionDefinitions = new List<ImmutableCollectionTypeInfo>
      {
        new ImmutableCollectionTypeInfo(ImmutableDictionaryGenericInterfaceTypeName, ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryTypeName),
        new ImmutableCollectionTypeInfo(ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryGenericTypeName, ImmutableSortedDictionaryTypeName),
        new ImmutableCollectionTypeInfo(ImmutableDictionaryGenericTypeName, ImmutableDictionaryGenericTypeName, ImmutableDictionaryTypeName)
      };

    internal static bool TryBuildImmutableForArrayContract(Type underlyingType, Type collectionItemType, out Type createdType, out MethodBase parameterizedCreator)
    {
      if (underlyingType.IsGenericType())
      {
        string name = underlyingType.GetGenericTypeDefinition().FullName;
        ImmutableCollectionTypeInfo definition = ArrayContractImmutableCollectionDefinitions.FirstOrDefault(d => d.ContractTypeName == name);
        if (definition != null)
        {
          Type createdTypeDefinition = Type.GetType(definition.CreatedTypeName + ", System.Collections.Immutable");
          Type builderTypeDefinition = Type.GetType(definition.BuilderTypeName + ", System.Collections.Immutable");
          if (createdTypeDefinition != null && builderTypeDefinition != null)
          {
            MethodInfo mb = builderTypeDefinition.GetMethods().FirstOrDefault(m => m.Name == "CreateRange" && m.GetParameters().Length == 1);
            if (mb != null)
            {
              createdType = createdTypeDefinition.MakeGenericType(collectionItemType);
              parameterizedCreator = mb.MakeGenericMethod(collectionItemType);
              return true;
            }
          }
        }
      }

      createdType = null;
      parameterizedCreator = null;
      return false;
    }

    internal static bool TryBuildImmutableForDictionaryContract(Type underlyingType, Type keyItemType, Type valueItemType, out Type createdType, out MethodBase parameterizedCreator)
    {
      if (underlyingType.IsGenericType())
      {
        string name = underlyingType.GetGenericTypeDefinition().FullName;
        ImmutableCollectionTypeInfo definition = DictionaryContractImmutableCollectionDefinitions.FirstOrDefault(d => d.ContractTypeName == name);
        if (definition != null)
        {
          Type createdTypeDefinition = Type.GetType(definition.CreatedTypeName + ", System.Collections.Immutable");
          Type builderTypeDefinition = Type.GetType(definition.BuilderTypeName + ", System.Collections.Immutable");
          if (createdTypeDefinition != null && builderTypeDefinition != null)
          {
            MethodInfo mb = builderTypeDefinition.GetMethods().FirstOrDefault(m =>
            {
              ParameterInfo[] parameters = m.GetParameters();

              return m.Name == "CreateRange" && parameters.Length == 1 && parameters[0].ParameterType.IsGenericType() && parameters[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>);
            });
            if (mb != null)
            {
              createdType = createdTypeDefinition.MakeGenericType(keyItemType, valueItemType);
              parameterizedCreator = mb.MakeGenericMethod(keyItemType, valueItemType);
              return true;
            }
          }
        }
      }

      createdType = null;
      parameterizedCreator = null;
      return false;
    }
  }
}
#endif