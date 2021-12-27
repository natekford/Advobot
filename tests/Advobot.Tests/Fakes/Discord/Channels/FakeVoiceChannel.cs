using AdvorangesUtils;

using Discord;
using Discord.Audio;

namespace Advobot.Tests.Fakes.Discord.Channels;

public sealed class FakeVoiceChannel : FakeGuildChannel, IVoiceChannel
{
	public int Bitrate { get; set; }
	public ulong? CategoryId => ProtectedCategoryId;
	public string Mention => MentionUtils.MentionChannel(Id);
	public string RTCRegion => throw new NotImplementedException();
	public int? UserLimit { get; set; }

	public FakeVoiceChannel(FakeGuild guild) : base(guild)
	{
	}

	public Task<IAudioClient> ConnectAsync(bool selfDeaf = false, bool selfMute = false, bool external = false)
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

	public Task DisconnectAsync()
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

	public async Task SyncPermissionsAsync(RequestOptions? options = null)
	{
		var category = await GetCategoryAsync().CAF();
		if (category == null)
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