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
#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
using System.Dynamic;
using System.Linq.Expressions;
#endif
using System.IO;
using Newtonsoft.Json.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
#if NET20
using Newtonsoft.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents an abstract JSON token.
  /// </summary>
  public abstract class JToken : IJEnumerable<JToken>, IJsonLineInfo
#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
, ICloneable
#endif
#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
, IDynamicMetaObjectProvider
#endif
  {
    private JContainer _parent;
    private JToken _previous;
    private JToken _next;
    private static JTokenEqualityComparer _equalityComparer;

    private int? _lineNumber;
    private int? _linePosition;

    /// <summary>
    /// Gets a comparer that can compare two tokens for value equality.
    /// </summary>
    /// <value>A <see cref="JTokenEqualityComparer"/> that can compare two nodes for value equality.</value>
    public static JTokenEqualityComparer EqualityComparer
    {
      get
      {
        if (_equalityComparer == null)
          _equalityComparer = new JTokenEqualityComparer();

        return _equalityComparer;
      }
    }

    /// <summary>
    /// Gets or sets the parent.
    /// </summary>
    /// <value>The parent.</value>
    public JContainer Parent
    {
      [DebuggerStepThrough]
      get { return _parent; }
      internal set { _parent = value; }
    }

    /// <summary>
    /// Gets the root <see cref="JToken"/> of this <see cref="JToken"/>.
    /// </summary>
    /// <value>The root <see cref="JToken"/> of this <see cref="JToken"/>.</value>
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

    internal abstract JToken CloneToken();
    internal abstract bool DeepEquals(JToken node);

    /// <summary>
    /// Gets the node type for this <see cref="JToken"/>.
    /// </summary>
    /// <value>The type.</value>
    public abstract JTokenType Type { get; }

    /// <summary>
    /// Gets a value indicating whether this token has childen tokens.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
    /// </value>
    public abstract bool HasValues { get; }

    /// <summary>
    /// Compares the values of two tokens, including the values of all descendant tokens.
    /// </summary>
    /// <param name="t1">The first <see cref="JToken"/> to compare.</param>
    /// <param name="t2">The second <see cref="JToken"/> to compare.</param>
    /// <returns>true if the tokens are equal; otherwise false.</returns>
    public static bool DeepEquals(JToken t1, JToken t2)
    {
      return (t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2)));
    }

    /// <summary>
    /// Gets the next sibling token of this node.
    /// </summary>
    /// <value>The <see cref="JToken"/> that contains the next sibling token.</value>
    public JToken Next
    {
      get { return _next; }
      internal set { _next = value; }
    }

    /// <summary>
    /// Gets the previous sibling token of this node.
    /// </summary>
    /// <value>The <see cref="JToken"/> that contains the previous sibling token.</value>
    public JToken Previous
    {
      get { return _previous; }
      internal set { _previous = value; }
    }

    internal JToken()
    {
    }

    /// <summary>
    /// Adds the specified content immediately after this token.
    /// </summary>
    /// <param name="content">A content object that contains simple content or a collection of content objects to be added after this token.</param>
    public void AddAfterSelf(object content)
    {
      if (_parent == null)
        throw new InvalidOperationException("The parent is missing.");

      int index = _parent.IndexOfItem(this);
      _parent.AddInternal(index + 1, content, false);
    }

    /// <summary>
    /// Adds the specified content immediately before this token.
    /// </summary>
    /// <param name="content">A content object that contains simple content or a collection of content objects to be added before this token.</param>
    public void AddBeforeSelf(object content)
    {
      if (_parent == null)
        throw new InvalidOperationException("The parent is missing.");

      int index = _parent.IndexOfItem(this);
      _parent.AddInternal(index, content, false);
    }

    /// <summary>
    /// Returns a collection of the ancestor tokens of this token.
    /// </summary>
    /// <returns>A collection of the ancestor tokens of this token.</returns>
    public IEnumerable<JToken> Ancestors()
    {
      for (JToken parent = Parent; parent != null; parent = parent.Parent)
      {
        yield return parent;
      }
    }

    /// <summary>
    /// Returns a collection of the sibling tokens after this token, in document order.
    /// </summary>
    /// <returns>A collection of the sibling tokens after this tokens, in document order.</returns>
    public IEnumerable<JToken> AfterSelf()
    {
      if (Parent == null)
        yield break;

      for (JToken o = Next; o != null; o = o.Next)
      {
        yield return o;
      }
    }

    /// <summary>
    /// Returns a collection of the sibling tokens before this token, in document order.
    /// </summary>
    /// <returns>A collection of the sibling tokens before this token, in document order.</returns>
    public IEnumerable<JToken> BeforeSelf()
    {
      for (JToken o = Parent.First; o != this; o = o.Next)
      {
        yield return o;
      }
    }

    /// <summary>
    /// Gets the <see cref="JToken"/> with the specified key.
    /// </summary>
    /// <value>The <see cref="JToken"/> with the specified key.</value>
    public virtual JToken this[object key]
    {
      get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
      set { throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
    }

    /// <summary>
    /// Gets the <see cref="JToken"/> with the specified key converted to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to convert the token to.</typeparam>
    /// <param name="key">The token key.</param>
    /// <returns>The converted token value.</returns>
    public virtual T Value<T>(object key)
    {
      JToken token = this[key];

      return Extensions.Convert<JToken, T>(token);
    }

    /// <summary>
    /// Get the first child token of this token.
    /// </summary>
    /// <value>A <see cref="JToken"/> containing the first child token of the <see cref="JToken"/>.</value>
    public virtual JToken First
    {
      get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
    }

    /// <summary>
    /// Get the last child token of this token.
    /// </summary>
    /// <value>A <see cref="JToken"/> containing the last child token of the <see cref="JToken"/>.</value>
    public virtual JToken Last
    {
      get { throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType())); }
    }

    /// <summary>
    /// Returns a collection of the child tokens of this token, in document order.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.</returns>
    public virtual JEnumerable<JToken> Children()
    {
      return JEnumerable<JToken>.Empty;
    }

    /// <summary>
    /// Returns a collection of the child tokens of this token, in document order, filtered by the specified type.
    /// </summary>
    /// <typeparam name="T">The type to filter the child tokens on.</typeparam>
    /// <returns>A <see cref="JEnumerable{T}"/> containing the child tokens of this <see cref="JToken"/>, in document order.</returns>
    public JEnumerable<T> Children<T>() where T : JToken
    {
      return new JEnumerable<T>(Children().OfType<T>());
    }

    /// <summary>
    /// Returns a collection of the child values of this token, in document order.
    /// </summary>
    /// <typeparam name="T">The type to convert the values to.</typeparam>
    /// <returns>A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="JToken"/>, in document order.</returns>
    public virtual IEnumerable<T> Values<T>()
    {
      throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
    }

    /// <summary>
    /// Removes this token from its parent.
    /// </summary>
    public void Remove()
    {
      if (_parent == null)
        throw new InvalidOperationException("The parent is missing.");

      _parent.RemoveItem(this);
    }

    /// <summary>
    /// Replaces this token with the specified token.
    /// </summary>
    /// <param name="value">The value.</param>
    public void Replace(JToken value)
    {
      if (_parent == null)
        throw new InvalidOperationException("The parent is missing.");

      _parent.ReplaceItem(this, value);
    }

    /// <summary>
    /// Writes this token to a <see cref="JsonWriter"/>.
    /// </summary>
    /// <param name="writer">A <see cref="JsonWriter"/> into which this method will write.</param>
    /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
    public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);

    /// <summary>
    /// Returns the indented JSON for this token.
    /// </summary>
    /// <returns>
    /// The indented JSON for this token.
    /// </returns>
    public override string ToString()
    {
      return ToString(Formatting.Indented);
    }

    /// <summary>
    /// Returns the JSON for this token using the given formatting and converters.
    /// </summary>
    /// <param name="formatting">Indicates how the output is formatted.</param>
    /// <param name="converters">A collection of <see cref="JsonConverter"/> which will be used when writing the token.</param>
    /// <returns>The JSON for this token using the given formatting and converters.</returns>
    public string ToString(Formatting formatting, params JsonConverter[] converters)
    {
      using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
      {
        JsonTextWriter jw = new JsonTextWriter(sw);
        jw.Formatting = formatting;

        WriteTo(jw, converters);

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

    private static string GetType(JToken token)
    {
      ValidationUtils.ArgumentNotNull(token, "token");

      if (token is JProperty)
        token = ((JProperty)token).Value;

      return token.Type.ToString();
    }

    private static bool IsNullable(JToken o)
    {
      return (o.Type == JTokenType.Undefined || o.Type == JTokenType.Null);
    }

    private static bool ValidateFloat(JToken o, bool nullable)
    {
      return (o.Type == JTokenType.Float || o.Type == JTokenType.Integer || (nullable && IsNullable(o)));
    }

    private static bool ValidateInteger(JToken o, bool nullable)
    {
      return (o.Type == JTokenType.Integer || (nullable && IsNullable(o)));
    }

    private static bool ValidateDate(JToken o, bool nullable)
    {
      return (o.Type == JTokenType.Date || (nullable && IsNullable(o)));
    }

    private static bool ValidateBoolean(JToken o, bool nullable)
    {
      return (o.Type == JTokenType.Boolean || (nullable && IsNullable(o)));
    }

    private static bool ValidateString(JToken o)
    {
      return (o.Type == JTokenType.String || o.Type == JTokenType.Comment || o.Type == JTokenType.Raw || IsNullable(o));
    }

    private static bool ValidateBytes(JToken o)
    {
      return (o.Type == JTokenType.Bytes || IsNullable(o));
    }

    #region Cast from operators
    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Boolean"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator bool(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateBoolean(v, false))
        throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture);
    }

