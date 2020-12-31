using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels
{
	public class FakeChannel : FakeSnowflake, IChannel
	{
		public string Name { get; set; } = "Fake Channel";

		public Task<IUser> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
			=> throw new NotImplementedException();

		public IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null)
			=> throw new NotImplementedException();
	}
}