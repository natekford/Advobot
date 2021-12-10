using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;

[TestClass]
public sealed class NotEveryoneAttribute_Tests
	: ParameterPreconditionTestsBase<NotEveryoneAttribute>
{
	protected override NotEveryoneAttribute Instance { get; } = new();

	[TestMethod]
	public async Task RoleIsEveryone_Test()
		=> await AssertFailureAsync(Context.Guild.FakeEveryoneRole).CAF();

	[TestMethod]
	public async Task RoleIsNotEveryone_Test()
		=> await AssertSuccessAsync(new FakeRole(Context.Guild) { Id = 73, }).CAF();
}