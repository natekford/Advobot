using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	//Because Discord.Net uses a Nuget package for IAsyncEnumerable from pre .Net Core 3.0/Standard 2.0
	extern alias oldasyncenumerable;

	public class FakeGuildChannel : FakeMessageChannel, IGuildChannel
	{
		public int Position => throw new NotImplementedException();
		public FakeGuild Guild { get; }
		public ulong GuildId => Guild.Id;
		public IReadOnlyCollection<Overwrite> PermissionOverwrites => throw new NotImplementedException();

		public FakeGuildChannel(FakeGuild guild)
		{
			Guild = guild;
		}

		public Task AddPermissionOverwriteAsync(IRole role, OverwritePermissions permissions, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task AddPermissionOverwriteAsync(IUser user, OverwritePermissions permissions, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task DeleteAsync(RequestOptions options = null)
			=> throw new NotImplementedException();
		public OverwritePermissions? GetPermissionOverwrite(IRole role)
			=> throw new NotImplementedException();
		public OverwritePermissions? GetPermissionOverwrite(IUser user)
			=> throw new NotImplementedException();
		public Task ModifyAsync(Action<GuildChannelProperties> func, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task RemovePermissionOverwriteAsync(IRole role, RequestOptions options = null)
			=> throw new NotImplementedException();
		public Task RemovePermissionOverwriteAsync(IUser user, RequestOptions options = null)
			=> throw new NotImplementedException();

		IGuild IGuildChannel.Guild => Guild;
		Task<IGuildUser> IGuildChannel.GetUserAsync(ulong id, CacheMode mode, RequestOptions options)
			=> throw new NotImplementedException();
		oldasyncenumerable::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IGuildUser>> IGuildChannel.GetUsersAsync(CacheMode mode, RequestOptions options)
			=> throw new NotImplementedException();
	}
}
