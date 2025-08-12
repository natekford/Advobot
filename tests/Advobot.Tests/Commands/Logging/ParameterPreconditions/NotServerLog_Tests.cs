using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions;

[TestClass]
public sealed class NotServerLog_Tests : ParameterPrecondition_Tests<NotServerLog>
{
	private readonly FakeLoggingDatabase _Db = new();

	protected override NotServerLog Instance { get; } = new();

	[TestMethod]
	public async Task LogExisting_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Server, Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

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