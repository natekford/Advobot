using Advobot.Invites.ReadOnlyModels;

using Discord;

namespace Advobot.Invites.Models
{
	public sealed class Keyword : IReadOnlyKeyword
	{
		public ulong GuildId { get; set; }
		public string Word { get; set; }

		public Keyword()
		{
			Word = "";
		}

		public Keyword(IGuild guild, string word)
		{
			GuildId = guild.Id;
			Word = word;
		}
	}
}