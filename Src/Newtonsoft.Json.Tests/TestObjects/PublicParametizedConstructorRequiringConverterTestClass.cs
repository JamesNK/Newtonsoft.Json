using System;

namespace Newtonsoft.Json.Tests.TestObjects
{
  public class NameContainer
  {
    public string Value { get; set; }
  }

  public class NameContainerConverter : JsonConverter
  {
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
      NameContainer nameContainer = value as NameContainer;

      if (nameContainer != null)
        writer.WriteValue(nameContainer.Value);
      else
        writer.WriteNull();
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
      NameContainer nameContainer = new NameContainer();
      nameContainer.Value = (string)reader.Value;

      return nameContainer;
    }

    public override bool CanConvert(Type objectType)
    {
      return objectType == typeof(NameContainer);
    }
  }

  public class PublicParametizedConstructorRequiringConverterTestClass
  {
    private readonly NameContainer _nameContainer;

    public PublicParametizedConstructorRequiringConverterTestClass(NameContainer nameParameter)
    {
      _nameContainer = nameParameter;
    }

    public NameContainer Name
    {
      get { return _nameContainer; }
    }
  }

  public class PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass
  {
    private readonly NameContainer _nameContainer;

    public PublicParametizedConstructorRequiringConverterWithParameterAttributeTestClass([JsonConverter(typeof(NameContainerConverter))] NameContainer nameParameter)
    {
      _nameContainer = nameParameter;
    }

    public NameContainer Name
    {
      get { return _nameContainer; }
    }
  }

  public class PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass
  {
    private readonly NameContainer _nameContainer;

    public PublicParametizedConstructorRequiringConverterWithPropertyAttributeTestClass(NameContainer name)
    {
      _nameContainer = name;
    }

    [JsonConverter(typeof(NameContainerConverter))]
    public NameContainer Name
    {
      get { return _nameContainer; }
    }
  }
}
