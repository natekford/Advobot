using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class NotManagedAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NotManagedAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotIRole_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task RoleIsManaged_Test()
		{
			var role = new FakeRole(Context.Guild)
			{
				IsManaged = true
			};

			var result = await CheckAsync(role).CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task RoleIsNotManaged_Test()
		{
			var role = new FakeRole(Context.Guild)
			{
				IsManaged = false
			};

			var result = await CheckAsync(role).CAF();
			Assert.AreEqual(true, result.IsSuccess);
		}
	}
}