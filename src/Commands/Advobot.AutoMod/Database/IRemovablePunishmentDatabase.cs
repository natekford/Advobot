
using Advobot.AutoMod.Models;

namespace Advobot.AutoMod.Database
{
	public interface IRemovablePunishmentDatabase
	{
		Task<int> AddRemovablePunishmentAsync(RemovablePunishment punishment);

		Task<int> DeleteRemovablePunishmentAsync(RemovablePunishment punishment);

		Task<int> DeleteRemovablePunishmentsAsync(IEnumerable<RemovablePunishment> punishments);

		Task<IReadOnlyList<RemovablePunishment>> GetOldPunishmentsAsync(long ticks);
	}
}