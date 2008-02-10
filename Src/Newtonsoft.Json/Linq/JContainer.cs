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

    public JToken First
    {
      get
      {
        if (Last == null)
          return null;

        return Last.Next;
      }
    }

    public JToken Last
    {
      get { return _content; }
    }

    public IEnumerable<JToken> Children()
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

    public IEnumerable<T> Children<T>()
    {
      return Children().Convert<JToken, T>();
    }

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

    public virtual void Add(object content)
    {
      if (content is IEnumerable && !(content is string))
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

        JToken last = Last;
        JToken next = First ?? o;

        o.Parent = this;
        o.Next = next;

        if (last != null)
          last.Next = o;
        _content = o;
      }
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
            throw new InvalidOperationException(string.Format("The JsonReader should not be on a token of type {0}.", r.TokenType));
        }
      }
      while (r.Read());
    }
  }
}