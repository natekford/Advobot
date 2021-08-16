using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.Numbers;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.Numbers
{
	[TestClass]
	public sealed class NotBotOwnerAttribute_Tests
		: ParameterPreconditionTestsBase<NotBotOwnerAttribute>
	{
		protected override NotBotOwnerAttribute Instance { get; } = new();

		[TestMethod]
		public async Task Invalid_Test()
		{
			Context.Client.FakeApplication.Owner = Context.User;

			var result = await CheckPermissionsAsync(Context.User.Id).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task Valid_Test()
		{
			var result = await CheckPermissionsAsync(Context.User.Id).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}