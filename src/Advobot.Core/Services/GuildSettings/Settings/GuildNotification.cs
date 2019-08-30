using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Classes;
using Advobot.Formatting;
using Advobot.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.WebSocket;

using Newtonsoft.Json;

namespace Advobot.Services.GuildSettings.Settings
{
	/// <summary>
	/// Notification that gets sent whenever certain events happen depending on what is linked to this notification.
	/// </summary>
	public sealed class GuildNotification : IGuildFormattable
	{
		/// <summary>
		/// What to replace with a user mention.
		/// </summary>
		public const string USER_MENTION = "%USERMENTION%";

		/// <summary>
		/// What to replace with a formatted user.
		/// </summary>
		public const string USER_STRING = "%USER%";

		/// <summary>
		/// The channel to send the message to.
		/// </summary>
		[JsonProperty]
		public ulong ChannelId { get; set; }

		/// <summary>
		/// The content to send in the message.
		/// </summary>
		[JsonProperty]
		public string? Content { get; set; }

		/// <summary>
		/// The embed to send with the message.
		/// </summary>
		[JsonProperty]
		public CustomEmbed? CustomEmbed { get; set; }

		/// <inheritdoc />
		public string Format(SocketGuild? guild = null)
		{
			var channel = guild?.GetTextChannel(ChannelId)?.Format() ?? ChannelId.ToString();
			return $"**Channel:** `{channel}`\n**Content:** `{Content}`\n{CustomEmbed}";
		}

		/// <inheritdoc />
		public IDiscordFormattableString GetFormattableString()
		{
			return new Dictionary<string, object?>
			{
				{ "Channel", ChannelId },
				{ "Content", Content },
			}.ToDiscordFormattableStringCollection();
		}

		/// <summary>
		/// Sends the notification to the channel.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task SendAsync(IGuild guild, IUser? user)
		{
			if (ChannelId == 0)
			{
				return;
			}

			var content = Content
				?.CaseInsReplace(USER_MENTION, user?.Mention ?? "Invalid User")
				?.CaseInsReplace(USER_STRING, user?.Format() ?? "Invalid User");

			var channel = await guild.GetTextChannelAsync(ChannelId).CAF();
			await MessageUtils.SendMessageAsync(channel, content, CustomEmbed?.BuildWrapper()).CAF();
		}
	}
}