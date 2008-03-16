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
using Newtonsoft.Json.Utilities;
using System.Collections;
using System.Diagnostics;

namespace Newtonsoft.Json.Linq
{
  public abstract class JContainer : JToken
  {
    private JToken _content;

    internal JToken Content
    {
      get { return _content; }
      set { _content = value; }
    }

    protected JContainer()
    {
    }

    protected JContainer(JContainer other)
    {
      ValidationUtils.ArgumentNotNull(other, "c");

      JToken content = other.Last;
      if (content != null)
      {
        do
        {
          content = content.Next;
          Add(content.CloneNode());
        }
        while (content != other.Last);
      }
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

    public override JToken First
    {
      get
      {
        if (Last == null)
          return null;

        return Last.Next;
      }
    }

    public override JToken Last
    {
      [DebuggerStepThrough]
      get { return _content; }
    }

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
      } while ((current = current.Next) != first);
    }

    public override IEnumerable<T> Values<T>()
    {
      return Children().Convert<JToken, T>();
    }

    //public override T Value<T>(object key)
    //{
    //  JToken token = this[key];

    //  return Extensions.Convert<JToken, T>(token);
    //}

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

    internal static JToken FromObjectInternal(object o)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      JsonSerializer jsonSerializer = new JsonSerializer();

      JToken token;
      using (JsonTokenWriter jsonWriter = new JsonTokenWriter())
      {
        jsonSerializer.Serialize(jsonWriter, o);
        token = jsonWriter.Token;
      }

      return token;
    }

    internal static JToken GetIndex(JContainer c, object o)
    {
      return c.Children().ElementAt((int)o);
    }

    internal bool IsMultiContent(object content)
    {
      return (content is IEnumerable && !(content is string));
    }

    public virtual void Add(object content)
    {
      if (IsMultiContent(content))
      {
        IEnumerable enumerable = content as IEnumerable;
        foreach (object c in enumerable)
        {
          Add(c);
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

        JToken last = Last;
        JToken next = First ?? o;

        o.Parent = this;
        o.Next = next;

        if (last != null)
          last.Next = o;
        _content = o;
      }
    }

    internal virtual void ValidateToken(JToken o)
    {
      //throw new NotImplementedException();
    }

    public void AddFirst(object content)
    {
      JToken o = CreateFromContent(content);

      JToken last = Parent.Last;

      o.Parent = Parent;
      o.Next = last.Next;

      last.Next = o;
    }

    protected JToken CreateFromContent(object content)
    {
      if (content is JToken)
        return (JToken)content;

      return new JValue(content);
    }

    public JsonWriter CreateWriter()
    {
      return null;
    }

    public void ReplaceAll(object content)
    {
      RemoveAll();
      Add(content);
    }

    public void RemoveAll()
    {
      while (_content != null)
      {
        JToken o = _content;

        JToken next = o.Next;
        if (o != _content || next != o.Next)
          throw new InvalidOperationException("This operation was corrupted by external code.");

        if (next != o)
          o.Next = next.Next;
        else
          _content = null;

        next.Parent = null;
        next.Next = null;
      }
    }

    internal void Remove(JToken o)
    {
      if (o.Parent != this)
        throw new InvalidOperationException("This operation was corrupted by external code.");

      JToken content = _content;

      while (_content.Next != o)
      {
        content = content.Next;
      }
      if (content == o)
      {
        _content = null;
      }
      else
      {
        if (_content == o)
        {
          _content = content;
        }
        content.Next = o.Next;
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

      JContainer parent = this;

      do
      {
        if (parent is JProperty)
        {
          if (((JProperty)parent).Value != null)
            parent = parent.Parent;
        }

        switch (r.TokenType)
        {
          case JsonToken.None:
            // new reader. move to actual content
            break;
          case JsonToken.StartArray:
            JArray a = new JArray();
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
            parent.AddObjectSkipNotify(o);
            parent = o;
            break;
          case JsonToken.EndObject:
            if (parent == this)
              return;

            parent = parent.Parent;
            break;
          case JsonToken.StartConstructor:
            JConstructor constructor = new JConstructor();
            constructor.Name = r.Value.ToString();
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
            parent.AddObjectSkipNotify(new JValue(r.Value));
            break;
          case JsonToken.Comment:
            parent.AddObjectSkipNotify(JValue.CreateComment(r.Value.ToString()));
            break;
          case JsonToken.Null:
            parent.AddObjectSkipNotify(JValue.Null);
            break;
          case JsonToken.Undefined:
            parent.AddObjectSkipNotify(JValue.Undefined);
            break;
          case JsonToken.PropertyName:
            JProperty property = new JProperty(r.Value.ToString());
            parent.AddObjectSkipNotify(property);
            parent = property;
            break;
          default:
            throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(r.TokenType));
        }
      }
      while (r.Read());
    }
  }
}