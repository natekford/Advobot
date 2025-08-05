using Advobot.ParameterPreconditions.DiscordObjectValidation;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

namespace Advobot.Tests.Core.ParameterPreconditions.Numbers;

[TestClass]
public sealed class NotBotOwner_Tests : ParameterPrecondition_Tests<NotBotOwner>
{
	protected override NotBotOwner Instance { get; } = new();

	[TestMethod]
	public async Task Invalid_Test()
	{
		Context.Client.FakeApplication.Owner = Context.User;

		await AssertFailureAsync(Context.User.Id).CAF();
	}

	[TestMethod]
	public async Task Valid_Test()
		=> await AssertSuccessAsync(Context.User.Id).CAF();
}