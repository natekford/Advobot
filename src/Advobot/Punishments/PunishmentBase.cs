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
/// <param name="type"></param>
public abstract class PunishmentBase(IGuild guild, ulong userId, bool isGive, PunishmentType type)
	: IPunishmentContext
{
	/// <inheritdoc/>
	public int Days { get; set; } = 1;
	/// <inheritdoc />
	public IGuild Guild { get; protected set; } = guild;
	/// <inheritdoc />
	public bool IsGive { get; protected set; } = isGive;
	/// <inheritdoc />
	public RequestOptions? Options { get; set; }
	/// <inheritdoc />
	public IRole? Role { get; protected set; }
	/// <inheritdoc />
	public TimeSpan? Time { get; set; }
	/// <inheritdoc />
	public PunishmentType Type { get; protected set; } = type;
	/// <inheritdoc />
	public ulong UserId { get; protected set; } = userId;

	/// <inheritdoc />
	public abstract Task ExecuteAsync();
}