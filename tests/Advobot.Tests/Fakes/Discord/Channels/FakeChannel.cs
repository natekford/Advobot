using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	//Because Discord.Net uses a Nuget package for IAsyncEnumerable from pre .Net Core 3.0/Standard 2.0
	extern alias oldasyncenumerable;

	public class FakeChannel : FakeSnowflake, IChannel
	{
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
		public string Name => "Fake Channel";

		public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

		public oldasyncenumerable::System.Collections.Generic.IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions options = null)
			=> throw new NotImplementedException();

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
	}
}