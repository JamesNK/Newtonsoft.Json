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
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  [TestFixture]
  public abstract class TestFixtureBase
  {
    [SetUp]
    protected void TestSetup()
    {
      //CultureInfo turkey = CultureInfo.CreateSpecificCulture("tr");
      //Thread.CurrentThread.CurrentCulture = turkey;
      //Thread.CurrentThread.CurrentUICulture = turkey;
    }

    //public string GetOffset(DateTime value)
    //{
    //  TimeSpan utcOffset = TimeZone.CurrentTimeZone.GetUtcOffset(value.ToLocalTime());

    //  return utcOffset.Hours.ToString("+00;-00") + utcOffset.Minutes.ToString("00;00");
    //}

    protected void WriteEscapedJson(string json)
    {
      Console.WriteLine(EscapeJson(json));
    }

    protected string EscapeJson(string json)
    {
      return @"@""" + json.Replace(@"""", @"""""") + @"""";
    }
  }
}
