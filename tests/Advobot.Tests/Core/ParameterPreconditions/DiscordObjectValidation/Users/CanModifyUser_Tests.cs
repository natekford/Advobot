using Advobot.ParameterPreconditions.Discord.Users;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Users;

[TestClass]
public sealed class CanModifyUser_Tests : ParameterPrecondition_Tests<CanModifyUser>
{
	private readonly FakeRole _HigherRole;
	private readonly FakeRole _LowerRole;
	private readonly FakeRole _Role;
	private readonly FakeGuildUser _User;
	protected override CanModifyUser Instance { get; } = new();

	public CanModifyUser_Tests()
	{
		_HigherRole = new(Context.Guild) { Position = 1, };
		_LowerRole = new(Context.Guild) { Position = -1, };
		_Role = new(Context.Guild) { Position = 0, };
		_User = new(Context.Guild);
	}

	[TestMethod]
	public async Task BotIsLower_Test()
	{
		await Context.User.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).ConfigureAwait(false);
		await _User.AddRoleAsync(_Role).ConfigureAwait(false);

		await AssertFailureAsync(_User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task FailsOnOwner_Test()
	{
		await Context.User.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).ConfigureAwait(false);

		await AssertFailureAsync(Context.Guild.FakeOwner).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerAndBotAreHigher_Test()
	{
		await Context.User.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await _User.AddRoleAsync(_Role).ConfigureAwait(false);

		await AssertSuccessAsync(_User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerIsLower_Test()
	{
		await Context.User.AddRoleAsync(_LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).ConfigureAwait(false);
		await _User.AddRoleAsync(_Role).ConfigureAwait(false);

		await AssertFailureAsync(_User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task NeitherHigher_Test()
	{
		await Context.User.AddRoleAsync(_LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).ConfigureAwait(false);
		await _User.AddRoleAsync(_Role).ConfigureAwait(false);

		await AssertFailureAsync(_User).ConfigureAwait(false);
	}
}