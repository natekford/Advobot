using Advobot.AutoMod.Models;

namespace Advobot.AutoMod.Database;

public interface ITimedPunishmentDatabase
{
	Task<int> AddRemovablePunishmentAsync(RemovablePunishment punishment);

	Task<int> DeleteRemovablePunishmentAsync(RemovablePunishment punishment);

	Task<int> DeleteRemovablePunishmentsAsync(IEnumerable<RemovablePunishment> punishments);

	Task<IReadOnlyList<RemovablePunishment>> GetOldPunishmentsAsync(long ticks);
}