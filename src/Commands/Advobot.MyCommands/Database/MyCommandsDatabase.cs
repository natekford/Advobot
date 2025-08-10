using Advobot.MyCommands.Models;
using Advobot.SQLite;

using System.Data.SQLite;

namespace Advobot.MyCommands.Database;

public sealed class MyCommandsDatabase(IConnectionString<MyCommandsDatabase> conn) : DatabaseBase<SQLiteConnection>(conn), IMyCommandsDatabase
{
	public async Task<DetectLanguageConfig> GetDetectLanguageConfigAsync()
	{
		return await GetOneAsync<DetectLanguageConfig>(@"
			SELECT * From DetectLanguageConfig
		", new()).ConfigureAwait(false) ?? new DetectLanguageConfig();
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