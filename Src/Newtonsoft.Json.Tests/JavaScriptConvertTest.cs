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

    [Test]
    [ExpectedException(typeof(ArgumentException), ExpectedMessage = "Unsupported type: System.Version. Use the JsonSerializer class to get the object's JSON representation.")]
    public void ToStringInvalid()
    {
      JavaScriptConvert.ToString(new Version());
    }

    [Test]
    public void GuidToString()
    {
      Guid guid = new Guid("BED7F4EA-1A96-11d2-8F08-00A0C9A6186D");
      string json = JavaScriptConvert.ToString(guid);
      Assert.AreEqual(@"""bed7f4ea-1a96-11d2-8f08-00a0c9a6186d""", json);
    }

    [Test]
    public void EnumToString()
    {
      string json = JavaScriptConvert.ToString(StringComparison.CurrentCultureIgnoreCase);
      Assert.AreEqual("1", json);
    }
  }
}