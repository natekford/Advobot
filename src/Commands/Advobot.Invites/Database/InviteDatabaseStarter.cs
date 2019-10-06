using Advobot.Databases.AbstractSQL;
using Advobot.Settings;
using Advobot.Utilities;

namespace Advobot.Invites.Database
{
	public sealed class InviteDatabaseStarter : SQLiteSystemFileDatabaseStarter, IInviteDatabaseStarter
	{
		public InviteDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation(IBotDirectoryAccessor accessor)
			=> AdvobotUtils.ValidateDbPath(accessor, "SQLite", "Invites.db").FullName;
	}
}