using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels;

public class FakeGuildChannel : FakeMessageChannel, IGuildChannel
{
	protected readonly Dictionary<ulong, Overwrite> _Permissions = [];
	public FakeGuild FakeGuild { get; }
	public ChannelFlags Flags => throw new NotImplementedException();
	public ulong GuildId => FakeGuild.Id;
	public IReadOnlyCollection<Overwrite> PermissionOverwrites => _Permissions.Values;
	public int Position { get; set; }
	IGuild IGuildChannel.Guild => FakeGuild;
	protected ulong? ProtectedCategoryId { get; set; }

	public FakeGuildChannel(FakeGuild guild)
	{
		FakeGuild = guild;
		FakeGuild.FakeChannels.Add(this);
	}

	public Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions? options = null)
	{
		_Permissions[role.Id] = new(role.Id, PermissionTarget.Role, permissions);
		return Task.CompletedTask;
	}

	public Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions? options = null)
	{
		_Permissions[user.Id] = new(user.Id, PermissionTarget.User, permissions);
		return Task.CompletedTask;
	}

	public Task DeleteAsync(RequestOptions? options = null)
		=> throw new NotImplementedException();

	public OverwritePermissions? GetPermissionOverwrite(IRole role)
		=> GetPermissionOverwrite(role.Id);

	public OverwritePermissions? GetPermissionOverwrite(IUser user)
		=> GetPermissionOverwrite(user.Id);

	// These should probably account for permissions to see the channels, but idc
	public override async Task<IUser?> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
		=> await FakeGuild.GetUserAsync(id, mode, options).ConfigureAwait(false);

	// These should probably account for permissions to see the channels, but idc
	public override async IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
	{
		yield return await FakeGuild.GetUsersAsync(mode, options).ConfigureAwait(false);
	}

	public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options)
	{
		var args = new GuildChannelProperties();
		func(args);

		ProtectedCategoryId = args.CategoryId.GetValueOrDefault(ProtectedCategoryId);
		Name = args.Name.GetValueOrDefault(Name);
		Position = args.Position.GetValueOrDefault(Position);

		return Task.CompletedTask;
	}

	public Task RemovePermissionOverwriteAsync(IRole role, RequestOptions? options = null)
	{
		_Permissions.Remove(role.Id);
		return Task.CompletedTask;
	}

	public Task RemovePermissionOverwriteAsync(IUser user, RequestOptions? options = null)
	{
		_Permissions.Remove(user.Id);
		return Task.CompletedTask;
	}

	Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
		=> throw new NotImplementedException();

	IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
		=> throw new NotImplementedException();

	private OverwritePermissions? GetPermissionOverwrite(ulong id)
		=> _Permissions.TryGetValue(id, out var value) ? value.Permissions : default(OverwritePermissions?);
}