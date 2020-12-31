using System.Data.SQLite;
using System.Threading.Tasks;

using Advobot.MyCommands.Models;
using Advobot.SQLite;

using AdvorangesUtils;

namespace Advobot.MyCommands.Database
{
	public sealed class MyCommandsDatabase : DatabaseBase<SQLiteConnection>, IMyCommandsDatabase
	{
		public MyCommandsDatabase(IConnectionStringFor<MyCommandsDatabase> conn) : base(conn)
		{
		}

		public async Task<DetectLanguageConfig> GetDetectLanguageConfig()
		{
			return await GetOneAsync<DetectLanguageConfig>(@"
				SELECT * From DetectLanguageConfig
			", new object()).CAF() ?? new DetectLanguageConfig();
		}

		public Task<int> UpsertDetectLanguageConfig(DetectLanguageConfig config)
		{
			return ModifyAsync(@"
				INSERT INTO DetectLanguageConfig
				( APIKey, ConfidenceLimit, CooldownStartTicks )
				VALUES
				( @APIKey, @ConfidenceLimit, @CooldownStartTicks )
			", config);
		}
	}
}