using System;
using System.IO;

using Advobot.Levels.Database;

namespace Advobot.Tests.Levels
{
	public sealed class SQLiteTestDatabaseFactory : IDatabaseStarter
	{
		private readonly string _ConnectionString;

		public SQLiteTestDatabaseFactory()
		{
			var file = Path.Combine(Environment.CurrentDirectory, "Database", "Levels.db");
			Directory.CreateDirectory(Path.GetDirectoryName(file));
			if (File.Exists(file))
			{
				File.Delete(file);
				using var _ = File.Create(file);
			}
			_ConnectionString = $"Data Source={file}";
		}

		public string GetConnectionString()
			=> _ConnectionString;

		public bool IsDatabaseCreated()
			=> false;
	}
}