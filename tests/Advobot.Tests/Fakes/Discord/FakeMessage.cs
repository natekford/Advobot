using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;

using Discord;

using static Discord.MentionUtils;

namespace Advobot.Tests.Fakes.Discord;

public class FakeMessage(FakeMessageChannel channel, FakeUser author, string content)
	: FakeSnowflake, IMessage
{
	public MessageActivity Activity => throw new NotImplementedException();
	public MessageApplication Application => throw new NotImplementedException();
	public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();
	public string CleanContent => throw new NotImplementedException();
	public IReadOnlyCollection<IMessageComponent> Components => throw new NotImplementedException();
	public string Content { get; set; } = content;
	public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();
	public IReadOnlyCollection<IEmbed> Embeds => throw new NotImplementedException();
	public FakeUser FakeAuthor { get; } = author;
	public FakeMessageChannel FakeChannel { get; } = channel;
	public MessageFlags? Flags { get; set; }
	public IMessageInteraction Interaction => throw new NotImplementedException();
	public bool IsPinned => throw new NotImplementedException();
	public bool IsSuppressed => throw new NotImplementedException();
	public bool IsTTS => throw new NotImplementedException();
	public IReadOnlyCollection<ulong> MentionedChannelIds => Content.GetMentions(TryParseChannel);
	public bool MentionedEveryone => throw new NotImplementedException();
	public IReadOnlyCollection<ulong> MentionedRoleIds => Content.GetMentions(TryParseRole);
	public IReadOnlyCollection<ulong> MentionedUserIds => Content.GetMentions(TryParseUser);
	public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();
	public MessageReference Reference => throw new NotImplementedException();
	public MessageRoleSubscriptionData RoleSubscriptionData => throw new NotImplementedException();
	public MessageSource Source => throw new NotImplementedException();
	public IReadOnlyCollection<IStickerItem> Stickers => throw new NotImplementedException();
	public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();
	public IThreadChannel Thread => throw new NotImplementedException();
	public DateTimeOffset Timestamp => CreatedAt;
	public MessageType Type => throw new NotImplementedException();
	IUser IMessage.Author => FakeAuthor;
	IMessageChannel IMessage.Channel => FakeChannel;

	public Task AddReactionAsync(IEmote emote, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task DeleteAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetReactionUsersAsync(IEmote emoji, int limit, RequestOptions options = null, ReactionType type = ReactionType.Normal)
		=> throw new NotImplementedException();

	public Task RemoveAllReactionsAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task RemoveAllReactionsForEmoteAsync(IEmote emote, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task RemoveReactionAsync(IEmote emote, IUser user, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task RemoveReactionAsync(IEmote emote, ulong userId, RequestOptions? options = null)
		=> throw new NotImplementedException();
}