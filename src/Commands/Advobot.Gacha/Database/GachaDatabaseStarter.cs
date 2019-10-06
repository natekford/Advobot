using Advobot.Databases.AbstractSQL;
using Advobot.Settings;
using Advobot.Utilities;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabaseStarter : SQLiteSystemFileDatabaseStarter, IGachaDatabaseStarter
	{
		public GachaDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation(IBotDirectoryAccessor accessor)
			=> AdvobotUtils.ValidateDbPath(accessor, "SQLite", "Gacha.db").FullName;
	}
}