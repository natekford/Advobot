using AdvorangesUtils;
using Discord;
using Discord.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Advobot.Tests.Mocks
{
	public class MockGuild : IGuild
	{
		public string Name => "Mock Guild";
		public int AFKTimeout => throw new NotImplementedException();
		public bool IsEmbeddable => throw new NotImplementedException();
		public DefaultMessageNotifications DefaultMessageNotifications => throw new NotImplementedException();
		public MfaLevel MfaLevel => throw new NotImplementedException();
		public VerificationLevel VerificationLevel => throw new NotImplementedException();
		public ExplicitContentFilterLevel ExplicitContentFilter => throw new NotImplementedException();
		public string IconId => throw new NotImplementedException();
		public string IconUrl => throw new NotImplementedException();
		public string SplashId => throw new NotImplementedException();
		public string SplashUrl => throw new NotImplementedException();
		public bool Available => throw new NotImplementedException();
		public ulong? AFKChannelId => throw new NotImplementedException();
		public ulong DefaultChannelId => throw new NotImplementedException();
		public ulong? EmbedChannelId => throw new NotImplementedException();
		public ulong? SystemChannelId => throw new NotImplementedException();
		public ulong OwnerId => throw new NotImplementedException();
		public ulong? ApplicationId => throw new NotImplementedException();
		public string VoiceRegionId => throw new NotImplementedException();
		public IAudioClient AudioClient => throw new NotImplementedException();
		public IRole EveryoneRole => throw new NotImplementedException();
		public IReadOnlyCollection<GuildEmote> Emotes => throw new NotImplementedException();
		public IReadOnlyCollection<string> Features => throw new NotImplementedException();
		public IReadOnlyCollection<IRole> Roles => throw new NotImplementedException();
		public DateTimeOffset CreatedAt => throw new NotImplementedException();
		public ulong Id => throw new NotImplementedException();

		public Dictionary<ulong, IGuildUser> Users { get; } = new Dictionary<ulong, IGuildUser>();
		public Dictionary<ulong, IWebhook> Webhooks { get; } = new Dictionary<ulong, IWebhook>();
		public Dictionary<ulong, IBan> Bans { get; } = new Dictionary<ulong, IBan>();

		public PremiumTier PremiumTier => throw new NotImplementedException();
		public string BannerId => throw new NotImplementedException();
		public string BannerUrl => throw new NotImplementedException();
		public string VanityURLCode => throw new NotImplementedException();
		public SystemChannelMessageDeny SystemChannelFlags => throw new NotImplementedException();
		public string Description => throw new NotImplementedException();
		public int PremiumSubscriptionCount => throw new NotImplementedException();

		public void AddMockUser(MockGuildUser user)
			=> Users[user.Id] = user;

		public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
			=> AddBanAsync(user.Id, pruneDays, reason, options);
		public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
		{
			Bans[userId] = default;
			Users.Remove(userId);
			return Task.CompletedTask;
		}
		public Task<IGuildUser> AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();
		public Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();
		public Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, RequestOptions options = null) => throw new NotImplementedException();
		public Task<ITextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();
		public Task DeleteAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null) => throw new NotImplementedException();
		public Task DownloadUsersAsync()
			=> Task.CompletedTask;
		public async Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> (IVoiceChannel)await GetChannelAsync(AFKChannelId ?? 0).CAF();
		public Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IBan> GetBanAsync(IUser user, RequestOptions options = null)
			=> GetBanAsync(user.Id, options);
		public Task<IBan> GetBanAsync(ulong userId, RequestOptions options = null)
			=> Task.FromResult(Bans.TryGetValue(userId, out var ban) ? ban : null);
		public Task<IReadOnlyCollection<IBan>> GetBansAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IGuildChannel> GetEmbedChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<IGuildIntegration>> GetIntegrationsAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public IRole GetRole(ulong id) => throw new NotImplementedException();
		public Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult(Users.TryGetValue(id, out var value) ? value : null);
		public Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult<IReadOnlyCollection<IGuildUser>>(Users.Values.ToArray());
		public Task<IInviteMetadata> GetVanityInviteAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();
		public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
			=> Task.FromResult(Webhooks.TryGetValue(id, out var value) ? value : null);
		public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
			=> Task.FromResult<IReadOnlyCollection<IWebhook>>(Webhooks.Values.ToArray());
		public Task LeaveAsync(RequestOptions options = null) => throw new NotImplementedException();
		public Task ModifyAsync(Action<GuildProperties> func, RequestOptions options = null) => throw new NotImplementedException();
		public Task ModifyEmbedAsync(Action<GuildEmbedProperties> func, RequestOptions options = null) => throw new NotImplementedException();
		public Task<GuildEmote> ModifyEmoteAsync(GuildEmote emote, Action<EmoteProperties> func, RequestOptions options = null) => throw new NotImplementedException();
		public Task<int> PruneUsersAsync(int days = 30, bool simulate = false, RequestOptions options = null) => throw new NotImplementedException();
		public Task RemoveBanAsync(IUser user, RequestOptions options = null) => throw new NotImplementedException();
		public Task RemoveBanAsync(ulong userId, RequestOptions options = null) => throw new NotImplementedException();
		public Task ReorderChannelsAsync(IEnumerable<ReorderChannelProperties> args, RequestOptions options = null) => throw new NotImplementedException();
		public Task ReorderRolesAsync(IEnumerable<ReorderRoleProperties> args, RequestOptions options = null) => throw new NotImplementedException();
	}
}
