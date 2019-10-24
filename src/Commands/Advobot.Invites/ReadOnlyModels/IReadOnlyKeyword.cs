using Advobot.Databases.Relationships;

namespace Advobot.Invites.ReadOnlyModels
{
	public interface IReadOnlyKeyword : IGuildChild
	{
		string Word { get; }
	}
}