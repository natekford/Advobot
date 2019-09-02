using System;
using System.IO;

using Advobot.Settings;
using Advobot.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Levels.Database
{
	public sealed class SQLiteFileDatabaseFactory : IDatabaseStarter
	{
		private readonly string _ConnectionString;
		private readonly IBotDirectoryAccessor _Directory;
		private readonly FileInfo _File;

		public SQLiteFileDatabaseFactory(IServiceProvider provider)
		{
			_Directory = provider.GetRequiredService<IBotDirectoryAccessor>();
			_File = AdvobotUtils.ValidateDbPath(_Directory, "SQLite", "Levels.db");
			_ConnectionString = $"Data Source={_File}";
		}

		public string GetConnectionString()
			=> _ConnectionString;

		public bool IsDatabaseCreated()
			=> File.Exists(_File.FullName);
	}
}