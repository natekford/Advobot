using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Advobot.Core.Classes.Settings
{
	/// <summary>
	/// Notification that gets sent whenever certain events happen depending on what is linked to this notification.
	/// </summary>
	public class GuildNotification : IGuildSetting
	{
		public const string USER_MENTION = "%USERMENTION%";
		public const string USER_STRING = "%USER%";

		[JsonProperty]
		public string Content { get; }
		[JsonProperty]
		public string Title { get; }
		[JsonProperty]
		public string Description { get; }
		[JsonProperty]
		public string ThumbUrl { get; }
		[JsonProperty]
		public ulong ChannelId { get; set; }
		[JsonIgnore]
		public EmbedWrapper Embed { get; }

		[JsonConstructor]
		internal GuildNotification(string content, string title, string description, string thumbUrl, ulong channelId)
		{
			Content = content;
			Title = title;
			Description = description;
			ThumbUrl = thumbUrl;
			ChannelId = channelId;
			if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbUrl)))
			{
				Embed = new EmbedWrapper
				{
					Title = title,
					Description = description,
					ThumbnailUrl = thumbUrl
				};
			}
		}
		[NamedArgumentConstructor]
		public GuildNotification(
			[NamedArgument] string content,
			[NamedArgument] string title,
			[NamedArgument] string description,
			[NamedArgument] string thumbUrl,
			ITextChannel channel) : this(content, title, description, thumbUrl, channel.Id)
		{
			ChannelId = channel.Id;
		}
		public GuildNotification() { }

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
			//Put a zero length character in between invite links for names so the invite links will no longer embed

			if (Embed != null)
			{
				await MessageUtils.SendEmbedMessageAsync(guild.GetTextChannel(ChannelId), Embed, content).CAF();
			}
			else
			{
				await MessageUtils.SendMessageAsync(guild.GetTextChannel(ChannelId), content).CAF();
			}
		}

		public override string ToString()
		{
			return $"**Channel:** `{ChannelId}`\n" +
				$"**Content:** `{Content}`\n" +
				$"**Title:** `{Title}`\n" +
				$"**Description:** `{Description}`\n" +
				$"**Thumbnail:** `{ThumbUrl}`\n";
		}
		public string ToString(SocketGuild guild)
		{
			return $"**Channel:** `{guild.GetTextChannel(ChannelId).Format()}`\n" +
				$"**Content:** `{Content}`\n" +
				$"**Title:** `{Title}`\n" +
				$"**Description:** `{Description}`\n" +
				$"**Thumbnail:** `{ThumbUrl}`\n";
		}
	}
}