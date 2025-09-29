using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord;

public sealed class FakeClient : IDiscordClient
{
	public ConnectionState ConnectionState { get; set; } = ConnectionState.Connected;
	public ISelfUser CurrentUser { get; set; } = new FakeSelfUser();
	public FakeApplication FakeApplication { get; set; } = new();
	public List<FakeGuild> FakeGuilds { get; set; } = [];
	public bool FakeIsActive { get; private set; }
	public List<FakeUser> FakeUsers { get; set; } = [];
	public List<FakeVoiceRegion> FakeVoiceRegions { get; set; } = [];
	public TokenType TokenType => TokenType.Bot;

	public Task<IReadOnlyCollection<IApplicationCommand>> BulkOverwriteGlobalApplicationCommand(ApplicationCommandProperties[] properties, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task ConsumeEntitlementAsync(ulong entitlementId, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<Emote> CreateApplicationEmoteAsync(string name, Image image, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IApplicationCommand> CreateGlobalApplicationCommand(ApplicationCommandProperties properties, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream? jpegIcon = null, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IEntitlement> CreateTestEntitlementAsync(ulong skuId, ulong ownerId, SubscriptionOwnerType ownerType, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task DeleteApplicationEmoteAsync(ulong emoteId, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task DeleteTestEntitlementAsync(ulong entitlementId, RequestOptions options = null)
		=> throw new NotImplementedException();

	public void Dispose()
		=> throw new NotImplementedException();

	public ValueTask DisposeAsync()
		=> throw new NotImplementedException();

	public Task<Emote> GetApplicationEmoteAsync(ulong emoteId, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<Emote>> GetApplicationEmotesAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IApplication> GetApplicationInfoAsync(RequestOptions? options = null)
		=> Task.FromResult<IApplication>(FakeApplication);

	public Task<BotGateway> GetBotGatewayAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<IEntitlement>> GetEntitlementsAsync(int? limit = 100, ulong? afterId = null, ulong? beforeId = null, bool excludeEnded = false, ulong? guildId = null, ulong? userId = null, ulong[] skuIds = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<IEntitlement>> GetEntitlementsAsync(int limit = 100, ulong? afterId = null, ulong? beforeId = null, bool excludeEnded = false, ulong? guildId = null, ulong? userId = null, ulong[] skuIds = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<IEntitlement>> GetEntitlementsAsync(int limit = 100, ulong? afterId = null, ulong? beforeId = null, bool excludeEnded = false, ulong? guildId = null, ulong? userId = null, ulong[] skuIds = null, RequestOptions options = null, bool? excludeDeleted = null)
		=> throw new NotImplementedException();

	public Task<IApplicationCommand> GetGlobalApplicationCommandAsync(ulong id, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IApplicationCommand>> GetGlobalApplicationCommandsAsync(bool withLocalizations = false, string locale = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IGuild?> GetGuildAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IGuild?>(FakeGuilds.SingleOrDefault(x => x.Id == id));

	public Task<IReadOnlyCollection<IGuild>> GetGuildsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<IGuild>>(FakeGuilds);

	public Task<IInvite> GetInviteAsync(string inviteId, RequestOptions? options = null)
	{
		foreach (var guild in FakeGuilds)
		{
			foreach (var invite in guild.FakeInvites)
			{
				if (invite.Id == inviteId)
				{
					return Task.FromResult<IInvite>(invite);
				}
			}
		}
		return Task.FromResult<IInvite>(null);
	}

	public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<int> GetRecommendedShardCountAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<SKU>> GetSKUsAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<ISubscription> GetSKUSubscriptionAsync(ulong skuId, ulong subscriptionId, RequestOptions options = null)
		=> throw new NotImplementedException();

	public IAsyncEnumerable<IReadOnlyCollection<ISubscription>> GetSKUSubscriptionsAsync(ulong skuId, int limit = 100, ulong? afterId = null, ulong? beforeId = null, ulong? userId = null, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task<IUser?> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> Task.FromResult<IUser?>(FakeUsers.SingleOrDefault(x => x.Id == id));

	public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions? options = null)
		=> Task.FromResult<IReadOnlyCollection<IVoiceRegion>>(FakeVoiceRegions);

	public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions? options = null)
		=> throw new NotImplementedException();

	public Task<Emote> ModifyApplicationEmoteAsync(ulong emoteId, Action<ApplicationEmoteProperties> args, RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task StartAsync()
	{
		FakeIsActive = true;
		return Task.CompletedTask;
	}

	public Task StopAsync()
	{
		FakeIsActive = false;
		return Task.CompletedTask;
	}
}