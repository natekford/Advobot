using Advobot.ParameterPreconditions.Discord.Users;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Users;

[TestClass]
public sealed class CanModifyUser_Tests : ParameterPrecondition_Tests<CanModifyUser>
{
	protected override CanModifyUser Instance { get; } = new();
	private FakeRole HigherRole { get; set; }
	private FakeRole LowerRole { get; set; }
	private FakeRole Role { get; set; }
	private FakeGuildUser User { get; set; }

	[TestMethod]
	public async Task BotIsLower_Test()
	{
		await Context.User.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(LowerRole).ConfigureAwait(false);
		await User.AddRoleAsync(Role).ConfigureAwait(false);

		await AssertFailureAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task FailsOnOwner_Test()
	{
		await Context.User.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(HigherRole).ConfigureAwait(false);

		await AssertFailureAsync(Context.Guild.FakeOwner).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerAndBotAreHigher_Test()
	{
		await Context.User.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await User.AddRoleAsync(Role).ConfigureAwait(false);

		await AssertSuccessAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerIsLower_Test()
	{
		await Context.User.AddRoleAsync(LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await User.AddRoleAsync(Role).ConfigureAwait(false);

		await AssertFailureAsync(User).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task NeitherHigher_Test()
	{
		await Context.User.AddRoleAsync(LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(LowerRole).ConfigureAwait(false);
		await User.AddRoleAsync(Role).ConfigureAwait(false);

		await AssertFailureAsync(User).ConfigureAwait(false);
	}

	protected override async Task SetupAsync()
	{
		HigherRole = new(Context.Guild) { Position = 1, };
		LowerRole = new(Context.Guild) { Position = -1, };
		Role = new(Context.Guild) { Position = 0, };
		User = new(Context.Guild);

		await Context.User.RemoveRolesAsync(Context.User.RoleIds).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.RemoveRolesAsync(Context.Guild.FakeCurrentUser.RoleIds).ConfigureAwait(false);
		await User.RemoveRolesAsync(User.RoleIds).ConfigureAwait(false);
	}
}