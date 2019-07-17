#if false
using System;
using System.IO;
using Advobot.Interfaces;
using Advobot.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Databases.EFCore
{
	/// <summary>
	/// Generates wrappers for SQLite through EF Core.
	/// </summary>
	internal sealed class SQLiteWrapperFactory : EFCoreWrapperFactory
	{
		private readonly IBotDirectoryAccessor _DirectoryAccessor;

		/// <summary>
		/// Creates an instance of <see cref="SQLiteWrapperFactory"/>.
		/// </summary>
		/// <param name="provider"></param>
		public SQLiteWrapperFactory(IServiceProvider provider)
		{
			_DirectoryAccessor = provider.GetRequiredService<IBotDirectoryAccessor>();
		}

		protected override DbContextOptionsBuilder GenerateOptions(string databaseName)
		{
			var dbFile = AdvobotUtils.EnsureDb("SQLite", databaseName);
			var path = _DirectoryAccessor.GetBaseBotDirectoryFile(dbFile).FullName;
			//Make sure the directory the db will be created in exists
			Directory.CreateDirectory(Path.GetDirectoryName(path));

			var connectionStringBuilder = new SqliteConnectionStringBuilder
			{
				DataSource = path,
				Mode = SqliteOpenMode.ReadWriteCreate,
			};

			var options = new DbContextOptionsBuilder();
			options.UseSqlite(connectionStringBuilder.ToString());
			return options;
		}
	}
}
#endif