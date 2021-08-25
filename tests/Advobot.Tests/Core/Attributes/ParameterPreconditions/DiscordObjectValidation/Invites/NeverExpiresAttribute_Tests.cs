using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	[TestClass]
	public sealed class NeverExpiresAttribute_Tests
		: ParameterPreconditionTestsBase<NeverExpiresAttribute>
	{
		protected override NeverExpiresAttribute Instance { get; } = new();

		[TestMethod]
		public async Task InviteExpires_Test()
		{
			await AssertFailureAsync(new FakeInviteMetadata(Context.Channel, Context.User)
			{
				MaxAge = 3600,
			}).CAF();
		}

		[TestMethod]
		public async Task InviteNeverExpires_Test()
		{
			await AssertSuccessAsync(new FakeInviteMetadata(Context.Channel, Context.User)
			{
				MaxAge = null,
			}).CAF();
		}
	}
}