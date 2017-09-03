using Advobot.Actions;
using Advobot.Interfaces;
using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Advobot.Classes
{
	public class GuildNotification : ISetting
	{
		[JsonProperty]
		public string Content { get; }
		[JsonProperty]
		public string Title { get; }
		[JsonProperty]
		public string Description { get; }
		[JsonProperty]
		public string ThumbURL { get; }
		[JsonProperty]
		public ulong ChannelId { get; }
		[JsonIgnore]
		public EmbedBuilder Embed { get; }
		[JsonIgnore]
		public ITextChannel Channel { get; private set; }

		[JsonConstructor]
		public GuildNotification(string content, string title, string description, string thumbURL, ulong channelID)
		{
			Content = content;
			Title = title;
			Description = description;
			ThumbURL = thumbURL;
			ChannelId = channelID;
			if (!(String.IsNullOrWhiteSpace(title) && String.IsNullOrWhiteSpace(description) && String.IsNullOrWhiteSpace(thumbURL)))
			{
				Embed = EmbedActions.MakeNewEmbed(title, description, null, null, null, thumbURL);
			}
		}
		public GuildNotification(string content, string title, string description, string thumbURL, ITextChannel channel) : this(content, title, description, thumbURL, channel.Id)
		{
			Channel = channel;
		}

		public void ChangeChannel(ITextChannel channel)
		{
			Channel = channel;
		}
		public void PostDeserialize(SocketGuild guild)
		{
			Channel = guild.GetTextChannel(ChannelId);
		}

		public override string ToString()
		{
			return $"**Channel:** `{Channel.FormatChannel()}`\n**Content:** `{Content}`\n**Title:** `{Title}`\n**Description:** `{Description}`\n**Thumbnail:** `{ThumbURL}`";
		}
		public string ToString(SocketGuild guild)
		{
			return ToString();
		}
	}

	public class MessageDeletion
	{
		public CancellationTokenSource CancelToken { get; private set; }
		private List<IMessage> _Messages = new List<IMessage>();

		public void SetCancelToken(CancellationTokenSource cancelToken)
		{
			CancelToken = cancelToken;
		}
		public List<IMessage> GetList()
		{
			return _Messages.ToList();
		}
		public void SetList(List<IMessage> InList)
		{
			_Messages = InList.ToList();
		}
		public void AddToList(IMessage Item)
		{
			_Messages.Add(Item);
		}
		public void ClearList()
		{
			_Messages.Clear();
		}
	}
}