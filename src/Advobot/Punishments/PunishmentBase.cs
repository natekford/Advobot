using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Context for a punishment being given or removed.
/// </summary>
/// <remarks>
/// Creates an instance of <see cref="PunishmentBase"/>.
/// </remarks>
/// <param name="guild"></param>
/// <param name="userId"></param>
/// <param name="isGive"></param>
public abstract class PunishmentBase(IGuild guild, ulong userId, bool isGive)
	: IPunishment
{
	/// <inheritdoc />
	public TimeSpan? Duration { get; set; }
	/// <inheritdoc />
	public IGuild Guild { get; init; } = guild;
	/// <inheritdoc />
	public bool IsGive { get; init; } = isGive;
	/// <inheritdoc />
	public ulong RoleId { get; init; }
	/// <inheritdoc />
	public abstract PunishmentType Type { get; }
	/// <inheritdoc />
	public ulong UserId { get; init; } = userId;

	/// <inheritdoc />
	public abstract Task ExecuteAsync(RequestOptions? options = null);
}