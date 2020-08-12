using System;
using System.IO;
using System.Threading.Tasks;

using Advobot.SQLite;

namespace Advobot.Tests.Fakes.Database
{
	public sealed class FakeSQLiteConnectionString : IConnectionStringFor<object>
	{
		public string ConnectionString { get; }
		public string Id { get; }
		public string Location { get; }

		public FakeSQLiteConnectionString()
		{
			Id = Guid.NewGuid().ToString();
			Location = Path.Combine(Environment.CurrentDirectory, "TestDatabases", $"{Id}.db");
			ConnectionString = $"Data Source={Location}";
		}

		/// <inheritdoc />
		public Task EnsureCreatedAsync()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(Location));
			using (File.Create(Location)) { }
			return Task.CompletedTask;
		}
	}
}