
using Advobot.Services.Time;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases
{
	public abstract class TestsBase
	{
		protected FakeCommandContext Context { get; } = FakeUtils.CreateContext();
		protected Random Rng { get; } = new Random();
		protected IServiceProvider Services { get; }
		protected ITime Time => Services.GetRequiredService<ITime>();

		protected TestsBase()
		{
			var services = new ServiceCollection();
			ModifyServices(services);
			Services = services.BuildServiceProvider();
		}

		protected virtual void ModifyServices(IServiceCollection services)
		{
		}
	}
}