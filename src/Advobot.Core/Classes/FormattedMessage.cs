using Advobot.Core.Actions.Formatting;
using Discord;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes
{
	public class FormattedMessage
	{
		public readonly IMessage Message;
		public readonly ImmutableList<string> Embeds;
		public readonly ImmutableList<string> Attachments;
		public readonly string Text;
		public readonly string Time;
		public readonly string User;
		public readonly string UserMention;
		public readonly string Channel;
		public readonly string ChannelMention;
		public readonly string MessageId;
		public readonly string Header;
		public readonly string Content;

		public FormattedMessage(IMessage message)
		{
			this.Message = message;
			this.Embeds = FormatEmbeds();
			this.Attachments = FormatAttachments();
			this.Text = String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content;
			this.Time = message.CreatedAt.ToString("HH:mm:ss");
			this.User = message.Author.FormatUser();
			this.UserMention = message.Author.Mention;
			this.Channel = message.Channel.FormatChannel();
			this.ChannelMention = (message.Channel as ITextChannel).Mention;
			this.MessageId = message.Id.ToString();

			this.Header = FormatHeader(true);
			this.Content = FormatContent();
		}

		private ImmutableList<string> FormatEmbeds()
			=> this.Message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue).Select((x, index) =>
			{
				var sb = new StringBuilder($"Embed {index + 1}: {x.Description ?? "No description"}");
				if (x.Url != null)
				{
					sb.Append($" URL: {x.Url}");
				}
				if (x.Image.HasValue)
				{
					sb.Append($" IURL: {x.Image.Value.Url}");
				}
				return sb.ToString();
			}).ToImmutableList();
		private ImmutableList<string> FormatAttachments() => this.Message.Attachments.Select(x => x.Filename).ToImmutableList();
		private string FormatHeader(bool withMentions)
		{
			var user = withMentions ? this.UserMention : this.User;
			var channel = withMentions ? this.ChannelMention : this.Channel;
			return $"`[{this.Time}]` `{this.MessageId}` {user.EscapeBackTicks()} IN {channel.EscapeBackTicks()}";
		}
		private string FormatContent()
		{
			var sb = new StringBuilder($"```\n{this.Text.EscapeBackTicks()}");
			foreach (var embed in this.Embeds)
			{
				sb.AppendLineFeed(embed.EscapeBackTicks());
			}
			if (this.Attachments.Any())
			{
				sb.AppendLineFeed($" + {String.Join(" + ", this.Attachments).EscapeBackTicks()}");
			}
			return sb.Append("```").ToString();
		}

		public string ToString(bool withMentions) => $"{(withMentions ? this.Header : FormatHeader(false))}\n{this.Content}";
	}
}
