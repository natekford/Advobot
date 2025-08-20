using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using Discord;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class ChannelPositionTypeReader_Tests
	: TypeReader_Tests<ChannelPositionTypeReader<IGuildChannel>>
{
	protected override ChannelPositionTypeReader<IGuildChannel> Instance { get; } = new();

	[TestMethod]
	public async Task InvalidChannel_Test()
	{
		var result = await ReadAsync(int.MaxValue.ToString()).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task InvalidMultipleMatches_Test()
	{
		const int POSITION = 2;

		var channel1 = await Context.Guild.CreateTextChannelAsync("asdf", x => x.Position = POSITION).ConfigureAwait(false);
		var channel2 = await Context.Guild.CreateTextChannelAsync("asdf", x => x.Position = POSITION).ConfigureAwait(false);

		var result = await ReadAsync(POSITION.ToString()).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task NotNumber_Test()
	{
		var result = await ReadAsync("asdf").ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task ValidChannel_Test()
	{
		const int POSITION = 2;

		var channel = await Context.Guild.CreateTextChannelAsync("asdf", x => x.Position = POSITION).ConfigureAwait(false);

		var result = await ReadAsync(POSITION.ToString()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType<IGuildChannel>(result.BestMatch);
		var parsed = (IGuildChannel)result.BestMatch;
		Assert.AreEqual(channel.Id, parsed.Id);
	}
}