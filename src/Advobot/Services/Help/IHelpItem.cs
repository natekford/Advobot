namespace Advobot.Services.Help;

/// <summary>
/// Base interface for a help item.
/// </summary>
public interface IHelpItem
{
	/// <summary>
	/// The name of this object.
	/// </summary>
	string Name { get; }
	/// <summary>
	/// Describes what this object does.
	/// </summary>
	string Summary { get; }
}