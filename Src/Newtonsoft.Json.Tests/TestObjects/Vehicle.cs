namespace Newtonsoft.Json.Tests.TestObjects
{
    public abstract class Vehicle
    {
        public string Registration { get; }

        protected Vehicle(string registration)
        {
            Registration = registration;
        }
    }
}