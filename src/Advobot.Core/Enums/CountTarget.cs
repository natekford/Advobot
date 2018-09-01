namespace Advobot.Enums
{
	/// <summary>
	/// Indicates whether when searching for a number to look at numbers exactly equal, below, or above.
	/// </summary>
	public enum CountTarget
	{
		/// <summary>
		/// Valid results are results that are the same.
		/// </summary>
		Equal,
		/// <summary>
		/// Valid results are results that are below.
		/// </summary>
		Below,
		/// <summary>
		/// Valid results are results that are above.
		/// </summary>
		Above,
	}
}
