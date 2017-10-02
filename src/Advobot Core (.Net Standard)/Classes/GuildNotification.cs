﻿using Advobot.Actions;
using Advobot.Actions.Formatting;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Advobot.Classes
{
	/// <summary>
	/// Notification that gets sent whenever certain events happen depending on what <see cref="GuildNotificationType"/> is linked to this notification.
	/// </summary>
	public class GuildNotification : ISetting
	{
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
		public EmbedBuilder Embed { get; }
		[JsonIgnore]
		public ITextChannel Channel { get; private set; }

		[JsonConstructor]
		public GuildNotification(string content, string title, string description, string thumbUrl, ulong channelID)
		{
			Content = content;
			Title = title;
			Description = description;
			ThumbUrl = thumbUrl;
			ChannelId = channelID;
			if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbUrl)))
			{
				Embed = EmbedActions.MakeNewEmbed(title, description, null, null, null, thumbUrl);
			}
		}
		public GuildNotification(string content, string title, string description, string thumbURL, ITextChannel channel) : this(content, title, description, thumbURL, channel.Id)
		{
			Channel = channel;
		}

		/// <summary>
		/// Changes the channel the notification gets sent to.
		/// </summary>
		/// <param name="channel"></param>
		public void ChangeChannel(ITextChannel channel)
		{
			Channel = channel;
		}
		/// <summary>
		/// Sends the notification to the channel.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public async Task Send(IUser user)
		{
			var content = Content
				.CaseInsReplace(Constants.USER_MENTION, user != null ? user.Mention : "Invalid User")
				.CaseInsReplace(Constants.USER_STRING, user != null ? user.FormatUser() : "Invalid User");
			//Put a zero length character in between invite links for names so the invite links will no longer embed

			if (Embed != null)
			{
				await MessageActions.SendEmbedMessage(Channel, Embed, content);
			}
			else
			{
				await MessageActions.SendMessage(Channel, content);
			}
		}
		/// <summary>
		/// Sets <see cref="Channel"/> to whichever text channel on <paramref name="guild"/> has the Id <see cref="ChannelId"/>.
		/// </summary>
		/// <param name="guild"></param>
		public void PostDeserialize(SocketGuild guild)
		{
			Channel = guild.GetTextChannel(ChannelId);
		}

		public override string ToString()
		{
			return $"**Channel:** `{Channel.FormatChannel()}`\n**Content:** `{Content}`\n**Title:** `{Title}`\n**Description:** `{Description}`\n**Thumbnail:** `{ThumbUrl}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}
}