using Discord;

namespace Advobot.Punishments;

/// <summary>
/// Add and remove punishments for guild users.
/// </summary>
public interface IPunishmentService
{
	/// <summary>
	/// Handles a punishment context.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="options"></param>
	/// <returns></returns>
	public Task PunishAsync(IPunishmentContext context, RequestOptions? options = null);
}