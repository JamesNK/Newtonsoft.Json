using Newtonsoft.Json.Utilities;

namespace Newtonsoft.Json.Linq
{
    /// <summary>
    /// Specifies the settings used when comparing JSON.
    /// </summary>
    public class JsonCompareSettings
    {
        /// <summary>
        /// EPS value used for floating point comparison
        /// </summary>
        public double Epsilon { get; set; } = MathUtils.DefaultEpsilon;
    }
}
