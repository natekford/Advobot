using Discord;

using PermissionUtils = Advobot.Tests.Utilities.PermissionUtils;

namespace Advobot.Tests.Fakes.Discord.Users;

public class FakeGuildUser : FakeUser, IGuildUser
{
	public string DisplayAvatarId => throw new NotImplementedException();
	public string DisplayName => throw new NotImplementedException();
	public GuildUserFlags Flags { get; set; }
	public FakeGuild Guild { get; }
	public string GuildAvatarId => throw new NotImplementedException();
	public string GuildBannerHash => throw new NotImplementedException();
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
	public bool IsVideoing => throw new NotImplementedException();
	public DateTimeOffset? JoinedAt { get; set; } = DateTimeOffset.UtcNow;
	public string Nickname { get; set; }
	public DateTimeOffset? PremiumSince => throw new NotImplementedException();
	public DateTimeOffset? RequestToSpeakTimestamp => throw new NotImplementedException();
	public HashSet<ulong> RoleIds { get; set; } = [];
	public DateTimeOffset? TimedOutUntil { get; set; }
	public IVoiceChannel VoiceChannel { get; set; }
	public string VoiceSessionId { get; set; }
	IGuild IGuildUser.Guild => Guild;
	IReadOnlyCollection<ulong> IGuildUser.RoleIds => RoleIds;

	public FakeGuildUser(FakeGuild guild)
	{
		Guild = guild;
		Guild.FakeUsers.Add(this);
		RoleIds.Add(guild.FakeEveryoneRole.Id);
	}

	public Task AddRoleAsync(IRole role, RequestOptions? options = null)
		=> AddRoleAsync(role.Id, options);

	public Task AddRoleAsync(ulong roleId, RequestOptions? options = null)
	{
		RoleIds.Add(roleId);
		return Task.CompletedTask;
	}

	public Task AddRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
		=> AddRolesAsync(roles.Select(x => x.Id), options);

	public Task AddRolesAsync(IEnumerable<ulong> roleIds, RequestOptions? options = null)
	{
		foreach (var roleId in roleIds)
		{
			RoleIds.Add(roleId);
		}
		return Task.CompletedTask;
	}

	public string GetGuildAvatarUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
		=> throw new NotImplementedException();

	public string GetGuildBannerUrl(ImageFormat format = ImageFormat.Auto, ushort size = 128)
		=> throw new NotImplementedException();

	public ChannelPermissions GetPermissions(IGuildChannel channel)
		=> new(PermissionUtils.ResolveChannel(Guild, this, channel, GuildPermissions.RawValue));

	public Task KickAsync(string? reason = null, RequestOptions? options = null)
	{
		Guild.FakeUsers.Remove(this);
		return Task.CompletedTask;
	}

	public Task ModifyAsync(Action<GuildUserProperties> func, RequestOptions? options = null)
	{
		var args = new GuildUserProperties();
		func(args);

		VoiceChannel = args.Channel.GetValueOrDefault(VoiceChannel);
		if (args.ChannelId.IsSpecified)
		{
			VoiceChannel = Guild.FakeChannels
				.OfType<IVoiceChannel>()
				.SingleOrDefault(x => x.Id == args.ChannelId.Value)!;
		}
		IsDeafened = args.Deaf.GetValueOrDefault(IsDeafened);
		Flags = args.Flags.GetValueOrDefault(Flags);
		IsMuted = args.Mute.GetValueOrDefault(IsMuted);
		Nickname = args.Nickname.GetValueOrDefault(Nickname);
		if (args.Roles.IsSpecified)
		{
			RoleIds.Clear();
			RoleIds.UnionWith(args.Roles.Value.Select(x => x.Id));
			RoleIds.Add(Guild.FakeEveryoneRole.Id);
		}
		if (args.RoleIds.IsSpecified)
		{
			RoleIds.Clear();
			RoleIds.UnionWith(args.RoleIds.Value);
			RoleIds.Add(Guild.FakeEveryoneRole.Id);
		}
		TimedOutUntil = args.TimedOutUntil.GetValueOrDefault(TimedOutUntil);

		return Task.CompletedTask;
	}

	public Task RemoveRoleAsync(IRole role, RequestOptions? options = null)
		=> RemoveRoleAsync(role.Id, options);

	public Task RemoveRoleAsync(ulong roleId, RequestOptions? options = null)
	{
		RoleIds.Remove(roleId);
		return Task.CompletedTask;
	}

	public Task RemoveRolesAsync(IEnumerable<IRole> roles, RequestOptions? options = null)
		=> RemoveRolesAsync(roles.Select(x => x.Id), options);

	public Task RemoveRolesAsync(IEnumerable<ulong> roleIds, RequestOptions? options = null)
	{
		RoleIds.ExceptWith(roleIds);
		return Task.CompletedTask;
	}

	public Task RemoveTimeOutAsync(RequestOptions options = null)
		=> throw new NotImplementedException();

	public Task SetTimeOutAsync(TimeSpan span, RequestOptions options = null)
		=> throw new NotImplementedException();
}