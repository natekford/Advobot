using Advobot.Settings.ReadOnlyModels;

namespace Advobot.Settings.Models
{
	public sealed class GuildSettings : IReadOnlyGuildSettings
	{
		public string? Culture { get; set; }
		public ulong GuildId { get; set; }
		public ulong MuteRoleId { get; set; }
		public string? Prefix { get; set; }

		public GuildSettings()
		{
		}

		public GuildSettings(IReadOnlyGuildSettings other)
		{
			Culture = other.Culture;
			GuildId = other.GuildId;
			MuteRoleId = other.MuteRoleId;
			Prefix = other.Prefix;
		}
	}
}