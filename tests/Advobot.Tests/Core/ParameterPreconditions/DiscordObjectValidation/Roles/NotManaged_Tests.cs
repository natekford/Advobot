using Advobot.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Roles;

[TestClass]
public sealed class NotManaged_Tests : ParameterPrecondition_Tests<NotManaged>
{
	protected override NotManaged Instance { get; } = new();

	[TestMethod]
	public async Task RoleIsManaged_Test()
		=> await AssertFailureAsync(new FakeRole(Context.Guild) { IsManaged = true }).CAF();

	[TestMethod]
	public async Task RoleIsNotManaged_Test()
		=> await AssertSuccessAsync(new FakeRole(Context.Guild) { IsManaged = false }).CAF();
}