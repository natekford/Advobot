using Advobot.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.Attributes.ParameterPreconditions.DiscordObjectValidation.Emotes;

[TestClass]
public sealed class HasRequiredRolesAttribute_Tests
	: ParameterPreconditionTestsBase<HasRequiredRolesAttribute>
{
	private readonly GuildEmote _Emote = new EmoteCreationArgs
	{
		Id = 73UL,
		Name = "emote name",
		RoleIds = new List<ulong>(),
	}.Build();

	protected override HasRequiredRolesAttribute Instance { get; } = new();

	[TestMethod]
	public async Task DoesNotHaveRequiredRoles_Test()
		=> await AssertFailureAsync(_Emote).CAF();

	[TestMethod]
	public async Task HasRequiredRoles_Test()
	{
		((IList<ulong>)_Emote.RoleIds).Add(35);

		await AssertSuccessAsync(_Emote).CAF();
	}
}