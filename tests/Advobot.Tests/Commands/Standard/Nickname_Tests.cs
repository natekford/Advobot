using Advobot.Standard.Commands;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Advobot.Tests.Commands.Standard;

[TestClass]
public sealed class Nickname_Tests : Command_Tests
{
	[TestMethod]
	public async Task RemoveAllNicknames_Test()
	{
		var user = new FakeGuildUser(Context.Guild)
		{
			Nickname = "asdf"
		};

		var input = $"{nameof(Nicknames.RemoveAllNickNames)} true";

		var result = await ExecuteWithResultAsync(input).ConfigureAwait(false);
		Assert.IsTrue(result.InnerResult.IsSuccess);
		Assert.IsNull(user.Nickname);
	}
}