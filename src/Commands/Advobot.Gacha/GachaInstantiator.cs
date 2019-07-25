using Advobot.CommandMarking;
using Advobot.Gacha.Database;
using Advobot.Interfaces;
using Advobot.Utilities;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Advobot.Gacha
{
	public sealed class GachaInstantiation : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services.AddSingleton<DbContextOptions>(services =>
			{
				var dir = services.GetRequiredService<IBotDirectoryAccessor>();
				var options = new DbContextOptionsBuilder();
				var file = AdvobotUtils.ValidateDbPath(dir, "SQLite", "Gacha.db");
				var connectionStringBuilder = new SqliteConnectionStringBuilder
				{
					DataSource = file.FullName,
					Mode = SqliteOpenMode.ReadWriteCreate,
				};
				options.UseSqlite(connectionStringBuilder.ToString());
				return options.Options;
			})
			.AddSingleton<GachaDatabase>()
			.AddTransient<GachaContext>();
			return Task.CompletedTask;
		}
		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			var db = services.GetRequiredService<GachaContext>();
			db.Database.OpenConnection();
			db.Database.EnsureCreated();
			return Task.CompletedTask;
		}
	}
}
