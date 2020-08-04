using Advobot.Settings;
using Advobot.SQLite;
using Advobot.Utilities;

namespace Advobot.Gacha.Database
{
	public sealed class GachaDatabaseStarter : SQLiteSystemFileDatabaseStarter, IGachaDatabaseStarter
	{
		public GachaDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation()
			=> Accessor.ValidateDbPath("SQLite", "Gacha.db").FullName;
	}
}