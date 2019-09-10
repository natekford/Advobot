using Advobot.Invites.ReadOnlyModels;

using Discord;

namespace Advobot.Invites.Models
{
	public sealed class Keyword : IReadOnlyKeyword
	{
		public string GuildId { get; set; }
		public string Word { get; set; }

		public Keyword()
		{
			GuildId = "";
			Word = "";
		}

		public Keyword(IGuild guild, string word)
		{
			GuildId = guild.Id.ToString();
			Word = word;
		}
	}
}