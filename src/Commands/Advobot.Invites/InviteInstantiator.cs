using System;
using System.Threading.Tasks;

using Advobot.CommandAssemblies;
using Advobot.Invites.Database;
using Advobot.Invites.Service;
using Advobot.SQLite;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Invites
{
	public sealed class InviteInstantiator : ICommandAssemblyInstantiator
	{
		public Task AddServicesAsync(IServiceCollection services)
		{
			services
				.AddSingleton<InviteDatabase>()
				.AddSingleton<IInviteListService, InviteListService>()
				.AddSingleton<IInviteDatabaseStarter, InviteDatabaseStarter>();

			return Task.CompletedTask;
		}

		public Task ConfigureServicesAsync(IServiceProvider services)
		{
			services.GetRequiredService<IInviteDatabaseStarter>().MigrateUp();

			return Task.CompletedTask;
		}
	}
}