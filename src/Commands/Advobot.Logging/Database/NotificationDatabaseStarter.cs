using Advobot.Databases.AbstractSQL;
using Advobot.Settings;
using Advobot.Utilities;

namespace Advobot.Logging.Database
{
	public sealed class NotificationDatabaseStarter : SQLiteSystemFileDatabaseStarter, INotificationDatabaseStarter
	{
		public NotificationDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation()
			=> Accessor.ValidateDbPath("SQLite", "Notification.db").FullName;
	}
}