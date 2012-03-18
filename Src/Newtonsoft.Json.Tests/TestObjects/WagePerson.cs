namespace Newtonsoft.Json.Tests.TestObjects
{
  public class WagePerson : Person
  {
    [JsonProperty]
    public decimal HourlyWage { get; set; }
  }
}