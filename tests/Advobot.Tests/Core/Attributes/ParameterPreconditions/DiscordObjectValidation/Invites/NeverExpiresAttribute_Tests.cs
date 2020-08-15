using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	[TestClass]
	public sealed class NeverExpiresAttribute_Tests : ParameterPreconditionTestsBase
	{
		protected override ParameterPreconditionAttribute Instance { get; }
			= new NeverExpiresAttribute();

		[TestMethod]
		public async Task InviteExpires_Test()
		{
			var invite = new FakeInviteMetadata(Context.Channel, Context.User)
			{
				MaxAge = 3600,
			};

			var result = await CheckPermissionsAsync(invite).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InviteNeverExpires_Test()
		{
			var invite = new FakeInviteMetadata(Context.Channel, Context.User)
			{
				MaxAge = null,
			};

			var result = await CheckPermissionsAsync(invite).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}