#if !PocketPC && !NET20
    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.DateTimeOffset"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator DateTimeOffset(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, false))
        throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (DateTimeOffset)v.Value;
    }
#endif

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Boolean}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator bool?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateBoolean(v, true))
        throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (bool?)Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Int64"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator long(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToInt64(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{DateTime}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator DateTime?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, true))
        throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (DateTime?)Convert.ToDateTime(v.Value, CultureInfo.InvariantCulture) : null;
    }

#if !PocketPC && !NET20
    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{DateTimeOffset}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator DateTimeOffset?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, true))
        throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (DateTimeOffset?)v.Value;
    }
#endif

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Decimal}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator decimal?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, true))
        throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (decimal?)Convert.ToDecimal(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Double}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator double?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, true))
        throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (double?)v.Value;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Int32"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator int(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToInt32(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Int16"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator short(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToInt16(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.UInt16"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static explicit operator ushort(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToUInt16(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Int32}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator int?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (int?)Convert.ToInt32(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Int16}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator short?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (short?)Convert.ToInt16(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{UInt16}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static explicit operator ushort?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (ushort?)Convert.ToInt16(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.DateTime"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator DateTime(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateDate(v, false))
        throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToDateTime(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Int64}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator long?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (long?)Convert.ToInt64(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{Single}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator float?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, true))
        throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (float?)Convert.ToSingle(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Decimal"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator decimal(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, false))
        throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToDecimal(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{UInt32}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static explicit operator uint?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (uint?)Convert.ToUInt32(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="Nullable{UInt64}"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static explicit operator ulong?(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, true))
        throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? (ulong?)Convert.ToUInt64(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Double"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator double(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, false))
        throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToDouble(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.Single"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator float(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateFloat(v, false))
        throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToSingle(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.String"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator string(JToken value)
    {
      if (value == null)
        return null;

      JValue v = EnsureValue(value);
      if (v == null || !ValidateString(v))
        throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (v.Value != null) ? Convert.ToString(v.Value, CultureInfo.InvariantCulture) : null;
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.UInt32"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static explicit operator uint(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToUInt32(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="System.UInt64"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    [CLSCompliant(false)]
    public static explicit operator ulong(JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateInteger(v, false))
        throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return Convert.ToUInt64(v.Value, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Performs an explicit conversion from <see cref="Newtonsoft.Json.Linq.JToken"/> to <see cref="T:System.Byte[]"/>.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The result of the conversion.</returns>
    public static explicit operator byte[](JToken value)
    {
      JValue v = EnsureValue(value);
      if (v == null || !ValidateBytes(v))
        throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));

      return (byte[])v.Value;
    }
    #endregion

    #region Cast to operators
    /// <summary>
    /// Performs an implicit conversion from <see cref="Boolean"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(bool value)
    {
      return new JValue(value);
    }

#if !PocketPC && !NET20
    /// <summary>
    /// Performs an implicit conversion from <see cref="DateTimeOffset"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(DateTimeOffset value)
    {
      return new JValue(value);
    }
#endif

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Boolean}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(bool? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Int64}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(long value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{DateTime}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(DateTime? value)
    {
      return new JValue(value);
    }

#if !PocketPC && !NET20
    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{DateTimeOffset}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(DateTimeOffset? value)
    {
      return new JValue(value);
    }
#endif

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Decimal}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(decimal? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Double}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(double? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Int16"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(short value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="UInt16"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(ushort value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Int32"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(int value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Int32}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(int? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="DateTime"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(DateTime value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Int64}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(long? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Single}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(float? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Decimal"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(decimal value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{Int16}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(short? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{UInt16}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(ushort? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{UInt32}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(uint? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Nullable{UInt64}"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(ulong? value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Double"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(double value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="Single"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(float value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="String"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(string value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="UInt32"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(uint value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="UInt64"/> to <see cref="JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    [CLSCompliant(false)]
    public static implicit operator JToken(ulong value)
    {
      return new JValue(value);
    }

    /// <summary>
    /// Performs an implicit conversion from <see cref="T:System.Byte[]"/> to <see cref="Newtonsoft.Json.Linq.JToken"/>.
    /// </summary>
    /// <param name="value">The value to create a <see cref="JValue"/> from.</param>
    /// <returns>The <see cref="JValue"/> initialized with the specified value.</returns>
    public static implicit operator JToken(byte[] value)
    {
      return new JValue(value);
    }
    #endregion

    IEnumerator IEnumerable.GetEnumerator()
    {
      return ((IEnumerable<JToken>)this).GetEnumerator();
    }

    IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
    {
      return Children().GetEnumerator();
    }

    internal abstract int GetDeepHashCode();

    IJEnumerable<JToken> IJEnumerable<JToken>.this[object key]
    {
      get { return this[key]; }
    }

    /// <summary>
    /// Creates an <see cref="JsonReader"/> for this token.
    /// </summary>
    /// <returns>An <see cref="JsonReader"/> that can be used to read this token and its descendants.</returns>
    public JsonReader CreateReader()
    {
      return new JTokenReader(this);
    }

    internal static JToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
    {
      ValidationUtils.ArgumentNotNull(o, "o");
      ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");

      JToken token;
      using (JTokenWriter jsonWriter = new JTokenWriter())
      {
        jsonSerializer.Serialize(jsonWriter, o);
        token = jsonWriter.Token;
      }

      return token;
    }

    /// <summary>
    /// Creates a <see cref="JToken"/> from an object.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="JToken"/>.</param>
    /// <returns>A <see cref="JToken"/> with the value of the specified object</returns>
    public static JToken FromObject(object o)
    {
      return FromObjectInternal(o, new JsonSerializer());
    }

    /// <summary>
    /// Creates a <see cref="JToken"/> from an object using the specified <see cref="JsonSerializer"/>.
    /// </summary>
    /// <param name="o">The object that will be used to create <see cref="JToken"/>.</param>
    /// <param name="jsonSerializer">The <see cref="JsonSerializer"/> that will be used when reading the object.</param>
    /// <returns>A <see cref="JToken"/> with the value of the specified object</returns>
    public static JToken FromObject(object o, JsonSerializer jsonSerializer)
    {
      return FromObjectInternal(o, jsonSerializer);
    }

    /// <summary>
    /// Creates the specified .NET type from the <see cref="JToken"/>.
    /// </summary>
    /// <returns>The new object created from the JSON value.</returns>
    public T ToObject<T>()
    {
      return ToObject<T>(new JsonSerializer());
    }

    /// <summary>
    /// Creates the specified .NET type from the <see cref="JToken"/> using the specified <see cref="JsonSerializer"/>.
    /// </summary>
    /// <param name="jsonSerializer">The <see cref="JsonSerializer"/> that will be used when creating the object.</param>
    /// <returns>The new object created from the JSON value.</returns>
    public T ToObject<T>(JsonSerializer jsonSerializer)
    {
      ValidationUtils.ArgumentNotNull(jsonSerializer, "jsonSerializer");

      using (JTokenReader jsonReader = new JTokenReader(this))
      {
        return jsonSerializer.Deserialize<T>(jsonReader);
      }
    }

    /// <summary>
    /// Creates a <see cref="JToken"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">An <see cref="JsonReader"/> positioned at the token to read into this <see cref="JToken"/>.</param>
    /// <returns>
    /// An <see cref="JToken"/> that contains the token and its descendant tokens
    /// that were read from the reader. The runtime type of the token is determined
    /// by the token type of the first token encountered in the reader.
    /// </returns>
    public static JToken ReadFrom(JsonReader reader)
    {
      ValidationUtils.ArgumentNotNull(reader, "reader");

      if (reader.TokenType == JsonToken.None)
      {
        if (!reader.Read())
          throw new Exception("Error reading JToken from JsonReader.");
      }

      if (reader.TokenType == JsonToken.StartObject)
        return JObject.Load(reader);

      if (reader.TokenType == JsonToken.StartArray)
        return JArray.Load(reader);

      if (reader.TokenType == JsonToken.PropertyName)
        return JProperty.Load(reader);

      if (reader.TokenType == JsonToken.StartConstructor)
        return JConstructor.Load(reader);

      if (!JsonReader.IsStartToken(reader.TokenType))
        return new JValue(reader.Value);

      // TODO: loading constructor and parameters?
      throw new Exception("Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
    }

    /// <summary>
    /// Load a <see cref="JToken"/> from a string that contains JSON.
    /// </summary>
    /// <param name="json">A <see cref="String"/> that contains JSON.</param>
    /// <returns>A <see cref="JToken"/> populated from the string that contains JSON.</returns>
    public static JToken Parse(string json)
    {
      JsonReader jsonReader = new JsonTextReader(new StringReader(json));

      JToken t = Load(jsonReader);

      if (jsonReader.Read() && jsonReader.TokenType != JsonToken.Comment)
        throw new Exception("Additional text found in JSON string after parsing content.");

      return t;
    }

    /// <summary>
    /// Creates a <see cref="JToken"/> from a <see cref="JsonReader"/>.
    /// </summary>
    /// <param name="reader">An <see cref="JsonReader"/> positioned at the token to read into this <see cref="JToken"/>.</param>
    /// <returns>
    /// An <see cref="JToken"/> that contains the token and its descendant tokens
    /// that were read from the reader. The runtime type of the token is determined
    /// by the token type of the first token encountered in the reader.
    /// </returns>
    public static JToken Load(JsonReader reader)
    {
      return ReadFrom(reader);
    }

    internal void SetLineInfo(IJsonLineInfo lineInfo)
    {
      if (lineInfo == null || !lineInfo.HasLineInfo())
        return;

      SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
    }

    internal void SetLineInfo(int lineNumber, int linePosition)
    {
      _lineNumber = lineNumber;
      _linePosition = linePosition;
    }

    bool IJsonLineInfo.HasLineInfo()
    {
      return (_lineNumber != null && _linePosition != null);
    }

    int IJsonLineInfo.LineNumber
    {
      get { return _lineNumber ?? 0; }
    }

    int IJsonLineInfo.LinePosition
    {
      get { return _linePosition ?? 0; }
    }

    /// <summary>
    /// Selects the token that matches the object path.
    /// </summary>
    /// <param name="path">
    /// The object path from the current <see cref="JToken"/> to the <see cref="JToken"/>
    /// to be returned. This must be a string of property names or array indexes separated
    /// by periods, such as <code>Tables[0].DefaultView[0].Price</code> in C# or
    /// <code>Tables(0).DefaultView(0).Price</code> in Visual Basic.
    /// </param>
    /// <returns>The <see cref="JToken"/> that matches the object path or a null reference if no matching token is found.</returns>
    public JToken SelectToken(string path)
    {
      return SelectToken(path, false);
    }

    /// <summary>
    /// Selects the token that matches the object path.
    /// </summary>
    /// <param name="path">
    /// The object path from the current <see cref="JToken"/> to the <see cref="JToken"/>
    /// to be returned. This must be a string of property names or array indexes separated
    /// by periods, such as <code>Tables[0].DefaultView[0].Price</code> in C# or
    /// <code>Tables(0).DefaultView(0).Price</code> in Visual Basic.
    /// </param>
    /// <param name="errorWhenNoMatch">A flag to indicate whether an error should be thrown if no token is found.</param>
    /// <returns>The <see cref="JToken"/> that matches the object path.</returns>
    public JToken SelectToken(string path, bool errorWhenNoMatch)
    {
      JPath p = new JPath(path);
      return p.Evaluate(this, errorWhenNoMatch);
    }

#if !(NET35 || NET20 || WINDOWS_PHONE || PORTABLE)
    /// <summary>
    /// Returns the <see cref="T:System.Dynamic.DynamicMetaObject"/> responsible for binding operations performed on this object.
    /// </summary>
    /// <param name="parameter">The expression tree representation of the runtime value.</param>
    /// <returns>
    /// The <see cref="T:System.Dynamic.DynamicMetaObject"/> to bind this object.
    /// </returns>
    protected virtual DynamicMetaObject GetMetaObject(Expression parameter)
    {
      return new DynamicProxyMetaObject<JToken>(parameter, this, new DynamicProxy<JToken>(), true);
    }

    /// <summary>
    /// Returns the <see cref="T:System.Dynamic.DynamicMetaObject"/> responsible for binding operations performed on this object.
    /// </summary>
    /// <param name="parameter">The expression tree representation of the runtime value.</param>
    /// <returns>
    /// The <see cref="T:System.Dynamic.DynamicMetaObject"/> to bind this object.
    /// </returns>
    DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
    {
      return GetMetaObject(parameter);
    }
#endif

#if !(SILVERLIGHT || NETFX_CORE || PORTABLE)
    object ICloneable.Clone()
    {
      return DeepClone();
    }
#endif

    /// <summary>
    /// Creates a new instance of the <see cref="JToken"/>. All child tokens are recursively cloned.
    /// </summary>
    /// <returns>A new instance of the <see cref="JToken"/>.</returns>
    public JToken DeepClone()
    {
      return CloneToken();
    }
  }
}