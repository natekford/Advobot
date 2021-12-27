using Advobot.MyCommands.Models;
using Advobot.SQLite;

using AdvorangesUtils;

using System.Data.SQLite;

namespace Advobot.MyCommands.Database;

public sealed class MyCommandsDatabase : DatabaseBase<SQLiteConnection>, IMyCommandsDatabase
{
	public MyCommandsDatabase(IConnectionStringFor<MyCommandsDatabase> conn) : base(conn)
	{
	}

	public async Task<DetectLanguageConfig> GetDetectLanguageConfigAsync()
	{
		return await GetOneAsync<DetectLanguageConfig>(@"
				SELECT * From DetectLanguageConfig
			", new object()).CAF() ?? new DetectLanguageConfig();
	}

	public Task<int> UpsertDetectLanguageConfigAsync(DetectLanguageConfig config)
	{
		return ModifyAsync(@"
				INSERT INTO DetectLanguageConfig
				( APIKey, ConfidenceLimit, CooldownStartTicks )
				VALUES
				( @APIKey, @ConfidenceLimit, @CooldownStartTicks )
			", config);
	}
}