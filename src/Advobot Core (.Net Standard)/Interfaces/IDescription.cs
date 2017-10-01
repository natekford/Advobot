namespace Advobot.Interfaces
{
	/// <summary>
	/// Signifies the object has a name and description.
	/// </summary>
	public interface IDescription
	{
		string Name { get; }
		string Description { get; }
	}
}
