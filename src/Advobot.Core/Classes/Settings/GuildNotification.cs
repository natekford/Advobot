using System.Threading.Tasks;
using Advobot.Classes.Attributes;
using Advobot.Interfaces;
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
	public class GuildNotification : IGuildSetting
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
		public string Content { get; private set; }
		/// <summary>
		/// The embed to send with the message.
		/// </summary>
		[JsonProperty]
		public CustomEmbed CustomEmbed { get; private set; }
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
		public async Task SendAsync(SocketGuild guild, IUser user)
		{
			var content = Content
				.CaseInsReplace(USER_MENTION, user?.Mention ?? "Invalid User")
				.CaseInsReplace(USER_STRING, user?.Format() ?? "Invalid User");

			await MessageUtils.SendMessageAsync(guild.GetTextChannel(ChannelId), content, CustomEmbed?.BuildWrapper()).CAF();
		}

		/// <inheritdoc />
		public override string ToString()
			=> $"**Channel:** `{ChannelId}`\n**Content:** `{Content}`\n{CustomEmbed}";
		/// <inheritdoc />
		public string ToString(SocketGuild guild)
			=> $"**Channel:** `{guild.GetTextChannel(ChannelId).Format()}`\n**Content:** `{Content}`\n{CustomEmbed}";
	}
}