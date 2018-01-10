using Advobot.Core.Utilities.Formatting;
using Discord;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace Advobot.Core.Classes
{
	public struct FormattedMessage
	{
		public readonly IMessage Message;
		public readonly ImmutableArray<string> Embeds;
		public readonly ImmutableArray<string> Attachments;
		public readonly string HeaderWithNoMentions;
		public readonly string HeaderWithMentions;
		public readonly string Content;

		public FormattedMessage(IMessage message)
		{
			Message = message;
			Embeds = Message.Embeds.Where(x => x.Description != null || x.Url != null || x.Image.HasValue).Select((x, index) =>
			{
				var embed = new StringBuilder($"Embed {index + 1}: {x.Description ?? "No description"}");
				if (x.Url != null)
				{
					embed.Append($" URL: {x.Url}");
				}
				if (x.Image.HasValue)
				{
					embed.Append($" IURL: {x.Image.Value.Url}");
				}
				return embed.ToString();
			}).ToImmutableArray();
			Attachments = Message.Attachments.Select(x => x.Filename).ToImmutableArray();

			var text = String.IsNullOrEmpty(message.Content) ? "Empty message content" : message.Content;
			var time = message.CreatedAt.ToString("HH:mm:ss");

			var user = message.Author.FormatUser();
			var channel = message.Channel.FormatChannel();
			HeaderWithNoMentions = $"`[{time}]` `{message.Id}` {user} IN {channel}".EscapeBackTicks();

			var userMention = message.Author.Mention;
			var channelMention = (message.Channel as ITextChannel).Mention;
			HeaderWithMentions = $"`[{time}]` `{message.Id}` {userMention} IN {channelMention}".EscapeBackTicks();

			var content = new StringBuilder($"```\n{text.EscapeBackTicks()}");
			foreach (var embed in Embeds)
			{
				content.AppendLineFeed(embed.EscapeBackTicks());
			}
			if (Attachments.Any())
			{
				content.AppendLineFeed($" + {String.Join(" + ", Attachments).EscapeBackTicks()}");
			}
			Content = content.Append("```").ToString();
		}

		public override string ToString() => ToString(true);
		public string ToString(bool withMentions) => $"{(withMentions ? HeaderWithMentions : HeaderWithNoMentions)}\n{Content}";
	}
}
