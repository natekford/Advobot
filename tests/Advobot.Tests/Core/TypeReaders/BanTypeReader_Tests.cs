using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;
using Advobot.TypeReaders.Discord;

using Discord;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class BanTypeReader_Tests : TypeReader_Tests<BanTypeReader>
{
	protected override BanTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task InvalidMultipleMatches_Test()
	{
		const string NAME = "bob";

		{
			var user = new FakeGuildUser(Context.Guild)
			{
				Username = NAME,
			};
			await Context.Guild.AddBanAsync(user).ConfigureAwait(false);
		}
		{
			var user = new FakeGuildUser(Context.Guild)
			{
				Username = NAME,
			};
			await Context.Guild.AddBanAsync(user).ConfigureAwait(false);
		}

		var result = await ReadAsync(NAME).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
	}

	[TestMethod]
	public async Task ValidId_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		await Context.Guild.AddBanAsync(user).ConfigureAwait(false);

		var result = await ReadAsync(user.Id.ToString()).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<IBan>(result.Value);
		var ban = (IBan)result.Value;
		Assert.AreEqual(user.Id, ban.User.Id);
	}

	[TestMethod]
	public async Task ValidMention_Test()
	{
		var user = new FakeGuildUser(Context.Guild);
		await Context.Guild.AddBanAsync(user).ConfigureAwait(false);

		var result = await ReadAsync(user.Mention).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsInstanceOfType<IBan>(result.Value);
		var ban = (IBan)result.Value;
		Assert.AreEqual(user.Id, ban.User.Id);
	}
}