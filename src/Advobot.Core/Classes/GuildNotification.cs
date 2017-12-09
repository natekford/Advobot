using Advobot.Core.Actions;
using Advobot.Core.Actions.Formatting;
using Advobot.Core.Classes.Attributes;
using Advobot.Core.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Core.Classes
{
	/// <summary>
	/// Notification that gets sent whenever certain events happen depending on what <see cref="GuildNotificationType"/> is linked to this notification.
	/// </summary>
	public class GuildNotification : ISetting
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
		public ulong ChannelId { get; }
		[JsonIgnore]
		public EmbedWrapper Embed { get; }
		[JsonIgnore]
		public ITextChannel Channel { get; private set; }

		[JsonConstructor]
		internal GuildNotification(string content, string title, string description, string thumbUrl, ulong channelID)
		{
			this.Content = content;
			this.Title = title;
			this.Description = description;
			this.ThumbUrl = thumbUrl;
			this.ChannelId = channelID;
			if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbUrl)))
			{
				this.Embed = new EmbedWrapper(title, description, null, null, null, thumbUrl);
			}
		}
		[CustomArgumentConstructor]
		public GuildNotification(
			[CustomArgument] string content,
			[CustomArgument] string title,
			[CustomArgument] string description,
			[CustomArgument] string thumbURL,
			ITextChannel channel) : this(content, title, description, thumbURL, channel.Id)
		{
			this.Channel = channel;
		}

		/// <summary>
		/// Changes the channel the notification gets sent to.
		/// </summary>
		/// <param name="channel"></param>
		public void ChangeChannel(ITextChannel channel) => this.Channel = channel;
		/// <summary>
		/// Sends the notification to the channel.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task SendAsync(IUser user)
		{
			var content = this.Content
				.CaseInsReplace(USER_MENTION, user != null ? user.Mention : "Invalid User")
				.CaseInsReplace(USER_STRING, user != null ? user.FormatUser() : "Invalid User");
			//Put a zero length character in between invite links for names so the invite links will no longer embed

			if (this.Embed != null)
			{
				await MessageActions.SendEmbedMessageAsync(this.Channel, this.Embed, content).CAF();
			}
			else
			{
				await MessageActions.SendMessageAsync(this.Channel, content).CAF();
			}
		}
		/// <summary>
		/// Sets <see cref="Channel"/> to whichever text channel on <paramref name="guild"/> has the Id <see cref="ChannelId"/>.
		/// </summary>
		/// <param name="guild"></param>
		public void PostDeserialize(SocketGuild guild) => this.Channel = guild.GetTextChannel(this.ChannelId);

		public override string ToString() => new StringBuilder()
			.AppendLineFeed($"**Channel:** `{this.Channel.FormatChannel()}`")
			.AppendLineFeed($"**Content:** `{this.Content}`")
			.AppendLineFeed($"**Title:** `{this.Title}`")
			.AppendLineFeed($"**Description:** `{this.Description}`")
			.AppendLineFeed($"**Thumbnail:** `{this.ThumbUrl}`").ToString();
		public string ToString(SocketGuild guild) => ToString();
	}
}