using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class NotBotOwnerAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NotBotOwnerAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotUlong_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync("")).CAF();

		[TestMethod]
		public async Task Invalid_Test()
		{
			Context.Client.FakeApplication.Owner = Context.User;

			var result = await CheckAsync(Context.User.Id).CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await CheckAsync(Context.User.Id).CAF();
			Assert.AreEqual(true, result.IsSuccess);
		}
	}
}