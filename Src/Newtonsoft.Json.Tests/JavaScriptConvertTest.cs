using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Utilities;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  public class JavaScriptConvertTest : TestFixtureBase
  {
    [Test]
    public void EscapeJavaScriptString()
    {
      string result;

      result = JavaScriptUtils.ToEscapedJavaScriptString("How now brown cow?", '"', true);
      Assert.AreEqual(@"""How now brown cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("How now 'brown' cow?", '"', true);
      Assert.AreEqual(@"""How now 'brown' cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("How now <brown> cow?", '"', true);
      Assert.AreEqual(@"""How now <brown> cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"How 
now brown cow?", '"', true);
      Assert.AreEqual(@"""How \r\nnow brown cow?""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007", '"', true);
      Assert.AreEqual(@"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007""", result);

      result =
        JavaScriptUtils.ToEscapedJavaScriptString("\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013", '"', true);
      Assert.AreEqual(@"""\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013""", result);

      result =
        JavaScriptUtils.ToEscapedJavaScriptString(
          "\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f ", '"', true);
      Assert.AreEqual(@"""\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f """, result);

      result =
        JavaScriptUtils.ToEscapedJavaScriptString(
          "!\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]", '"', true);
      Assert.AreEqual(@"""!\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]""", result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("^_`abcdefghijklmnopqrstuvwxyz{|}~", '"', true);
      Assert.AreEqual(@"""^_`abcdefghijklmnopqrstuvwxyz{|}~""", result);

      string data =
        "\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\"#$%&\u0027()*+,-./0123456789:;\u003c=\u003e?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~";
      string expected =
        @"""\u0000\u0001\u0002\u0003\u0004\u0005\u0006\u0007\b\t\n\u000b\f\r\u000e\u000f\u0010\u0011\u0012\u0013\u0014\u0015\u0016\u0017\u0018\u0019\u001a\u001b\u001c\u001d\u001e\u001f !\""#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\\]^_`abcdefghijklmnopqrstuvwxyz{|}~""";

      result = JavaScriptUtils.ToEscapedJavaScriptString(data, '"', true);
      Assert.AreEqual(expected, result);

      result = JavaScriptUtils.ToEscapedJavaScriptString("Fred's cat.", '\'', true);
      Assert.AreEqual(result, @"'Fred\'s cat.'");

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"""How are you gentlemen?"" said Cats.", '"', true);
      Assert.AreEqual(result, @"""\""How are you gentlemen?\"" said Cats.""");

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"""How are' you gentlemen?"" said Cats.", '"', true);
      Assert.AreEqual(result, @"""\""How are' you gentlemen?\"" said Cats.""");

      result = JavaScriptUtils.ToEscapedJavaScriptString(@"Fred's ""cat"".", '\'', true);
      Assert.AreEqual(result, @"'Fred\'s ""cat"".'");
    }
  }
}