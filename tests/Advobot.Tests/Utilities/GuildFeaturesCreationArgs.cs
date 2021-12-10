using Discord;

using System.Reflection;

namespace Advobot.Tests.Utilities;

public sealed class GuildFeaturesCreationArgs
{
	private static readonly ConstructorInfo _Constructor = typeof(GuildFeatures)
		.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
		.Single();

	public IReadOnlyCollection<string> Experimental { get; set; } = Array.Empty<string>();
	public GuildFeature Value { get; set; }

	public GuildFeatures Build()
	{
		return (GuildFeatures)_Constructor.Invoke(new object?[]
		{
				Value,
				Experimental
		});
	}
}