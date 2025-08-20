using Advobot.Tests.Utilities;

namespace Advobot.Tests.TestBases;

public abstract class Database_Tests<T> : TestsBase
	where T : class
{
	protected T Db { get; set; }

	protected override async Task SetupAsync()
		=> Db = await Services.GetDatabaseAsync<T>().ConfigureAwait(false);
}