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
	/// <returns></returns>
	public Task HandleAsync(IPunishmentContext context);
}