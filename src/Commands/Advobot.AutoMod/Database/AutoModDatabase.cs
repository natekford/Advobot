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
	//Path.Combine("SQLite", "AutoMod.db")
	public sealed class AutoModDatabase : DatabaseBase<SQLiteConnection>
	{
		public AutoModDatabase(IConnectionStringFor<AutoModDatabase> conn) : base(conn)
		{
		}

		public Task<int> AddPersistentRoleAsync(IReadOnlyPersistentRole role)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO PersistentRole
				( GuildId, UserId, RoleId )
				VALUES
				( @GuildId, @UserId, @RoleId )
			", role);
		}

		public Task<int> DeleteBannedNameAsync(IReadOnlyBannedPhrase name)
		{
			return ModifyAsync(@"
				DELETE FROM BannedName
				WHERE GuildId = @GuildId AND Phrase = @Phrase
			", name);
		}

		public Task<int> DeletedBannedPhraseAsync(IReadOnlyBannedPhrase phrase)
		{
			return ModifyAsync(@"
				DELETE FROM BannedPhrase
				WHERE GuildId = @GuildId AND Phrase = @Phrase
			", phrase);
		}

		public Task<int> DeletePersistentRoleAsync(IReadOnlyPersistentRole role)
		{
			return ModifyAsync(@"
				DELETE FROM PersistentRole
				WHERE GuildId = @GuildId AND UserId = @UserId AND RoleId = @RoleId
			", role);
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

		public async Task<IReadOnlyList<IReadOnlyChannelSettings>> GetChannelSettingsListAsync(
			ulong guildId)
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

		public Task<int> UpsertAutoModSettingsAsync(IReadOnlyAutoModSettings settings)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO GuildSetting
					( GuildId, Ticks, IgnoreAdmins, IgnoreHigherHierarchy )
					VALUES
					( @GuildId, @Ticks, @IgnoreAdmins, @IgnoreHigherHierarchy );
				UPDATE GuildSetting
				SET
					Ticks = @Ticks,
					IgnoreAdmins = @IgnoreAdmins,
					IgnoreHigherHierarchy = @IgnoreHigherHierarchy
				WHERE GuildId = @GuildId
			", settings);
		}

		public Task<int> UpsertBannedNameAsync(IReadOnlyBannedPhrase name)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO BannedName
					( GuildId, Phrase, IsContains, IsRegex, PunishmentType )
					VALUES
					( @GuildId, @Phrase, @IsContains, @IsRegex, @PunishmentType );
				UPDATE BannedName
				SET
					IsContains = @IsContains,
					IsRegex = @IsRegex,
					PunishmentType = @PunishmentType
				WHERE GuildId = @GuildId AND Phrase = @Phrase
			", name);
		}

		public Task<int> UpsertBannedPhraseAsync(IReadOnlyBannedPhrase phrase)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO BannedPhrase
					( GuildId, Phrase, IsContains, IsRegex, PunishmentType )
					VALUES
					( @GuildId, @Phrase, @IsContains, @IsRegex, @PunishmentType );
				UPDATE BannedPhrase
				SET
					IsContains = @IsContains,
					IsRegex = @IsRegex,
					PunishmentType = @PunishmentType
				WHERE GuildId = @GuildId AND Phrase = @Phrase
			", phrase);
		}

		public Task<int> UpsertChannelSettings(IReadOnlyChannelSettings settings)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO ChannelSetting
					( GuildId, ChannelId, ImageOnly )
					VALUES
					( @GuildId, @ChannelId, @ImageOnly );
				UPDATE ChannelSetting
				SET
					ImageOnly = @ImageOnly
				WHERE ChannelId = @ChannelId
			", settings);
		}
	}
}