using AdvorangesUtils;

using Discord.Commands;

namespace Advobot.Tests.TestBases;

public abstract class TypeReader_Tests<T> : TestsBase
	where T : TypeReader
{
	protected abstract T Instance { get; }
	protected virtual string? NotExisting { get; } = "asdf";

	[TestMethod]
	public async Task NotExisting_Test()
	{
		if (NotExisting == null)
		{
			return;
		}

		var result = await ReadAsync(NotExisting).CAF();
		Assert.IsFalse(result.IsSuccess);
	}

	protected Task<TypeReaderResult> ReadAsync(string input)
		=> Instance.ReadAsync(Context, input, Services.Value);
}