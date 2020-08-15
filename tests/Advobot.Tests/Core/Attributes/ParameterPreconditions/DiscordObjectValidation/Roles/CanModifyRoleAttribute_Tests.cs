using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Discord;
using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Roles
{
	[TestClass]
	public sealed class CanModifyRoleAttribute_Tests : ParameterPreconditionTestsBase
	{
		private readonly IRole _HigherRole;
		private readonly IRole _LowerRole;
		private readonly IRole _Role;
		protected override ParameterPreconditionAttribute Instance { get; }
			= new CanModifyRoleAttribute();

		public CanModifyRoleAttribute_Tests()
		{
			_HigherRole = new FakeRole(Context.Guild) { Position = 1, };
			_LowerRole = new FakeRole(Context.Guild) { Position = -1, };
			_Role = new FakeRole(Context.Guild) { Position = 0, };
		}

		[TestMethod]
		public async Task BotIsLower_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();

			var result = await CheckPermissionsAsync(_Role).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerAndBotAreHigher_Test()
		{
			await Context.User.AddRoleAsync(_HigherRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

			var result = await CheckPermissionsAsync(_Role).CAF();
			Assert.IsTrue(result.IsSuccess);
		}

		[TestMethod]
		public async Task InvokerIsLower_Test()
		{
			await Context.User.AddRoleAsync(_LowerRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_HigherRole).CAF();

			var result = await CheckPermissionsAsync(_Role).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task NeitherHigher_Test()
		{
			await Context.User.AddRoleAsync(_LowerRole).CAF();
			await Context.Guild.FakeCurrentUser.AddRoleAsync(_LowerRole).CAF();

			var result = await CheckPermissionsAsync(_Role).CAF();
			Assert.IsFalse(result.IsSuccess);
		}
	}
}