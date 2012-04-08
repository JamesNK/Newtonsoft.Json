namespace Newtonsoft.Json.Tests.TestObjects
{
  public class PublicParametizedConstructorWithNonPropertyParameterTestClass
  {
    private readonly string _name;

    public PublicParametizedConstructorWithNonPropertyParameterTestClass(string nameParameter)
    {
      _name = nameParameter;
    }

    public string Name
    {
      get { return _name; }
    }
  }
}
