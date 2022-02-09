using Advobot.ParameterPreconditions.DiscordObjectValidation.Emotes;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Emotes;

[TestClass]
public sealed class HasRequiredRoles_Tests : ParameterPrecondition_Tests<HasRequiredRoles>
{
	private readonly GuildEmote _Emote = new EmoteCreationArgs
	{
		Id = 73UL,
		Name = "emote name",
		RoleIds = new List<ulong>(),
	}.Build();

	protected override HasRequiredRoles Instance { get; } = new();

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