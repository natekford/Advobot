using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Context for a punishment being given or removed.
/// </summary>
public interface IPunishmentContext
{
	/// <summary>
	/// The amount of time the punishment should last for.
	/// </summary>
	public TimeSpan? Duration { get; }
	/// <summary>
	/// The guild for the punishment.
	/// </summary>
	public IGuild Guild { get; }
	/// <summary>
	/// Whether this punishment is being given or removed.
	/// </summary>
	public bool IsGive { get; }
	/// <summary>
	/// The role to give or remove. This will only be used if the punishment involves roles.
	/// </summary>
	public IRole? Role { get; }
	/// <summary>
	/// The type of the punishment.
	/// </summary>
	public PunishmentType Type { get; }
	/// <summary>
	/// The user for the punishment.
	/// </summary>
	public ulong UserId { get; }

	/// <summary>
	/// Punishes the user.
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	public Task ExecuteAsync(RequestOptions? options = null);
}