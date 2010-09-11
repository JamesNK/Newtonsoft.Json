#if !(NET35 || NET20 || SILVERLIGHT)
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Newtonsoft.Json.Utilities
{
  internal class DynamicProxy<T>
  {
    public T Value { get; private set; }

    public DynamicProxy(T value)
    {
      Value = value;
    }

    public virtual IEnumerable<string> GetDynamicMemberNames()
    {
      return new string[0];
    }

    public virtual bool TryBinaryOperation(BinaryOperationBinder binder, object arg, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TryConvert(ConvertBinder binder, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TryCreateInstance(CreateInstanceBinder binder, object[] args, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TryDeleteIndex(DeleteIndexBinder binder, object[] indexes)
    {
      return false;
    }

    public virtual bool TryDeleteMember(DeleteMemberBinder binder)
    {
      return false;
    }

    public virtual bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TryGetMember(GetMemberBinder binder, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TryInvoke(InvokeBinder binder, object[] args, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
    {
      result = null;
      return false;
    }

    public virtual bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
    {
      return false;
    }

    public virtual bool TrySetMember(SetMemberBinder binder, object value)
    {
      return false;
    }

    public virtual bool TryUnaryOperation(UnaryOperationBinder binder, out object result)
    {
      result = null;
      return false;
    }
  }
}
#endif