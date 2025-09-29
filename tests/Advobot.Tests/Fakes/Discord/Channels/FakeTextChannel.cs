using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels;

public class FakeTextChannel(FakeGuild guild)
	: FakeGuildChannel(guild), ITextChannel, IIntegrationChannel
{
	public ulong? CategoryId => ProtectedCategoryId;
	public ThreadArchiveDuration DefaultArchiveDuration => throw new NotImplementedException();
	public int DefaultSlowModeInterval => throw new NotImplementedException();
	public bool IsNsfw { get; set; }
	public string Mention => $"<#{Id}>";
	public int SlowModeInterval { get; set; }
	public string Topic { get; set; }

	public Task<IInviteMetadata> CreateInviteAsync(int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions? options = null)
	{
		var invite = new FakeInviteMetadata(this, FakeGuild.FakeCurrentUser)
		{
			MaxAge = maxAge,
			MaxUses = maxUses,
			IsTemporary = isTemporary,
		};
		return Task.FromResult<IInviteMetadata>(invite);
	}

	public Task<IInviteMetadata> CreateInviteToApplicationAsync(ulong applicationId, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IInviteMetadata> CreateInviteToApplicationAsync(DefaultApplications application, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IInviteMetadata> CreateInviteToStreamAsync(IUser user, int? maxAge = 86400, int? maxUses = null, bool isTemporary = false, bool isUnique = false, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IThreadChannel> CreateThreadAsync(string name, ThreadType type = ThreadType.PublicThread, ThreadArchiveDuration autoArchiveDuration = ThreadArchiveDuration.OneDay, IMessage message = null, bool? invitable = null, int? slowmode = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IWebhook> CreateWebhookAsync(string name, Stream avatar = null, RequestOptions options = null)
	{
		var wh = new FakeWebhook(this, FakeGuild.FakeCurrentUser)
		{
			Name = name,
		};
		FakeGuild.FakeWebhooks.Add(wh);
		return Task.FromResult<IWebhook>(wh);
	}

	public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IThreadChannel>> GetActiveThreadsAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICategoryChannel?> GetCategoryAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
	{
		var match = FakeGuild.FakeChannels.SingleOrDefault(x => x.Id == CategoryId);
		return Task.FromResult(match as ICategoryChannel);
	}

	public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions? options = null)
	{
		var matches = FakeGuild.FakeInvites.Where(x => x.ChannelId == Id).ToArray();
		return Task.FromResult<IReadOnlyCollection<IInviteMetadata>>(matches);
	}

	public Task<IWebhook?> GetWebhookAsync(ulong id, RequestOptions options = null)
	{
		var matches = FakeGuild.FakeWebhooks.Where(x => x.ChannelId == Id);
		var match = matches.SingleOrDefault(x => x.Id == id);
		return Task.FromResult<IWebhook?>(match);
	}

	public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
	{
		var matches = FakeGuild.FakeWebhooks.Where(x => x.ChannelId == Id).ToArray();
		return Task.FromResult<IReadOnlyCollection<IWebhook>>(matches);
	}

	public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions? options = null)
	{
		var args = new TextChannelProperties();
		func(args);

		ProtectedCategoryId = args.CategoryId.GetValueOrDefault(ProtectedCategoryId);
		Position = args.Position.GetValueOrDefault(Position);
		IsNsfw = args.IsNsfw.GetValueOrDefault(IsNsfw);
		SlowModeInterval = args.SlowModeInterval.GetValueOrDefault(SlowModeInterval);
		Topic = args.Topic.GetValueOrDefault(Topic);

		return Task.CompletedTask;
	}

	public async Task SyncPermissionsAsync(RequestOptions? options = null)
	{
		var category = await GetCategoryAsync().ConfigureAwait(false);
		if (category is null)
		{
			return;
		}

		_Permissions.Clear();
		foreach (var overwrite in category.PermissionOverwrites)
		{
			_Permissions[overwrite.TargetId] = overwrite;
		}
	}
}