using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Tests.Fakes.Discord;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	[TestClass]
	public sealed class NeverExpiresAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<NeverExpiresAttribute>
	{
		[TestMethod]
		public async Task FailsOnNotIInviteMetadata_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task InviteExpires_Test()
		{
			var invite = new FakeInviteMetadata(Context.Channel, Context.User)
			{
				MaxAge = 3600,
			};

			var result = await CheckAsync(invite).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InviteNeverExpires_Test()
		{
			var invite = new FakeInviteMetadata(Context.Channel, Context.User)
			{
				MaxAge = null,
			};

			var result = await CheckAsync(invite).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}