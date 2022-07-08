using Advobot.AutoMod.Models;
using Advobot.SQLite;

using AdvorangesUtils;

using System.Data.SQLite;

namespace Advobot.AutoMod.Database;

public sealed class RemovablePunishmentDatabase : DatabaseBase<SQLiteConnection>, IRemovablePunishmentDatabase
{
	private const string DELETE_REMOVABLE_PUNISHMENT_SQL = @"
			DELETE FROM RemovablePunishment
			WHERE GuildId = @GuildId AND UserId = @UserId AND PunishmentType = @PunishmentType
		";

	public RemovablePunishmentDatabase(IConnectionString<RemovablePunishmentDatabase> conn) : base(conn)
	{
	}

	public Task<int> AddRemovablePunishmentAsync(RemovablePunishment punishment)
	{
		return ModifyAsync(@"
				INSERT OR IGNORE INTO RemovablePunishment
				( GuildId, UserId, PunishmentType, EndTimeTicks, RoleId )
				VALUES
				( @GuildId, @UserId, @PunishmentType, @EndTimeTicks, @RoleId )
			", punishment);
	}

	public Task<int> DeleteRemovablePunishmentAsync(RemovablePunishment punishment)
		=> ModifyAsync(DELETE_REMOVABLE_PUNISHMENT_SQL, punishment);

	public Task<int> DeleteRemovablePunishmentsAsync(IEnumerable<RemovablePunishment> punishments)
		=> BulkModifyAsync(DELETE_REMOVABLE_PUNISHMENT_SQL, punishments);

	public async Task<IReadOnlyList<RemovablePunishment>> GetOldPunishmentsAsync(long ticks)
	{
		var param = new { Ticks = ticks };
		return await GetManyAsync<RemovablePunishment>(@"
				SELECT *
				FROM RemovablePunishment
				WHERE EndTimeTicks < @Ticks
			", param).CAF();
	}
}