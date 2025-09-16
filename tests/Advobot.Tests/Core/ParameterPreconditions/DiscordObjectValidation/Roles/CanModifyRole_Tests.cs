using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Roles;

[TestClass]
public sealed class CanModifyRole_Tests : ParameterPrecondition_Tests<CanModifyRole>
{
	protected override CanModifyRole Instance { get; } = new();
	private FakeRole HigherRole { get; set; }
	private FakeRole LowerRole { get; set; }
	private FakeRole Role { get; set; }

	[TestMethod]
	public async Task BotIsLower_Test()
	{
		await Context.User.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(LowerRole).ConfigureAwait(false);

		await AssertFailureAsync(Role).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerAndBotAreHigher_Test()
	{
		await Context.User.AddRoleAsync(HigherRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(HigherRole).ConfigureAwait(false);

		await AssertSuccessAsync(Role).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InvokerIsLower_Test()
	{
		await Context.User.AddRoleAsync(LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(HigherRole).ConfigureAwait(false);

		await AssertFailureAsync(Role).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task NeitherHigher_Test()
	{
		await Context.User.AddRoleAsync(LowerRole).ConfigureAwait(false);
		await Context.Guild.FakeCurrentUser.AddRoleAsync(LowerRole).ConfigureAwait(false);

		await AssertFailureAsync(Role).ConfigureAwait(false);
	}

	protected override Task SetupAsync()
	{
		HigherRole = new(Context.Guild) { Position = 1, };
		LowerRole = new(Context.Guild) { Position = -1, };
		Role = new(Context.Guild) { Position = 0, };
		return Task.CompletedTask;
	}
}