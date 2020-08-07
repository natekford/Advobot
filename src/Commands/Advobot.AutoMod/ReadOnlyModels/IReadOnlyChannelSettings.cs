using Advobot.Databases.Relationships;

using Discord;

namespace Advobot.AutoMod.ReadOnlyModels
{
	public interface IReadOnlyChannelSettings : IGuildChild, IChannelChild
	{
		bool IsImageOnly { get; }

		bool IsAllowed(IMessage message);
	}
}