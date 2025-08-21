using Advobot.Standard.Commands;
using Advobot.Tests.TestBases;

namespace Advobot.Tests.Commands.Standard;

[TestClass]
public sealed class Channels_Tests : Command_Tests
{
	[TestMethod]
	public async Task ClearChannelPermsEmpty_Test()
	{
		const string INPUT = $"{nameof(Channels.ClearChannelPerms)} {CHANNEL}";

		await ExecuteAsync(INPUT).ConfigureAwait(false);

		var result = await WaitForResultAsync().ConfigureAwait(false);
	}
}