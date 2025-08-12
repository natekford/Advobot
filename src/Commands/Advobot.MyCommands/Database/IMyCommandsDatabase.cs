using Advobot.MyCommands.Database.Models;

namespace Advobot.MyCommands.Database;

public interface IMyCommandsDatabase
{
	Task<DetectLanguageConfig> GetDetectLanguageConfigAsync();

	Task<int> UpsertDetectLanguageConfigAsync(DetectLanguageConfig config);
}