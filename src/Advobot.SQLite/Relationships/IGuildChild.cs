namespace Advobot.SQLite.Relationships;

/// <summary>
/// Represents an object which belongs to a guild.
/// </summary>
public interface IGuildChild
{
	/// <summary>
	/// The guild's id.
	/// </summary>
	ulong GuildId { get; }
}