using Advobot.Standard.Commands;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Standard.Commands;

[TestClass]
public sealed class Webhooks_Tests : Command_Tests
{
	[TestMethod]
	public async Task CreateWebhook_Test()
	{
		const string NAME = "asdf";
		var input = $"{nameof(Webhooks.CreateWebhook)} {Context.Channel} {NAME}";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		var webhooks = await Context.Channel.GetWebhooksAsync().ConfigureAwait(false);
		Assert.HasCount(1, webhooks);
		var webhook = webhooks.Single();
		Assert.AreEqual(NAME, webhook.Name);
	}
}