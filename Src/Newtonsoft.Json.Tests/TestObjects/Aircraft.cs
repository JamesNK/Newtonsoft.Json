namespace Newtonsoft.Json.Tests.TestObjects
{
    public enum AircraftType
    {
        FixedWing,
        RotaryWing
    }

    public class Aircraft : Vehicle
    {
        public AircraftType Type { get; }

        public Aircraft(AircraftType type, string registration) : base(registration)
        {
            Type = type;
        }
    }
}