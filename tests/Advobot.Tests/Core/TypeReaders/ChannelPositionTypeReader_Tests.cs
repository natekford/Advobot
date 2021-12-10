using Advobot.Tests.TestBases;
using Advobot.TypeReaders;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class ChannelPositionTypeReader_Tests : TypeReaderTestsBase
{
	protected override TypeReader Instance { get; } = new ChannelPositionTypeReader<IGuildChannel>();

	[TestMethod]
	public async Task InvalidChannel_Test()
	{
		var result = await ReadAsync(int.MaxValue.ToString()).CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task InvalidMultipleMatches_Test()
	{
		const int POSITION = 2;

		var channel1 = await Context.Guild.CreateTextChannelAsync("asdf", x => x.Position = POSITION).CAF();
		var channel2 = await Context.Guild.CreateTextChannelAsync("asdf", x => x.Position = POSITION).CAF();

		var result = await ReadAsync(POSITION.ToString()).CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task NotNumber_Test()
	{
		var result = await ReadAsync("asdf").CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task ValidChannel_Test()
	{
		const int POSITION = 2;

		var channel = await Context.Guild.CreateTextChannelAsync("asdf", x => x.Position = POSITION).CAF();

		var result = await ReadAsync(POSITION.ToString()).CAF();
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType(result.BestMatch, typeof(IGuildChannel));
		var parsed = (IGuildChannel)result.BestMatch;
		Assert.AreEqual(channel.Id, parsed.Id);
	}
}