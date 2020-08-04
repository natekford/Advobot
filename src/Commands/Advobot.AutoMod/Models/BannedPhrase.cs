using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

using AdvorangesUtils;

namespace Advobot.AutoMod.Models
{
	public sealed class BannedPhrase : IReadOnlyBannedPhrase
	{
		public bool Contains { get; set; }
		public string GuildId { get; set; } = null!;
		public bool IsRegex { get; set; }
		public string Phrase { get; set; } = null!;
		public PunishmentType PunishmentType { get; set; }

		ulong IGuildChild.GuildId => GuildId.ToId();

		public bool IsMatch(string content)
		{
			if (IsRegex)
			{
				return RegexUtils.IsMatch(content, Phrase);
			}

			return Contains ? content.CaseInsContains(Phrase) : content.CaseInsEquals(Phrase);
		}
	}
}