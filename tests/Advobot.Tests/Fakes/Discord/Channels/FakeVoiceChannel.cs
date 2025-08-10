using Discord;
using Discord.Audio;

namespace Advobot.Tests.Fakes.Discord.Channels;

public sealed class FakeVoiceChannel(FakeGuild guild)
	: FakeGuildChannel(guild), IVoiceChannel
{
	public int Bitrate { get; set; }
	public ulong? CategoryId => ProtectedCategoryId;
	public ThreadArchiveDuration DefaultArchiveDuration => throw new NotImplementedException();
	public int DefaultSlowModeInterval => throw new NotImplementedException();
	public bool IsNsfw => throw new NotImplementedException();
	public string Mention => MentionUtils.MentionChannel(Id);
	public string RTCRegion => throw new NotImplementedException();
	public int SlowModeInterval => throw new NotImplementedException();
	public string Topic => throw new NotImplementedException();
	public int? UserLimit { get; set; }
	public VideoQualityMode VideoQualityMode => throw new NotImplementedException();

	public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
		=> throw new NotImplementedException();

	public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false, bool disconnect = true)
		=> throw new NotImplementedException();

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
		=> throw new NotImplementedException();

	public Task DeleteMessagesAsync(IEnumerable<IMessage> messages, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task DeleteMessagesAsync(IEnumerable<ulong> messageIds, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task DisconnectAsync()
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

	public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task ModifyAsync(Action<VoiceChannelProperties> func, RequestOptions? options = null)
	{
		ModifyAsync((Action<GuildChannelProperties>)func);

		var args = new VoiceChannelProperties();
		func(args);

		Bitrate = args.Bitrate.GetValueOrDefault();
		UserLimit = args.UserLimit.GetValueOrDefault();

		return Task.CompletedTask;
	}

	public Task ModifyAsync(Action<AudioChannelProperties> func, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task ModifyAsync(Action<TextChannelProperties> func, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task SetStatusAsync(string status, RequestOptions options = null)
		=> throw new NotImplementedException();

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