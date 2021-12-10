namespace Advobot.Settings;

/// <summary>
/// Indicates the object can be saved to a file.
/// </summary>
public interface ISavable
{
	/// <summary>
	/// Serializes this object and then overwrites the file.
	/// </summary>
	void Save();
}