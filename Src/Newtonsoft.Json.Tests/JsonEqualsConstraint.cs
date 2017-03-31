using NUnit.Framework.Constraints;

namespace Newtonsoft.Json.Tests
{
    public class JsonEqualsConstraint : EqualConstraint
    {
        public JsonEqualsConstraint(object expected)
            : base(ToJson(expected))
        {
        }

        private static object ToJson(object value)
        {
            if (ReferenceEquals(value, null))
            {
                return null;
            }
            if (value is string)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(Newtonsoft.Json.JsonConvert.DeserializeObject((string)value), Formatting.Indented);
            }
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, Formatting.Indented);
        }

        public override bool Matches(object actual)
        {
            return base.Matches(ToJson(actual));
        }
    }
}
