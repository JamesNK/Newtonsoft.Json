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

using System.Collections.Generic;
using System.Text;

namespace Newtonsoft.Json
{
  internal enum JsonContainerType
  {
    None,
    Object,
    Array,
    Constructor
  }

  internal struct JsonPosition
  {
    internal JsonContainerType Type;
    internal int? Position;
    internal string PropertyName;

    internal void WriteTo(StringBuilder sb)
    {
      switch (Type)
      {
        case JsonContainerType.Object:
          if (PropertyName != null)
          {
            if (sb.Length > 0)
              sb.Append(".");
            sb.Append(PropertyName);
          }
          break;
        case JsonContainerType.Array:
        case JsonContainerType.Constructor:
          if (Position != null)
          {
            sb.Append("[");
            sb.Append(Position);
            sb.Append("]");
          }
          break;
      }
    }

    internal bool InsideContainer()
    {
      switch (Type)
      {
        case JsonContainerType.Object:
          return (PropertyName != null);
        case JsonContainerType.Array:
        case JsonContainerType.Constructor:
          return (Position != null);
      }

      return false;
    }

    internal static string BuildPath(IEnumerable<JsonPosition> positions)
    {
      StringBuilder sb = new StringBuilder();

      foreach (JsonPosition state in positions)
      {
        state.WriteTo(sb);
      }

      return sb.ToString();
    }
  }
}