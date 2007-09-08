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
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Collections.Generic;
using System.Drawing;
using System.Web.UI.WebControls;
using System.ComponentModel;

namespace Newtonsoft.Json
{
  public class Identifier
  {
    private string _name;

    public string Name
    {
      get { return _name; }
    }

    public Identifier(string name)
    {
      _name = name;
    }

    private static bool IsAsciiLetter(char c)
    {
      return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    public override bool Equals(object obj)
    {
      Identifier function = obj as Identifier;

      return Equals(function);
    }

    public bool Equals(Identifier function)
    {
      return (_name == function.Name);
    }

    public static bool Equals(Identifier a, Identifier b)
    {
      if (a == b)
        return true;

      if (a != null && b != null)
        return a.Equals(b);

      return false;
    }

    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    public override string ToString()
    {
      return _name;
    }

    public static bool operator ==(Identifier a, Identifier b)
    {
      return Identifier.Equals(a, b);
    }

    public static bool operator !=(Identifier a, Identifier b)
    {
      return !Identifier.Equals(a, b);
    }
  }
}