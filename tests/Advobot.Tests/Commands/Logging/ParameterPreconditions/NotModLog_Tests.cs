using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.ParameterPreconditions;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.ParameterPreconditions;

[TestClass]
public sealed class NotModLog_Tests : ParameterPrecondition_Tests<NotModLog>
{
	protected override NotModLog Instance { get; } = new();

	[TestMethod]
	public async Task LogExisting_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);
		await db.UpsertLogChannelAsync(Log.Mod, Context.Guild.Id, Context.Channel.Id).ConfigureAwait(false);

		await AssertFailureAsync(Context.Channel).ConfigureAwait(false);
	}

	[TestMethod]
	public async Task LogNotExisting_Test()
		=> await AssertSuccessAsync(Context.Channel).ConfigureAwait(false);

	protected override Task SetupAsync()
		=> GetDatabaseAsync();

	private Task<LoggingDatabase> GetDatabaseAsync()
		=> Services.GetDatabaseAsync<LoggingDatabase>();
}