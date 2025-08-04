using Advobot.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Roles;

[TestClass]
public sealed class NotEveryone_Tests : ParameterPrecondition_Tests<NotEveryone>
{
	protected override NotEveryone Instance { get; } = new();

	[TestMethod]
	public async Task RoleIsEveryone_Test()
		=> await AssertFailureAsync(Context.Guild.FakeEveryoneRole).CAF();

	[TestMethod]
	public async Task RoleIsNotEveryone_Test()
		=> await AssertSuccessAsync(new FakeRole(Context.Guild) { Id = 73, }).CAF();
}