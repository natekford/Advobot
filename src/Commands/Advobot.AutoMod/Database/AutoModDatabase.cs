using System.Collections.Generic;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.AutoMod.Database
{
	public sealed class AutoModDatabase : DatabaseBase<SQLiteConnection>, IAutoModDatabase
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
				FROM BannedPhrase
				WHERE GuildId = @GuildId AND IsName = 1
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

		public async Task<IReadOnlyList<IReadOnlyRaidPrevention>> GetRaidPreventionAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<RaidPrevention>(@"
				SELECT *
				FROM RaidPrevention
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyRaidPrevention?> GetRaidPreventionAsync(
			ulong guildId,
			RaidType raidType)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				RaidType = raidType,
			};
			return await GetOneAsync<RaidPrevention?>(@"
				SELECT *
				FROM RaidPrevention
				WHERE GuildId = @GuildId AND RaidType = @RaidType
			", param).CAF();
		}

		public async Task<IReadOnlyList<IReadOnlySpamPrevention>> GetSpamPreventionAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<SpamPrevention>(@"
				SELECT *
				FROM SpamPrevention
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlySpamPrevention?> GetSpamPreventionAsync(
			ulong guildId,
			SpamType spamType)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				SpamType = spamType,
			};
			return await GetOneAsync<SpamPrevention?>(@"
				SELECT *
				FROM SpamPrevention
				WHERE GuildId = @GuildId AND SpamType = @SpamType
			", param).CAF();
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

		public Task<int> UpsertBannedPhraseAsync(IReadOnlyBannedPhrase phrase)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO BannedPhrase
					( GuildId, Phrase, IsContains, IsName, IsRegex, PunishmentType )
					VALUES
					( @GuildId, @Phrase, @IsContains, @IsName, @IsRegex, @PunishmentType );
				UPDATE BannedPhrase
				SET
					IsContains = @IsContains,
					IsName = @IsName,
					IsRegex = @IsRegex,
					PunishmentType = @PunishmentType
				WHERE GuildId = @GuildId AND Phrase = @Phrase
			", phrase);
		}

		public Task<int> UpsertChannelSettings(IReadOnlyChannelSettings settings)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO ChannelSetting
					( GuildId, ChannelId, IsImageOnly )
					VALUES
					( @GuildId, @ChannelId, @IsImageOnly );
				UPDATE ChannelSetting
				SET
					IsImageOnly = @IsImageOnly
				WHERE ChannelId = @ChannelId
			", settings);
		}

		public Task<int> UpsertRaidPreventionAsync(IReadOnlyRaidPrevention prevention)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO RaidPrevention
					( GuildId, PunishmentType, Instances, LengthTicks, RoleId, Enabled, IntervalTicks, Size, RaidType )
					VALUES
					( @GuildId, @PunishmentType, @Instances, @LengthTicks, @RoleId, @Enabled, @IntervalTicks, @Size, @RaidType );
				UPDATE RaidPrevention
				SET
					PunishmentType = @PunishmentType,
					Instances = @Instances,
					LengthTicks = @LengthTicks,
					RoleId = @RoleId,
					Enabled = @Enabled,
					IntervalTicks = @IntervalTicks,
					Size = @Size
				WHERE GuildId = @GuildId AND RaidType = @RaidType
			", prevention);
		}

		public Task<int> UpsertSpamPreventionAsync(IReadOnlySpamPrevention prevention)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO SpamPrevention
					( GuildId, PunishmentType, Instances, LengthTicks, RoleId, Enabled, IntervalTicks, Size, SpamType )
					VALUES
					( @GuildId, @PunishmentType, @Instances, @LengthTicks, @RoleId, @Enabled, @IntervalTicks, @Size, @SpamType );
				UPDATE SpamPrevention
				SET
					PunishmentType = @PunishmentType,
					Instances = @Instances,
					LengthTicks = @LengthTicks,
					RoleId = @RoleId,
					Enabled = @Enabled,
					IntervalTicks = @IntervalTicks,
					Size = @Size
				WHERE GuildId = @GuildId AND SpamType = @SpamType
			", prevention);
		}
	}
}