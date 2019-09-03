using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;

using AdvorangesUtils;
using Discord;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes
{
	[TestClass]
	public sealed class HasRequiredRolesAttribute_Tests
		: ParameterlessParameterPreconditions_TestsBase<HasRequiredRolesAttribute>
	{
		private readonly GuildEmote _Emote;
		private readonly List<ulong> _Roles;

		public HasRequiredRolesAttribute_Tests()
		{
			_Roles = new List<ulong>();

			var constructor = typeof(GuildEmote)
				.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
				.Single();
			var args = new object[]
			{
				73UL,
				"emote name",
				false,
				false,
				false,
				_Roles,
				null,
			};
			_Emote = (GuildEmote)constructor.Invoke(args);
		}

		[TestMethod]
		public async Task DoesNotHaveRequiredRoles_Test()
		{
			var result = await CheckAsync(_Emote).CAF();
			Assert.IsFalse(result.IsSuccess);
		}

		[TestMethod]
		public async Task FailsOnNotGuildEmote_Test()
			=> await AssertPreconditionFailsOnInvalidType(CheckAsync(1)).CAF();

		[TestMethod]
		public async Task HasRequiredRoles_Test()
		{
			_Roles.Add(35);

			var result = await CheckAsync(_Emote).CAF();
			Assert.IsTrue(result.IsSuccess);
		}
	}
}