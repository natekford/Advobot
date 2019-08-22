namespace Advobot.Services.HelpEntries
{
	/// <summary>
	/// Describes what an object does.
	/// </summary>
	public interface ISummarizable
	{
		/// <summary>
		/// Describes what this object does.
		/// </summary>
		string Summary { get; }
	}
}