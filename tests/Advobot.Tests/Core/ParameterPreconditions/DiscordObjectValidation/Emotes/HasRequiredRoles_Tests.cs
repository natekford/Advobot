using Advobot.ParameterPreconditions.Discord.Emotes;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using AdvorangesUtils;

using Discord;

namespace Advobot.Tests.Core.ParameterPreconditions.DiscordObjectValidation.Emotes;

[TestClass]
public sealed class HasRequiredRoles_Tests : ParameterPrecondition_Tests<HasRequiredRoles>
{
	private readonly GuildEmote _Emote = new EmoteCreationArgs
	{
		Id = 73UL,
		Name = "emote name",
#pragma warning disable IDE0028 // Simplify collection initialization
		RoleIds = new List<ulong>(),
#pragma warning restore IDE0028 // Simplify collection initialization
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