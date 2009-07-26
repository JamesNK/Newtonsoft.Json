using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Schema;
using NUnit.Framework;

namespace Newtonsoft.Json.Tests
{
  public class ExceptionTests : TestFixtureBase
  {
    [Test]
    public void JsonSerializationException()
    {
      JsonSerializationException exception = new JsonSerializationException();
      Assert.AreEqual("Exception of type 'Newtonsoft.Json.JsonSerializationException' was thrown.", exception.Message);

      exception = new JsonSerializationException("Message!");
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual(null, exception.InnerException);

      exception = new JsonSerializationException("Message!", new Exception("Inner!"));
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual("Inner!", exception.InnerException.Message);
    }

    [Test]
    public void JsonWriterException()
    {
      JsonWriterException exception = new JsonWriterException();
      Assert.AreEqual("Exception of type 'Newtonsoft.Json.JsonWriterException' was thrown.", exception.Message);

      exception = new JsonWriterException("Message!");
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual(null, exception.InnerException);

      exception = new JsonWriterException("Message!", new Exception("Inner!"));
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual("Inner!", exception.InnerException.Message);
    }

    [Test]
    public void JsonReaderException()
    {
      JsonReaderException exception = new JsonReaderException();
      Assert.AreEqual("Exception of type 'Newtonsoft.Json.JsonReaderException' was thrown.", exception.Message);

      exception = new JsonReaderException("Message!");
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual(null, exception.InnerException);

      exception = new JsonReaderException("Message!", new Exception("Inner!"));
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual("Inner!", exception.InnerException.Message);
    }

    [Test]
    public void JsonSchemaException()
    {
      JsonSchemaException exception = new JsonSchemaException();
      Assert.AreEqual("Exception of type 'Newtonsoft.Json.Schema.JsonSchemaException' was thrown.", exception.Message);

      exception = new JsonSchemaException("Message!");
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual(null, exception.InnerException);

      exception = new JsonSchemaException("Message!", new Exception("Inner!"));
      Assert.AreEqual("Message!", exception.Message);
      Assert.AreEqual("Inner!", exception.InnerException.Message);
    }
  }
}