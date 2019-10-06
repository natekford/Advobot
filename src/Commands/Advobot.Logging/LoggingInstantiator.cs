using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Logging.Database;
using Advobot.Logging.Service;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Logging
{
	public sealed class LoggingInstantiator : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<LoggingDatabase>()
				.AddSingleton<ILoggingService, LoggingService>()
				.AddSingleton<ILoggingDatabaseStarter, LoggingDatabaseStarter>();
			return Task.CompletedTask;
		}

		public async Task ConfigureServicesAsync(IServiceProvider services)
		{
			var db = services.GetRequiredService<LoggingDatabase>();
			await db.CreateDatabaseAsync().CAF();

			/*
			var settings = services.GetRequiredService<IGuildSettingsFactory>();
			var allSettings = await settings.GetAllAsync().CAF();
			foreach (var s in allSettings)
			{
				await db.AddIgnoredChannelsAsync(s.GuildId, s.IgnoredLogChannels).CAF();
				await db.AddLogActionsAsync(s.GuildId, s.LogActions).CAF();
				await db.AddLogChannelsAsync(s.GuildId, new LogChannels
				{
					ImageLogId = s.ImageLogId.ToString()
				}).CAF();
			}*/

			//Needed to instasntiate the log service
			services.GetRequiredService<ILoggingService>();
		}
	}
}