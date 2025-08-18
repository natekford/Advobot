using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions;

[TestClass]
public sealed class NotImageLog_Tests : ParameterPrecondition_Tests<NotImageLog>
{
	protected override NotImageLog Instance { get; } = new();

	[TestMethod]
	public async Task LogExisting_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);
		await db.UpsertLogChannelAsync(Log.Image, Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

		await AssertFailureAsync(Context.Channel).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task LogNotExisting_Test()
		=> await AssertSuccessAsync(Context.Channel).ConfigureAwait(false);

	protected override void ModifyServices(IServiceCollection services)
		=> services.AddFakeDatabase<LoggingDatabase>();

	protected override Task SetupAsync()
		=> GetDatabaseAsync();

	private Task<LoggingDatabase> GetDatabaseAsync()
		=> Services.Value.GetDatabaseAsync<LoggingDatabase>();
}