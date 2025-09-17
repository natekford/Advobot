using Advobot.Tests.Utilities;

using Discord;

using System.Reflection;

namespace Advobot.Tests.Fakes.Discord;

public sealed record FakeGuildEmoji
{
	private static readonly ConstructorInfo _Constructor = typeof(GuildEmote)
		.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
		.Single();

	public bool Animated { get; set; }
	public ulong Id { get; set; } = SnowflakeGenerator.UTCNext();
	public bool? IsAvailable { get; set; }
	public bool IsManaged { get; set; }
	public string Name { get; set; }
	public bool RequireColons { get; set; }
	public IReadOnlyList<ulong> RoleIds { get; set; } = [];
	public ulong? UserId { get; set; }

	public GuildEmote Build()
	{
		return (GuildEmote)_Constructor.Invoke(
		[
			Id,
			Name,
			Animated,
			IsManaged,
			RequireColons,
			RoleIds,
			UserId,
			IsAvailable
		]);
	}
}