using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;

using Discord;

using static Discord.MentionUtils;

namespace Advobot.Tests.Fakes.Discord;

public class FakeMessage : FakeSnowflake, IMessage
{
	public MessageActivity Activity => throw new NotImplementedException();
	public MessageApplication Application => throw new NotImplementedException();
	public IReadOnlyCollection<IAttachment> Attachments => throw new NotImplementedException();
	public MessageCallData? CallData => throw new NotImplementedException();
	public string CleanContent => throw new NotImplementedException();
	public IReadOnlyCollection<IMessageComponent> Components { get; set; } = [];
	public string Content { get; set; }
	public DateTimeOffset? EditedTimestamp => throw new NotImplementedException();
	public IReadOnlyCollection<IEmbed> Embeds { get; set; } = [];
	public FakeUser FakeAuthor { get; }
	public FakeMessageChannel FakeChannel { get; }
	public MessageFlags? Flags { get; set; }
	public IMessageInteraction Interaction => throw new NotImplementedException();
	public bool IsPinned => throw new NotImplementedException();
	public bool IsSuppressed => throw new NotImplementedException();
	public bool IsTTS { get; set; }
	public IReadOnlyCollection<ulong> MentionedChannelIds => Content.GetMentions(TryParseChannel);
	public bool MentionedEveryone => throw new NotImplementedException();
	public IReadOnlyCollection<ulong> MentionedRoleIds => Content.GetMentions(TryParseRole);
	public IReadOnlyCollection<ulong> MentionedUserIds => Content.GetMentions(TryParseUser);
	public PurchaseNotification PurchaseNotification => throw new NotImplementedException();
	public IReadOnlyDictionary<IEmote, ReactionMetadata> Reactions => throw new NotImplementedException();
	public MessageReference Reference { get; set; }
	public MessageRoleSubscriptionData RoleSubscriptionData => throw new NotImplementedException();
	public MessageSource Source => throw new NotImplementedException();
	public IReadOnlyCollection<IStickerItem> Stickers { get; set; } = [];
	public IReadOnlyCollection<ITag> Tags => throw new NotImplementedException();
	public IThreadChannel Thread => throw new NotImplementedException();
	public DateTimeOffset Timestamp => CreatedAt;
	public MessageType Type => throw new NotImplementedException();
	IUser IMessage.Author => FakeAuthor;
	IMessageChannel IMessage.Channel => FakeChannel;

	public FakeMessage(FakeMessageChannel channel, FakeUser author, string content)
	{
		FakeChannel = channel;
		FakeChannel.FakeMessages.Add(this);
		FakeAuthor = author;
		Content = content;
	}

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