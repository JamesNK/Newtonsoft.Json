using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Newtonsoft.Json.Serialization
{
  public interface IValueProvider
  {
    void SetValue(object target, object value);
    object GetValue(object target);
  }
}