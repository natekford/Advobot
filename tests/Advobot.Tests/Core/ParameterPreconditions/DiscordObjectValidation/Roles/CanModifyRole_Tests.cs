using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

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
		await Context.User.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).ConfigureAwait(false);

		await AssertFailureAsync(_Role).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerAndBotAreHigher_Test()
	{
		await Context.User.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).ConfigureAwait(false);

		await AssertSuccessAsync(_Role).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerIsLower_Test()
	{
		await Context.User.AddRoleAsync(_LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).ConfigureAwait(false);

		await AssertFailureAsync(_Role).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task NeitherHigher_Test()
	{
		await Context.User.AddRoleAsync(_LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).ConfigureAwait(false);

		await AssertFailureAsync(_Role).ConfigureAwait(false);
	}
}