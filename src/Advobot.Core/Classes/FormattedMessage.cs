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
			Message = message;
			Embeds = FormatEmbeds();
			Attachments = FormatAttachments();
			Text = String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content;
			Time = message.CreatedAt.ToString("HH:mm:ss");
			User = message.Author.FormatUser();
			UserMention = message.Author.Mention;
			Channel = message.Channel.FormatChannel();
			ChannelMention = (message.Channel as ITextChannel).Mention;
			MessageId = message.Id.ToString();

			Header = FormatHeader(true);
			Content = FormatContent();
		}

		private ImmutableList<string> FormatEmbeds()
		{
			return Message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue).Select((x, index) =>
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
		}
		private ImmutableList<string> FormatAttachments()
		{
			return Message.Attachments.Select(x => x.Filename).ToImmutableList();
		}
		private string FormatHeader(bool withMentions)
		{
			var user = withMentions ? UserMention : User;
			var channel = withMentions ? ChannelMention : Channel;
			return $"`[{Time}]` `{MessageId}` {user.EscapeBackTicks()} IN {channel.EscapeBackTicks()}";
		}
		private string FormatContent()
		{
			var sb = new StringBuilder($"```\n{Text.EscapeBackTicks()}");
			foreach (var embed in Embeds)
			{
				sb.AppendLineFeed(embed.EscapeBackTicks());
			}
			if (Attachments.Any())
			{
				sb.AppendLineFeed($" + {String.Join(" + ", Attachments).EscapeBackTicks()}");
			}
			return sb.Append("```").ToString();
		}

		public string ToString(bool withMentions)
		{
			return $"{(withMentions ? Header : FormatHeader(false))}\n{Content}";
		}
	}
}
