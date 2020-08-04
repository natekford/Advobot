using Advobot.Settings;
using Advobot.SQLite;
using Advobot.Utilities;

namespace Advobot.Invites.Database
{
	public sealed class InviteDatabaseStarter : SQLiteSystemFileDatabaseStarter, IInviteDatabaseStarter
	{
		public InviteDatabaseStarter(IBotDirectoryAccessor accessor) : base(accessor)
		{
		}

		public override string GetLocation()
			=> Accessor.ValidateDbPath("SQLite", "Invites.db").FullName;
	}
}