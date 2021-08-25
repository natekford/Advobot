
using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Channels;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Invites
{
	[TestClass]
	public sealed class FromThisGuildAttribute_Tests
		: ParameterPreconditionTestsBase<FromThisGuildAttribute>
	{
		protected override FromThisGuildAttribute Instance { get; } = new();

		[TestMethod]
		public async Task FromThisGuild_Test()
		{
			var invite = new FakeInviteMetadata(Context.Channel, Context.User);

			var result = await CheckPermissionsAsync(invite).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task NotFromThisGuild_Test()
		{
			var guild = new FakeGuild(Context.Client);
			var channel = new FakeTextChannel(guild);
			var user = new FakeGuildUser(guild);
			var invite = new FakeInviteMetadata(channel, user);

			var result = await CheckPermissionsAsync(invite).CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}