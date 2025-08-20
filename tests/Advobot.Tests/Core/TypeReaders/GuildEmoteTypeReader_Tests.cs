using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;
using Advobot.TypeReaders;

using Discord;

namespace Advobot.Tests.Core.TypeReaders;

[TestClass]
public sealed class GuildEmoteTypeReader_Tests : TypeReader_Tests<GuildEmoteTypeReader>
{
	private readonly GuildEmote _Emote = new EmoteCreationArgs
	{
		Id = 73UL,
		Name = "emote name",
	}.Build();
	protected override GuildEmoteTypeReader Instance { get; } = new();

	[TestMethod]
	public async Task InvalidNotOnThisGuildId_Test()
	{
		var result = await ReadAsync(_Emote.Id.ToString()).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task InvalidNotOnThisGuildName_Test()
	{
		var result = await ReadAsync(_Emote.Name).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task ValidId_Test()
	{
		Context.Guild.Emotes.Add(_Emote);

		var result = await ReadAsync(_Emote.Id.ToString()).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType<Emote>(result.BestMatch);
	}

	[TestMethod]
	public async Task ValidName_Test()
	{
		Context.Guild.Emotes.Add(_Emote);

		var result = await ReadAsync(_Emote.Name).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		Assert.IsInstanceOfType<Emote>(result.BestMatch);
	}
}