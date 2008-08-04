using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json
{
  [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
  public class JsonConverterAttribute : Attribute
  {
    private readonly Type _converterType;

    public Type ConverterType
    {
      get { return _converterType; }
    }

    public JsonConverterAttribute(Type converterType)
    {
      if (converterType == null)
        throw new ArgumentNullException("converterType");

      _converterType = converterType;
    }

    internal JsonConverter CreateJsonConverterInstance()
    {
      try
      {
        return (JsonConverter)Activator.CreateInstance(_converterType);
      }
      catch (Exception ex)
      {
        throw new Exception("Error creating {0}".FormatWith(CultureInfo.InvariantCulture, _converterType), ex);
      }
    }
  }
}