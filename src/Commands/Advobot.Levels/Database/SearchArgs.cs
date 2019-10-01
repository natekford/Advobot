using Discord.Commands;

namespace Advobot.Levels.Database
{
	[NamedArgumentType]
	public sealed class SearchArgs : ISearchArgs
	{
		public ulong? ChannelId { get; set; }
		public ulong? GuildId { get; set; }
		public ulong? UserId { get; set; }

		string? ISearchArgs.ChannelId => ChannelId?.ToString();
		string? ISearchArgs.GuildId => GuildId?.ToString();
		string? ISearchArgs.UserId => UserId?.ToString();

		public SearchArgs()
		{
		}

		public SearchArgs(ulong? userId = null, ulong? guildId = null, ulong? channelId = null)
		{
			UserId = userId;
			GuildId = guildId;
			ChannelId = channelId;
		}
	}
}