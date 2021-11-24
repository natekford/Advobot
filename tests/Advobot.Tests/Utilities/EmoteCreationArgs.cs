using Discord;

using System.Reflection;

namespace Advobot.Tests.Utilities
{
	public sealed class EmoteCreationArgs
	{
		private static readonly ConstructorInfo _Constructor = typeof(GuildEmote)
			.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)
			.Single();

		public bool Animated { get; set; }
		public ulong Id { get; set; } = SnowflakeGenerator.UTCNext();
		public bool IsManaged { get; set; }
		public string Name { get; set; }
		public bool RequireColons { get; set; }
		public IReadOnlyList<ulong> RoleIds { get; set; } = Array.Empty<ulong>();
		public ulong? UserId { get; set; }

		public GuildEmote Build()
		{
			return (GuildEmote)_Constructor.Invoke(new object?[]
			{
				Id,
				Name,
				Animated,
				IsManaged,
				RequireColons,
				RoleIds,
				UserId,
			});
		}
	}
}