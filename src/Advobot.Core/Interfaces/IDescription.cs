namespace Advobot.Core.Interfaces
{
	/// <summary>
	/// An object which has a name and a description.
	/// </summary>
	public interface IDescription
	{
		string Name { get; }
		string Description { get; }
	}
}
