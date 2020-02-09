using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Databases.Relationships;
using Advobot.Services.GuildSettings.Settings;
using Advobot.Utilities;

namespace Advobot.AutoMod.Models
{
	public sealed class BannedPhrasePunishment : IReadOnlyBannedPhrasePunishment
	{
		public string GuildId { get; set; } = null!;
		public int Instances { get; set; }
		public Punishment Punishment { get; set; }
		public ulong? RoleId { get; set; }
		public int? Time { get; set; }

		ulong IGuildChild.GuildId => GuildId.ToId();
	}
}