using System.Collections.Generic;
using System.Threading.Tasks;

using Advobot.AutoMod.ReadOnlyModels;

namespace Advobot.AutoMod.Database
{
	public interface IRemovablePunishmentDatabase
	{
		Task<int> AddRemovablePunishmentAsync(IReadOnlyRemovablePunishment punishment);

		Task<int> DeleteRemovablePunishmentAsync(IReadOnlyRemovablePunishment punishment);

		Task<int> DeleteRemovablePunishmentsAsync(IEnumerable<IReadOnlyRemovablePunishment> punishments);

		Task<IReadOnlyList<IReadOnlyRemovablePunishment>> GetOldPunishmentsAsync(long ticks);
	}
}