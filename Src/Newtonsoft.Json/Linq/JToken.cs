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
using System.Linq;
using System.Text;
using System.IO;
using Newtonsoft.Json.Utilities;
using System.Diagnostics;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  public abstract class JToken
  {
    private JContainer _parent;
    internal JToken _next;

    public JContainer Parent
    {
      [DebuggerStepThrough]
      get { return _parent; }
      internal set { _parent = value; }
    }

    public JToken Root
    {
      get
      {
        JContainer parent = Parent;
        if (parent == null)
          return this;

        while (parent.Parent != null)
        {
          parent = parent.Parent;
        }

        return parent;
      }
    }

    internal abstract JToken CloneNode();
    internal abstract bool DeepEquals(JToken node);

    public abstract JsonTokenType Type { get; }
    public abstract bool HasValues { get; }

    public static bool DeepEquals(JToken t1, JToken t2)
    {
      return (t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2)));
    }

    public JToken Next
    {
      get
      {
        if (_parent != null && _next != _parent.First)
          return _next;

        return null;
      }
      internal set { _next = value; }
    }

    public JToken Previous
    {
      get
      {
        if (_parent == null)
          return null;

        JToken parentNext = ((JToken)_parent.Content)._next;
        JToken parentNextBefore = null;
        while (parentNext != this)
        {
          parentNextBefore = parentNext;
          parentNext = parentNext.Next;
        }
        return parentNextBefore;
      }
    }

    public IEnumerable<JToken> Ancestors()
    {
      for (JToken parent = Parent; parent != null; parent = parent.Parent)
      {
        yield return parent;
      }
    }

    public IEnumerable<JToken> AfterSelf()
    {
      if (Parent == null)
        yield break;

      for (JToken o = Next; o != null; o = o.Next)
        yield return o;
    }

    public IEnumerable<JToken> BeforeSelf()
    {
      for (JToken o = Parent.First; o != this; o = o.Next)
        yield return o;
    }

    public virtual JToken this[object key]
    {
      get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
    }

    public virtual T Value<T>(object key)
    {
      JToken token = this[key];

      return Extensions.Convert<JToken, T>(token);
    }

    public virtual JToken First
    {
      get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
      //get { return null; }
    }

    public virtual JToken Last
    {
      get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
      //get { return null; }
    }

    public virtual JEnumerable<JToken> Children()
    {
      throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
      //return JEnumerable<JToken>.Empty;
    }

    public JEnumerable<T> Children<T>() where T : JToken
    {
      return new JEnumerable<T>(Children().OfType<T>());
    }

    public virtual IEnumerable<T> Values<T>()
    {
      throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
    }

    //public virtual IEnumerable<T> Children<T>()
    // {
    //   throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
    //   //return Enumerable.Empty<T>();
    // }

    public void Remove()
    {
      if (_parent == null)
        throw new InvalidOperationException("The parent is missing.");

      _parent.Remove(this);
    }

    public void Replace(JToken value)
    {
      if (_parent == null)
        throw new InvalidOperationException("The parent is missing.");

      JContainer parent = _parent;

      JToken previous = this;
      while (previous._next != this)
      {
        previous = previous._next;
      }
      if (previous == this)
        previous = null;

      bool isLast = (this == _parent.Last);

      Remove();
      parent.AddInternal(isLast, previous, value);
    }

    public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

    public override string ToString()
    {
      return ToString(null);
    }

    private string ToString(params JsonConverter[] converters)
    {
      using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
      {
        JsonTextWriter jw = new JsonTextWriter(sw);
        jw.Formatting = Formatting.Indented;

        WriteTo(jw);

        return sw.ToString();
      }
    }

    private static JValue EnsureValue(JToken value)
    {
      if (value == null)
        throw new ArgumentNullException("value");

      if (value is JProperty)
        value = ((JProperty)value).Value;

      JValue v = value as JValue;

      return v;
    }

    private static bool IsNullable(JToken o)
    {
      return (o.Type == JsonTokenType.Undefined || o.Type == JsonTokenType.Null);
    }

    private static bool ValidateFloat(JToken o, bool nullable)
    {
      return (o.Type == JsonTokenType.Float || o.Type == JsonTokenType.Integer || (nullable && IsNullable(o)));
    }

    private static bool ValidateInteger(JToken o, bool nullable)
    {
      return (o.Type == JsonTokenType.Integer || (nullable && IsNullable(o)));
    }

    private static bool ValidateDate(JToken o, bool nullable)
    {
      return (o.Type == JsonTokenType.Date || (nullable && IsNullable(o)));
    }

    private static bool ValidateBoolean(JToken o, bool nullable)
    {
      return (o.Type == JsonTokenType.Boolean || (nullable && IsNullable(o)));
    }

    private static bool ValidateString(JToken o)
    {
      return (o.Type == JsonTokenType.String || IsNullable(o));
    }

    private static string GetType(JToken t)
    {
      return (t != null) ? t.Type.ToString() : "{null}";
    }

    #region Cast operators
    public static explicit operator bool(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateBoolean(v, false))
        throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (bool)v.Value;
    }

    public static explicit operator DateTimeOffset(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, false))
        throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (DateTimeOffset)v.Value;
    }

    public static explicit operator bool?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateBoolean(v, true))
        throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (bool?)v.Value;
    }

    public static explicit operator long(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (long)v.Value;
    }

    public static explicit operator DateTime?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, true))
        throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (DateTime?)v.Value;
    }

    public static explicit operator DateTimeOffset?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, true))
        throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (DateTimeOffset?)v.Value;
    }

    public static explicit operator decimal?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, true))
        throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (decimal?)v.Value;
    }

    public static explicit operator double?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, true))
        throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (double?)v.Value;
    }

    public static explicit operator int(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return Convert.ToInt32(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator int?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (v.Value != null) ? (int?)Convert.ToInt32(v.Value, CultureInfo.InvariantCulture) : null;
    }

    public static explicit operator DateTime(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, false))
        throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (DateTime)v.Value;
    }

    public static explicit operator long?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (long?)v.Value;
    }

    public static explicit operator float?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, true))
        throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (float?)v.Value;
    }

    public static explicit operator decimal(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, false))
        throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (decimal)v.Value;
    }

    public static explicit operator uint?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (uint?)v.Value;
    }

    public static explicit operator ulong?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (ulong?)v.Value;
    }

    public static explicit operator double(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, false))
        throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (double)v.Value;
    }

    public static explicit operator float(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, false))
        throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return Convert.ToSingle(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator string(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateString(v))
        throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return (string)v.Value;
    }

    public static explicit operator uint(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return Convert.ToUInt32(v.Value, CultureInfo.InvariantCulture);
    }

    public static explicit operator ulong(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(v)));

      return Convert.ToUInt64(v.Value, CultureInfo.InvariantCulture);
    }
    #endregion
  }
}