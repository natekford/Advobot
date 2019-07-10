using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Tests.Mocks
{
	public class MockMessage : IMessage
	{
		public MessageType Type => throw new NotImplementedException();
		public MessageSource Source => throw new NotImplementedException();
		public bool IsTTS => throw new NotImplementedException();
		public bool IsPinned => throw new NotImplementedException();
		public string Content { get; }
		public DateTimeOffset Timestamp => throw new NotImplementedException();
		public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();
		public IMessageChannel Channel => throw new NotImplementedException();
		public IUser Author { get; }
		public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();
		public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();
		public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();
		public IReadOnlyCollection<ulong> MentionedChannelIds { get; } = new HashSet<ulong>();
		public IReadOnlyCollection<ulong> MentionedRoleIds { get; } = new HashSet<ulong>();
		public IReadOnlyCollection<ulong> MentionedUserIds { get; } = new HashSet<ulong>();
		public MessageActivity Activity => throw new NotImplementedException();
		public MessageApplication Application => throw new NotImplementedException();
		public DateTimeOffset CreatedAt => SnowflakeUtils.FromSnowflake(Id);
		public ulong Id { get; }
		public bool IsSuppressed => throw new NotImplementedException();

		public MockMessage(IUser author, string content, ulong? id = null)
		{
			Author = author;
			Content = content;
			Id = id ?? SnowflakeUtils.ToSnowflake(DateTimeOffset.UtcNow);

			foreach (var part in content?.Split(' ') ?? Enumerable.Empty<string>())
			{
				if (MentionUtils.TryParseChannel(part, out var channelId))
				{
					((ICollection<ulong>)MentionedChannelIds).Add(channelId);
				}
				else if (MentionUtils.TryParseRole(part, out var roleId))
				{
					((ICollection<ulong>)MentionedRoleIds).Add(roleId);
				}
				else if (MentionUtils.TryParseUser(part, out var userId))
				{
					((ICollection<ulong>)MentionedUserIds).Add(userId);
				}
			}
		}

		public Task DeleteAsync(RequestOptions options = null) => throw new NotImplementedException();
	}
}
