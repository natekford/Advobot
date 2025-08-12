using Advobot.AutoMod.Database.Models;
using Advobot.SQLite;

using System.Data.SQLite;

namespace Advobot.AutoMod.Database;

public sealed class TimedPunishmentDatabase(IConnectionString<TimedPunishmentDatabase> conn) : DatabaseBase<SQLiteConnection>(conn), ITimedPunishmentDatabase
{
	private const string DELETE_REMOVABLE_PUNISHMENT_SQL = @"
		DELETE FROM TimedPunishment
		WHERE GuildId = @GuildId AND UserId = @UserId AND PunishmentType = @PunishmentType
	";

	public Task<int> AddTimedPunishmentAsync(TimedPunishment punishment)
	{
		return ModifyAsync(@"
			INSERT OR IGNORE INTO TimedPunishment
			( GuildId, UserId, PunishmentType, EndTimeTicks, RoleId )
			VALUES
			( @GuildId, @UserId, @PunishmentType, @EndTimeTicks, @RoleId )
		", punishment);
	}

	public Task<int> DeleteTimedPunishmentAsync(TimedPunishment punishment)
		=> ModifyAsync(DELETE_REMOVABLE_PUNISHMENT_SQL, punishment);

	public Task<int> DeleteTimedPunishmentsAsync(IEnumerable<TimedPunishment> punishments)
		=> BulkModifyAsync(DELETE_REMOVABLE_PUNISHMENT_SQL, punishments);

	public async Task<IReadOnlyList<TimedPunishment>> GetExpiredPunishmentsAsync(long ticks)
	{
		var param = new { Ticks = ticks };
		return await GetManyAsync<TimedPunishment>(@"
			SELECT *
			FROM TimedPunishment
			WHERE EndTimeTicks < @Ticks
		", param).ConfigureAwait(false);
	}
}