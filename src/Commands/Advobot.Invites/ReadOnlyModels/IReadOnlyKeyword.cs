using Advobot.Invites.Relationships;

namespace Advobot.Invites.ReadOnlyModels
{
	public interface IReadOnlyKeyword : IGuildChild
	{
		string Word { get; }
	}
}