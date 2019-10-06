using Advobot.Databases.AbstractSQL;
using Advobot.Settings;
using Advobot.Utilities;

namespace Advobot.Levels.Database
{
	public sealed class LevelDatabaseStarter : SQLiteSystemFileDatabaseStarter, ILevelDatabaseStarter
	{
		public LevelDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation(IBotDirectoryAccessor accessor)
			=> AdvobotUtils.ValidateDbPath(accessor, "SQLite", "Levels.db").FullName;
	}
}