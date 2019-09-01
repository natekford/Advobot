using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using AdvorangesUtils;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class CanModifyRoleAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<CanModifyRoleAttribute>
	{
		private readonly IRole _HigherRole;
		private readonly IRole _LowerRole;
		private readonly IRole _Role;

		public CanModifyRoleAttribute_Tests()
		{
			_HigherRole = new FakeRole(Context.Guild)
			{
				Position = int.MaxValue,
			};
			_Role = new FakeRole(Context.Guild)
			{
				Position = 1,
			};
			_LowerRole = new FakeRole(Context.Guild)
			{
				Position = -1,
			};
		}

		[TestMethod]
		public async Task BotIsLower_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();

			var result = await CheckAsync(_Role).CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotIRole_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task InvokerAndBotAreHigher_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

			var result = await CheckAsync(_Role).CAF();
			Assert.AreEqual(true, result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerIsLower_Test()
		{
			await Context.User.AddRoleAsync(_LowerRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

			var result = await CheckAsync(_Role).CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}

		[TestMethod]
		public async Task NeitherHigher_Test()
		{
			await Context.User.AddRoleAsync(_LowerRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();

			var result = await CheckAsync(_Role).CAF();
			Assert.AreEqual(false, result.IsSuccess);
		}
	}
}