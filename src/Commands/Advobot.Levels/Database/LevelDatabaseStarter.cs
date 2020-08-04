using Advobot.Settings;
using Advobot.SQLite;
using Advobot.Utilities;

namespace Advobot.Levels.Database
{
	public sealed class LevelDatabaseStarter : SQLiteSystemFileDatabaseStarter, ILevelDatabaseStarter
	{
		public LevelDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation()
			=> Accessor.ValidateDbPath("SQLite", "Levels.db").FullName;
	}
}