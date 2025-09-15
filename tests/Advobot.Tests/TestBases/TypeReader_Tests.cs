using MorseCode.ITask;

using YACCS.TypeReaders;

namespace Advobot.Tests.TestBases;

public abstract class TypeReader_Tests<T> : TestsBase
	where T : ITypeReader
{
	protected abstract T Instance { get; }
	protected virtual string? NotExisting { get; } = "asdf";

	[TestMethod]
	public async Task NotExisting_Test()
	{
		if (NotExisting is null)
		{
			return;
		}

		var result = await ReadAsync(NotExisting).ConfigureAwait(false);
		Assert.IsFalse(result.InnerResult.IsSuccess);
	}

	protected ITask<ITypeReaderResult> ReadAsync(string input)
		=> Instance.ReadAsync(Context, new[] { input });
}