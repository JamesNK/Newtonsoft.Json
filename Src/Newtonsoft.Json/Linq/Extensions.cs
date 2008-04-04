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
using System.Globalization;

namespace Newtonsoft.Json.Linq
{
  public static class Extensions
  {
    public static IEnumerable<JToken> Ancestors<T>(this IEnumerable<T> source) where T : JToken
    {
      ValidationUtils.ArgumentNotNull(source, "source");

      return source.SelectMany(j => j.Ancestors());
    }

    //TODO
    //public static IEnumerable<JObject> AncestorsAndSelf<T>(this IEnumerable<T> source) where T : JObject
    //{
    //  ValidationUtils.ArgumentNotNull(source, "source");

    //  return source.SelectMany(j => j.AncestorsAndSelf());
    //}

    public static IEnumerable<JToken> Descendants<T>(this IEnumerable<T> source) where T : JContainer
    {
      ValidationUtils.ArgumentNotNull(source, "source");

      return source.SelectMany(j => j.Descendants());
    }

    //TODO
    //public static IEnumerable<JObject> DescendantsAndSelf<T>(this IEnumerable<T> source) where T : JContainer
    //{
    //  ValidationUtils.ArgumentNotNull(source, "source");

    //  return source.SelectMany(j => j.DescendantsAndSelf());
    //}

    public static IEnumerable<JProperty> Properties(this IEnumerable<JObject> source)
    {
      ValidationUtils.ArgumentNotNull(source, "source");

      return source.SelectMany(d => d.Properties());
    }

    public static IEnumerable<JToken> Values(this IEnumerable<JToken> source, object key)
    {
      return Values<JToken, JToken>(source, key);
    }

    public static IEnumerable<JToken> Values(this IEnumerable<JToken> source)
    {
      return Values<JToken, JToken>(source, null);
    }

    public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source, object key)
    {
      return Values<JToken, U>(source, key);
    }

    public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source)
    {
      return Values<JToken, U>(source, null);
    }

    //public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source, object key)
    //{
    //  return Values<JToken, U>(source, null);
    //}

    internal static IEnumerable<U> Values<T, U>(this IEnumerable<T> source, object key) where T : JToken
    {
      ValidationUtils.ArgumentNotNull(source, "source");

      foreach (JToken token in source)
      {
        if (key == null)
        {
          foreach (JToken t in token.Children())
          {
            yield return t.Convert<JToken, U>(); ;
          }
        }
        else
        {
          JToken value = token[key];
          if (value != null)
            yield return value.Convert<JToken, U>();
        }
      }

      yield break;

      //IEnumerable<JToken> allChildren = source.SelectMany(d => d.Children());
      //IEnumerable<JToken> childrenValues = (key != null) ? allChildren.Select(p => p[key]) : allChildren.SelectMany(p => p.Children());

      //List<U> s = childrenValues.Convert<JToken, U>().ToList();
      //return childrenValues.Convert<JToken, U>();

      //  return source.SelectMany(d => d.Properties())
      //.Where(p => string.Compare(p.Name, name, StringComparison.Ordinal) == 0)
      //.Select(p => p.Value)
      //.Convert<JToken, T>();
    }

    //TODO
    //public static IEnumerable<T> InDocumentOrder<T>(this IEnumerable<T> source) where T : JObject;

    //public static IEnumerable<JToken> Children<T>(this IEnumerable<T> source) where T : JToken
    //{
    //  ValidationUtils.ArgumentNotNull(source, "source");

    //  return source.SelectMany(c => c.Children());
    //}

    public static IEnumerable<JToken> Children(this IEnumerable<JArray> source)
    {
      return Children<JToken>(source);
    }

    public static IEnumerable<U> Children<U>(this IEnumerable<JArray> source)
    {
      ValidationUtils.ArgumentNotNull(source, "source");

      return source.SelectMany(c => c.Children()).Convert<JToken, U>();
    }

    internal static IEnumerable<U> Convert<T, U>(this IEnumerable<T> source) where T : JToken
    {
      ValidationUtils.ArgumentNotNull(source, "source");

      bool cast = typeof(JToken).IsAssignableFrom(typeof(U));

      foreach (JToken token in source)
      {
        yield return Convert<JToken, U>(token, cast);
      }
    }

    internal static U Convert<T, U>(this T token) where T : JToken
    {
      bool cast = typeof(JToken).IsAssignableFrom(typeof(U));

      return Convert<T, U>(token, cast);
    }

    internal static U Convert<T, U>(this T token, bool cast) where T : JToken
    {
      if (cast)
      {
        // HACK
        return (U)(object)token;
      }
      else
      {
        if (token == null)
          return default(U);

        JValue value = token as JValue;
        if (value == null)
          throw new InvalidCastException("Cannot cast {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, token.GetType(), typeof(T)));

        return (U)System.Convert.ChangeType(value.Value, typeof(U), CultureInfo.InvariantCulture);
      }
    }


    //TODO
    //public static void Remove<T>(this IEnumerable<T> source) where T : JContainer;


  }
}