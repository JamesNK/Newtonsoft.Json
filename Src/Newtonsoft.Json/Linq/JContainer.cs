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
using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Diagnostics;
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  /// <summary>
  /// Represents a token that can contain other tokens.
  /// </summary>
  public abstract class JContainer : JToken
  {
    private JToken _content;

    internal JToken Content
    {
      get { return _content; }
      set { _content = value; }
    }

    internal JContainer()
    {
    }

    internal JContainer(JContainer other)
    {
      ValidationUtils.ArgumentNotNull(other, "c");

      JToken content = other.Last;
      if (content != null)
      {
        do
        {
          content = content._next;
          Add(content.CloneNode());
        }
        while (content != other.Last);
      }
    }

    /// <summary>
    /// Gets a value indicating whether this token has childen tokens.
    /// </summary>
    /// <value>
    /// 	<c>true</c> if this token has child values; otherwise, <c>false</c>.
    /// </value>
    public override bool HasValues
    {
      get { return (_content != null); }
    }

    internal bool ContentsEqual(JContainer container)
    {
      JToken t1 = First;
      JToken t2 = container.First;

      if (t1 == t2)
        return true;

      do
      {
        if (t1 == null && t2 == null)
          return true;

        if (t1 != null && t2 != null && t1.DeepEquals(t2))
        {
          t1 = (t1 != Last) ? t1.Next : null;
          t2 = (t2 != container.Last) ? t2.Next : null;
        }
        else
        {
          return false;
        }
      }
      while (true);
    }

    /// <summary>
    /// Get the first child token of this token.
    /// </summary>
    /// <value>
    /// A <see cref="JToken"/> containing the first child token of the <see cref="JToken"/>.
    /// </value>
    public override JToken First
    {
      get
      {
        if (Last == null)
          return null;

        return Last._next;
      }
    }

    /// <summary>
    /// Get the last child token of this token.
    /// </summary>
    /// <value>
    /// A <see cref="JToken"/> containing the last child token of the <see cref="JToken"/>.
    /// </value>
    public override JToken Last
    {
      [DebuggerStepThrough]
      get { return _content; }
    }

    /// <summary>
    /// Returns a collection of the child tokens of this token, in document order.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of <see cref="JToken"/> containing the child tokens of this <see cref="JToken"/>, in document order.
    /// </returns>
    public override JEnumerable<JToken> Children()
    {
      return new JEnumerable<JToken>(ChildrenInternal());
    }

    private IEnumerable<JToken> ChildrenInternal()
    {
      JToken first = First;
      JToken current = first;
      if (current == null)
        yield break;

      do
      {
        yield return current;
      }
      while (current != null
        && ((current = current.Next) != null));
    }

    /// <summary>
    /// Returns a collection of the child values of this token, in document order.
    /// </summary>
    /// <typeparam name="T">The type to convert the values to.</typeparam>
    /// <returns>
    /// A <see cref="IEnumerable{T}"/> containing the child values of this <see cref="JToken"/>, in document order.
    /// </returns>
    public override IEnumerable<T> Values<T>()
    {
      return Children().Convert<JToken, T>();
    }

    /// <summary>
    /// Returns a collection of the descendant tokens for this token in document order.
    /// </summary>
    /// <returns>An <see cref="IEnumerable{JToken}"/> containing the descendant tokens of the <see cref="JToken"/>.</returns>
    public IEnumerable<JToken> Descendants()
    {
      foreach (JToken o in Children())
      {
        yield return o;
        JContainer c = o as JContainer;
        if (c != null)
        {
          foreach (JToken d in c.Descendants())
          {
            yield return d;
          }
        }
      }
    }

    internal static JToken GetIndex(JContainer c, object o)
    {
      return c.Children().ElementAt((int)o);
    }

    internal bool IsMultiContent(object content)
    {
      return (content is IEnumerable && !(content is string) && !(content is JToken));
    }

    internal void AddInternal(bool isLast, JToken previous, object content)
    {
      if (IsMultiContent(content))
      {
        IEnumerable enumerable = content as IEnumerable;

        JToken multiPrevious = previous;
        foreach (object c in enumerable)
        {
          AddInternal(isLast, multiPrevious, c);
          multiPrevious = (multiPrevious != null) ? multiPrevious._next : Last;
        }
      }
      else
      {
        JToken o = CreateFromContent(content);

        ValidateToken(o);

        if (o.Parent != null)
        {
          o = o.CloneNode();
        }
        else
        {
          JContainer parent = this;
          while (parent.Parent != null)
          {
            parent = parent.Parent;
          }
          if (o == parent)
          {
            o = o.CloneNode();
          }
        }

        JToken next = (previous != null) ? previous._next : o;

        o.Parent = this;
        o.Next = next;

        if (previous != null)
          previous.Next = o;

        if (isLast || previous == null)
          _content = o;
      }
    }

    internal virtual void ValidateToken(JToken o)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      if (o.Type == JsonTokenType.Property)
        throw new Exception("Can not add {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, o.GetType(), GetType()));
    }

    /// <summary>
    /// Adds the specified content as children of this <see cref="JToken"/>.
    /// </summary>
    /// <param name="content">The content to be added.</param>
    public virtual void Add(object content)
    {
      AddInternal(true, Last, content);
    }

    /// <summary>
    /// Adds the specified content as the first children of this <see cref="JToken"/>.
    /// </summary>
    /// <param name="content">The content to be added.</param>
    public void AddFirst(object content)
    {
      AddInternal(false, Last, content);
    }

    internal JToken CreateFromContent(object content)
    {
      if (content is JToken)
        return (JToken)content;
      else
        return new JValue(content);
    }

    /// <summary>
    /// Creates an <see cref="JsonWriter"/> that can be used to add tokens to the <see cref="JToken"/>.
    /// </summary>
    /// <returns>An <see cref="JsonWriter"/> that is ready to have content written to it.</returns>
    public JsonWriter CreateWriter()
    {
      return new JsonTokenWriter(this);
    }

    /// <summary>
    /// Replaces the children nodes of this token with the specified content.
    /// </summary>
    /// <param name="content">The content.</param>
    public void ReplaceAll(object content)
    {
      RemoveAll();
      Add(content);
    }

    /// <summary>
    /// Removes the child nodes from this token.
    /// </summary>
    public void RemoveAll()
    {
      while (_content != null)
      {
        JToken o = _content;

        JToken next = o._next;
        if (o != _content || next != o._next)
          throw new InvalidOperationException("This operation was corrupted by external code.");

        if (next != o)
          o._next = next._next;
        else
          _content = null;

        next.Parent = null;
        next._next = null;
      }
    }

    internal void Remove(JToken o)
    {
      if (o.Parent != this)
        throw new InvalidOperationException("This operation was corrupted by external code.");

      JToken content = _content;

      while (content._next != o)
      {
        content = content._next;
      }
      if (content == o)
      {
        // token is containers last child
        _content = null;
      }
      else
      {
        if (_content == o)
        {
          _content = content;
        }
        content._next = o._next;
      }
      o.Parent = null;
      o.Next = null;
    }

    internal abstract void ValidateObject(JToken o, JToken previous);

    internal void AddObjectSkipNotify(JToken o)
    {
      ValidateObject(o, this);

      Add(o);
    }

    internal void ReadContentFrom(JsonReader r)
    {
      ValidationUtils.ArgumentNotNull(r, "r");
      IJsonLineInfo lineInfo = r as IJsonLineInfo;

      JContainer parent = this;

      do
      {
        if (parent is JProperty && ((JProperty)parent).Value != null)
        {
          if (parent == this)
            return;

          parent = parent.Parent;
        }

        switch (r.TokenType)
        {
          case JsonToken.None:
            // new reader. move to actual content
            break;
          case JsonToken.StartArray:
            JArray a = new JArray();
            a.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(a);
            parent = a;
            break;

          case JsonToken.EndArray:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.StartObject:
            JObject o = new JObject();
            o.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(o);
            parent = o;
            break;
          case JsonToken.EndObject:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.StartConstructor:
            JConstructor constructor = new JConstructor(r.Value.ToString());
            constructor.SetLineInfo(constructor);
            parent.AddObjectSkipNotify(constructor);
            parent = constructor;
            break;
          case JsonToken.EndConstructor:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.String:
          case JsonToken.Integer:
          case JsonToken.Float:
          case JsonToken.Date:
          case JsonToken.Boolean:
            JValue v = new JValue(r.Value);
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.Comment:
            v = JValue.CreateComment(r.Value.ToString());
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.Null:
            v = new JValue(null, JsonTokenType.Null);
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.Undefined:
            v = new JValue(null, JsonTokenType.Undefined);
            v.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(v);
            break;
          case JsonToken.PropertyName:
            JProperty property = new JProperty(r.Value.ToString());
            property.SetLineInfo(lineInfo);
            parent.AddObjectSkipNotify(property);
            parent = property;
            break;
          default:
            throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, r.TokenType));
        }
      }
      while (r.Read());
    }

    internal int ContentsHashCode()
    {
      int hashCode = 0;
      foreach (JToken item in Children())
      {
        hashCode ^= item.GetDeepHashCode();
      }
      return hashCode;
    }
  }
}