using Advobot.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation;

[TestClass]
public sealed class NotBanned_Tests : ParameterPrecondition_Tests<NotBanned>
{
	private const ulong ID = 1;
	protected override NotBanned Instance { get; } = new();

	[TestMethod]
	public async Task BanExisting_Test()
	{
		await Context.Guild.AddBanAsync(ID).CAF();

		await AssertFailureAsync(ID).CAF();
	}

	[TestMethod]
	public async Task BanNotExisting_Test()
		=> await AssertSuccessAsync(ID).CAF();
}