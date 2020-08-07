using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

using Dapper;

namespace Advobot.AutoMod.Database
{
	public sealed class AutoModDatabase : DatabaseBase<SQLiteConnection>
	{
		public AutoModDatabase(IAutoModDatabaseStarter starter) : base(starter)
		{
		}

		public async Task<IReadOnlyAutoModSettings> GetAutoModSettingsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetOneAsync<AutoModSettings?>(@"
				SELECT *
				FROM GuildSetting
				WHERE GuildId = @GuildId
			", param).CAF() ?? new AutoModSettings(guildId);
		}

		public async Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedNamesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<BannedPhrase>(@"
				SELECT *
				FROM BannedName
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedPhrasesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<BannedPhrase>(@"
				SELECT *
				FROM BannedPhrase
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyChannelSettings?> GetChannelSettingsAsync(ulong channelId)
		{
			var param = new { ChannelId = channelId.ToString(), };
			return await GetOneAsync<ChannelSettings>(@"
				SELECT *
				FROM ChannelSetting
				WHERE ChannelId = @ChannelId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyChannelSettings>> GetChannelSettingsListAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<ChannelSettings>(@"
				SELECT *
				FROM ChannelSetting
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(
			ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<PersistentRole>(@"
				SELECT *
				FROM PersistentRole
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(
			ulong guildId,
			ulong userId)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				UserId = userId.ToString(),
			};
			return await GetManyAsync<PersistentRole>(@"
				SELECT *
				FROM PersistentRole
				WHERE GuildId = @GuildId AND UserId = @UserId
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlyPunishment>> GetPunishmentsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<Punishment>(@"
				SELECT *
				FROM Punishment
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public Task<IReadOnlyList<IReadOnlyRaidPrevention>> GetRaidPreventionAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlySpamPrevention>> GetSpamPreventionAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}
	}
}