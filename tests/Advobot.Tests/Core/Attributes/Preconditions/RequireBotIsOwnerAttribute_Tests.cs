using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireBotIsOwnerAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireBotIsOwnerAttribute>
	{
		[TestMethod]
		public async Task BotIsNotOwner_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task BotIsOwner_Test()
		{
			Context.Guild.FakeOwner = Context.Guild.FakeCurrentUser;

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}