using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Advobot.Tests.Utilities;
using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeGuildUser : FakeUser, IGuildUser
	{
		private readonly HashSet<ulong> _RoleIds = new HashSet<ulong>();
		public FakeGuild Guild { get; }
		public ulong GuildId => Guild.Id;
		public GuildPermissions GuildPermissions => new GuildPermissions(PermissionUtils.ResolveGuild(Guild, this));
		public bool IsDeafened { get; set; }
		public bool IsMuted { get; set; }
		public bool IsSelfDeafened => false;
		public bool IsSelfMuted => false;
		public bool IsSuppressed => false;
		public DateTimeOffset? JoinedAt => throw new NotImplementedException();
		public string Nickname { get; set; }
		public DateTimeOffset? PremiumSince => throw new NotImplementedException();
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

		public Task AddRoleAsync(IRole role, RequestOptions options = null)
		{
			_RoleIds.Add(role.Id);
			return Task.CompletedTask;
		}

		public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
		{
			foreach (var role in roles)
			{
				_RoleIds.Add(role.Id);
			}
			return Task.CompletedTask;
		}

		public ChannelPermissions GetPermissions(IGuildChannel channel)
			=> new ChannelPermissions(PermissionUtils.ResolveChannel(Guild, this, channel, GuildPermissions.RawValue));

		public Task KickAsync(string reason = null, RequestOptions options = null)
		{
			Guild.FakeUsers.Remove(this);
			return Task.CompletedTask;
		}

		public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions options = null) => throw new NotImplementedException();

		public Task RemoveRoleAsync(IRole role, RequestOptions options = null)
		{
			_RoleIds.Remove(role.Id);
			return Task.CompletedTask;
		}

		public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions options = null)
		{
			_RoleIds.RemoveWhere(r => roles.Select(x => x.Id).Contains(r));
			return Task.CompletedTask;
		}
	}
}