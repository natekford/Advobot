using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.AutoMod.Models;
using Advobot.AutoMod.ReadOnlyModels;
using Advobot.Databases.AbstractSQL;

using AdvorangesUtils;

using Dapper;

namespace Advobot.AutoMod.Database
{
	public sealed class AutoModDatabase : DatabaseBase<SQLiteConnection>
	{
		public AutoModDatabase(IAutoModDatabaseStarter starter) : base(starter)
		{
		}

		public override async Task<IReadOnlyList<string>> CreateDatabaseAsync()
		{
			await Starter.EnsureCreatedAsync().CAF();

			using var connection = await GetConnectionAsync().CAF();

			//Auto Mod Settings
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS GuildSetting
			(
				GuildId					TEXT NOT NULL,
				Ticks					INTEGER NOT NULL,
				IgnoreAdmins			INTEGER NOT NULL,
				IgnoreHigherHierarchy	INTEGET NOT NULL,
				PRIMARY KEY(GuildId)
			);
			").CAF();

			//Banned Phrases
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS BannedPhrase
			(
				GuildId					TEXT NOT NULL,
				Phrase					TEXT NOT NULL,
				IsContains				INTEGER NOT NULL,
				IsRegex					INTEGER NOT NULL,
				PunishmentType			TEXT NOT NULL,
				PRIMARY KEY(GuildId, Phrase)
			);
			CREATE INDEX IF NOT EXISTS BannedPhrase_GuildId_Index ON BannedPhrase
			(
				GuildId
			);
			").CAF();

			//Persistent Roles
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS PersistentRole
			(
				GuildId					TEXT NOT NULL,
				UserId					TEXT NOT NULL,
				RoleId					TEXT NOT NULL,
				PRIMARY KEY(GuildId, UserId, RoleId)
			);
			CREATE INDEX IF NOT EXISTS PersistentRole_GuildId_Index ON PersistentRole
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS PersistentRole_GuildId_UserId_Index ON PersistentRole
			(
				GuildId,
				UserId
			);
			").CAF();

			//Punishments
			await connection.ExecuteAsync(@"
			CREATE TABLE IF NOT EXISTS Punishment
			(
				GuildId					TEXT NOT NULL,
				PunishmentType			TEXT NOT NULL,
				Instances				INTEGER NOT NULL,
				LengthTicks				INTEGER,
				RoleId					TEXT,
				PRIMARY KEY(GuildId, PunishmentType, Instances)
			);
			CREATE INDEX IF NOT EXISTS Punishment_GuildId_Index ON Punishment
			(
				GuildId
			);
			CREATE INDEX IF NOT EXISTS Punishment_GuildId_PunishmentType_Index ON Punishment
			(
				GuildId,
				PunishmentType
			);
			").CAF();

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
		}

		public Task<AutoModSettings> GetAutoModSettingsAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedNamesAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlyPunishment>> GetBannedPhrasePunishmentsAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedPhrasesAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<ulong>> GetImageOnlyChannelsAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId, ulong userId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlyRaidPrevention>> GetRaidPreventionAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyList<IReadOnlySpamPrevention>> GetSpamPreventionAsync(ulong guildId)
		{
			throw new NotImplementedException();
		}

		protected override Task<int> BulkModifyAsync<TParams>(
			IDbConnection connection,
			string sql,
			IEnumerable<TParams> @params,
			IDbTransaction transaction)
			=> connection.ExecuteAsync(sql, @params, transaction);
	}
}