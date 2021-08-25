
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class NotMentionableAttribute_Tests
		: ParameterPreconditionTestsBase<NotMentionableAttribute>
	{
		protected override NotMentionableAttribute Instance { get; } = new();

		[TestMethod]
		public async Task RoleIsMentionable_Test()
		{
			var result = await CheckPermissionsAsync(new FakeRole(Context.Guild)
			{
				IsMentionable = true
			}).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task RoleIsNotMentionable_Test()
		{
			var result = await CheckPermissionsAsync(new FakeRole(Context.Guild)
			{
				IsMentionable = false
			}).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}