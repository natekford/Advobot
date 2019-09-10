using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;

using Discord;

using static Discord.MentionUtils;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeMessage : FakeSnowflake, IMessage
	{
		public MessageActivity Activity => throw new NotImplementedException();
		public MessageApplication Application => throw new NotImplementedException();
		public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();
		public string Content { get; set; }
		public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();
		public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();
		public FakeUser FakeAuthor { get; }
		public FakeMessageChannel FakeChannel { get; }
		public bool IsPinned => throw new NotImplementedException();
		public bool IsSuppressed => throw new NotImplementedException();
		public bool IsTTS => throw new NotImplementedException();
		public IReadOnlyCollection<ulong> MentionedChannelIds => Content.GetMentions(TryParseChannel);
		public IReadOnlyCollection<ulong> MentionedRoleIds => Content.GetMentions(TryParseRole);
		public IReadOnlyCollection<ulong> MentionedUserIds => Content.GetMentions(TryParseUser);
		public MessageSource Source => throw new NotImplementedException();
		public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();
		public DateTimeOffset Timestamp => CreatedAt;
		public MessageType Type => throw new NotImplementedException();
		IUser IMessage.Author => FakeAuthor;

		IMessageChannel IMessage.Channel => FakeChannel;

		public FakeMessage(FakeMessageChannel channel, FakeUser author, string content)
		{
			FakeChannel = channel;
			FakeAuthor = author;
			Content = content;
		}

		public Task DeleteAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
	}
}