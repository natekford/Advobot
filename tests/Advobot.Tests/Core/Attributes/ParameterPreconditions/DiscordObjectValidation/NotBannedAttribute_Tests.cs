using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation;

[TestClass]
public sealed class NotBannedAttribute_Tests
	: ParameterPreconditionTestsBase<NotBannedAttribute>
{
	private const ulong ID = 1;
	protected override NotBannedAttribute Instance { get; } = new();

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