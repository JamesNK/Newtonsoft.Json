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
  /// <summary>
  /// 
  /// </summary>
  public class Identifier
  {
    private string _name;

    /// <summary>
    /// Gets the name.
    /// </summary>
    /// <value>The name.</value>
    public string Name
    {
      get { return _name; }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Identifier"/> class.
    /// </summary>
    /// <param name="name">The name.</param>
    public Identifier(string name)
    {
      _name = name;
    }

    private static bool IsAsciiLetter(char c)
    {
      return (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');
    }

    /// <summary>
    /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
    /// <returns>
    /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
    /// </returns>
    /// <exception cref="T:System.NullReferenceException">The <paramref name="obj"/> parameter is null.</exception>
    public override bool Equals(object obj)
    {
      Identifier function = obj as Identifier;

      return Equals(function);
    }

    /// <summary>
    /// Equalses the specified function.
    /// </summary>
    /// <param name="function">The function.</param>
    /// <returns></returns>
    public bool Equals(Identifier function)
    {
      return (_name == function.Name);
    }

    /// <summary>
    /// Equalses the specified a.
    /// </summary>
    /// <param name="a">A.</param>
    /// <param name="b">The b.</param>
    /// <returns></returns>
    public static bool Equals(Identifier a, Identifier b)
    {
      if (a == b)
        return true;

      if (a != null && b != null)
        return a.Equals(b);

      return false;
    }

    /// <summary>
    /// Serves as a hash function for a particular type.
    /// </summary>
    /// <returns>
    /// A hash code for the current <see cref="T:System.Object"/>.
    /// </returns>
    public override int GetHashCode()
    {
      return _name.GetHashCode();
    }

    /// <summary>
    /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
    /// </returns>
    public override string ToString()
    {
      return _name;
    }

    /// <summary>
    /// Implements the operator ==.
    /// </summary>
    /// <param name="a">A.</param>
    /// <param name="b">The b.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator ==(Identifier a, Identifier b)
    {
      return Identifier.Equals(a, b);
    }

    /// <summary>
    /// Implements the operator !=.
    /// </summary>
    /// <param name="a">A.</param>
    /// <param name="b">The b.</param>
    /// <returns>The result of the operator.</returns>
    public static bool operator !=(Identifier a, Identifier b)
    {
      return !Identifier.Equals(a, b);
    }
  }
}