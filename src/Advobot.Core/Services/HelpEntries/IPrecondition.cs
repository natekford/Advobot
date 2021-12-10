namespace Advobot.Services.HelpEntries;

/// <summary>
/// Contains information about a command precondition.
/// </summary>
public interface IPrecondition : ISummarizable
{
	/// <summary>
	/// The group this precondition belongs to.
	/// </summary>
	string Group { get; }
}