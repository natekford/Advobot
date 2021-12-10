using Advobot.Logging;
using Advobot.Logging.Database;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.Fakes.Services.Logging;
using Advobot.Tests.TestBases;

using AdvorangesUtils;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions;

[TestClass]
public sealed class NotImageLogAttribute_Tests
	: ParameterPreconditionTestsBase<NotImageLogAttribute>
{
	private readonly FakeLoggingDatabase _Db = new();

	protected override NotImageLogAttribute Instance { get; } = new();

	[TestMethod]
	public async Task LogExisting_Test()
	{
		await _Db.UpsertLogChannelAsync(Log.Image, Context.Guild.Id, Context.Channel.Id).CAF();

		await AssertFailureAsync(Context.Channel).CAF();
	}

	[TestMethod]
	public async Task LogNotExisting_Test()
		=> await AssertSuccessAsync(Context.Channel).CAF();

	protected override void ModifyServices(IServiceCollection services)
	{
		services
			.AddSingleton<ILoggingDatabase>(_Db);
	}
}