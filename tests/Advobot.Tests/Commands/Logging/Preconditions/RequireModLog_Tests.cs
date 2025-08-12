using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Preconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.Preconditions;

[TestClass]
public sealed class RequireModLog_Tests : Precondition_Tests<RequireModLog>
{
	private readonly FakeLoggingDatabase _Db = new();
	protected override RequireModLog Instance { get; } = new();

	[TestMethod]
	public async Task DoesNotHaveLog_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Mod, Context.Guild.Id, null).ConfigureAwait(false);
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task HasLog_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Mod, Context.Guild.Id, 73).ConfigureAwait(false);
		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase>(_Db);
	}
}