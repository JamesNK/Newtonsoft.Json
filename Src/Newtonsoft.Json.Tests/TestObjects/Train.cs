namespace Newtonsoft.Json.Tests.TestObjects
{
    public class Train : Vehicle
    {
        public int NumberOfCars { get; set; }
        public Train(string registration) : base(registration)
        {
        }
    }
}