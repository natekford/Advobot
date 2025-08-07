using Advobot.ParameterPreconditions.Discord.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Roles;

[TestClass]
public sealed class NotMentionable_Tests : ParameterPrecondition_Tests<NotMentionable>
{
	protected override NotMentionable Instance { get; } = new();

	[TestMethod]
	public async Task RoleIsMentionable_Test()
		=> await AssertFailureAsync(new FakeRole(Context.Guild) { IsMentionable = true }).CAF();

	[TestMethod]
	public async Task RoleIsNotMentionable_Test()
		=> await AssertSuccessAsync(new FakeRole(Context.Guild) { IsMentionable = false }).CAF();
}