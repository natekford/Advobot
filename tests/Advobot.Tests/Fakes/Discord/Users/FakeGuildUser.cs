using Advobot.Tests.Utilities;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Users
{
	public class FakeGuildUser : FakeUser, IGuildUser
	{
		private readonly HashSet<ulong> _RoleIds = new();
		public FakeGuild Guild { get; }
		public string GuildAvatarId => throw new NotImplementedException();
		public ulong GuildId => Guild.Id;
		public GuildPermissions GuildPermissions => new(PermissionUtils.ResolveGuild(Guild, this));
		public int Hierarchy => throw new NotImplementedException();
		public bool IsDeafened { get; set; }
		public bool IsMuted { get; set; }
		public bool? IsPending { get; set; }
		public bool IsSelfDeafened => false;
		public bool IsSelfMuted => false;
		public bool IsStreaming { get; set; }
		public bool IsSuppressed => false;
		public DateTimeOffset? JoinedAt => throw new NotImplementedException();
		public string Nickname { get; set; }
		public DateTimeOffset? PremiumSince => throw new NotImplementedException();
		public DateTimeOffset? RequestToSpeakTimestamp => throw new NotImplementedException();
		public IReadOnlyCollection<ulong> RoleIds => _RoleIds;
		public IVoiceChannel VoiceChannel { get; set; }
		public string VoiceSessionId { get; set; }
		IGuild IGuildUser.Guild => Guild;

		public FakeGuildUser(FakeGuild guild)
		{
			Guild = guild;
			Guild.FakeUsers.Add(this);
			_RoleIds.Add(guild.FakeEveryoneRole.Id);
		}

		public Task AddRoleAsync(IRole role, RequestOptions? options = null)
		{
			_RoleIds.Add(role.Id);
			return Task.CompletedTask;
		}

		public Task AddRoleAsync(ulong roleId, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
		{
			foreach (var role in roles)
			{
				_RoleIds.Add(role.Id);
			}
			return Task.CompletedTask;
		}

		public Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public string GetGuildAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
			=> throw new NotImplementedException();

		public ChannelPermissions GetPermissions(IGuildChannel channel)
					=> new(PermissionUtils.ResolveChannel(Guild, this, channel, GuildPermissions.RawValue));

		public Task KickAsync(string? reason = null, RequestOptions? options = null)
		{
			Guild.FakeUsers.Remove(this);
			return Task.CompletedTask;
		}

		public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task RemoveRoleAsync(IRole role, RequestOptions? options = null)
		{
			_RoleIds.Remove(role.Id);
			return Task.CompletedTask;
		}

		public Task RemoveRoleAsync(ulong roleId, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
		{
			_RoleIds.RemoveWhere(r => roles.Select(x => x.Id).Contains(r));
			return Task.CompletedTask;
		}

		public Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions? options = null)
			=> throw new NotImplementedException();
	}
}