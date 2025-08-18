using Advobot.Services.Time;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases;

public abstract class Database_Tests<TDb> : TestsBase where TDb : class
{
	protected Task<TDb> GetDatabaseAsync()
		=> Services.Value.GetDatabaseAsync<TDb>();

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddFakeDatabase<TDb>()
			.AddSingleton<ITimeService, NaiveTimeService>();
	}
}