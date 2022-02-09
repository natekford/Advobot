using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.Preconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.Preconditions;

[TestClass]
public sealed class RequireServerLog_Tests : Precondition_Tests<RequireServerLog>
{
	private readonly FakeLoggingDatabase _Db = new();
	protected override RequireServerLog Instance { get; } = new();

	[TestMethod]
	public async Task DoesNotHaveLog_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Server, Context.Guild.Id, null).CAF();
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task HasLog_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Server, Context.Guild.Id, 73).CAF();
		var result = await CheckPermissionsAsync().CAF();
		Assert.IsTrue(result.IsSuccess);
	}

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase>(_Db);
	}
}