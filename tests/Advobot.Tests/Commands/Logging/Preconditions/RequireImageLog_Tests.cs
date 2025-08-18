using Advobot.Logging.Database;
using Advobot.Logging.Database.Models;
using Advobot.Logging.Preconditions;
using Advobot.Tests.TestBases;
using Advobot.Tests.Utilities;

using Microsoft.Extensions.DependencyInjection;

namespace Advobot.Tests.Commands.Logging.Preconditions;

[TestClass]
public sealed class RequireImageLog_Tests : Precondition_Tests<RequireImageLog>
{
	protected override RequireImageLog Instance { get; } = new();

	[TestMethod]
	public async Task DoesNotHaveLog_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);
		await db.UpsertLogChannelAsync(Log.Image, Context.Guild.Id, null).ConfigureAwait(false);

		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
	}

	[TestMethod]
	public async Task HasLog_Test()
	{
		var db = await GetDatabaseAsync().ConfigureAwait(false);
		await db.UpsertLogChannelAsync(Log.Image, Context.Guild.Id, 73).ConfigureAwait(false);

		var result = await CheckPermissionsAsync().ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
	}

	protected override Task SetupAsync()
		=> GetDatabaseAsync();

	private Task<LoggingDatabase> GetDatabaseAsync()
		=> Services.GetDatabaseAsync<LoggingDatabase>();
}