using Advobot.Interfaces;
using Advobot.Utilities;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace Advobot.Gacha.Database
{
	public sealed class SQLiteFileDatabaseFactory : IDatabaseStarter
	{
		private readonly IBotDirectoryAccessor _Directory;
		private readonly FileInfo _File;
		private readonly string _ConnectionString;

		public SQLiteFileDatabaseFactory(IServiceProvider provider)
		{
			_Directory = provider.GetRequiredService<IBotDirectoryAccessor>();
			_File = AdvobotUtils.ValidateDbPath(_Directory, "SQLite", "Gacha.db");
			_ConnectionString = $"Data Source={_File}";
		}

		public string GetConnectionString()
			=> _ConnectionString;
		public bool IsDatabaseCreated()
			=> File.Exists(_File.FullName);
	}
}
