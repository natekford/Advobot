using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.AutoMod.Database
{
	public sealed class AutoModDatabase : DatabaseBase<SQLiteConnection>, IAutoModDatabase
	{
		public AutoModDatabase(IConnectionStringFor<AutoModDatabase> conn) : base(conn)
		{
		}

		public Task<int> AddPersistentRoleAsync(PersistentRole role)
		{
			return ModifyAsync(@"
				INSERT OR IGNORE INTO PersistentRole
				( GuildId, UserId, RoleId )
				VALUES
				( @GuildId, @UserId, @RoleId )
			", role);
		}

		public Task<int> DeletedBannedPhraseAsync(BannedPhrase phrase)
		{
			return ModifyAsync(@"
				DELETE FROM BannedPhrase
				WHERE GuildId = @GuildId AND Phrase = @Phrase
			", phrase);
		}

		public Task<int> DeletePersistentRoleAsync(PersistentRole role)
		{
			return ModifyAsync(@"
				DELETE FROM PersistentRole
				WHERE GuildId = @GuildId AND UserId = @UserId AND RoleId = @RoleId
			", role);
		}

		public Task<int> DeleteSelfRolesAsync(IEnumerable<ulong> roles)
		{
			return BulkModifyAsync(@"
				DELETE FROM SelfRole
				WHERE RoleId = @RoleId
			", roles.Select(x => new { RoleId = x.ToString() }));
		}

		public Task<int> DeleteSelfRolesGroupAsync(ulong guildId, int groupId)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				GroupId = groupId,
			};
			return ModifyAsync(@"
				DELETE FROM SelfRole
				WHERE GuildId = @GuildId AND GroupId = @GroupId
			", param);
		}

		public async Task<AutoModSettings> GetAutoModSettingsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetOneAsync<AutoModSettings?>(@"
				SELECT *
				FROM GuildSetting
				WHERE GuildId = @GuildId
			", param).CAF() ?? new AutoModSettings { GuildId = guildId };
		}

		public async Task<IReadOnlyList<BannedPhrase>> GetBannedNamesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<BannedPhrase>(@"
				SELECT *
				FROM BannedPhrase
				WHERE GuildId = @GuildId AND IsName = 1
			", param).CAF();
		}

		public async Task<IReadOnlyList<BannedPhrase>> GetBannedPhrasesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<BannedPhrase>(@"
				SELECT *
				FROM BannedPhrase
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<ChannelSettings?> GetChannelSettingsAsync(ulong channelId)
		{
			var param = new { ChannelId = channelId.ToString(), };
			return await GetOneAsync<ChannelSettings>(@"
				SELECT *
				FROM ChannelSetting
				WHERE ChannelId = @ChannelId
			", param).CAF();
		}

		public async Task<IReadOnlyList<ChannelSettings>> GetChannelSettingsListAsync(
			ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<ChannelSettings>(@"
				SELECT *
				FROM ChannelSetting
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<PersistentRole>> GetPersistentRolesAsync(
			ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<PersistentRole>(@"
				SELECT *
				FROM PersistentRole
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<PersistentRole>> GetPersistentRolesAsync(
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

		public async Task<IReadOnlyList<Punishment>> GetPunishmentsAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<Punishment>(@"
				SELECT *
				FROM Punishment
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<RaidPrevention>> GetRaidPreventionAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<RaidPrevention>(@"
				SELECT *
				FROM RaidPrevention
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<RaidPrevention?> GetRaidPreventionAsync(
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

		public async Task<SelfRole?> GetSelfRoleAsync(ulong roleId)
		{
			var param = new { RoleId = roleId.ToString() };
			return await GetOneAsync<SelfRole>(@"
				SELECT *
				FROM SelfRole
				WHERE RoleId = @RoleId
			", param).CAF();
		}

		public async Task<IReadOnlyList<SelfRole>> GetSelfRolesAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString() };
			return await GetManyAsync<SelfRole>(@"
				SELECT *
				FROM SelfRole
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<IReadOnlyList<SelfRole>> GetSelfRolesAsync(ulong guildId, int groupId)
		{
			var param = new
			{
				GuildId = guildId.ToString(),
				GroupId = groupId
			};
			return await GetManyAsync<SelfRole>(@"
				SELECT *
				FROM SelfRole
				WHERE GuildId = @GuildId AND GroupId = @GroupId
			", param).CAF();
		}

		public async Task<IReadOnlyList<SpamPrevention>> GetSpamPreventionAsync(ulong guildId)
		{
			var param = new { GuildId = guildId.ToString(), };
			return await GetManyAsync<SpamPrevention>(@"
				SELECT *
				FROM SpamPrevention
				WHERE GuildId = @GuildId
			", param).CAF();
		}

		public async Task<SpamPrevention?> GetSpamPreventionAsync(
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

		public Task<int> UpsertAutoModSettingsAsync(AutoModSettings settings)
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

		public Task<int> UpsertBannedPhraseAsync(BannedPhrase phrase)
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

		public Task<int> UpsertChannelSettings(ChannelSettings settings)
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

		public Task<int> UpsertRaidPreventionAsync(RaidPrevention prevention)
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

		public Task<int> UpsertSelfRolesAsync(IEnumerable<SelfRole> roles)
		{
			return BulkModifyAsync(@"
				INSERT OR IGNORE INTO SelfRole
					( GuildId, RoleId, GroupId )
					VALUES
					( @GuildId, @RoleId, @GroupId );
				UPDATE SelfRole
				SET
					GroupId = @GroupId
				WHERE RoleId = @RoleId
			", roles);
		}

		public Task<int> UpsertSpamPreventionAsync(SpamPrevention prevention)
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