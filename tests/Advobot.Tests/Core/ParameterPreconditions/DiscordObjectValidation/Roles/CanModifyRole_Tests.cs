using Advobot.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Roles;

[TestClass]
public sealed class CanModifyRole_Tests : ParameterPrecondition_Tests<CanModifyRole>
{
	private readonly FakeRole _HigherRole;
	private readonly FakeRole _LowerRole;
	private readonly FakeRole _Role;
	protected override CanModifyRole Instance { get; } = new();

	public CanModifyRole_Tests()
	{
		_HigherRole = new(Context.Guild) { Position = 1, };
		_LowerRole = new(Context.Guild) { Position = -1, };
		_Role = new(Context.Guild) { Position = 0, };
	}

	[TestMethod]
	public async Task BotIsLower_Test()
	{
		await Context.User.AddRoleAsync(_HigherRole).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();

		await AssertFailureAsync(_Role).CAF();
	}

	[TestMethod]
	public async Task InvokerAndBotAreHigher_Test()
	{
		await Context.User.AddRoleAsync(_HigherRole).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

		await AssertSuccessAsync(_Role).CAF();
	}

	[TestMethod]
	public async Task InvokerIsLower_Test()
	{
		await Context.User.AddRoleAsync(_LowerRole).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

		await AssertFailureAsync(_Role).CAF();
	}

	[TestMethod]
	public async Task NeitherHigher_Test()
	{
		await Context.User.AddRoleAsync(_LowerRole).CAF();
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();

		await AssertFailureAsync(_Role).CAF();
	}
}