using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.PreconditionTestsBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class NotEveryoneAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NotEveryoneAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotIRole_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task RoleIsEveryone_Test()
		{
			var role = Context.Guild.FakeEveryoneRole;

			var result = await CheckAsync(role).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task RoleIsNotEveryone_Test()
		{
			var role = new FakeRole(Context.Guild)
			{
				Id = 73,
			};

			var result = await CheckAsync(role).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}