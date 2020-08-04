using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
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
	}
}