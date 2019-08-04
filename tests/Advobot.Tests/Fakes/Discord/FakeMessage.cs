using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Utilities;
using Discord;
using static Discord.MentionUtils;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeMessage : FakeSnowflake, IMessage
	{
		public MessageType Type => throw new NotImplementedException();
		public MessageSource Source => throw new NotImplementedException();
		public bool IsTTS => throw new NotImplementedException();
		public bool IsPinned => throw new NotImplementedException();
		public bool IsSuppressed => throw new NotImplementedException();
		public string Content { get; set; }
		public DateTimeOffset Timestamp => CreatedAt;
		public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();
		public FakeMessageChannel Channel { get; }
		public FakeUser Author { get; }
		public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();
		public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();
		public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();
		public IReadOnlyCollection<ulong> MentionedChannelIds => Content.GetMentions(TryParseChannel);
		public IReadOnlyCollection<ulong> MentionedRoleIds => Content.GetMentions(TryParseRole);
		public IReadOnlyCollection<ulong> MentionedUserIds => Content.GetMentions(TryParseUser);
		public MessageActivity Activity => throw new NotImplementedException();
		public MessageApplication Application => throw new NotImplementedException();

		public FakeMessage(FakeMessageChannel channel, FakeUser author, string content)
		{
			Channel = channel;
			Author = author;
			Content = content;
		}

		public Task DeleteAsync(RequestOptions options = null)
			=> throw new NotImplementedException();

		IMessageChannel IMessage.Channel => Channel;
		IUser IMessage.Author => Author;
	}
}
