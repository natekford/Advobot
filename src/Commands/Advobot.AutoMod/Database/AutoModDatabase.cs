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

			return await connection.GetTableNames((c, sql) => c.QueryAsync<string>(sql)).CAF();
		}

		public Task<AutoModSettings> GetAutoModSettingsAsync(ulong guildId)
		{
		}

		public Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedNamesAsync(ulong guildId)
		{
		}

		public Task<IReadOnlyList<IReadOnlyPunishment>> GetBannedPhrasePunishmentsAsync(ulong guildId)
		{
		}

		public Task<IReadOnlyList<IReadOnlyBannedPhrase>> GetBannedPhrasesAsync(ulong guildId)
		{
		}

		public Task<IReadOnlyList<ulong>> GetImageOnlyChannelsAsync(ulong guildId)
		{
		}

		public Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId)
		{
		}

		public Task<IReadOnlyList<IReadOnlyPersistentRole>> GetPersistentRolesAsync(ulong guildId, ulong userId)
		{
		}

		public Task<IReadOnlyList<IReadOnlySpamPrevention>> GetSpamPreventionAsync(ulong guildId)
		{
		}

		protected override Task<int> BulkModifyAsync<TParams>(
			IDbConnection connection,
			string sql,
			IEnumerable<TParams> @params,
			IDbTransaction transaction)
			=> connection.ExecuteAsync(sql, @params, transaction);
	}
}