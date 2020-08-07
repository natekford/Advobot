using Advobot.AutoMod.ReadOnlyModels;
using Advobot.AutoMod.Utils;
using Advobot.Databases.Relationships;
using Advobot.Utilities;

using Discord;

namespace Advobot.AutoMod.Models
{
	public sealed class ChannelSettings : IReadOnlyChannelSettings
	{
		public string ChannelId { get; set; } = null!;
		public string GuildId { get; set; } = null!;
		public bool IsImageOnly { get; set; }
		ulong IChannelChild.ChannelId => ChannelId.ToId();
		ulong IGuildChild.GuildId => GuildId.ToId();

		public bool IsAllowed(IMessage message)
			=> !IsImageOnly || message.GetImageCount() > 0;
	}
}