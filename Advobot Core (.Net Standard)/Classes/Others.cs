using Advobot.Actions;
using Advobot.Enums;
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

		public void ChangeChannel(ITextChannel channel)
		{
			Channel = channel;
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

	/// <summary>
	/// Handles deleted message collection for <see cref="Modules.Log.MyLogModule.OnMessageDeleted(Cacheable{IMessage, ulong}, ISocketMessageChannel)"/>.
	/// </summary>
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
			_Messages = InList;
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

	/// <summary>
	/// Holds a <see cref="DateTime"/> object and implements <see cref="ITimeInterface"/> so certain methods can restrict generics easier.
	/// </summary>
	public struct BasicTimeInterface : ITimeInterface
	{
		private DateTime _Time;

		public BasicTimeInterface(DateTime time)
		{
			_Time = time.ToUniversalTime();
		}

		public DateTime GetTime() => _Time;
	}

	/// <summary>
	/// Basically a tuple for three bools which represent critical information.
	/// </summary>
	public struct CriticalInformation
	{
		/// <summary>
		/// True if the system is windows, false otherwise.
		/// </summary>
		public bool Windows { get; }
		/// <summary>
		/// True if the program is in console mode, false otherwise.
		/// </summary>
		public bool Console { get; }
		/// <summary>
		/// True if the bot Id held in <see cref="Properties.Settings.Path"/> does not match the current bot's Id.
		/// </summary>
		public bool FirstInstance { get; }

		public CriticalInformation(bool windows, bool console, bool firstInstance)
		{
			Windows = windows;
			Console = console;
			FirstInstance = firstInstance;
		}
	}
}