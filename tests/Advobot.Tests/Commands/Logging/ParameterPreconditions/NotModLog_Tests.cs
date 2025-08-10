using Advobot.Logging.Database;
using Advobot.Logging.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions;

[TestClass]
public sealed class NotModLog_Tests : ParameterPrecondition_Tests<NotModLog>
{
	private readonly FakeLoggingDatabase _Db = new();

	protected override NotModLog Instance { get; } = new();

	[TestMethod]
	public async Task LogExisting_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Mod, Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

		await AssertFailureAsync(Context.Channel).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task LogNotExisting_Test()
		=> await AssertSuccessAsync(Context.Channel).ConfigureAwait(false);

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase>(_Db);
	}
}