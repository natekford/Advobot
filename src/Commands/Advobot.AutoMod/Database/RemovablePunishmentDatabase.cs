using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.AutoMod.Database
{
	public sealed class RemovablePunishmentDatabase : DatabaseBase<SQLiteConnection>
	{
		public RemovablePunishmentDatabase(IConnectionStringFor<RemovablePunishmentDatabase> conn) : base(conn)
		{
		}

		public Task<int> AddRemovablePunishment(IReadOnlyRemovablePunishment punishment)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO RemovablePunishment
				( GuildId, UserId, PunishmentType, EndTimeTicks, RoleId )
				VALUES
				( @GuildId, @UserId, @PunishmentType, @EndTimeTicks, @RoleId )
			", punishment);
		}

		public Task<int> DeleteRemovablePunishment(IReadOnlyRemovablePunishment punishment)
		{
			return ModifyAsync(@"
				DELETE FROM RemovablePunishment
				WHERE GuildId = @GuildId AND UserId = @UserId AND PunishmentType = @PunishmentType
			", punishment);
		}

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