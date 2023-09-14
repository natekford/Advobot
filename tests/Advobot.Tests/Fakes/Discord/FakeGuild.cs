using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord;
using Discord.Audio;

using System.Globalization;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeGuild : FakeSnowflake, IGuild
{
	public ulong? AFKChannelId => throw new NotImplementedException();
	public int AFKTimeout => throw new NotImplementedException();
	public ulong? ApplicationId => throw new NotImplementedException();
	public int? ApproximateMemberCount => throw new NotImplementedException();
	public int? ApproximatePresenceCount => throw new NotImplementedException();
	public IAudioClient AudioClient => throw new NotImplementedException();
	public bool Available => throw new NotImplementedException();
	public string BannerId => throw new NotImplementedException();
	public string BannerUrl => throw new NotImplementedException();
	public ulong DefaultChannelId => throw new NotImplementedException();
	public DefaultMessageNotifications DefaultMessageNotifications => throw new NotImplementedException();
	public string Description => throw new NotImplementedException();
	public string DiscoverySplashId => throw new NotImplementedException();
	public string DiscoverySplashUrl => throw new NotImplementedException();
	public ulong? EmbedChannelId => throw new NotImplementedException();
	public List<GuildEmote> Emotes { get; } = new();
	public ExplicitContentFilterLevel ExplicitContentFilter => throw new NotImplementedException();
	public List<FakeBan> FakeBans { get; } = new();
	public List<FakeGuildChannel> FakeChannels { get; } = new();
	public FakeClient FakeClient { get; }
	public FakeGuildUser FakeCurrentUser { get; }
	public FakeRole FakeEveryoneRole { get; }
	public List<FakeInviteMetadata> FakeInvites { get; } = new();
	public FakeGuildUser FakeOwner { get; set; }
	public List<FakeRole> FakeRoles { get; } = new();
	public List<FakeGuildUser> FakeUsers { get; } = new();
	public List<FakeWebhook> FakeWebhooks { get; } = new();
	public GuildFeatures Features { get; set; } = new GuildFeaturesCreationArgs().Build();
	public string IconId => throw new NotImplementedException();
	public string IconUrl => throw new NotImplementedException();
	public override ulong Id
	{
		get => base.Id;
		set
		{
			FakeEveryoneRole.Id = value;
			base.Id = value;
		}
	}
	public bool IsBoostProgressBarEnabled => throw new NotImplementedException();
	public bool IsEmbeddable => throw new NotImplementedException();
	public bool IsWidgetEnabled => throw new NotImplementedException();
	public int MaxBitrate => throw new NotImplementedException();
	public int? MaxMembers => throw new NotImplementedException();
	public int? MaxPresences => throw new NotImplementedException();
	public int? MaxStageVideoChannelUsers => throw new NotImplementedException();
	public ulong MaxUploadLimit => throw new NotImplementedException();
	public int? MaxVideoChannelUsers => throw new NotImplementedException();
	public MfaLevel MfaLevel => throw new NotImplementedException();
	public string Name => "Fake Guild";
	public NsfwLevel NsfwLevel => throw new NotImplementedException();
	public ulong OwnerId => FakeOwner.Id;
	public CultureInfo PreferredCulture => throw new NotImplementedException();
	public string PreferredLocale => throw new NotImplementedException();
	public int PremiumSubscriptionCount { get; set; }
	public PremiumTier PremiumTier => PremiumSubscriptionCount switch
	{
		int i when i >= 50 => PremiumTier.Tier3,
		int i when i >= 10 => PremiumTier.Tier2,
		int i when i >= 2 => PremiumTier.Tier1,
		_ => PremiumTier.None,
	};
	public ulong? PublicUpdatesChannelId => throw new NotImplementedException();
	public ulong? RulesChannelId => throw new NotImplementedException();
	public ulong? SafetyAlertsChannelId => throw new NotImplementedException();
	public string SplashId => throw new NotImplementedException();
	public string SplashUrl => throw new NotImplementedException();
	public IReadOnlyCollection<ICustomSticker> Stickers => throw new NotImplementedException();
	public SystemChannelMessageDeny SystemChannelFlags => throw new NotImplementedException();
	public ulong? SystemChannelId => throw new NotImplementedException();
	public string VanityURLCode => throw new NotImplementedException();
	public VerificationLevel VerificationLevel => throw new NotImplementedException();
	public string VoiceRegionId => throw new NotImplementedException();
	public ulong? WidgetChannelId => throw new NotImplementedException();
	IReadOnlyCollection<GuildEmote> IGuild.Emotes => Emotes;
	IRole IGuild.EveryoneRole => FakeEveryoneRole;
	IReadOnlyCollection<IRole> IGuild.Roles => FakeRoles;

	public FakeGuild(FakeClient client)
	{
		FakeClient = client;
		FakeClient.FakeGuilds.Add(this);
		//This has to go before the two created users so they can get it.
		FakeEveryoneRole = new(this)
		{
			Id = Id,
		};
		FakeCurrentUser = new(this)
		{
			Id = client.CurrentUser.Id,
		};
		FakeOwner = new(this)
		{
			Id = Id,
		};
	}

	public Task AddBanAsync(IUser user, int pruneDays = 0, string? reason = null, RequestOptions? options = null)
	{
		FakeBans.Add(new FakeBan(user));
		FakeUsers.RemoveAll(x => x.Id == user.Id);
		return Task.CompletedTask;
	}

	public Task AddBanAsync(ulong userId, int pruneDays = 0, string? reason = null, RequestOptions? options = null)
	{
		FakeBans.Add(new FakeBan(userId));
		FakeUsers.RemoveAll(x => x.Id == userId);
		return Task.CompletedTask;
	}

	public Task<IGuildUser> AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties>? func = null, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IApplicationCommand>> BulkOverwriteApplicationCommandsAsync(ApplicationCommandProperties[] properties, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IApplicationCommand> CreateApplicationCommandAsync(ApplicationCommandProperties properties, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IAutoModRule> CreateAutoModRuleAsync(Action<AutoModRuleProperties> props, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties>? func = null, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default, RequestOptions? options = null)
	{
		var args = new EmoteCreationArgs
		{
			Name = name,
			RoleIds = roles.GetValueOrDefault(Array.Empty<IRole>()).Select(x => x.Id).ToArray(),
			UserId = FakeCurrentUser.Id,
		};
		var emote = args.Build();
		Emotes.Add(emote);
		return Task.FromResult(emote);
	}

	public Task<IGuildScheduledEvent> CreateEventAsync(string name, DateTimeOffset startTime, GuildScheduledEventType type, GuildScheduledEventPrivacyLevel privacyLevel = GuildScheduledEventPrivacyLevel.Private, string description = null, DateTimeOffset? endTime = null, ulong? channelId = null, string location = null, Image? coverImage = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IForumChannel> CreateForumChannelAsync(string name, Action<ForumChannelProperties> func = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, RequestOptions? options = null)
	{
		var role = new FakeRole(this)
		{
			Name = name,
			Permissions = permissions.GetValueOrDefault(),
			Color = color.GetValueOrDefault(),
			IsHoisted = isHoisted
		};
		return Task.FromResult<IRole>(role);
	}

	public Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, bool isMentionable = false, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IStageChannel> CreateStageChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, Image image, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, string path, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICustomSticker> CreateStickerAsync(string name, string description, IEnumerable<string> tags, Stream stream, string filename, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICustomSticker> CreateStickerAsync(string name, Image image, IEnumerable<string> tags, string description = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICustomSticker> CreateStickerAsync(string name, Stream stream, string filename, IEnumerable<string> tags, string description = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public async Task<ITextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties>? func = null, RequestOptions? options = null)
	{
		var channel = new FakeTextChannel(this)
		{
			Name = name,
		};
		if (func is not null)
		{
			await channel.ModifyAsync(func, options).CAF();
		}
		FakeChannels.Add(channel);
		return channel;
	}

	public Task<IVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties>? func = null, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task DeleteAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions? options = null)
	{
		Emotes.Remove(emote);
		return Task.CompletedTask;
	}

	public Task DeleteIntegrationAsync(ulong id, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task DeleteStickerAsync(ICustomSticker sticker, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task DisconnectAsync(IGuildUser user)
		=> throw new NotImplementedException();

	public Task DownloadUsersAsync()
		=> Task.CompletedTask;

	public async Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> (IVoiceChannel)await GetChannelAsync(AFKChannelId ?? 0).CAF();

	public Task<IApplicationCommand> GetApplicationCommandAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IApplicationCommand>> GetApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null, ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null, ulong? beforeId = null, ulong? userId = null, ActionType? actionType = null, ulong? afterId = null)
		=> throw new NotImplementedException();

	public Task<IAutoModRule> GetAutoModRuleAsync(ulong ruleId, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IAutoModRule[]> GetAutoModRulesAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IBan> GetBanAsync(IUser user, RequestOptions? options = null)
		=> GetBanAsync(user.Id, options);

	public Task<IBan> GetBanAsync(ulong userId, RequestOptions? options = null)
		// D.Net returns null when not found
		=> Task.FromResult<IBan>(FakeBans.SingleOrDefault(x => x.User.Id == userId)!);

	public IAsyncEnumerable<IReadOnlyCollection<IBan>> GetBansAsync(int limit = 1000, RequestOptions options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<IBan>> GetBansAsync(ulong fromUserId, Direction dir, int limit = 1000, RequestOptions options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<IBan>> GetBansAsync(IUser fromUser, Direction dir, int limit = 1000, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<IGuildChannel>>(FakeChannels);

	public Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IGuildUser>(FakeCurrentUser);

	public Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<GuildEmote>> GetEmotesAsync(RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<GuildEmote>>(Emotes);

	public Task<IGuildScheduledEvent> GetEventAsync(ulong id, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IGuildScheduledEvent>> GetEventsAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IIntegration>> GetIntegrationsAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<IInviteMetadata>>(FakeInvites);

	public Task<IGuildOnboarding> GetOnboardingAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
			=> Task.FromResult<IGuildUser>(FakeOwner);

	public Task<ITextChannel> GetPublicUpdatesChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public IRole? GetRole(ulong id)
		=> FakeRoles.SingleOrDefault(x => x.Id == id);

	public Task<ITextChannel> GetRulesChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IStageChannel> GetStageChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IStageChannel>> GetStageChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ICustomSticker> GetStickerAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<ICustomSticker>> GetStickersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IThreadChannel> GetThreadChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IThreadChannel>> GetThreadChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IGuildUser?> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IGuildUser?>(FakeUsers.SingleOrDefault(x => x.Id == id));

	public Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<IGuildUser>>(FakeUsers);

	public Task<IInviteMetadata> GetVanityInviteAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions? options = null)
		=> FakeClient.GetVoiceRegionsAsync(options);

	public Task<IWebhook?> GetWebhookAsync(ulong id, RequestOptions? options = null)
		=> Task.FromResult<IWebhook?>(FakeWebhooks.SingleOrDefault(x => x.Id == id));

	public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<IWebhook>>(FakeWebhooks);

	public Task<WelcomeScreen> GetWelcomeScreenAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IGuildChannel> GetWidgetChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task LeaveAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task ModifyAsync(Action<GuildProperties> func, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IGuildOnboarding> ModifyOnboardingAsync(Action<GuildOnboardingProperties> props, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<WelcomeScreen> ModifyWelcomeScreenAsync(bool enabled, WelcomeScreenChannelProperties[] channels, string description = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task ModifyWidgetAsync(Action<GuildWidgetProperties> func, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task MoveAsync(IGuildUser user, IVoiceChannel targetChannel)
		=> throw new NotImplementedException();

	public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions? options = null, IEnumerable<ulong>? includeRoleIds = null)
		=> throw new NotImplementedException();

	public Task RemoveBanAsync(IUser user, RequestOptions? options = null)
		=> RemoveBanAsync(user.Id, options);

	public Task RemoveBanAsync(ulong userId, RequestOptions? options = null)
	{
		FakeBans.RemoveAll(x => x.User.Id == userId);
		return Task.CompletedTask;
	}

	public Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IGuildUser>> SearchUsersAsync(string query, int limit = 1000, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();
}