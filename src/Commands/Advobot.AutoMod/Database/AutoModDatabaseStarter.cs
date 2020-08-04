using Advobot.Settings;
using Advobot.SQLite;
using Advobot.Utilities;

namespace Advobot.AutoMod.Database
{
	public sealed class AutoModDatabaseStarter : SQLiteSystemFileDatabaseStarter, IAutoModDatabaseStarter
	{
		public AutoModDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation()
			=> Accessor.ValidateDbPath("SQLite", "Invites.db").FullName;
	}
}