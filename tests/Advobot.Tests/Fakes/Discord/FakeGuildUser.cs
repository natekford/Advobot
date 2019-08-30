﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord
{
	public class FakeGuildUser : FakeUser, IGuildUser
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public DateTimeOffset? JoinedAt => throw new NotImplementedException();
		public string Nickname { get; set; }
		public GuildPermissions GuildPermissions => throw new NotImplementedException();
		public IGuild Guild => _Guild;
		public ulong GuildId => Guild.Id;
		public IReadOnlyCollection<ulong> RoleIds => _RoleIds;
		public bool IsDeafened { get; set; }
		public bool IsMuted { get; set; }
		public bool IsSelfDeafened => false;
		public bool IsSelfMuted => false;
		public bool IsSuppressed => false;
		public IVoiceChannel VoiceChannel { get; set; }
		public string VoiceSessionId { get; set; }
		public DateTimeOffset? PremiumSince => throw new NotImplementedException();

		private readonly FakeGuild _Guild;
		private readonly HashSet<ulong> _RoleIds = new HashSet<ulong>();

		public FakeGuildUser(FakeGuild guild)
		{
			_Guild = guild;
			_Guild.AddFakeUser(this);
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

		public ChannelPermissions GetPermissions(IGuildChannel channel) => throw new NotImplementedException();

		public Task KickAsync(string reason = null, RequestOptions options = null)
		{
			_Guild.Users.Remove(Id);
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

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}