using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Classes.Formatting;
using Advobot.Utilities;
using AdvorangesUtils;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace Advobot.Classes.Settings
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
		/// The content to send in the message.
		/// </summary>
		[JsonProperty]
		public string? Content { get; set; }
		/// <summary>
		/// The embed to send with the message.
		/// </summary>
		[JsonProperty]
		public CustomEmbed? CustomEmbed { get; set; }
		/// <summary>
		/// The channel to send the message to.
		/// </summary>
		[JsonProperty]
		public ulong ChannelId { get; set; }

		/// <summary>
		/// Sends the notification to the channel.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="user"></param>
		/// <returns></returns>
		public Task SendAsync(SocketGuild guild, IUser? user)
		{
			if (ChannelId == 0)
			{
				return Task.CompletedTask;
			}

			var content = Content
                ?.CaseInsReplace(USER_MENTION, user?.Mention ?? "Invalid User")
				?.CaseInsReplace(USER_STRING, user?.Format() ?? "Invalid User");

			return MessageUtils.SendMessageAsync(guild.GetTextChannel(ChannelId), content, CustomEmbed?.BuildWrapper());
		}
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
	}
}