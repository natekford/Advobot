namespace Advobot.Interfaces;

/// <summary>
/// Interface indicating the object has a name.
/// </summary>
public interface INameable
{
	/// <summary>
	/// The name of the object.
	/// </summary>
	string Name { get; }
}