using Advobot.Punishments;

using Discord;

namespace Advobot.Services.Punishments;

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
	public Task PunishAsync(IPunishment context, RequestOptions? options = null);
}