namespace Advobot.Services.HelpEntries;

/// <summary>
/// An object that has a name.
/// </summary>
public interface INameable
{
	/// <summary>
	/// The name of this object.
	/// </summary>
	string Name { get; }
}