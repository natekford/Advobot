using System;
using System.IO;
using System.Threading.Tasks;

using Advobot.Databases.AbstractSQL;

namespace Advobot.Tests.Fakes.Database
{
	public abstract class FakeSQLiteDatabaseStarter : IDatabaseStarter
	{
		/// <inheritdoc />
		public virtual Task EnsureCreatedAsync()
		{
			var location = GetLocation();
			Directory.CreateDirectory(Path.GetDirectoryName(location));
			if (File.Exists(location))
			{
				File.Delete(location);
			}
			using (File.Create(location)) { }
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public virtual string GetConnectionString()
			=> $"Data Source={GetLocation()}";

		public abstract string GetDbFileName();

		private string GetLocation()
			=> Path.Combine(Environment.CurrentDirectory, "TestDatabases", GetDbFileName());
	}
}