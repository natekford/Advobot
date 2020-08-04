using Advobot.Settings;
using Advobot.SQLite;
using Advobot.Utilities;

namespace Advobot.Logging.Database
{
	public sealed class LoggingDatabaseStarter : SQLiteSystemFileDatabaseStarter, ILoggingDatabaseStarter
	{
		public LoggingDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation()
			=> Accessor.ValidateDbPath("SQLite", "Logging.db").FullName;
	}
}