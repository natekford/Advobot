using AdvorangesUtils;

using Discord.Commands;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Advobot.Tests.TestBases;

public abstract class TypeReaderTestsBase : TestsBase
{
	protected abstract TypeReader Instance { get; }
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
		=> Instance.ReadAsync(Context, input, Services);
}