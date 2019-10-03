using Discord.Commands;

namespace Advobot.Levels.Database
{
	[NamedArgumentType]
	public sealed class SearchArgs : ISearchArgs
	{
		public ulong? Channel { get; set; }
		public ulong? Guild { get; set; }
		public ulong? User { get; set; }

		string? ISearchArgs.ChannelId => Channel?.ToString();
		ulong? ISearchArgs.ChannelIdValue => Channel;
		string? ISearchArgs.GuildId => Guild?.ToString();
		ulong? ISearchArgs.GuildIdValue => Guild;
		string? ISearchArgs.UserId => User?.ToString();
		ulong? ISearchArgs.UserIdValue => User;

		public SearchArgs()
		{
		}

		public SearchArgs(ulong? userId = null, ulong? guildId = null, ulong? channelId = null)
		{
			User = userId;
			Guild = guildId;
			Channel = channelId;
		}
	}
}