using YACCS.Preconditions;
using YACCS.Results;

namespace Advobot.Tests.TestBases;

public abstract class Precondition_Tests<T> : TestsBase
	where T : IPrecondition
{
	protected abstract T Instance { get; }

	protected ValueTask<IResult> CheckPermissionsAsync()
		=> Instance.CheckAsync(null!, Context);
}