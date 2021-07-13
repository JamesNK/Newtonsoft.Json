namespace Newtonsoft.Json
{
    /// <summary>
    /// Options for comparing json values
    /// </summary>
    public class JsonCompareOptions
    {
        /// <summary>
        /// EPS value used for floating point comparison
        /// </summary>
        public static double Epsilon { get; set; } = 2.2204460492503131E-16;
    }
}
