using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Users;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Fakes.Discord.Users;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Users
{
	[TestClass]
	public sealed class CanModifyUserAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<CanModifyUserAttribute>
	{
		private readonly IRole _HigherRole;
		private readonly IRole _LowerRole;
		private readonly IRole _Role;
		private readonly IGuildUser _User;

		public CanModifyUserAttribute_Tests()
		{
			_HigherRole = new FakeRole(Context.Guild)
			{
				Position = int.MaxValue,
			};
			_LowerRole = new FakeRole(Context.Guild)
			{
				Position = -1,
			};
			_Role = new FakeRole(Context.Guild)
			{
				Position = 1,
			};
			_User = new FakeGuildUser(Context.Guild);
		}

		[TestMethod]
		public async Task BotIsLower_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();
			await _User.AddRoleAsync(_Role).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotIGuildUser_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task FailsOnOwner_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

			var result = await CheckAsync(Context.Guild.FakeOwner).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerAndBotAreHigher_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();
			await _User.AddRoleAsync(_Role).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerIsLower_Test()
		{
			await Context.User.AddRoleAsync(_LowerRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();
			await _User.AddRoleAsync(_Role).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task NeitherHigher_Test()
		{
			await Context.User.AddRoleAsync(_LowerRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();
			await _User.AddRoleAsync(_Role).CAF();

			var result = await CheckAsync(_User).CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}