using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Punishments;

using AdvorangesUtils;

namespace Advobot.AutoMod.Models
{
	public sealed class BannedPhrase : IReadOnlyBannedPhrase
	{
		public ulong GuildId { get; set; }
		public bool IsContains { get; set; }
		public bool IsName { get; set; }
		public bool IsRegex { get; set; }
		public string Phrase { get; set; }
		public PunishmentType PunishmentType { get; set; }

		public BannedPhrase()
		{
			Phrase = "";
		}

		public BannedPhrase(IReadOnlyBannedPhrase other)
		{
			GuildId = other.GuildId;
			IsContains = other.IsContains;
			IsRegex = other.IsRegex;
			IsName = other.IsName;
			Phrase = other.Phrase;
			PunishmentType = other.PunishmentType;
		}

		public bool IsMatch(string content)
		{
			if (IsRegex)
			{
				return RegexUtils.IsMatch(content, Phrase);
			}
			else if (IsContains)
			{
				return content.CaseInsContains(Phrase);
			}
			return content.CaseInsEquals(Phrase);
		}
	}
}