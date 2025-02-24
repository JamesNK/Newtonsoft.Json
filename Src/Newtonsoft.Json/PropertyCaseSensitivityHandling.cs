namespace Newtonsoft.Json
{
	/// <summary>
	/// Specifies the case sensitivity setting that JSON deserialization should follow, when matching property names. 
	/// </summary>
	public enum PropertyCaseSensitivityHandling
	{
		/// <summary>
		/// JSON deserialization will match property names using case-sensitive comparisons.
		/// </summary>
		CaseSensitive = 0,
		/// <summary>
		/// JSON deserialization will match property names using case-insensitive comparisons.
		/// </summary>
		CaseInsensitive = 1
	}
}