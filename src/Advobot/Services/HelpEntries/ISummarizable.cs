namespace Advobot.Services.HelpEntries;

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