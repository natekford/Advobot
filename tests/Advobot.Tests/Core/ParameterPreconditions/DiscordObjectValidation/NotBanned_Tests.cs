using Advobot.ParameterPreconditions.Discord;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation;

[TestClass]
public sealed class NotBanned_Tests : ParameterPrecondition_Tests<NotBanned>
{
	private const ulong ID = 1;
	protected override NotBanned Instance { get; } = new();

	[TestMethod]
	public async Task BanExisting_Test()
	{
		await Context.Guild.AddBanAsync(ID).ConfigureAwait(false);

		await AssertFailureAsync(ID).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task BanNotExisting_Test()
		=> await AssertSuccessAsync(ID).ConfigureAwait(false);
}