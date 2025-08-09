using Advobot.ParameterPreconditions.Discord.Invites;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Invites;

[TestClass]
public sealed class NeverExpires_Tests : ParameterPrecondition_Tests<NeverExpires>
{
	protected override NeverExpires Instance { get; } = new();

	[TestMethod]
	public async Task InviteExpires_Test()
	{
		await AssertFailureAsync(new FakeInviteMetadata(Context.Channel, Context.User)
		{
			MaxAge = 3600,
		}).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task InviteNeverExpires_Test()
	{
		await AssertSuccessAsync(new FakeInviteMetadata(Context.Channel, Context.User)
		{
			MaxAge = null,
		}).ConfigureAwait(false);
	}
}