using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using Advobot.Core.Utilities;
using AdvorangesUtils;
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
		public string Content { get; }
		/// <summary>
		/// The title to user in the embed.
		/// </summary>
		[JsonProperty]
		public string Title { get; }
		/// <summary>
		/// The description to user in the embed.
		/// </summary>
		[JsonProperty]
		public string Description { get; }
		/// <summary>
		/// The thumbnail url to use in the embed.
		/// </summary>
		[JsonProperty]
		public string ThumbUrl { get; }
		/// <summary>
		/// The channel to send the message to.
		/// </summary>
		[JsonProperty]
		public ulong ChannelId { get; set; }
		/// <summary>
		/// The embed to send.
		/// </summary>
		[JsonIgnore]
		public EmbedWrapper Embed { get; }

		/// <summary>
		/// Creates an instance of guild notification.
		/// </summary>
		public GuildNotification() { }
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
		/// <summary>
		/// Uses user input to create an instance of guild notification.
		/// </summary>
		/// <param name="content"></param>
		/// <param name="title"></param>
		/// <param name="description"></param>
		/// <param name="thumbUrl"></param>
		/// <param name="channel"></param>
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
				await MessageUtils.SendMessageAsync(guild.GetTextChannel(ChannelId), content, Embed).CAF();
			}
			else
			{
				await MessageUtils.SendMessageAsync(guild.GetTextChannel(ChannelId), content).CAF();
			}
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return $"**Channel:** `{ChannelId}`\n" +
				$"**Content:** `{Content}`\n" +
				$"**Title:** `{Title}`\n" +
				$"**Description:** `{Description}`\n" +
				$"**Thumbnail:** `{ThumbUrl}`\n";
		}
		/// <inheritdoc />
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