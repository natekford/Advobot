using System.Threading.Tasks;

using Advobot.Attributes.Preconditions;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.Preconditions
{
	[TestClass]
	public sealed class RequireGuildOwnerAttribute_Tests
		: ParameterlessPreconditions_TestBase<RequireGuildOwnerAttribute>
	{
		[TestMethod]
		public async Task InvokerIsNotOwner_Test()
		{
			var result = await CheckAsync().CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerIsOwner_Test()
		{
			Context.Guild.FakeOwner = Context.User;

			var result = await CheckAsync().CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}