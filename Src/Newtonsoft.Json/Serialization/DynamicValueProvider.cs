#if !PocketPC
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Newtonsoft.Json.Utilities;
using System.Globalization;

namespace Newtonsoft.Json.Serialization
{
  public class DynamicValueProvider : IValueProvider
  {
    private readonly MemberInfo _memberInfo;
    private Func<object, object> _getter;
    private Action<object, object> _setter;

    public DynamicValueProvider(MemberInfo memberInfo)
    {
      ValidationUtils.ArgumentNotNull(memberInfo, "memberInfo");
      _memberInfo = memberInfo;
    }

    public void SetValue(object target, object value)
    {
      try
      {
        if (_setter == null)
          _setter = LateBoundDelegateFactory.CreateSet<object>(_memberInfo);

        _setter(target, value);
      }
      catch (Exception ex)
      {
        throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
      }
    }

    public object GetValue(object target)
    {
      try
      {
        if (_getter == null)
          _getter = LateBoundDelegateFactory.CreateGet<object>(_memberInfo);

        return _getter(target);
      }
      catch (Exception ex)
      {
        throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
      }
    }
  }
}
#endif