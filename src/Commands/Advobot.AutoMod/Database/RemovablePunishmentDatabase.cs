using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.AutoMod.Database
{
	public sealed class RemovablePunishmentDatabase : DatabaseBase<SQLiteConnection>, IRemovablePunishmentDatabase
	{
		private const string DELETE_REMOVABLE_PUNISHMENT_SQL = @"
			DELETE FROM RemovablePunishment
			WHERE GuildId = @GuildId AND UserId = @UserId AND PunishmentType = @PunishmentType
		";

		public RemovablePunishmentDatabase(IConnectionStringFor<RemovablePunishmentDatabase> conn) : base(conn)
		{
		}

		public Task<int> AddRemovablePunishmentAsync(IReadOnlyRemovablePunishment punishment)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO RemovablePunishment
				( GuildId, UserId, PunishmentType, EndTimeTicks, RoleId )
				VALUES
				( @GuildId, @UserId, @PunishmentType, @EndTimeTicks, @RoleId )
			", punishment);
		}

		public Task<int> DeleteRemovablePunishmentAsync(IReadOnlyRemovablePunishment punishment)
			=> ModifyAsync(DELETE_REMOVABLE_PUNISHMENT_SQL, punishment);

		public Task<int> DeleteRemovablePunishmentsAsync(IEnumerable<IReadOnlyRemovablePunishment> punishments)
			=> BulkModifyAsync(DELETE_REMOVABLE_PUNISHMENT_SQL, punishments);

		public async Task<IReadOnlyList<IReadOnlyRemovablePunishment>> GetOldPunishmentsAsync(long ticks)
		{
			var param = new { Ticks = ticks };
			return await GetManyAsync<RemovablePunishment>(@"
				SELECT *
				FROM RemovablePunishment
				WHERE EndTimeTicks < @Ticks
			", param).CAF();
		}
	}
}