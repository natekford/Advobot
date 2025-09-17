using Discord;

using System.Reflection;

namespace Advobot.Tests.Fakes.Discord;

public sealed record FakeGuildFeatures
{
	private static readonly ConstructorInfo _Constructor = typeof(GuildFeatures)
		.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
		.Single();

	public IReadOnlyCollection<string> Experimental { get; set; } = [];
	public GuildFeature Value { get; set; }

	public GuildFeatures Build()
	{
		return (GuildFeatures)_Constructor.Invoke(
		[
			Value,
			Experimental
		]);
	}
}