namespace Advobot.Services.Help;

/// <summary>
/// Contains information about a command precondition.
/// </summary>
public interface IHelpPrecondition : IHelpItem
{
	/// <summary>
	/// The group this precondition belongs to.
	/// </summary>
	string Group { get; }
}