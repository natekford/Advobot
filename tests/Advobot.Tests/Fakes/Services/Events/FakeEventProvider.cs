using Advobot.Services.Events;

namespace Advobot.Tests.Fakes.Services.Events;

public sealed class FakeEventProvider : EventProvider
{
	protected override Task StartAsyncImpl()
		=> Task.CompletedTask;
}