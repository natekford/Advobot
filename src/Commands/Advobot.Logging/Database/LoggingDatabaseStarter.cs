using Advobot.Databases.AbstractSQL;
using Advobot.Settings;
using Advobot.Utilities;

namespace Advobot.Logging.Database
{
	public sealed class LoggingDatabaseStarter : SQLiteSystemFileDatabaseStarter, ILoggingDatabaseStarter
	{
		public LoggingDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation(IBotDirectoryAccessor accessor)
			=> AdvobotUtils.ValidateDbPath(accessor, "SQLite", "Logging.db").FullName;
	}
}