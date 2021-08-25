
using Advobot.MyCommands.Models;

namespace Advobot.MyCommands.Database
{
	public interface IMyCommandsDatabase
	{
		Task<DetectLanguageConfig> GetDetectLanguageConfig();

		Task<int> UpsertDetectLanguageConfig(DetectLanguageConfig config);
	}
}