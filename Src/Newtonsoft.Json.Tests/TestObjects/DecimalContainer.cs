namespace Newtonsoft.Json.Tests.TestObjects
{
    [JsonConverter(typeof(DecimalContainerConverter))]
    public class DecimalContainer
    {
        public decimal Value { get; set; }
    }
}
