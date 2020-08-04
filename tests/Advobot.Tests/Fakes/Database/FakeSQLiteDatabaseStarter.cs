using System;
using System.IO;
using System.Threading.Tasks;

using Advobot.Gacha.Database;
using Advobot.Invites.Database;
using Advobot.Levels.Database;
using Advobot.Logging.Database;

namespace Advobot.Tests.Fakes.Database
{
	public sealed class FakeSQLiteDatabaseStarter :
		ILevelDatabaseStarter,
		ILoggingDatabaseStarter,
		INotificationDatabaseStarter,
		IInviteDatabaseStarter,
		IGachaDatabaseStarter
	{
		private readonly string Id = Guid.NewGuid().ToString();

		/// <inheritdoc />
		public Task EnsureCreatedAsync()
		{
			var location = GetLocation();
			Directory.CreateDirectory(Path.GetDirectoryName(location));
			using (File.Create(location)) { }
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public string GetConnectionString()
			=> $"Data Source={GetLocation()}";

		private string GetLocation()
			=> Path.Combine(Environment.CurrentDirectory, "TestDatabases", $"{Id}.db");
	}
}