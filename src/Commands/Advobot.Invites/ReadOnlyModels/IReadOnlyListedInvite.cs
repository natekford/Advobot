using Advobot.Databases.Relationships;

namespace Advobot.Invites.ReadOnlyModels
{
	public interface IReadOnlyListedInvite : IGuildChild
	{
		string Code { get; }
		bool HasGlobalEmotes { get; }
		long LastBumped { get; }
		int MemberCount { get; }
		string Name { get; }
	}
}