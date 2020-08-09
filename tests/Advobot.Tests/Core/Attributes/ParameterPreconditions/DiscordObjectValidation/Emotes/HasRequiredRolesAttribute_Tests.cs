using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

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
			_Emote = new EmoteCreationArgs
			{
				Id = 73UL,
				Name = "emote name",
				RoleIds = _Roles,
			}.Build();
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