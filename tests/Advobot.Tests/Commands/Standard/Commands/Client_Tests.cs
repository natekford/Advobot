using Advobot.Standard.Commands;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Standard.Commands;

[TestClass]
public sealed class Client_Tests : Command_Tests
{
	[TestMethod]
	public async Task DisconnectBot_Test()
	{
		Context.User.Id = Context.Client.FakeApplication.Owner.Id;

		const string INPUT = nameof(Client.DisconnectBot);

		var result = await ExecuteWithResultAsync(INPUT).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsTrue(HasBeenShutdown);
	}

	[TestMethod]
	public async Task ModifyBotName_Test()
	{
		Context.User.Id = Context.Client.FakeApplication.Owner.Id;

		const string NAME = "asdf";
		var input = $"{nameof(Client.ModifyBotName)} {NAME}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.AreEqual(NAME, Context.Client.CurrentUser.Username);
	}
}