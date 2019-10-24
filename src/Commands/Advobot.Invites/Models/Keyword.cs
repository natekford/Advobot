using Advobot.Databases.Relationships;
using Advobot.Invites.ReadOnlyModels;

using Discord;

namespace Advobot.Invites.Models
{
	public sealed class Keyword : IReadOnlyKeyword
	{
		public string GuildId { get; set; }
		public string Word { get; set; }

		ulong IGuildChild.GuildId => ulong.Parse(GuildId);

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