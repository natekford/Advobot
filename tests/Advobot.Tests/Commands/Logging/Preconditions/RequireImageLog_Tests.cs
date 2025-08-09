using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.Preconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.Preconditions;

[TestClass]
public sealed class RequireImageLog_Tests : Precondition_Tests<RequireImageLog>
{
	private readonly FakeLoggingDatabase _Db = new();
	protected override RequireImageLog Instance { get; } = new();

	[TestMethod]
	public async Task DoesNotHaveLog_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Image, Context.Guild.Id, null).ConfigureAwait(false);
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task HasLog_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Image, Context.Guild.Id, 73).ConfigureAwait(false);
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase>(_Db);
	}
}