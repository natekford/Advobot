using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class NotMentionableAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NotMentionableAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotIRole_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task RoleIsMentionable_Test()
		{
			var role = new FakeRole(Context.Guild)
			{
				IsMentionable = true
			};

			var result = await CheckAsync(role).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task RoleIsNotMentionable_Test()
		{
			var role = new FakeRole(Context.Guild)
			{
				IsMentionable = false
			};

			var result = await CheckAsync(role).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}