using Discord;

namespace Advobot.Tests.Fakes.Discord.Channels;

public abstract class FakeChannel : FakeSnowflake, IChannel
{
	public ChannelType ChannelType => throw new NotImplementedException();
	public string Name { get; set; } = "Fake Channel";

	public abstract Task<IUser?> GetUserAsync(ulong id, CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null);

	public abstract IAsyncEnumerable<IReadOnlyCollection<IUser>> GetUsersAsync(CacheMode mode = CacheMode.AllowDownload, RequestOptions? options = null);
}