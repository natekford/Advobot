using Advobot.Services.Time;
using Advobot.Tests.Fakes.Discord;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.TestBases;

public abstract class TestsBase
{
	protected FakeCommandContext Context { get; } = FakeUtils.CreateContext();
	protected Random Rng { get; } = new();
	protected Lazy<IServiceProvider> Services { get; }
	protected ITime Time => Services.Value.GetRequiredService<ITime>();

	protected TestsBase()
	{
		Services = new(() =>
		{
			var services = new ServiceCollection();
			ModifyServices(services);
			return services.BuildServiceProvider();
		});
	}

	protected virtual void ModifyServices(IServiceCollection services)
	{
	}
}