using Advobot.AutoMod.ReadOnlyModels;
using Advobot.AutoMod.Utils;

using Discord;

namespace Advobot.AutoMod.Models
{
	public sealed class ChannelSettings : IReadOnlyChannelSettings
	{
		public ulong ChannelId { get; set; }
		public ulong GuildId { get; set; }
		public bool IsImageOnly { get; set; }

		public bool IsAllowed(IMessage message)
			=> !IsImageOnly || message.GetImageCount() > 0;
	}
}