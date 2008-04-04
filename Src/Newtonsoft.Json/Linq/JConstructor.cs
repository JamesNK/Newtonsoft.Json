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
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  public class JConstructor : JContainer
  {
    private string _name;

    public string Name
    {
      get { return _name; }
      set { _name = value; }
    }

    public override JsonTokenType Type
    {
      get { return JsonTokenType.Constructor; }
    }

    public JConstructor()
    {
    }

    public JConstructor(JConstructor other)
      : base(other)
    {
      _name = other.Name;
    }

    public JConstructor(string name, params object[] content)
      : this(name, (object)content)
    {
    }

    public JConstructor(string name, object content)
      : this(name)
    {
      Add(content);
    }

    public JConstructor(string name)
    {
      _name = name;
    }

    internal override bool DeepEquals(JToken node)
    {
      JConstructor c = node as JConstructor;
      return (c != null && _name == c.Name && ContentsEqual(c));
    }

    internal override JToken CloneNode()
    {
      return new JConstructor(this);
    }

    internal override void ValidateObject(JToken o, JToken previous)
    {
      ValidationUtils.ArgumentNotNull(o, "o");

      switch (o.Type)
      {
        case JsonTokenType.Property:
          throw new ArgumentException("An item of type {0} cannot be added to content.".FormatWith(CultureInfo.InvariantCulture, o.Type));
      }
    }

    public override void WriteTo(JsonWriter writer, params JsonConverter[] converters)
    {
      writer.WriteStartConstructor(_name);

      foreach (JToken token in Children())
      {
        token.WriteTo(writer, converters);
      }

      writer.WriteEndConstructor();
    }

    public override JToken this[object key]
    {
      get
      {
        ValidationUtils.ArgumentNotNull(key, "o");

        if (!(key is int))
          throw new ArgumentException("Accessed JConstructor values with invalid key value: {0}. Argument position index expected.".FormatWith(CultureInfo.InvariantCulture, MiscellaneousUtils.ToString(key)));

        return GetIndex(this, (int)key);
      }
    }
  }
}