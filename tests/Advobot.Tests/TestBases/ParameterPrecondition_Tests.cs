using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.Tests.TestBases;

public abstract class ParameterPrecondition_Tests<T> : TestsBase
	 where T : IParameterPrecondition
{
	protected abstract T Instance { get; }

	[TestMethod]
	public async Task InvalidType_Test()
		=> await AssertFailureAsync(new object()).ConfigureAwait(false);

	protected async Task<IResult> AssertFailureAsync(object value)
	{
		var result = await Instance.CheckAsync(
			new(), Context, value
		).ConfigureAwait(false);
		Assert.IsFalse(result.IsSuccess);
		return result;
	}

	protected async Task<IResult> AssertSuccessAsync(object value)
	{
		var result = await Instance.CheckAsync(
			new(), Context, value
		).ConfigureAwait(false);
		Assert.IsTrue(result.IsSuccess);
		return result;
	}
}