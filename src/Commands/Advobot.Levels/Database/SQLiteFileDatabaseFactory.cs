using System.IO;

using Advobot.Settings;
using Advobot.Utilities;

namespace Advobot.Levels.Database
{
	public sealed class SQLiteFileDatabaseFactory : IDatabaseStarter
	{
		private readonly string _ConnectionString;
		private readonly FileInfo _File;

		public SQLiteFileDatabaseFactory(IBotDirectoryAccessor accessor)
		{
			_File = AdvobotUtils.ValidateDbPath(accessor, "SQLite", "Levels.db");
			_ConnectionString = $"Data Source={_File}";
		}

		public string GetConnectionString()
			=> _ConnectionString;

		public bool IsDatabaseCreated()
			=> File.Exists(_File.FullName);
	}
}