using Advobot.AutoMod.Database.Models;

namespace Advobot.AutoMod.Database;

public interface ITimedPunishmentDatabase
{
	Task<int> AddTimedPunishmentAsync(TimedPunishment punishment);

	Task<int> DeleteTimedPunishmentAsync(TimedPunishment punishment);

	Task<int> DeleteTimedPunishmentsAsync(IEnumerable<TimedPunishment> punishments);

	Task<IReadOnlyList<TimedPunishment>> GetExpiredPunishmentsAsync(long ticks);
}