using Discord.Commands;

namespace Advobot.Levels.Database
{
	[NamedArgumentType]
	public sealed class SearchArgs : ISearchArgs
	{
		public string? ChannelId => ChannelIdValue?.ToString();
		public ulong? ChannelIdValue { get; set; }
		public string? GuildId => GuildIdValue?.ToString();
		public ulong? GuildIdValue { get; set; }
		public string? UserId => UserIdValue?.ToString();
		public ulong? UserIdValue { get; set; }

		public SearchArgs()
		{
		}

		public SearchArgs(ulong? userId = null, ulong? guildId = null, ulong? channelId = null)
		{
			UserIdValue = userId;
			GuildIdValue = guildId;
			ChannelIdValue = channelId;
		}
	}
}