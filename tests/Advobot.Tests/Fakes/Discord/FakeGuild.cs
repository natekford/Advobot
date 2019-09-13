using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.Utilities;
using AdvorangesUtils;

using Discord;
using Discord.Audio;

namespace Advobot.Tests.Fakes.Discord
{
	public sealed class FakeGuild : FakeSnowflake, IGuild
	{
		public ulong? AFKChannelId => throw new NotImplementedException();
		public int AFKTimeout => throw new NotImplementedException();
		public ulong? ApplicationId => throw new NotImplementedException();
		public IAudioClient AudioClient => throw new NotImplementedException();
		public bool Available => throw new NotImplementedException();
		public string BannerId => throw new NotImplementedException();
		public string BannerUrl => throw new NotImplementedException();
		public ulong DefaultChannelId => throw new NotImplementedException();
		public DefaultMessageNotifications DefaultMessageNotifications => throw new NotImplementedException();
		public string Description => throw new NotImplementedException();
		public ulong? EmbedChannelId => throw new NotImplementedException();
		public List<GuildEmote> Emotes { get; } = new List<GuildEmote>();
		public ExplicitContentFilterLevel ExplicitContentFilter => throw new NotImplementedException();
		public List<FakeBan> FakeBans { get; } = new List<FakeBan>();
		public List<FakeGuildChannel> FakeChannels { get; } = new List<FakeGuildChannel>();
		public FakeClient FakeClient { get; }
		public FakeGuildUser FakeCurrentUser { get; }
		public FakeRole FakeEveryoneRole { get; }
		public List<FakeInviteMetadata> FakeInvites { get; } = new List<FakeInviteMetadata>();
		public FakeGuildUser FakeOwner { get; set; }
		public List<FakeRole> FakeRoles { get; } = new List<FakeRole>();
		public List<FakeGuildUser> FakeUsers { get; } = new List<FakeGuildUser>();
		public List<IWebhook> FakeWebhooks { get; } = new List<IWebhook>();
		public List<string> Features { get; } = new List<string>();
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

		public bool IsEmbeddable => throw new NotImplementedException();
		public MfaLevel MfaLevel => throw new NotImplementedException();
		public string Name => "Fake Guild";
		public ulong OwnerId => FakeOwner.Id;
		public int PremiumSubscriptionCount { get; set; }

		public PremiumTier PremiumTier => PremiumSubscriptionCount switch
		{
			int i when i >= 50 => PremiumTier.Tier3,
			int i when i >= 10 => PremiumTier.Tier2,
			int i when i >= 2 => PremiumTier.Tier1,
			_ => PremiumTier.None,
		};

		public IReadOnlyCollection<IRole> Roles => throw new NotImplementedException();
		public string SplashId => throw new NotImplementedException();
		public string SplashUrl => throw new NotImplementedException();
		public SystemChannelMessageDeny SystemChannelFlags => throw new NotImplementedException();
		public ulong? SystemChannelId => throw new NotImplementedException();
		public string VanityURLCode => throw new NotImplementedException();
		public VerificationLevel VerificationLevel => throw new NotImplementedException();
		public string VoiceRegionId => throw new NotImplementedException();
		IReadOnlyCollection<GuildEmote> IGuild.Emotes => Emotes;
		IRole IGuild.EveryoneRole => FakeEveryoneRole;
		IReadOnlyCollection<string> IGuild.Features => Features;

		public FakeGuild(FakeClient client)
		{
			FakeClient = client;
			FakeClient.FakeGuilds.Add(this);
			//This has to go before the two created users so they can get it.
			FakeEveryoneRole = new FakeRole(this)
			{
				Id = Id,
			};
			FakeCurrentUser = new FakeGuildUser(this)
			{
				Id = client.CurrentUser.Id,
			};
			FakeOwner = new FakeGuildUser(this)
			{
				Id = Id,
			};
		}

		public Task AddBanAsync(IUser user, int pruneDays = 0, string reason = null, RequestOptions options = null)
		{
			FakeBans.Add(new FakeBan(user));
			FakeUsers.RemoveAll(x => x.Id == user.Id);
			return Task.CompletedTask;
		}

		public Task AddBanAsync(ulong userId, int pruneDays = 0, string reason = null, RequestOptions options = null)
		{
			FakeBans.Add(new FakeBan(userId));
			FakeUsers.RemoveAll(x => x.Id == userId);
			return Task.CompletedTask;
		}

