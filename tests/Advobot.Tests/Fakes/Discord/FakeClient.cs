
using Advobot.Tests.Fakes.Discord.Users;

using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public sealed class FakeClient : IDiscordClient
	{
		public ConnectionState ConnectionState => throw new NotImplementedException();
		public ISelfUser CurrentUser { get; set; } = new FakeSelfUser();
		public FakeApplication FakeApplication { get; set; } = new FakeApplication();
		public List<FakeGuild> FakeGuilds { get; set; } = new List<FakeGuild>();
		public List<FakeVoiceRegion> FakeVoiceRegions { get; set; } = new List<FakeVoiceRegion>();
		public TokenType TokenType => throw new NotImplementedException();

		public Task<IGuild> CreateGuildAsync(string name, IVoiceRegion region, Stream? jpegIcon = null, RequestOptions? options = null) => throw new NotImplementedException();

		public void Dispose() => throw new NotImplementedException();

		public Task<IApplication> GetApplicationInfoAsync(RequestOptions? options = null)
			=> Task.FromResult<IApplication>(FakeApplication);

		public Task<BotGateway> GetBotGatewayAsync(RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task<IChannel> GetChannelAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IConnection>> GetConnectionsAsync(RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IDMChannel>> GetDMChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IGroupChannel>> GetGroupChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => throw new NotImplementedException();

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

		public Task<IReadOnlyCollection<IPrivateChannel>> GetPrivateChannelsAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => throw new NotImplementedException();

		public Task<int> GetRecommendedShardCountAsync(RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IUser> GetUserAsync(string username, string discriminator, RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IVoiceRegion> GetVoiceRegionAsync(string id, RequestOptions? options = null) => throw new NotImplementedException();

		public Task<IReadOnlyCollection<IVoiceRegion>> GetVoiceRegionsAsync(RequestOptions? options = null)
			=> Task.FromResult<IReadOnlyCollection<IVoiceRegion>>(FakeVoiceRegions);

		public Task<IWebhook> GetWebhookAsync(ulong id, RequestOptions? options = null) => throw new NotImplementedException();

		public Task StartAsync() => throw new NotImplementedException();

		public Task StopAsync() => throw new NotImplementedException();
	}
}