		public Task<IGuildUser> AddGuildUserAsync(ulong userId, string accessToken, Action<AddGuildUserProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();

		public Task<ICategoryChannel> CreateCategoryAsync(string name, Action<GuildChannelProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();

		public Task<GuildEmote> CreateEmoteAsync(string name, Image image, Optional<IEnumerable<IRole>> roles = default, RequestOptions options = null)
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

		public Task<IGuildIntegration> CreateIntegrationAsync(ulong id, string type, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IRole> CreateRoleAsync(string name, GuildPermissions? permissions = null, Color? color = null, bool isHoisted = false, RequestOptions options = null) => throw new NotImplementedException();

		public async Task<ITextChannel> CreateTextChannelAsync(string name, Action<TextChannelProperties> func = null, RequestOptions options = null)
		{
			var channel = new FakeTextChannel(this)
			{
				Name = name,
			};
			await channel.ModifyAsync(func, options).CAF();
			FakeChannels.Add(channel);
			return channel;
		}

		public Task<IVoiceChannel> CreateVoiceChannelAsync(string name, Action<VoiceChannelProperties> func = null, RequestOptions options = null) => throw new NotImplementedException();

		public Task DeleteAsync(RequestOptions options = null) => throw new NotImplementedException();

		public Task DeleteEmoteAsync(GuildEmote emote, RequestOptions options = null)
		{
			Emotes.Remove(emote);
			return Task.CompletedTask;
		}

		public Task DownloadUsersAsync()
			=> Task.CompletedTask;

		public async Task<IVoiceChannel> GetAFKChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> (IVoiceChannel)await GetChannelAsync(AFKChannelId ?? 0).CAF();

		public Task<IReadOnlyCollection<IAuditLogEntry>> GetAuditLogsAsync(int limit = 100, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IBan> GetBanAsync(IUser user, RequestOptions options = null)
			=> GetBanAsync(user.Id, options);

		public Task<IBan> GetBanAsync(ulong userId, RequestOptions options = null)
			=> Task.FromResult<IBan>(FakeBans.SingleOrDefault(x => x.User.Id == userId));

		public Task<IReadOnlyCollection<IBan>> GetBansAsync(RequestOptions options = null)
			=> Task.FromResult<IReadOnlyCollection<IBan>>(FakeBans);

		public Task<IReadOnlyCollection<ICategoryChannel>> GetCategoriesAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IGuildChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IGuildChannel>> GetChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult<IReadOnlyCollection<IGuildChannel>>(FakeChannels);

		public Task<IGuildUser> GetCurrentUserAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult<IGuildUser>(FakeCurrentUser);

		public Task<ITextChannel> GetDefaultChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IGuildChannel> GetEmbedChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<GuildEmote> GetEmoteAsync(ulong id, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IGuildIntegration>> GetIntegrationsAsync(RequestOptions options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IInviteMetadata>> GetInvitesAsync(RequestOptions options = null) => throw new NotImplementedException();

		public Task<IGuildUser> GetOwnerAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult<IGuildUser>(FakeOwner);

		public IRole GetRole(ulong id)
			=> FakeRoles.SingleOrDefault(x => x.Id == id);

		public Task<ITextChannel> GetSystemChannelAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<ITextChannel> GetTextChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<ITextChannel>> GetTextChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IGuildUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult<IGuildUser>(FakeUsers.SingleOrDefault(x => x.Id == id));

		public Task<IReadOnlyCollection<IGuildUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> Task.FromResult<IReadOnlyCollection<IGuildUser>>(FakeUsers);

		public Task<IInviteMetadata> GetVanityInviteAsync(RequestOptions options = null) => throw new NotImplementedException();

		public Task<IVoiceChannel> GetVoiceChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IVoiceChannel>> GetVoiceChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions options = null) => throw new NotImplementedException();

		public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions options = null)
			=> Task.FromResult(FakeWebhooks.SingleOrDefault(x => x.Id == id));

		public Task<IReadOnlyCollection<IWebhook>> GetWebhooksAsync(RequestOptions options = null)
			=> Task.FromResult<IReadOnlyCollection<IWebhook>>(FakeWebhooks);

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